using ManagerUser.Application.Users.Queries.GetUserById;

namespace ManagerUser.Application.Common.Abstractions.Persistence;
public interface IUserQueryRepository
{
    Task<GetUserByIdQueryResult?> GetByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default);
}
