using ECJ.EventQuiz.Core.Models;

namespace EventQuiz.Api.DTOs;

public static class Mapping
{
    public static UserDto ToDto(this User user) => new(user.Id, user.Username);

    public static QuizDto ToDto(this Quiz quiz) => new(
        quiz.Id,
        quiz.Name,
        quiz.CreatorId,
        quiz.Questions
            .OrderBy(q => q.Id)
            .Select(q => new QuestionDto(
                q.Id,
                q.Text,
                q.TimeLimitSeconds,
                q.AnswerOptions.OrderBy(a => a.Id).Select(a => new AnswerOptionDto(a.Id, a.Answer, a.IsCorrect)).ToList()))
            .ToList());

    public static ParticipantDto ToDto(this Participant participant) => new(participant.Id, participant.Name);

    public static QuizSessionDto ToDto(this QuizSession session) => new(
        session.RoomId,
        session.QuizId,
        session.Quiz!.Name,
        session.Status,
        session.CurrentQuestionId,
        session.Participants.OrderBy(p => p.Id).Select(p => p.ToDto()).ToList());

    public static PlayerQuestionDto ToPlayerDto(this Question question, int questionNumber, int totalQuestions) => new(
        question.Id,
        question.Text,
        question.TimeLimitSeconds,
        questionNumber,
        totalQuestions,
        question.AnswerOptions.OrderBy(a => a.Id).Select(a => new PlayerAnswerOptionDto(a.Id, a.Answer)).ToList());
}
