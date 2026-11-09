using System.Collections.Concurrent;
using ECJ.EventQuiz.Core.Data;
using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Core.Models;

namespace ECJ.EventQuiz.Core.Services;

/// <summary>
/// In-memory store for live quiz rooms.
/// The SignalR hub calls this service; React clients never talk to it directly.
/// Replace seed lookups with EF later without changing the hub method names.
/// </summary>
public class QuizSessionService
{
    private readonly ConcurrentDictionary<string, QuizSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private int _nextParticipantId = 1;
    private readonly Random _random = new();

    public SessionCreatedDto CreateSession(int quizId, string? hostConnectionId)
    {
        var quiz = SeedData.GetQuiz(quizId)
            ?? throw new InvalidOperationException($"Quiz {quizId} was not found.");

        string roomId;
        do
        {
            roomId = _random.Next(1000, 10000).ToString();
        } while (_sessions.ContainsKey(roomId));

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
            throw new InvalidOperationException("Could not create session.");

        return new SessionCreatedDto(roomId, quiz.Id, quiz.Name);
    }

    public QuizSession GetRequired(string roomId)
    {
        if (!_sessions.TryGetValue(roomId, out var session))
            throw new InvalidOperationException($"Room {roomId} was not found.");
        return session;
    }

    public void SetHostConnection(string roomId, string connectionId)
    {
        GetRequired(roomId).HostConnectionId = connectionId;
    }

    public LobbyUpdatedDto JoinLobby(string roomId, string name, string connectionId)
    {
        var session = GetRequired(roomId);
        if (session.Status != SessionStatus.Lobby)
            throw new InvalidOperationException("The quiz has already started.");

        var clean = string.IsNullOrWhiteSpace(name) ? "Gæst" : name.Trim();
        if (session.Participants.Any(p => p.Name.Equals(clean, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("That name is already used in this lobby.");

        var participant = new Participant
        {
            Id = Interlocked.Increment(ref _nextParticipantId),
            Name = clean,
            ConnectionId = connectionId
        };
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
            throw new InvalidOperationException("At least one participant is required.");
        if (session.Quiz is null || session.Quiz.Questions.Count == 0)
            throw new InvalidOperationException("The quiz has no questions.");

        session.Status = SessionStatus.Running;
        session.Phase = LivePhase.Countdown;
        session.CurrentQuestionIndex = 0;
        session.CurrentQuestionId = session.Quiz.Questions[0].Id;
        session.Answers.Clear();
        foreach (var id in session.Scores.Keys.ToList())
            session.Scores[id] = 0;

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
            throw new InvalidOperationException("Answering is not open right now.");

        var participant = session.Participants.FirstOrDefault(p => p.ConnectionId == connectionId)
            ?? throw new InvalidOperationException("Participant was not found.");

        if (session.Answers.Any(a => a.ParticipantId == participant.Id && a.QuestionId == questionId))
            throw new InvalidOperationException("You already answered this question.");

        var question = session.Quiz!.Questions.FirstOrDefault(q => q.Id == questionId)
            ?? throw new InvalidOperationException("Question was not found.");

        var option = question.AnswerOptions.FirstOrDefault(o => o.Id == answerOptionId)
            ?? throw new InvalidOperationException("Answer option was not found.");

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
            CountAnswers(session, question));
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

        return ToQuestionChanged(session);
    }

    public QuizEndedDto EndQuiz(string roomId)
    {
        var session = GetRequired(roomId);
        session.Status = SessionStatus.Finished;
        session.Phase = LivePhase.Results;
        return new QuizEndedDto(session.RoomId, LeaderBoardCalculator.Calculate(session));
    }

    public AnswerCountsUpdatedDto GetAnswerCounts(string roomId)
    {
        var session = GetRequired(roomId);
        return ToAnswerCounts(session, CurrentQuestion(session));
    }

    private static void EnsureRunning(QuizSession session)
    {
        if (session.Status != SessionStatus.Running)
            throw new InvalidOperationException("The quiz is not running.");
    }

    private static Question CurrentQuestion(QuizSession session)
    {
        if (session.Quiz is null)
            throw new InvalidOperationException("Quiz is missing.");
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
