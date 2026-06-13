namespace ManagerUser.Application.Users.Commands.ChangeUserStatus;

public sealed record ChangeUserStatusCommand(
    Guid Id,
    byte Status,
    Guid? ModifiedBy = null);
