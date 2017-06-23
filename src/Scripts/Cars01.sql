-- add SQL errors
-- script for SQL Server 2008

CREATE TABLE [dbo].[Cars]
(
    [Registration] VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL PRIMARY KEY,
    [TotalDistanceTravelled] INT NOT NULL DEFAULT 0,
    [__State] VARCHAR(36) NOT NULL
);
GO

CREATE PROCEDURE [dbo].[SaveCar]
    @Registration VARCHAR(50),
    @TotalDistanceTravelled INT,
    @IsDestroyed BIT,
    @__State VARCHAR(8),
    @__StateOut VARCHAR(8) = NULL OUTPUT
AS
SET NOCOUNT ON;

IF @IsDestroyed = 1 AND @__State IS NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Cars] WHERE [Registration] = @Registration)
    RETURN;

SET @__StateOut = LEFT(NEWID(), 8);

DECLARE @Changes TABLE
(
    [Action] NVARCHAR(10)
);

MERGE [dbo].[Cars] AS [Target]
USING (
    VALUES (@Registration, @TotalDistanceTravelled, @IsDestroyed, @__State)
) AS [Source] ([Registration], [TotalDistanceTravelled], [IsDestroyed], [__State]) ON [Target].[Registration] = [Source].[Registration]
WHEN MATCHED AND [Target].[__State] = @__State AND [Source].[IsDestroyed] = 0 THEN
    UPDATE SET
        [Target].[TotalDistanceTravelled] = [Source].[TotalDistanceTravelled],
        [Target].[__State] = @__StateOut
WHEN MATCHED AND [Target].[__State] = @__State AND [Source].[IsDestroyed] = 1 THEN
    DELETE
WHEN NOT MATCHED BY TARGET AND [Source].[__State] IS NULL THEN
    INSERT ([Registration], [TotalDistanceTravelled], [__State])
    VALUES (
        [Source].[Registration],
        [Source].[TotalDistanceTravelled],
        @__StateOut)
OUTPUT $action
INTO @Changes;

IF NOT EXISTS (SELECT 1 FROM @Changes)
    THROW 50409, 'Concurrency error (client side). Commit state mismatch.', 1;

GO

CREATE PROCEDURE [dbo].[LoadCar]
    @Registration VARCHAR(50)
AS
SET NOCOUNT ON;

SELECT [Registration], [TotalDistanceTravelled], [__State]
FROM [dbo].[Cars]
WHERE [Registration] = @Registration;

GO

CREATE PROCEDURE [dbo].[GetCars]
AS
SET NOCOUNT ON;

SELECT [Registration], [TotalDistanceTravelled], [__State]
FROM [dbo].[Cars]
ORDER BY [Registration];

GO