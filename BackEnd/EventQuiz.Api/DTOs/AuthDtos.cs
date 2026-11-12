using System.ComponentModel.DataAnnotations;

namespace EventQuiz.Api.DTOs;

public record CreateUserRequest(
    [Required, StringLength(255, MinimumLength = 3)] string Username,
    [Required, StringLength(255, MinimumLength = 6)] string Password);

public record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public record UserDto(int Id, string Username);
