
CREATE TABLE [dbo].[ActivityLog](
	[ActivityLogId] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [uniqueidentifier] NULL,	
	[MandateId] [uniqueidentifier] NULL,
	[Request] [nvarchar](max) NULL,
	[Response] [nvarchar](max) NULL,
	[EventName] [varchar](50) NULL,
	[LogType] [varchar](50) NULL,
	[CreatedOn] [datetime] NULL  DEFAULT (getutcdate()),
 [ModifiedOn] DATETIME NULL, 
    CONSTRAINT [PK_ActivityLog] PRIMARY KEY CLUSTERED 
(
	[ActivityLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX [NONCLST_IDX_ActivityLog_RequestId_LogType] ON [dbo].[ActivityLog] ([RequestId], [LogType])
GO