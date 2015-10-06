USE [AISLocal]
GO

/****** Object:  Table [dbo].[MergedFloorTable]    Script Date: 29-08-2014 11:24:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MergedFloorTable](
	[MergedFloorTableId] [bigint] IDENTITY(1,1) NOT NULL,
	[FloorPlanId] [bigint] NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[HtmlId] [nvarchar](100) NOT NULL,
	[Shape] [nvarchar](50) NOT NULL,
	[Size] [nvarchar](50) NOT NULL,
	[MinCover] [int] NOT NULL,
	[MaxCover] [int] NOT NULL,
	[Angle] [int] NOT NULL,
	[TTop] [nvarchar](50) NOT NULL,
	[TRight] [nvarchar](50) NULL,
	[TBottom] [nvarchar](50) NULL,
	[TLeft] [nvarchar](50) NOT NULL,
	[TableDesign] [nvarchar](max) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [bigint] NOT NULL,
 CONSTRAINT [PK_MergedFloorTable] PRIMARY KEY CLUSTERED 
(
	[MergedFloorTableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[MergedTableOrigionalTable](
	[MergedTableOrigionalTableId] [bigint] IDENTITY(1,1) NOT NULL,
	[MergedFloorTableId] [bigint] NOT NULL,
	[FloorTableId] [bigint] NOT NULL,
 CONSTRAINT [PK_MergedTableOrigionalTable] PRIMARY KEY CLUSTERED 
(
	[MergedTableOrigionalTableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Reservation]
ADD [MergedFloorTableId] bigint
GO

UPDATE  [dbo].[Reservation]
SET [MergedFloorTableId] = '0'
GO



