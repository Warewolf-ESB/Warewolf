CREATE TABLE [dbo].[ProfileDates](
	[ProfileDateId] INT NOT NULL IDENTITY, 
	[CollectionDate] [date] NOT NULL,
	[DebitRunId] [tinyint] NOT NULL,
	[ProfileId] [tinyint] NOT NULL,
	[ExtractScheduleId] [int] NULL,
    PRIMARY KEY CLUSTERED ([ProfileDateId])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	CONSTRAINT UK_ProfileDates UNIQUE(CollectionDate,DebitRunId,ProfileId) 
) ON [PRIMARY]
GO

CREATE INDEX [NONCLST_IDX_ProfileDates_ProfileId_ExtractScheduleId_DebitRunId_CollectionDate] ON [dbo].[ProfileDates] ([ProfileId], [ExtractScheduleId],[DebitRunId]) INCLUDE ([CollectionDate])
GO

CREATE NONCLUSTERED INDEX [IX_ProfileDates_PaymentCycle_CollectionDate]
ON [Profiler].[ProfileDates] ([PaymentCycle],[CollectionDate])
INCLUDE ([DebitRunId],[ProfileId])