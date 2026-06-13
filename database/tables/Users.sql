IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        FullName NVARCHAR(255) NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        Status TINYINT NOT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT (0),
        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2(7) NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        DeletedAt DATETIME2(7) NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    CREATE UNIQUE INDEX UX_Users_Username
        ON dbo.Users(Username)
        WHERE IsDeleted = 0;

    CREATE UNIQUE INDEX UX_Users_Email
        ON dbo.Users(Email)
        WHERE IsDeleted = 0;
END;
