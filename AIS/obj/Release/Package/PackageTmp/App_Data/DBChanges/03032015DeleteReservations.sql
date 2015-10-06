USE [AISLocal]
GO

ALTER TABLE [dbo].[Reservation]
ADD [IsDeleted] [bit] NOT NULL DEFAULT(0)
GO