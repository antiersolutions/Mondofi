truncate table [dbo].[TableAvailabilityFloorTable]
GO
truncate table [dbo].[TableAvailabilityWeekDay]
GO
truncate table [dbo].[TableAvailability]
GO
truncate table [dbo].[TempFloorTable]
GO
truncate table [dbo].[TempFloorPlan]
GO
truncate table [dbo].[Waiting]
GO
truncate table [dbo].[MergedTableOrigionalTable]
GO
truncate table [dbo].[MergedFloorTable]	 
GO
truncate table [dbo].[Reservation]  
GO
truncate table [dbo].[ShiftNotes]
GO
truncate table [dbo].[Section]
GO
truncate table [dbo].[FloorTable]
GO
truncate table [dbo].[FloorPlan]
GO
truncate table [dbo].[CustomersRestrictions]
GO
truncate table [dbo].[CustomersPhoneNumbers]
GO
DELETE FROM [dbo].[UserPhones]
GO
DELETE FROM [dbo].[PhoneTypes]
GO
truncate table [dbo].[CustomerSpecialStatus]
GO
truncate table [dbo].[CustomersEmails]
GO
truncate table [dbo].[CustomersAllergies]
GO
DELETE FROM [dbo].[EmailTypes]
GO
DELETE FROM [dbo].[Customers]
GO
DELETE FROM [dbo].[Cities]
GO
DELETE FROM [dbo].[webpages_UsersInRoles]
GO
DELETE FROM [dbo].[webpages_Membership]
GO
DELETE FROM [dbo].[UserPhones]
GO
DELETE FROM [dbo].[UserProfile]
GO


SET IDENTITY_INSERT [dbo].[EmailTypes] ON;
GO

INSERT INTO [dbo].[EmailTypes]
			([EmailTypeId],[EmailType])
	 VALUES (1,'Personal')
			,(2,'Work')
GO

SET IDENTITY_INSERT [dbo].[EmailTypes] OFF;
GO

SET IDENTITY_INSERT [dbo].[PhoneTypes] ON;
GO

INSERT INTO	[dbo].[PhoneTypes] 
			([PhoneTypeId]
			,[PhoneType])
	 VALUES	(1
			,'Mobile')
			,(2
			,'Landline');
GO

SET IDENTITY_INSERT [dbo].[PhoneTypes] OFF;
GO

SET IDENTITY_INSERT [dbo].[UserProfile] ON;
GO

INSERT INTO [dbo].[UserProfile]
           ([UserId]
		   ,[UserName]
           ,[FirstName]
           ,[LastName]
           ,[DesignationId]
           ,[PhotoPath]
           ,[Availability])
     VALUES
           (1
		   ,'admin@admin.com'
           ,'Leigh'
           ,'Angman'
           ,NULL
           ,NULL
           ,1)
GO

SET IDENTITY_INSERT [dbo].[UserProfile] OFF;
GO


INSERT INTO [dbo].[webpages_Membership]
           ([UserId]
           ,[CreateDate]
           ,[ConfirmationToken]
           ,[IsConfirmed]
           ,[LastPasswordFailureDate]
           ,[PasswordFailuresSinceLastSuccess]
           ,[Password]
           ,[PasswordChangedDate]
           ,[PasswordSalt]
           ,[PasswordVerificationToken]
           ,[PasswordVerificationTokenExpirationDate])
     VALUES
           (1
           ,'2014-09-10 17:10:13.533'
           ,NULL
           ,1
           ,NULL
           ,0
           ,'ACyr12O1M8RI/Q6zVSwZjNUKorgv8dHcCYEF5xWGhMNd8cI95aFv2l1+6GKrhkAdXw=='
           ,'2014-09-10 17:10:13.533'
           ,''
           ,NULL
           ,NULL)
GO

INSERT INTO [dbo].[webpages_UsersInRoles]
           ([UserId]
           ,[RoleId])
     VALUES
           (1
           ,1)
GO

SET IDENTITY_INSERT [dbo].[UserPhones] ON;
GO

INSERT INTO [dbo].[UserPhones]
           ([UserPhoneId]
		   ,[UserId]
           ,[PhoneTypeId]
           ,[PhoneNumber])
     VALUES
           (1
		   ,1
           ,1
           ,'1234567890')
GO

SET IDENTITY_INSERT [dbo].[UserPhones] OFF;
GO