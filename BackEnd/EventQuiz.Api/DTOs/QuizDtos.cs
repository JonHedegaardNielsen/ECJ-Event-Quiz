using System.ComponentModel.DataAnnotations;

namespace EventQuiz.Api.DTOs;

public record CreateAnswerOptionRequest(
    [Required, StringLength(255)] string Answer,
    bool IsCorrect);

public record CreateQuestionRequest(
    [Required, StringLength(255)] string Question,
    [Required, MinLength(2)] List<CreateAnswerOptionRequest> AnswerOptions,
    [Range(5, 240)] int TimeLimitSeconds = 20);

public record CreateQuizRequest(
    [Required, StringLength(255)] string Name,
    int CreatorId,
    [Required, MinLength(1)] List<CreateQuestionRequest> Questions);

public record AnswerOptionDto(int Id, string Answer, bool IsCorrect);

public record QuestionDto(int Id, string Question, int TimeLimitSeconds, List<AnswerOptionDto> AnswerOptions);

public record QuizDto(int Id, string Name, int CreatorId, List<QuestionDto> Questions);

public record QuizSummaryDto(int Id, string Name, int CreatorId, int QuestionCount);
