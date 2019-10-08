USE [MFG_EDI]
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Edi_Config', @level2type=N'COLUMN',@level2name=N'executionType'

GO

/****** Object:  Table [dbo].[Edi_Config]    Script Date: 11/22/2018 12:13:16 PM ******/
DROP TABLE [dbo].[Edi_Config]
GO

/****** Object:  Table [dbo].[Edi_Config]    Script Date: 11/22/2018 12:13:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Edi_Config](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userName] [nvarchar](80) NOT NULL,
	[workstation] [nvarchar](200) NOT NULL,
	[engine] [nvarchar](20) NOT NULL,
	[soCreation] [bit] NOT NULL,
	[soCreationStatus] [nvarchar](10) NOT NULL,
	[executionType] [nvarchar](10) NOT NULL,
	[template] [nvarchar](max) NOT NULL,
	[clients] [nvarchar](max) NOT NULL,
	[filetype] [nvarchar](100) NOT NULL,
	[extras] [nvarchar](50) NOT NULL,
	[writeScheduleLineToNote] [bit] NOT NULL,
 CONSTRAINT [PK_Edi_Config] PRIMARY KEY CLUSTERED
(
	[userName] ASC,
	[workstation] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'simple (only delivery), double (delivery and forecast), complex (like BRP where we give a folder the engine determines what all the files are)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Edi_Config', @level2type=N'COLUMN',@level2name=N'executionType'
GO
