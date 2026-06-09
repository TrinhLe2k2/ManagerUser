CREATE OR ALTER PROCEDURE dbo.User_GetList
    @Keyword NVARCHAR(255) = NULL,
    @Status TINYINT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize < 1 SET @PageSize = 20;

    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT
        Id,
        Username,
        Email,
        FullName,
        Status,
        CreatedAt,
        ModifiedAt,
        COUNT(1) OVER() AS TotalCount
    FROM dbo.Users
    WHERE IsDeleted = 0
      AND (@Status IS NULL OR Status = @Status)
      AND (
            @Keyword IS NULL
            OR Username LIKE N'%' + @Keyword + N'%'
            OR Email LIKE N'%' + @Keyword + N'%'
            OR FullName LIKE N'%' + @Keyword + N'%'
          )
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
