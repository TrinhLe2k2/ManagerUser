using Dapper;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Infrastructure.Persistence.Mappings;
using ManagerUser.Infrastructure.Persistence.Records;
using ManagerUser.Infrastructure.Persistence.Sessions;
using System.Data;

namespace ManagerUser.Infrastructure.Persistence.Repositories;
public sealed class UserQueryRepository : IUserQueryRepository
{
    private readonly DbSession _dbSession;

    public UserQueryRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task<GetUserByIdQueryResult?> GetByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            "dbo.User_GetById",
            new { Id = query.Id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var record = await _dbSession.Connection.QuerySingleOrDefaultAsync<UserDetailRecord>(command);

        if (record is null)
            return null;

        return record.ToQueryResul();
    }
}
