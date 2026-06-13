using Dapper;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Users.Commands.ChangeUserStatus;
using ManagerUser.Application.Users.Commands.CreateUser;
using ManagerUser.Application.Users.Commands.DeleteUser;
using ManagerUser.Application.Users.Commands.UpdateUser;
using ManagerUser.Infrastructure.Persistence.Sessions;
using System.Data;

namespace ManagerUser.Infrastructure.Persistence.Repositories;

public sealed class UserCommandRepository : IUserCommandRepository
{
    private readonly DbSession _dbSession;

    public UserCommandRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task<Guid> CreateAsync(
        CreateUserCommand command,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();

        var sqlCommand = new CommandDefinition(
            "dbo.User_Create",
            new
            {
                Id = id,
                command.Username,
                command.Email,
                command.FullName,
                PasswordHash = passwordHash,
                command.Status,
                command.CreatedBy
            },
            transaction: _dbSession.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await _dbSession.Connection.QuerySingleAsync<Guid>(sqlCommand);
    }

    public async Task UpdateAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var sqlCommand = new CommandDefinition(
            "dbo.User_Update",
            new
            {
                command.Id,
                command.Email,
                command.FullName,
                command.ModifiedBy
            },
            transaction: _dbSession.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await _dbSession.Connection.ExecuteAsync(sqlCommand);
    }

    public async Task DeleteAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var sqlCommand = new CommandDefinition(
            "dbo.User_Delete",
            new
            {
                command.Id,
                command.DeletedBy
            },
            transaction: _dbSession.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await _dbSession.Connection.ExecuteAsync(sqlCommand);
    }

    public async Task ChangeStatusAsync(
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var sqlCommand = new CommandDefinition(
            "dbo.User_ChangeStatus",
            new
            {
                command.Id,
                command.Status,
                command.ModifiedBy
            },
            transaction: _dbSession.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await _dbSession.Connection.ExecuteAsync(sqlCommand);
    }
}
