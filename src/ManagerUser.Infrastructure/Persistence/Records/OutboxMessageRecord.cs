namespace ManagerUser.Infrastructure.Persistence.Records;

public sealed class OutboxMessageRecord
{
    public Guid Id { get; init; }
    public string Type { get; init; } = "";
    public string Payload { get; init; } = "";
    public byte Status { get; init; }
    public int RetryCount { get; init; }
    public int MaxRetryCount { get; init; }
    public DateTime OccurredAt { get; init; }
    public DateTime AvailableAt { get; init; }
    public DateTime? ProcessingStartedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? LockedUntil { get; init; }
    public string? LastError { get; init; }
}
