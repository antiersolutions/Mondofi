USE [AISORIGIONAL]
GO

/****** Object:  Table [dbo].[Reservation]    Script Date: 14-07-2015 12:38:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Setting](
	[SettingId] [int] IDENTITY(1,1) NOT NULL,
	[Name]		[nvarchar](200) NOT NULL,
	[Value]		[nvarchar](max) NOT NULL,
	[UpdatedBy] [bigint] NULL,
	[UpdatedOn] [datetime] NULL,
 CONSTRAINT [PK_Setting] PRIMARY KEY CLUSTERED 
(
	[SettingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO