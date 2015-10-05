USE [AISLocal]
GO

INSERT INTO [dbo].[webpages_Roles]([RoleName])
     VALUES('Online')
GO

INSERT INTO [dbo].[UserProfile]([UserName],[FirstName],[LastName],[DesignationId],[PhotoPath],[Availability],[StaffColor],[UserCode],[EnablePIN])
     VALUES('online@user.com','Online','User',null,null,1,null,0,0)
GO

INSERT INTO [dbo].[webpages_UsersInRoles]([UserId],[RoleId])
     VALUES('{online user id}',5)
GO

INSERT INTO [dbo].[webpages_Membership]
([UserId],[CreateDate],[ConfirmationToken],[IsConfirmed],[LastPasswordFailureDate],[PasswordFailuresSinceLastSuccess],[Password],[PasswordChangedDate],[PasswordSalt],[PasswordVerificationToken],[PasswordVerificationTokenExpirationDate])
     VALUES('{online user id}',getdate(),null,1,null,0,'ACyr12O1M8RI/Q6zVSwZjNUKorgv8dHcCYEF5xWGhMNd8cI95aFv2l1+6GKrhkAdXw==',getdate(),'',null,null)
GO

/****** Object:  Table [dbo].[FloorTableBlock]    Script Date: 19-03-2015 17:27:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FloorTableBlock](
	[FloorTableBlockId] [uniqueidentifier] NOT NULL,
	[FloorTableId] [bigint] NOT NULL,
	[BlockDate] [datetime] NOT NULL,
	[BlockFrom] [datetime] NOT NULL,
	[BlockTo] [datetime] NOT NULL,
 CONSTRAINT [PK_FloorTableBlock] PRIMARY KEY CLUSTERED 
(
	[FloorTableBlockId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO