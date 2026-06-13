namespace ManagerUser.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string Email,
    string? FullName,
    Guid? ModifiedBy = null);
