namespace ManagerUser.Application.Common.Models;

public sealed record OutboxMessage(
    Guid Id,
    string Type,
    string Payload,
    byte Status,
    int RetryCount,
    int MaxRetryCount,
    DateTime OccurredAt,
    DateTime AvailableAt,
    DateTime? ProcessingStartedAt,
    DateTime? ProcessedAt,
    DateTime? LockedUntil,
    string? LastError);
