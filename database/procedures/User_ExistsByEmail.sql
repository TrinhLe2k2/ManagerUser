CREATE OR ALTER PROCEDURE dbo.User_ExistsByEmail
    @Email NVARCHAR(255),
    @ExcludeUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Users
            WHERE Email = @Email
              AND IsDeleted = 0
              AND (@ExcludeUserId IS NULL OR Id <> @ExcludeUserId)
        )
        THEN 1 ELSE 0 END AS bit
    ) AS ExistsValue;
END;
