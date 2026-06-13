CREATE OR ALTER PROCEDURE dbo.User_ChangeStatus
    @Id UNIQUEIDENTIFIER,
    @Status TINYINT,
    @ModifiedBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET
        Status = @Status,
        ModifiedAt = SYSUTCDATETIME(),
        ModifiedBy = @ModifiedBy
    WHERE Id = @Id
      AND IsDeleted = 0;
END;
