CREATE OR ALTER PROCEDURE dbo.OutboxMessage_SeedSamples
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.OutboxMessages
        WHERE Type = N'SampleE.UserCreated'
          AND Payload LIKE N'%sample-e-user-created-001%'
    )
    BEGIN
        INSERT INTO dbo.OutboxMessages
        (
            Id,
            Type,
            Payload,
            Status,
            RetryCount,
            MaxRetryCount,
            OccurredAt,
            AvailableAt
        )
        VALUES
        (
            NEWID(),
            N'SampleE.UserCreated',
            N'{"eventId":"sample-e-user-created-001","userId":"00000000-0000-0000-0000-000000000001"}',
            0,
            0,
            3,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.OutboxMessages
        WHERE Type = N'SampleE.AlwaysFail'
          AND Payload LIKE N'%sample-e-fail-001%'
    )
    BEGIN
        INSERT INTO dbo.OutboxMessages
        (
            Id,
            Type,
            Payload,
            Status,
            RetryCount,
            MaxRetryCount,
            OccurredAt,
            AvailableAt
        )
        VALUES
        (
            NEWID(),
            N'SampleE.AlwaysFail',
            N'{"eventId":"sample-e-fail-001","reason":"demo retry"}',
            0,
            0,
            3,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
    END;
END;
