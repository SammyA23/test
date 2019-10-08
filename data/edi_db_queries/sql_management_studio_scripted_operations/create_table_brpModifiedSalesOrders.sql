USE [MFG_EDI]
GO

/****** Object:  Table [dbo].[brpModifiedSalesOrders]    Script Date: 03/08/2019 12:04:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[brpModifiedSalesOrders](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[modifiedSO] [nvarchar](10) NOT NULL,
	[tempStatus] [nvarchar](20) NOT NULL,
	[releaseNumber] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_brpModifiedSalesOrders] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

