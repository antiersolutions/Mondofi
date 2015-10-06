USE [AISLocal]
GO

ALTER TABLE [dbo].[Reservation]
ADD [CreatedOn] datetime null
,[UpdatedBy] bigint null
,[UpdatedOn] datetime null
GO

UPDATE  [dbo].[Reservation]
SET [CreatedOn] = '2014-08-07 09:30:00.000'
,[UpdatedBy] = '6'
,[UpdatedOn] = '2014-08-07 09:30:00.000'
GO

ALTER TABLE [dbo].[Waiting]
ADD [CreatedOn] datetime null;
GO

UPDATE  [dbo].[Waiting]
SET [CreatedOn] = '2014-08-07 09:30:00.000';
GO

ALTER TABLE [dbo].[Waiting]
ALTER COLUMN [CreatedOn] datetime NOT NULL;
GO