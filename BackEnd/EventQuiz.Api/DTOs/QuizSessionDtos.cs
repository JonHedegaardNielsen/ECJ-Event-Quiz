using ECJ.EventQuiz.Core.Models;

namespace EventQuiz.Api.DTOs;

public record StartSessionRequest(int QuizId);

public record ParticipantDto(int Id, string Name);

public record QuizSessionDto(
    string RoomId,
    int QuizId,
    string QuizName,
    SessionStatus Status,
    int? CurrentQuestionId,
    List<ParticipantDto> Participants);

// Sent to participants: never exposes which answer is correct.
public record PlayerAnswerOptionDto(int Id, string Answer);

public record PlayerQuestionDto(
    int Id,
    string Question,
    int TimeLimitSeconds,
    int QuestionNumber,
    int TotalQuestions,
    List<PlayerAnswerOptionDto> AnswerOptions);
