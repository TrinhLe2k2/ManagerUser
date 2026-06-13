CREATE OR ALTER PROCEDURE dbo.User_ExistsByUsername
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Users
            WHERE Username = @Username
              AND IsDeleted = 0
        )
        THEN 1 ELSE 0 END AS bit
    ) AS ExistsValue;
END;
