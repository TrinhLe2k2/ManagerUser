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
            """
            SELECT CAST(
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Users
                    WHERE Id = @Id
                      AND IsDeleted = 0
                )
                THEN 1 ELSE 0 END AS bit
            );
            """,
            new { Id = id },
            cancellationToken: cancellationToken);

        return await _dbSession.Connection.QuerySingleAsync<bool>(command);
    }

    public async Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            """
            SELECT CAST(
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Users
                    WHERE Username = @Username
                      AND IsDeleted = 0
                )
                THEN 1 ELSE 0 END AS bit
            );
            """,
            new { Username = username },
            cancellationToken: cancellationToken);

        return await _dbSession.Connection.QuerySingleAsync<bool>(command);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            """
            SELECT CAST(
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Users
                    WHERE Email = @Email
                      AND IsDeleted = 0
                      AND (@ExcludeUserId IS NULL OR Id <> @ExcludeUserId)
                )
                THEN 1 ELSE 0 END AS bit
            );
            """,
            new
            {
                Email = email,
                ExcludeUserId = excludeUserId
            },
            cancellationToken: cancellationToken);

        return await _dbSession.Connection.QuerySingleAsync<bool>(command);
    }
}
