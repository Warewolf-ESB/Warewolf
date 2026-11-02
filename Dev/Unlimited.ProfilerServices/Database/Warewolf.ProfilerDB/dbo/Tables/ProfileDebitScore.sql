CREATE TABLE [dbo].[ProfileDebitScore](
 [DebitScoreId] INT NOT NULL IDENTITY, 
	[MandateId] [uniqueidentifier] NOT NULL,
	[ExtractScheduleId] [int] NOT NULL,
	[ProfileId] [tinyint] NOT NULL,
	[ScoredOn] [datetime] NOT NULL,
	[Score] [int] NOT NULL,
	[ScoreAdjustedOn] [datetime] NULL,
	[ScoreAdjustmentReason] [nvarchar](200) NULL,
    CONSTRAINT [PK_ProfileDebitScore] PRIMARY KEY CLUSTERED 
	([DebitScoreId])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	CONSTRAINT UK_DebitScore UNIQUE(MandateId,ExtractScheduleId,ProfileId) 
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProfileDebitScore]  WITH NOCHECK ADD  CONSTRAINT [FK_ProfileDebitScore_Profile_ProfileId] FOREIGN KEY([ProfileId])
REFERENCES [dbo].[Profiles] ([ProfileId])
GO

ALTER TABLE [dbo].[ProfileDebitScore] CHECK CONSTRAINT [FK_ProfileDebitScore_Profile_ProfileId]
GO

CREATE NONCLUSTERED INDEX [IX_MandateId_Includes]
ON ProfileDebitScore
(
	[MandateId] ASC	
)
INCLUDE ([ExtractScheduleId],[ProfileId],[ScoredOn],[Score],[ScoreAdjustedOn],[ScoreAdjustmentReason])
GO

