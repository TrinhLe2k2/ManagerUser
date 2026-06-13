CREATE OR ALTER PROCEDURE dbo.User_Delete
    @Id UNIQUEIDENTIFIER,
    @DeletedBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET
        IsDeleted = 1,
        DeletedAt = SYSUTCDATETIME(),
        DeletedBy = @DeletedBy
    WHERE Id = @Id
      AND IsDeleted = 0;
END;
