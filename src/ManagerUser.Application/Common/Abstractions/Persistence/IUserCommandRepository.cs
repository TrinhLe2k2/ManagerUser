using ManagerUser.Application.Users.Commands.ChangeUserStatus;
using ManagerUser.Application.Users.Commands.CreateUser;
using ManagerUser.Application.Users.Commands.DeleteUser;
using ManagerUser.Application.Users.Commands.UpdateUser;

namespace ManagerUser.Application.Common.Abstractions.Persistence;
public interface IUserCommandRepository
{
    Task<Guid> CreateAsync(
        CreateUserCommand command,
        string passwordHash,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default);

    Task ChangeStatusAsync(
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken = default);
}
