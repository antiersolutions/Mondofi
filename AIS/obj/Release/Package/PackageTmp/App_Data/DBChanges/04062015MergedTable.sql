﻿USE [AISLocal]
GO

ALTER TABLE [dbo].[MergedFloorTable]
ALTER COLUMN [TableName] NVARCHAR(MAX) NOT NULL
GO