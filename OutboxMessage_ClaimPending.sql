CREATE OR ALTER PROCEDURE dbo.OutboxMessage_ClaimPending
    @BatchSize INT = 10,
    @LockTimeoutSeconds INT = 60
AS
BEGIN
    SET NOCOUNT ON;

    IF @BatchSize < 1 SET @BatchSize = 10;
    IF @BatchSize > 100 SET @BatchSize = 100;
    IF @LockTimeoutSeconds < 5 SET @LockTimeoutSeconds = 60;

    DECLARE @Now DATETIME2(7) = SYSUTCDATETIME();

    ;WITH PendingMessages AS
    (
        SELECT TOP (@BatchSize)
            *
        FROM dbo.OutboxMessages WITH (ROWLOCK, READPAST, UPDLOCK)
        WHERE
            (Status = 0 AND AvailableAt <= @Now)
            OR
            (Status = 1 AND LockedUntil IS NOT NULL AND LockedUntil <= @Now)
        ORDER BY OccurredAt ASC, CreatedAt ASC
    )
    UPDATE PendingMessages
    SET
        Status = 1,
        ProcessingStartedAt = @Now,
        LockedUntil = DATEADD(SECOND, @LockTimeoutSeconds, @Now),
        ModifiedAt = @Now
    OUTPUT
        inserted.Id,
        inserted.Type,
        inserted.Payload,
        inserted.Status,
        inserted.RetryCount,
        inserted.MaxRetryCount,
        inserted.OccurredAt,
        inserted.AvailableAt,
        inserted.ProcessingStartedAt,
        inserted.ProcessedAt,
        inserted.LockedUntil,
        inserted.LastError;
END;
