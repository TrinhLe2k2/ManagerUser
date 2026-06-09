CREATE OR ALTER PROCEDURE dbo.User_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Username,
        Email,
        FullName,
        Status,
        CreatedAt,
        ModifiedAt
    FROM dbo.Users
    WHERE Id = @Id
      AND IsDeleted = 0;
END;
