USE [AISLocal]
GO

CREATE TABLE [dbo].[ReservationServer](
	[ReservationId] [bigint] NOT NULL,
	[ServerId] [bigint] NULL,
 CONSTRAINT [PK_ReservationServer] PRIMARY KEY CLUSTERED 
(
	[ReservationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[FloorTableServer](
	[FloorTableId] [bigint] NOT NULL,
	[ServerId] [bigint] NULL,
 CONSTRAINT [PK_FloorTableServer] PRIMARY KEY CLUSTERED 
(
	[FloorTableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UserProfile]
ADD [StaffColor] [nvarchar](10) NULL
GO
