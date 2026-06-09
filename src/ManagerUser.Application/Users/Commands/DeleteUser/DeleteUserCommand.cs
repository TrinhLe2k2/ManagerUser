namespace ManagerUser.Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(
    Guid Id,
    Guid? DeletedBy = null);
