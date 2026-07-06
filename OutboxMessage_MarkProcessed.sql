CREATE OR ALTER PROCEDURE dbo.OutboxMessage_MarkProcessed
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.OutboxMessages
    SET
        Status = 2,
        ProcessedAt = SYSUTCDATETIME(),
        LockedUntil = NULL,
        LastError = NULL,
        ModifiedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END;
