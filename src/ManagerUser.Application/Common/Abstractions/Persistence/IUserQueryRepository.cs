using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Application.Users.Queries.GetUsers;

namespace ManagerUser.Application.Common.Abstractions.Persistence;
public interface IUserQueryRepository
{
    Task<GetUserByIdQueryResult?> GetByIdAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<GetUsersQueryResult> GetListAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);
}
