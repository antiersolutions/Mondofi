USE [AISLocal]
GO

/****** Object:  Table [dbo].[ReservationAudit]    Script Date: 25-02-2015 14:57:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReservationAudit](
	[ReservationAuditId] [bigint] IDENTITY(1,1) NOT NULL,
	[ReservationId] [bigint] NOT NULL,
	[LoginUserId] [bigint] NOT NULL,
	[PinUserId] [bigint] NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_ReservationAudit] PRIMARY KEY CLUSTERED 
(
	[ReservationAuditId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[ReservationAudit]
ADD [Action] [nvarchar](50) NOT NULL DEFAULT('Edited')
GO

--2015-03-03

ALTER TABLE [dbo].[ReservationAudit]
ADD [TimeForm] [datetime] NOT NULL default(getdate()),
	[Covers] [int] NOT NULL default(0),
	[TableName] [nvarchar](max) NOT NULL default('T-0'),
	[Notes] [nvarchar](max) NULL
GO

ALTER TABLE [dbo].[ReservationAudit]
ALTER COLUMN [TimeForm] [datetime] NOT NULL;
ALTER TABLE [dbo].[ReservationAudit]
ALTER COLUMN [Covers] [int] NOT NULL;
ALTER TABLE [dbo].[ReservationAudit]
ALTER COLUMN [TableName] [nvarchar](max) NOT NULL;

update audit
set audit.Covers = res.Covers,
	audit.Notes = res.ReservationNote,
	audit.TimeForm = res.TimeForm,
	audit.TableName = (CASE WHEN res.FloorTableId <> 0 THEN 
	(select [TableName] from [FloorTable] where [FloorTableId] = res.FloorTableId) ELSE
	(select [TableName] from [MergedFloorTable] where [MergedFloorTableId] = res.MergedFloorTableId) END)
from [dbo].[ReservationAudit] audit
inner join Reservation res
	on audit.ReservationId = res.ReservationId
where audit.TableName = 'T-0'

-- 2015-03-05

ALTER TABLE [dbo].[ReservationAudit]
ADD [StatusId] [int] NOT NULL default(0)
GO

ALTER TABLE [dbo].[ReservationAudit]
ALTER COLUMN [StatusId] [int] NOT NULL

update audit
set audit.StatusId = res.StatusId
from [dbo].[ReservationAudit] audit
inner join Reservation res
	on audit.ReservationId = res.ReservationId
where audit.StatusId = 0

ALTER TABLE [dbo].[ReservationAudit]
ADD [TimeTo] [datetime] NOT NULL default(getdate())
GO

ALTER TABLE [dbo].[ReservationAudit]
ALTER COLUMN [TimeTo] [datetime] NOT NULL

update audit
set audit.TimeTo = res.TimeTo
from [dbo].[ReservationAudit] audit
inner join Reservation res
	on audit.ReservationId = res.ReservationId