CREATE TABLE [dbo].[DebitRunIds]
(
	[DebitRunId] [tinyint] NOT NULL PRIMARY KEY,
	[Description] [varchar](100) NOT NULL,
	[IsDeleted] BIT CONSTRAINT [DF_DebitRunIds_IsDeleted] DEFAULT ((0)) NULL,
)
