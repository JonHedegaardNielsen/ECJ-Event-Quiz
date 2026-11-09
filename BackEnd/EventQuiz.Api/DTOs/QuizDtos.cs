using System.ComponentModel.DataAnnotations;

namespace EventQuiz.Api.DTOs;

public record CreateAnswerOptionRequest(
    [Required, StringLength(255)] string Answer,
    bool IsCorrect);

public record CreateQuestionRequest(
    [Required, StringLength(255)] string Question,
    [Required, MinLength(2)] List<CreateAnswerOptionRequest> AnswerOptions);

public record CreateQuizRequest(
    [Required, StringLength(255)] string Name,
    int CreatorId,
    [Required, MinLength(1)] List<CreateQuestionRequest> Questions);

public record AnswerOptionDto(int ID, string Answer, bool IsCorrect);

public record QuestionDto(int ID, string Question, List<AnswerOptionDto> AnswerOptions);

public record QuizDto(int ID, string Name, int CreatorId, List<QuestionDto> Questions);

public record QuizSummaryDto(int ID, string Name, int CreatorId, int QuestionCount);
