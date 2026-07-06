IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OutboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OutboxMessages PRIMARY KEY,
        [Type] NVARCHAR(200) NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,

        -- 0: Pending, 1: Processing, 2: Processed, 3: Failed
        [Status] TINYINT NOT NULL CONSTRAINT DF_OutboxMessages_Status DEFAULT (0),
        RetryCount INT NOT NULL CONSTRAINT DF_OutboxMessages_RetryCount DEFAULT (0),
        MaxRetryCount INT NOT NULL CONSTRAINT DF_OutboxMessages_MaxRetryCount DEFAULT (3),

        OccurredAt DATETIME2(7) NOT NULL CONSTRAINT DF_OutboxMessages_OccurredAt DEFAULT (SYSUTCDATETIME()),
        AvailableAt DATETIME2(7) NOT NULL CONSTRAINT DF_OutboxMessages_AvailableAt DEFAULT (SYSUTCDATETIME()),
        ProcessingStartedAt DATETIME2(7) NULL,
        ProcessedAt DATETIME2(7) NULL,
        LockedUntil DATETIME2(7) NULL,
        LastError NVARCHAR(4000) NULL,

        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_OutboxMessages_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ModifiedAt DATETIME2(7) NULL
    );

    CREATE INDEX IX_OutboxMessages_Dispatch
        ON dbo.OutboxMessages([Status], AvailableAt, OccurredAt, CreatedAt)
        INCLUDE (RetryCount, MaxRetryCount, LockedUntil);
END;
