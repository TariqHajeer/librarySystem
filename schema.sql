CREATE DATABASE librarySystem;
GO
use  librarySystem;
GO
-- Use your DB name first e.g. CREATE DATABASE LibraryDb; USE LibraryDb;

CREATE TABLE [Users] (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE [Books] (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(300) NOT NULL,
    Author NVARCHAR(200) NOT NULL,
    ISBN VARCHAR(20) NOT NULL UNIQUE,
    PublishedYear INT NULL,
    TotalCopies INT NOT NULL DEFAULT 1,
    AvailableCopies INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE [Borrowings] (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    BookId UNIQUEIDENTIFIER NOT NULL,
    BorrowDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ReturnDate DATETIME2 NULL,
    CONSTRAINT FK_Borrowings_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Borrowings_Books FOREIGN KEY (BookId) REFERENCES Books(Id)
);

-- Helpful indexes
CREATE INDEX IX_Books_Title ON Books (Title);
CREATE INDEX IX_Books_Author ON Books (Author);
CREATE INDEX IX_Books_ISBN ON Books (ISBN);
CREATE INDEX IX_Borrowings_UserId ON Borrowings (UserId);
CREATE INDEX IX_Borrowings_BookId ON Borrowings (BookId);
GO

CREATE PROCEDURE [dbo].[sp_insertUser]
    @FullName NVARCHAR(200),
    @Email NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for duplicate email
    IF EXISTS (SELECT 1 FROM [LibrarySystem].[dbo].[Users] WHERE Email = @Email)
    BEGIN
        RAISERROR('A user with this email already exists.', 16, 1,50001);
        RETURN;
    END

    -- Insert new user
    DECLARE @NewUserId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO [LibrarySystem].[dbo].[Users] (
        Id,
        FullName,
        Email,
        CreatedAt
    )
    VALUES (
        @NewUserId,
        @FullName,
        @Email,
        GETUTCDATE()   -- UTC timestamp
    );

    -- Return the new UserId
    SELECT @NewUserId AS Id;
END

GO

CREATE PROCEDURE [dbo].[sp_getUsers]
    @FullName NVARCHAR(200) = NULL,
    @Email NVARCHAR(200) = NULL,
    @CreatedFrom DATETIME2 = NULL,
    @CreatedTo DATETIME2 = NULL,
    @Skip INT = NULL,
    @Take INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Paged data
    SELECT 
        [Id],
        [FullName],
        [Email],
        [CreatedAt]
    FROM [dbo].[Users]
    WHERE (@FullName IS NULL OR [FullName] LIKE '%' + @FullName + '%')
      AND (@Email IS NULL OR [Email] LIKE '%' + @Email + '%')
      AND (@CreatedFrom IS NULL OR [CreatedAt] >= @CreatedFrom)
      AND (@CreatedTo IS NULL OR [CreatedAt] <= @CreatedTo)
    ORDER BY [CreatedAt] DESC
    OFFSET ISNULL(@Skip, 0) ROWS
    FETCH NEXT ISNULL(@Take, 1000000) ROWS ONLY;

    -- Total count
    SELECT COUNT(1)
    FROM [dbo].[Users]
    WHERE (@FullName IS NULL OR [FullName] LIKE '%' + @FullName + '%')
      AND (@Email IS NULL OR [Email] LIKE '%' + @Email + '%')
      AND (@CreatedFrom IS NULL OR [CreatedAt] >= @CreatedFrom)
      AND (@CreatedTo IS NULL OR [CreatedAt] <= @CreatedTo);
END
GO


CREATE PROCEDURE [dbo].[sp_insertBook]
    @Title NVARCHAR(300),
    @Author NVARCHAR(200),
    @ISBN VARCHAR(20),
    @PublishedYear INT = NULL,
    @TotalCopies INT,
    @AvailableCopies INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for duplicate ISBN
    IF EXISTS (SELECT 1 FROM [dbo].[Books] WHERE [ISBN] = @ISBN)
    BEGIN
        RAISERROR('A book with this ISBN already exists.', 16, 1,50001);
        RETURN;
    END
    DECLARE @NewBookId UNIQUEIDENTIFIER = NEWID();
    -- Insert new book
    INSERT INTO [dbo].[Books] (
        [Id],
        [Title],
        [Author],
        [ISBN],
        [PublishedYear],
        [TotalCopies],
        [AvailableCopies],
        [CreatedAt]
    )
    VALUES (
        @NewBookId,
        @Title,
        @Author,
        @ISBN,
        @PublishedYear,
        @TotalCopies,
        @AvailableCopies,
        GETUTCDATE()
    );


    SELECT @NewBookId AS Id;
END
GO

CREATE PROCEDURE [dbo].[sp_getBooks]
    @Title NVARCHAR(300) = NULL,
    @Author NVARCHAR(200) = NULL,
    @ISBN VARCHAR(20) = NULL,
    @Skip INT = NULL,
    @Take INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Paged data
    SELECT 
        [Id],
        [Title],
        [Author],
        [ISBN],
        [PublishedYear],
        [TotalCopies],
        [AvailableCopies],
        [CreatedAt]
    FROM [dbo].[Books]
    WHERE (@Title IS NULL OR [Title] LIKE '%' + @Title + '%')
      AND (@Author IS NULL OR [Author] LIKE '%' + @Author + '%')
      AND (@ISBN IS NULL OR [ISBN] LIKE '%' + @ISBN + '%')
    ORDER BY [CreatedAt] DESC
    OFFSET ISNULL(@Skip, 0) ROWS
    FETCH NEXT ISNULL(@Take, 1000000) ROWS ONLY;

    -- Total count
    SELECT COUNT(1)
    FROM [dbo].[Books]
    WHERE (@Title IS NULL OR [Title] LIKE '%' + @Title + '%')
      AND (@Author IS NULL OR [Author] LIKE '%' + @Author + '%')
      AND (@ISBN IS NULL OR [ISBN] LIKE '%' + @ISBN + '%');
END
GO



CREATE PROCEDURE [dbo].[sp_seedBooks]
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert sample books
    INSERT INTO [dbo].[Books] ([Title], [Author], [ISBN], [PublishedYear], [TotalCopies], [AvailableCopies])
    VALUES 
        ('The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', 1925, 5, 5),
        ('To Kill a Mockingbird', 'Harper Lee', '9780060935467', 1960, 3, 3),
        ('1984', 'George Orwell', '9780451524935', 1949, 4, 4),
        ('Pride and Prejudice', 'Jane Austen', '9780141439518', 1813, 6, 6),
        ('Moby-Dick', 'Herman Melville', '9781503280786', 1851, 2, 2),
        ('The Catcher in the Rye', 'J.D. Salinger', '9780316769488', 1951, 5, 5),
        ('Brave New World', 'Aldous Huxley', '9780060850524', 1932, 3, 3),
        ('War and Peace', 'Leo Tolstoy', '9781853260629', 1869, 4, 4),
        ('The Odyssey', 'Homer', '9780140268867', -800, 5, 5),
        ('Crime and Punishment', 'Fyodor Dostoevsky', '9780140449136', 1866, 3, 3);
END
GO
exec sp_seedBooks;
GO


CREATE PROCEDURE sp_getBookById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        Title,
        Author,
        ISBN,
        PublishedYear,
        TotalCopies,
        AvailableCopies,
        CreatedAt
    FROM Books
    WHERE Id = @Id;
END;
GO

CREATE PROCEDURE [dbo].[sp_updateBook]
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(300),
    @Author NVARCHAR(200),
    @ISBN VARCHAR(20),
    @PublishedYear INT,
    @TotalCopies INT,
    @AvailableCopies INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for invalid values
    IF @TotalCopies < @AvailableCopies
    BEGIN
        RAISERROR('TotalCopies cannot be less than AvailableCopies.', 16, 1,50002);
        RETURN;
    END

    IF @AvailableCopies > @TotalCopies
    BEGIN
        RAISERROR('AvailableCopies cannot be greater than TotalCopies.', 16, 1,50002);
        RETURN;
    END

    -- Update the book
    UPDATE [dbo].[Books]
    SET
        [Title] = @Title,
        [Author] = @Author,
        [ISBN] = @ISBN,
        [PublishedYear] = @PublishedYear,
        [TotalCopies] = @TotalCopies,
        [AvailableCopies] = @AvailableCopies
    WHERE [Id] = @Id;
END
GO

CREATE PROCEDURE sp_seedUsers
AS
BEGIN
    SET NOCOUNT ON;

    -- Example 1
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'alice@example.com')
    BEGIN
        INSERT INTO Users (FullName, Email)
        VALUES ('Alice Johnson', 'alice@example.com');
    END

    -- Example 2
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'bob@example.com')
    BEGIN
        INSERT INTO Users (FullName, Email)
        VALUES ('Bob Smith', 'bob@example.com');
    END

    -- Example 3
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'charlie@example.com')
    BEGIN
        INSERT INTO Users (FullName, Email)
        VALUES ('Charlie Brown', 'charlie@example.com');
    END

