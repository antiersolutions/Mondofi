USE [AISLocal]
GO

ALTER TABLE [dbo].[UserProfile]
ADD [UserCode] [int] NOT NULL DEFAULT(0)
GO

ALTER TABLE [dbo].[UserProfile]
ADD [EnablePIN] [bit] NOT NULL DEFAULT(1)
GO