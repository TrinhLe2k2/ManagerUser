CREATE OR ALTER PROCEDURE dbo.OutboxMessage_MarkFailed
    @Id UNIQUEIDENTIFIER,
    @Error NVARCHAR(4000),
    @RetryDelaySeconds INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    IF @RetryDelaySeconds < 1 SET @RetryDelaySeconds = 30;

    DECLARE @Now DATETIME2(7) = SYSUTCDATETIME();

    UPDATE dbo.OutboxMessages
    SET
        RetryCount = RetryCount + 1,
        Status = CASE
            WHEN RetryCount + 1 >= MaxRetryCount THEN 3
            ELSE 0
        END,
        AvailableAt = CASE
            WHEN RetryCount + 1 >= MaxRetryCount THEN AvailableAt
            ELSE DATEADD(SECOND, @RetryDelaySeconds, @Now)
        END,
        LockedUntil = NULL,
        LastError = LEFT(@Error, 4000),
        ModifiedAt = @Now
    WHERE Id = @Id;
END;
