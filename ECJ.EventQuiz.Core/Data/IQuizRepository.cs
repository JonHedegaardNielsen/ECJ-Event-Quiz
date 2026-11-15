using ECJ.EventQuiz.Core.Models;

namespace ECJ.EventQuiz.Core.Data;

/// <summary>
/// Supplies quizzes (with questions and answer options) to the live session service.
/// The production implementation reads the same SQLite database as EventQuiz.Api.
/// </summary>
public interface IQuizRepository
{
    Quiz? GetQuiz(int quizId);
}
