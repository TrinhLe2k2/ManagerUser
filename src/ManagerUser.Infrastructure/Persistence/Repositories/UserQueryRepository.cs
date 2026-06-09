using Dapper;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Application.Users.Queries.GetUsers;
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

    public async Task<GetUserByIdQueryResult?> GetByIdAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            "dbo.User_GetById",
            new { Id = query.Id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var record = await _dbSession.Connection
            .QuerySingleOrDefaultAsync<UserDetailRecord>(command);

        return record?.ToQueryResult();
    }

    public async Task<GetUsersQueryResult> GetListAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            "dbo.User_GetList",
            new
            {
                query.Keyword,
                query.Status,
                query.PageIndex,
                query.PageSize
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var records = (await _dbSession.Connection
                .QueryAsync<UserListItemRecord>(command))
            .ToList();

        var totalCount = records.FirstOrDefault()?.TotalCount ?? 0;

        return new GetUsersQueryResult(
            Items: records.Select(record => record.ToQueryItem()).ToList(),
            PageIndex: query.PageIndex,
            PageSize: query.PageSize,
            TotalCount: totalCount);
    }

    public async Task<bool> ExistsByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            "dbo.User_ExistsById",
            new { Id = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await _dbSession.Connection.QuerySingleAsync<bool>(command);
    }

    public async Task<bool> ExistsByUsernameAsync(
    string username,
    CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            "dbo.User_ExistsByUsername",
            new { Username = username },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await _dbSession.Connection.QuerySingleAsync<bool>(command);
    }

    public async Task<bool> ExistsByEmailAsync(
    string email,
    Guid? excludeUserId = null,
    CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            "dbo.User_ExistsByEmail",
            new
            {
                Email = email,
                ExcludeUserId = excludeUserId
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await _dbSession.Connection.QuerySingleAsync<bool>(command);
    }
}
