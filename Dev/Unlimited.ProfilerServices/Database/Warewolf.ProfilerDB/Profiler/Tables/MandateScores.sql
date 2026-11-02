CREATE TABLE [Profiler].[MandateScores]
(
    [MandateId] UNIQUEIDENTIFIER NOT NULL, 
    [ProfileId] VARCHAR(50) NOT NULL, 
    [DebitRunId] tinyint not null,
    [PaymentCycle] INT NOT NULL, 
    [ScoredOn] DATETIME NOT NULL, 
    [Score] INT NOT NULL, 
    [ScoreAdjustedOn] DATETIME NOT NULL, 
    [ScoreAdjustmentReason] NVARCHAR(200) NULL, 
	[SysStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START NOT NULL default getutcdate(),
	[SysEnd] DATETIME2 (7) GENERATED ALWAYS AS ROW END NOT NULL DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
    PERIOD FOR SYSTEM_TIME ([SysStart], [SysEnd]),
    CONSTRAINT [PK_MandateProfileScores] PRIMARY KEY ([MandateId], [ProfileId], [DebitRunId]), 
    CONSTRAINT [FK_MandateProfileScores_Profiles] FOREIGN KEY ([ProfileId]) REFERENCES [Profiler].[Profiles]([ProfileId]),
    
    
)
WITH (SYSTEM_VERSIONING = ON(HISTORY_TABLE=[Profiler].[MandateScores_HISTORY], DATA_CONSISTENCY_CHECK=ON))
GO
CREATE NONCLUSTERED INDEX [IX_MandateScores_MandateScores]
ON [Profiler].[MandateScores] ([MandateId],[DebitRunId],[Score])
    