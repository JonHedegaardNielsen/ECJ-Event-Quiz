using EventQuiz.Api.Models;

namespace EventQuiz.Api.DTOs;

public static class Mapping
{
    public static UserDto ToDto(this User user) => new(user.ID, user.Username);

    public static QuizDto ToDto(this Quiz quiz) => new(
        quiz.ID,
        quiz.Name,
        quiz.CreatorId,
        quiz.Questions
            .OrderBy(q => q.ID)
            .Select(q => new QuestionDto(
                q.ID,
                q.Text,
                q.AnswerOptions.OrderBy(a => a.ID).Select(a => new AnswerOptionDto(a.ID, a.Answer, a.IsCorrect)).ToList()))
            .ToList());

    public static ParticipantDto ToDto(this Participant participant) => new(participant.ID, participant.Name);

    public static QuizSessionDto ToDto(this QuizSession session) => new(
        session.RoomId,
        session.QuizId,
        session.Quiz.Name,
        session.Status,
        session.CurrentQuestionId,
        session.Participants.OrderBy(p => p.ID).Select(p => p.ToDto()).ToList());

    public static PlayerQuestionDto ToPlayerDto(this Question question, int questionNumber, int totalQuestions) => new(
        question.ID,
        question.Text,
        questionNumber,
        totalQuestions,
        question.AnswerOptions.OrderBy(a => a.ID).Select(a => new PlayerAnswerOptionDto(a.ID, a.Answer)).ToList());
}
