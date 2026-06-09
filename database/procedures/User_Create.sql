CREATE OR ALTER PROCEDURE dbo.User_Create
    @Id UNIQUEIDENTIFIER,
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @FullName NVARCHAR(255) = NULL,
    @PasswordHash NVARCHAR(500),
    @Status TINYINT,
    @CreatedBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users
    (
        Id,
        Username,
        Email,
        FullName,
        PasswordHash,
        Status,
        CreatedAt,
        CreatedBy
    )
    VALUES
    (
        @Id,
        @Username,
        @Email,
        @FullName,
        @PasswordHash,
        @Status,
        SYSUTCDATETIME(),
        @CreatedBy
    );

    SELECT @Id AS Id;
END;
