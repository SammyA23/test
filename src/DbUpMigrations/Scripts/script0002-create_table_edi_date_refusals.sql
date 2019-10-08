USE [MFG_EDI]
GO

/****** Object:  Table [dbo].[edi_date_refusals]    Script Date: 11/22/2018 1:20:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[edi_date_refusals](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[reason] [nvarchar](120) NOT NULL,
 CONSTRAINT [PK_edi_date_refusals] PRIMARY KEY CLUSTERED
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
