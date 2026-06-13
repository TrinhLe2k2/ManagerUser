CREATE OR ALTER PROCEDURE dbo.User_ExistsById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Users
            WHERE Id = @Id
              AND IsDeleted = 0
        )
        THEN 1 ELSE 0 END AS bit
    ) AS ExistsValue;
END;
