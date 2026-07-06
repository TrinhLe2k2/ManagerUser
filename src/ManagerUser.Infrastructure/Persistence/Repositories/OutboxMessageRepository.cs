using Dapper;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Models;
using ManagerUser.Infrastructure.Persistence.Mappings;
using ManagerUser.Infrastructure.Persistence.Records;
using ManagerUser.Infrastructure.Persistence.Sessions;
using System.Data;

namespace ManagerUser.Infrastructure.Persistence.Repositories;

// Implementation dùng stored procedure để mọi thao tác claim/update diễn ra ở DB.
// Phần claim nên nằm trong DB vì SQL Server có thể khóa row atomically bằng UPDLOCK/READPAST.
public sealed class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly DbSession _dbSession;

    public OutboxMessageRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(int batchSize, int lockTimeoutSeconds, CancellationToken cancellationToken = default)
    {
        // Procedure OutboxMessage_ClaimPending vừa tìm message pending vừa update sang Processing.
        // Đây là điểm quan trọng: nếu chỉ SELECT rồi mới UPDATE ở app, nhiều worker có thể lấy trùng message.
        var command = new CommandDefinition(
            "dbo.OutboxMessage_ClaimPending",
            new
            {
                BatchSize = batchSize,
                LockTimeoutSeconds = lockTimeoutSeconds
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var records = (await _dbSession.Connection
                .QueryAsync<OutboxMessageRecord>(command))
                .ToList();

        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Thành công thì message kết thúc vòng đời dispatch.
        var command = new CommandDefinition(
            "dbo.OutboxMessage_MarkProcessed",
            new { Id = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await _dbSession.Connection.ExecuteAsync(command);
    }

    public async Task MarkFailedAsync(Guid id, string error, int retryDelaySeconds, CancellationToken cancellationToken = default)
    {
        // Lỗi thì DB quyết định message quay lại Pending hay chuyển Failed dựa trên RetryCount/MaxRetryCount.
        var command = new CommandDefinition(
            "dbo.OutboxMessage_MarkFailed",
            new
            {
                Id = id,
                Error = error,
                RetryDelaySeconds = retryDelaySeconds
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await _dbSession.Connection.ExecuteAsync(command);
    }
}
