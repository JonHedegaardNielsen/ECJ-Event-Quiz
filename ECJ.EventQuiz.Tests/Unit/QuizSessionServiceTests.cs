using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Core.Models;
using ECJ.EventQuiz.Core.Services;
using ECJ.EventQuiz.Tests.Infrastructure;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for live room rules in <see cref="ECJ.EventQuiz.Core.Services.QuizSessionService"/>.
/// </summary>
public class QuizSessionServiceTests
{
    private readonly QuizSessionService _sut = new(new InMemoryQuizRepository(TestQuizzes.All));

    [Fact]
    public void CreateSession_creates_lobby_with_room_id()
    {
        var created = _sut.CreateSession(1, "host-1");

        Assert.False(string.IsNullOrWhiteSpace(created.RoomId));
        Assert.Equal(1, created.QuizId);
        Assert.Equal("Firmafest 2026", created.QuizName);

        var session = _sut.GetRequired(created.RoomId);
        Assert.Equal(SessionStatus.Lobby, session.Status);
        Assert.Equal(LivePhase.Lobby, session.Phase);
    }

    [Fact]
    public void CreateSession_unknown_quiz_throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.CreateSession(404, "host"));
    }

    [Fact]
    public void JoinLobby_adds_participant_and_rejects_duplicate_name()
    {
        var room = _sut.CreateSession(1, "host").RoomId;

        var lobby = _sut.JoinLobby(room, "Anna", "p1");
        Assert.Single(lobby.Participants);
        Assert.Equal("Anna", lobby.Participants[0].Name);

        Assert.Throws<InvalidOperationException>(() => _sut.JoinLobby(room, "anna", "p2"));
    }

    [Fact]
    public void JoinLobby_blank_name_becomes_Gaest()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        var lobby = _sut.JoinLobby(room, "   ", "p1");
        Assert.Equal("Gæst", lobby.Participants[0].Name);
    }

    [Fact]
    public void StartQuiz_requires_participant()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        Assert.Throws<InvalidOperationException>(() => _sut.StartQuiz(room));
    }

    [Fact]
    public void Full_question_flow_awards_points_and_reveals()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");

        var started = _sut.StartQuiz(room);
        Assert.Equal("countdown", started.Phase);

        var open = _sut.BeginQuestion(room);
        Assert.Equal("question", open.Phase);

        var correct = TestQuizzes.GetQuiz(1)!.Questions[0].AnswerOptions.First(o => o.IsCorrect);
        var (answer, counts, _) = _sut.SubmitAnswer(room, "p1", open.Question.Id, correct.Id);

        Assert.True(answer.IsCorrect);
        Assert.True(answer.Points > 0);
        Assert.Equal(1, counts.Counts.Sum());

        var reveal = _sut.CloseQuestion(room);
        Assert.Equal(correct.Id, reveal.CorrectAnswerOptionId);
    }

    [Fact]
    public void SubmitAnswer_rejects_before_question_opens()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        var started = _sut.StartQuiz(room);

        Assert.Throws<InvalidOperationException>(() =>
            _sut.SubmitAnswer(room, "p1", started.Question.Id, started.Question.Options[0].Id));
    }

    [Fact]
    public void SubmitAnswer_auto_closes_when_all_participants_have_answered()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        _sut.StartQuiz(room);
        var open = _sut.BeginQuestion(room);
        var correct = TestQuizzes.GetQuiz(1)!.Questions[0].AnswerOptions.First(o => o.IsCorrect);

        var (_, _, reveal) = _sut.SubmitAnswer(room, "p1", open.Question.Id, correct.Id);

        Assert.NotNull(reveal);
        Assert.Equal(correct.Id, reveal!.CorrectAnswerOptionId);
        Assert.Equal(LivePhase.Reveal, _sut.GetRequired(room).Phase);
    }

    [Fact]
    public void SubmitAnswer_waits_until_every_participant_has_answered()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        _sut.JoinLobby(room, "Bo", "p2");
        _sut.StartQuiz(room);
        var open = _sut.BeginQuestion(room);
        var correct = TestQuizzes.GetQuiz(1)!.Questions[0].AnswerOptions.First(o => o.IsCorrect);

        var (_, _, first) = _sut.SubmitAnswer(room, "p1", open.Question.Id, correct.Id);
        Assert.Null(first);
        Assert.Equal(LivePhase.Question, _sut.GetRequired(room).Phase);

        var (_, _, second) = _sut.SubmitAnswer(room, "p2", open.Question.Id, correct.Id);
        Assert.NotNull(second);
        Assert.Equal(LivePhase.Reveal, _sut.GetRequired(room).Phase);
    }

    [Fact]
    public void SubmitAnswer_disconnected_participant_does_not_block_auto_close()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        _sut.JoinLobby(room, "Bo", "p2");
        _sut.StartQuiz(room);
        _sut.RemoveByConnection("p2"); // Bo mistede forbindelsen under quizzen
        var open = _sut.BeginQuestion(room);
        var correct = TestQuizzes.GetQuiz(1)!.Questions[0].AnswerOptions.First(o => o.IsCorrect);

        var (_, _, reveal) = _sut.SubmitAnswer(room, "p1", open.Question.Id, correct.Id);

        Assert.NotNull(reveal);
        Assert.Equal(LivePhase.Reveal, _sut.GetRequired(room).Phase);
    }

    [Fact]
    public void SubmitAnswer_rejects_double_answer()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        _sut.StartQuiz(room);
        var open = _sut.BeginQuestion(room);
        var optionId = open.Question.Options[0].Id;

        _sut.SubmitAnswer(room, "p1", open.Question.Id, optionId);
        Assert.Throws<InvalidOperationException>(() =>
            _sut.SubmitAnswer(room, "p1", open.Question.Id, optionId));
    }

    [Fact]
    public void NextQuestion_advances_until_end()
    {
        var quiz = TestQuizzes.GetQuiz(1)!;
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        _sut.StartQuiz(room);

        for (var i = 0; i < quiz.Questions.Count - 1; i++)
        {
            _sut.BeginQuestion(room);
            _sut.CloseQuestion(room);
            var next = _sut.NextQuestionOrEnd(room);
            Assert.IsType<QuestionChangedDto>(next);
        }

        _sut.BeginQuestion(room);
        _sut.CloseQuestion(room);
        var ended = _sut.NextQuestionOrEnd(room);
        Assert.IsType<QuizEndedDto>(ended);
        Assert.Equal(SessionStatus.Finished, _sut.GetRequired(room).Status);
    }

    [Fact]
    public void RemoveByConnection_removes_from_lobby()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        var updated = _sut.RemoveByConnection("p1");

        Assert.Equal(room, updated.RoomId);
        Assert.Empty(updated.Participants);
    }

    [Fact]
    public void JoinLobby_after_start_throws()
    {
        var room = _sut.CreateSession(1, "host").RoomId;
        _sut.JoinLobby(room, "Anna", "p1");
        _sut.StartQuiz(room);

        Assert.Throws<InvalidOperationException>(() => _sut.JoinLobby(room, "Bo", "p2"));
    }
}
