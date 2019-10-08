USE [MFG_EDI]
GO

/****** Object:  Table [dbo].[brpCreatedSalesOrders]    Script Date: 12/09/2018 2:41:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[brpCreatedSalesOrders](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[createdSO] [nvarchar](10) NOT NULL,
	[tempStatus] [nvarchar](20) NOT NULL,
	[releaseNumber] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_brpCreatedSalesOrders] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

