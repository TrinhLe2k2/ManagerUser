using ManagerUser.Application.Common.Models;
using ManagerUser.Infrastructure.Persistence.Records;

namespace ManagerUser.Infrastructure.Persistence.Mappings;

public static class OutboxMessageRecordMapping
{
    public static OutboxMessage ToModel(this OutboxMessageRecord record)
    {
        return new OutboxMessage(
            record.Id,
            record.Type,
            record.Payload,
            record.Status,
            record.RetryCount,
            record.MaxRetryCount,
            record.OccurredAt,
            record.AvailableAt,
            record.ProcessingStartedAt,
            record.ProcessedAt,
            record.LockedUntil,
            record.LastError);
    }
}
