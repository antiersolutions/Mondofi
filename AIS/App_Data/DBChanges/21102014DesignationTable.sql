
ALTER TABLE dbo.Designations ADD
	IsAssignable bit NOT NULL CONSTRAINT DF_Designations_IsAssignable DEFAULT 0
GO
ALTER TABLE dbo.Designations SET (LOCK_ESCALATION = TABLE)
GO

