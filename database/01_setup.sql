-- ProductsDb setup | SQL Server 2019+
-- Run as sysadmin or db_owner on target instance

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'ProductsDb')
BEGIN
    CREATE DATABASE ProductsDb;
END
GO

USE ProductsDb;
GO

-- ============================================================
-- Table
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Products')
BEGIN
    CREATE TABLE dbo.Products (
        Id          INT            IDENTITY(1,1)   NOT NULL,
        Name        NVARCHAR(100)                  NOT NULL,
        Description NVARCHAR(500)                      NULL,
        Price       DECIMAL(18,2)                  NOT NULL,
        CreatedDate DATETIME2      DEFAULT SYSUTCDATETIME() NOT NULL,
        IsActive    BIT            DEFAULT 1       NOT NULL,
        CONSTRAINT PK_Products PRIMARY KEY (Id),
        CONSTRAINT CK_Products_Price CHECK (Price >= 0)
    );
END
GO

-- ============================================================
-- sp_Product_Create
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Product_Create
    @Name        NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Price       DECIMAL(18,2),
    @NewId       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Products (Name, Description, Price)
        VALUES (@Name, @Description, @Price);

        SET @NewId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- ============================================================
-- sp_Product_GetById
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Product_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Description, Price, CreatedDate, IsActive
    FROM   dbo.Products
    WHERE  Id = @Id;
END
GO

-- ============================================================
-- sp_Product_GetAll  (active only, newest first)
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Product_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Description, Price, CreatedDate, IsActive
    FROM   dbo.Products
    WHERE  IsActive = 1
    ORDER BY CreatedDate DESC;
END
GO

-- ============================================================
-- sp_Product_Update
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Product_Update
    @Id          INT,
    @Name        NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Price       DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Products
        SET    Name        = @Name,
               Description = @Description,
               Price       = @Price
        WHERE  Id = @Id
          AND  IsActive = 1;

        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- ============================================================
-- sp_Product_Delete  (logical delete)
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Product_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Products
        SET    IsActive = 0
        WHERE  Id = @Id
          AND  IsActive = 1;

        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