END
GO

exec sp_seedUsers;
GO


CREATE PROCEDURE [dbo].[sp_getBorrowings]
    @UserId UNIQUEIDENTIFIER = NULL,
    @BookId UNIQUEIDENTIFIER = NULL,
    @ReturnDate DATETIME2(7) = NULL,
    @Skip INT = 0,
    @Take INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    -- Paged data
    SELECT 
        [Id],
        [UserId],
        [BookId],
        [BorrowDate],
        [ReturnDate]
    FROM [dbo].[Borrowings]
    WHERE (@UserId IS NULL OR [UserId] = @UserId)
      AND (@BookId IS NULL OR [BookId] = @BookId)
      AND (
            @ReturnDate IS NULL AND [ReturnDate] IS NULL -- filter for null
            OR @ReturnDate IS NOT NULL AND [ReturnDate] = @ReturnDate -- filter for specific date
          )
    ORDER BY [BorrowDate] DESC
    OFFSET @Skip ROWS
    FETCH NEXT @Take ROWS ONLY;

    -- Total count
    SELECT COUNT(1)
    FROM [dbo].[Borrowings]
    WHERE (@UserId IS NULL OR [UserId] = @UserId)
      AND (@BookId IS NULL OR [BookId] = @BookId)
      AND (
            @ReturnDate IS NULL AND [ReturnDate] IS NULL
            OR @ReturnDate IS NOT NULL AND [ReturnDate] = @ReturnDate
          );
END
GO


CREATE PROCEDURE [dbo].[sp_insertBorrowing]
    @UserId UNIQUEIDENTIFIER,
    @BookId UNIQUEIDENTIFIER,
    @BorrowDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @newId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Borrowings (Id,UserId, BookId, BorrowDate)
    VALUES (@newId,@UserId, @BookId, @BorrowDate);

    -- Optionally, return the inserted BorrowingId
    SELECT @newId AS Id;
END;
GO


CREATE OR ALTER PROCEDURE sp_updateBorrowing
    @Id UNIQUEIDENTIFIER,
    @ReturnDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Borrowings
    SET ReturnDate = @ReturnDate
    WHERE Id = @Id;
END;
GO
CREATE PROCEDURE [dbo].[sp_getUserById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        FullName,
        Email,
        CreatedAt
    FROM Users
    WHERE Id = @Id;
END;
GO
