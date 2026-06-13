CREATE OR ALTER PROCEDURE dbo.User_Update
    @Id UNIQUEIDENTIFIER,
    @Email NVARCHAR(255),
    @FullName NVARCHAR(255) = NULL,
    @ModifiedBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET
        Email = @Email,
        FullName = @FullName,
        ModifiedAt = SYSUTCDATETIME(),
        ModifiedBy = @ModifiedBy
    WHERE Id = @Id
      AND IsDeleted = 0;
END;
