CREATE TABLE [Profiler].[ProfileDates](
	[ProfileDateId] INT NOT NULL IDENTITY, 
	[CollectionDate] [date] NOT NULL,
	[DebitRunId] [tinyint] NOT NULL,
	[PaymentCycle] INT NOT NULL,
	[ProfileId] VARCHAR(50) NOT NULL,
	CONSTRAINT UK_ProfileDatesMaster UNIQUE(CollectionDate,DebitRunId,ProfileId), 
    CONSTRAINT [FK_ProfileDatesMaster_ProfileMaster] FOREIGN KEY (ProfileId) REFERENCES [Profiler].Profiles(ProfileId), 
    CONSTRAINT [PK_ProfileDatesMaster] PRIMARY KEY ([ProfileDateId]) 
) ON [PRIMARY]
GO

CREATE INDEX [NONCLST_IDX_ProfileDates_ProfileId_ExtractScheduleId_DebitRunId_CollectionDate] ON [Profiler].[ProfileDates] ([ProfileId],[DebitRunId]) INCLUDE ([CollectionDate])
GO

