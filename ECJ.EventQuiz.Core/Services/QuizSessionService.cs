using System.Collections.Concurrent;
using ECJ.EventQuiz.Core.Data;
using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ECJ.EventQuiz.Core.Services;

/// <summary>
/// Live quiz rooms. Quizzes are loaded from the SQLite database, and sessions,
/// participants and answers are saved there too. Live-only state (phase, connection ids,
/// timer) is kept in memory.
/// The SignalR hub calls this service; React clients never talk to it directly.
/// </summary>
public class QuizSessionService(IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly ConcurrentDictionary<string, QuizSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Random _random = new();

    public SessionCreatedDto CreateSession(int quizId, string? hostConnectionId)
    {
        using var db = dbFactory.CreateDbContext();
        var quiz = LoadQuiz(db, quizId)
            ?? throw new InvalidOperationException($"Quizzen {quizId} findes ikke.");

        string roomId;
        do
        {
            roomId = _random.Next(1000, 10000).ToString();
        } while (_sessions.ContainsKey(roomId) || db.QuizSessions.Any(s => s.RoomId == roomId));

        db.QuizSessions.Add(new QuizSession { RoomId = roomId, QuizId = quiz.Id, Status = SessionStatus.Lobby });
        db.SaveChanges();

        var session = new QuizSession
        {
            RoomId = roomId,
            QuizId = quiz.Id,
            Quiz = quiz,
            Status = SessionStatus.Lobby,
            Phase = LivePhase.Lobby,
            HostConnectionId = hostConnectionId
        };

        if (!_sessions.TryAdd(roomId, session))
            throw new InvalidOperationException("Kunne ikke oprette rummet.");

        return new SessionCreatedDto(roomId, quiz.Id, quiz.Name);
    }

    public QuizSession GetRequired(string roomId)
    {
        if (_sessions.TryGetValue(roomId, out var session))
            return session;

        // Rooms created via the API (POST /api/quizsession) are loaded from the database.
        return LoadLobbyFromDatabase(roomId)
            ?? throw new InvalidOperationException($"Rummet {roomId} findes ikke. Tjek Room ID'et.");
    }

    /// <summary>True if the room exists and is still open for joining (lobby).</summary>
    public bool CanJoin(string roomId)
    {
        try { return GetRequired(roomId).Status == SessionStatus.Lobby; }
        catch (InvalidOperationException) { return false; }
    }

    /// <summary>Live lobby-rum som deltagere kan tilmelde sig lige nu.</summary>
    public IReadOnlyList<(string RoomId, string QuizName, int ParticipantCount)> ListOpenLobbies()
    {
        return _sessions.Values
            .Where(s => s.Status == SessionStatus.Lobby)
            .OrderBy(s => s.RoomId)
            .Select(s => (s.RoomId, s.Quiz?.Name ?? "Quiz", s.Participants.Count))
            .ToList();
    }

    public void SetHostConnection(string roomId, string connectionId)
    {
        GetRequired(roomId).HostConnectionId = connectionId;
    }

    public LobbyUpdatedDto JoinLobby(string roomId, string name, string connectionId)
    {
        var session = GetRequired(roomId);
        if (session.Status != SessionStatus.Lobby)
            throw new InvalidOperationException("Quizzen er allerede startet.");

        // Værten styrer rummet og skal ikke kunne tilmelde sig som spiller på samme forbindelse.
        if (!string.IsNullOrEmpty(session.HostConnectionId)
            && string.Equals(session.HostConnectionId, connectionId, StringComparison.Ordinal))
            throw new InvalidOperationException("Værten kan ikke deltage som spiller. Åbn quizzen på en anden enhed eller i et andet browservindue.");

        var clean = string.IsNullOrWhiteSpace(name) ? "Gæst" : name.Trim();
        if (session.Participants.Any(p => p.Name.Equals(clean, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Navnet er allerede i brug. Vælg et andet.");

        var participant = new Participant { Name = clean };
        using (var db = dbFactory.CreateDbContext())
        {
            db.Participants.Add(participant);
            db.QuizSessions.Attach(new QuizSession { RoomId = session.RoomId, QuizId = session.QuizId }).Entity.Participants.Add(participant);
            db.SaveChanges();
        }

        participant.ConnectionId = connectionId;
        session.Participants.Add(participant);
        session.Scores[participant.Id] = 0;

        return ToLobbyUpdated(session);
    }

    public LobbyUpdatedDto RemoveByConnection(string connectionId)
    {
        foreach (var session in _sessions.Values)
        {
            var participant = session.Participants.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (participant is null)
                continue;

            if (session.Status == SessionStatus.Lobby)
            {
                session.Participants.Remove(participant);
                session.Scores.Remove(participant.Id);
                using (var db = dbFactory.CreateDbContext())
                    db.Participants.Where(p => p.Id == participant.Id).ExecuteDelete();
                return ToLobbyUpdated(session);
            }

            participant.ConnectionId = null;
            return ToLobbyUpdated(session);
        }

        return new LobbyUpdatedDto(string.Empty, []);
    }

    public QuestionChangedDto StartQuiz(string roomId)
    {
        var session = GetRequired(roomId);
        if (session.Participants.Count == 0)
            throw new InvalidOperationException("Der skal være mindst én deltager.");
        if (session.Quiz is null || session.Quiz.Questions.Count == 0)
            throw new InvalidOperationException("Quizzen har ingen spørgsmål.");

        session.Status = SessionStatus.Running;
        session.Phase = LivePhase.Countdown;
        session.CurrentQuestionIndex = 0;
        session.CurrentQuestionId = session.Quiz.Questions[0].Id;
        session.Answers.Clear();
        foreach (var id in session.Scores.Keys.ToList())
            session.Scores[id] = 0;

        using (var db = dbFactory.CreateDbContext())
        {
            db.ParticipantAnswers.Where(a => EF.Property<string>(a, "RoomId") == session.RoomId).ExecuteDelete();
            SaveSessionState(db, session);
        }

        return ToQuestionChanged(session);
    }

    public QuestionChangedDto BeginQuestion(string roomId)
    {
        var session = GetRequired(roomId);
        EnsureRunning(session);
        session.Phase = LivePhase.Question;
        session.QuestionStartedAtUtc = DateTime.UtcNow;
        return ToQuestionChanged(session);
    }

    public (ParticipantAnswer Answer, AnswerCountsUpdatedDto Counts) SubmitAnswer(
        string roomId,
        string connectionId,
        int questionId,
        int answerOptionId)
    {
        var session = GetRequired(roomId);
        EnsureRunning(session);

        if (session.Phase != LivePhase.Question)
            throw new InvalidOperationException("Der kan ikke svares lige nu.");

        var participant = session.Participants.FirstOrDefault(p => p.ConnectionId == connectionId)
            ?? throw new InvalidOperationException("Deltageren findes ikke.");

        if (session.Answers.Any(a => a.ParticipantId == participant.Id && a.QuestionId == questionId))
            throw new InvalidOperationException("Du har allerede svaret på dette spørgsmål.");

        var question = session.Quiz!.Questions.FirstOrDefault(q => q.Id == questionId)
            ?? throw new InvalidOperationException("Spørgsmålet findes ikke.");

        var option = question.AnswerOptions.FirstOrDefault(o => o.Id == answerOptionId)
            ?? throw new InvalidOperationException("Svarmuligheden findes ikke.");

        var elapsed = session.QuestionStartedAtUtc.HasValue
            ? (DateTime.UtcNow - session.QuestionStartedAtUtc.Value).TotalSeconds
            : 0;

        var points = ScoreCalculator.Calculate(option.IsCorrect, elapsed, question.TimeLimitSeconds);
        var answer = new ParticipantAnswer
        {
            ParticipantId = participant.Id,
            QuestionId = questionId,
            AnswerOptionId = answerOptionId,
            IsCorrect = option.IsCorrect,
            Points = points,
            AnsweredAfterSeconds = elapsed
        };

        using (var db = dbFactory.CreateDbContext())
        {
            db.ParticipantAnswers.Add(answer);
            db.Entry(answer).Property("RoomId").CurrentValue = session.RoomId;
            db.SaveChanges();
        }

        session.Answers.Add(answer);
        session.Scores[participant.Id] = session.Scores.GetValueOrDefault(participant.Id) + points;

        return (answer, ToAnswerCounts(session, question));
    }

    public RevealDto CloseQuestion(string roomId)
    {
        var session = GetRequired(roomId);
        EnsureRunning(session);

        var question = CurrentQuestion(session);
        session.Phase = LivePhase.Reveal;

        var correct = question.AnswerOptions.First(o => o.IsCorrect);
        return new RevealDto(
            session.RoomId,
            question.Id,
            correct.Id,
            CountAnswers(session, question),
            LeaderBoardCalculator.Calculate(session));
    }

    public object NextQuestionOrEnd(string roomId)
    {
        var session = GetRequired(roomId);
        EnsureRunning(session);

        var nextIndex = session.CurrentQuestionIndex + 1;
        if (nextIndex >= session.Quiz!.Questions.Count)
            return EndQuiz(roomId);

        session.CurrentQuestionIndex = nextIndex;
        session.CurrentQuestionId = session.Quiz.Questions[nextIndex].Id;
        session.Phase = LivePhase.Countdown;
        session.QuestionStartedAtUtc = null;

        using (var db = dbFactory.CreateDbContext())
            SaveSessionState(db, session);

        return ToQuestionChanged(session);
    }

    public QuizEndedDto EndQuiz(string roomId)
    {
        var session = GetRequired(roomId);
        session.Status = SessionStatus.Finished;
        session.Phase = LivePhase.Results;

        using (var db = dbFactory.CreateDbContext())
            SaveSessionState(db, session);

        return new QuizEndedDto(session.RoomId, session.Quiz?.Name ?? string.Empty, LeaderBoardCalculator.Calculate(session));
    }

    public AnswerCountsUpdatedDto GetAnswerCounts(string roomId)
    {
        var session = GetRequired(roomId);
        return ToAnswerCounts(session, CurrentQuestion(session));
    }

    private static Quiz? LoadQuiz(AppDbContext db, int quizId)
    {
        var quiz = db.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions).ThenInclude(q => q.AnswerOptions)
            .AsSplitQuery()
            .FirstOrDefault(q => q.Id == quizId);
        if (quiz is null)
            return null;

        quiz.Questions = quiz.Questions.OrderBy(q => q.Id).ToList();
        foreach (var question in quiz.Questions)
            question.AnswerOptions = question.AnswerOptions.OrderBy(o => o.Id).ToList();
        return quiz;
    }

    private QuizSession? LoadLobbyFromDatabase(string roomId)
    {
        using var db = dbFactory.CreateDbContext();
        var stored = db.QuizSessions
            .AsNoTracking()
            .Include(s => s.Participants)
            .FirstOrDefault(s => s.RoomId == roomId && s.Status == SessionStatus.Lobby);
        if (stored is null)
            return null;

        var session = new QuizSession
        {
            RoomId = stored.RoomId,
            QuizId = stored.QuizId,
            Quiz = LoadQuiz(db, stored.QuizId),
            Status = SessionStatus.Lobby,
            Phase = LivePhase.Lobby,
            Participants = stored.Participants,
            Scores = stored.Participants.ToDictionary(p => p.Id, _ => 0)
        };
        return _sessions.GetOrAdd(session.RoomId, session);
    }

    private static void SaveSessionState(AppDbContext db, QuizSession session)
    {
        db.QuizSessions
            .Where(s => s.RoomId == session.RoomId)
            .ExecuteUpdate(s => s
                .SetProperty(x => x.Status, session.Status)
                .SetProperty(x => x.CurrentQuestionId, session.CurrentQuestionId));
    }

    private static void EnsureRunning(QuizSession session)
    {
        if (session.Status != SessionStatus.Running)
            throw new InvalidOperationException("Quizzen kører ikke.");
    }

    private static Question CurrentQuestion(QuizSession session)
    {
        if (session.Quiz is null)
            throw new InvalidOperationException("Quizzen mangler.");
        return session.Quiz.Questions[session.CurrentQuestionIndex];
    }

    private static LobbyUpdatedDto ToLobbyUpdated(QuizSession session) =>
        new(session.RoomId, session.Participants.Select(p => new LobbyParticipantDto(p.Id, p.Name)).ToList());

    private static QuestionChangedDto ToQuestionChanged(QuizSession session)
    {
        var question = CurrentQuestion(session);
        var dto = new QuestionDto(
            question.Id,
            question.Text,
            question.TimeLimitSeconds,
            session.CurrentQuestionIndex,
            session.Quiz!.Questions.Count,
            question.AnswerOptions.Select(o => new AnswerOptionDto(o.Id, o.Answer)).ToList());

        return new QuestionChangedDto(session.RoomId, session.Phase.ToString().ToLowerInvariant(), dto);
    }

    private static AnswerCountsUpdatedDto ToAnswerCounts(QuizSession session, Question question) =>
        new(session.RoomId, question.Id, CountAnswers(session, question));

    private static IReadOnlyList<int> CountAnswers(QuizSession session, Question question) =>
        question.AnswerOptions
            .Select(o => session.Answers.Count(a => a.QuestionId == question.Id && a.AnswerOptionId == o.Id))
            .ToList();
}
