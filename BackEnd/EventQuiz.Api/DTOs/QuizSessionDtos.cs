using EventQuiz.Api.Models;

namespace EventQuiz.Api.DTOs;

public record StartSessionRequest(int QuizId);

public record ParticipantDto(int ID, string Name);

public record QuizSessionDto(
    string RoomId,
    int QuizId,
    string QuizName,
    SessionStatus Status,
    int? CurrentQuestionId,
    List<ParticipantDto> Participants);

// Sent to participants: never exposes which answer is correct.
public record PlayerAnswerOptionDto(int ID, string Answer);

public record PlayerQuestionDto(
    int ID,
    string Question,
    int QuestionNumber,
    int TotalQuestions,
    List<PlayerAnswerOptionDto> AnswerOptions);

public record LeaderboardEntryDto(int Rank, int ParticipantId, string Name, int Score, int CorrectAnswers);
