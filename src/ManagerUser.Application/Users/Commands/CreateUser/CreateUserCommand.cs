namespace ManagerUser.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Username,
    string Email,
    string? FullName,
    string Password,
    byte Status,
    Guid? CreatedBy = null);
