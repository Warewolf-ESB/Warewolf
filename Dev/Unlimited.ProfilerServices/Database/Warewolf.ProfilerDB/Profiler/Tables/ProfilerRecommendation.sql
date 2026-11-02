CREATE TABLE [Profiler].[ProfilerRecommendation](
 [ProfilerRecommendationId] INT NOT NULL IDENTITY, 
    [MandateId] UNIQUEIDENTIFIER NOT NULL, 
    [ForPaymentCycle] INT NOT NULL, 
    [CollectionMethod] VARCHAR(50) NOT NULL,
    [ServiceType] VARCHAR(50) NULL ,
    [TrackingDays] INT NOT NULL, 
    [MandateDRI] INT NOT NULL,
    [RecommendationTypes] VARCHAR(100) NULL, 
    [ProfileDateId] INT NOT NULL, 
    [SourcePaymentId] INT NULL,
    [InsertedOn] DATETIME NOT NULL, 
    [SysStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START NOT NULL default getutcdate(),
	[SysEnd] DATETIME2 (7) GENERATED ALWAYS AS ROW END NOT NULL DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
    PERIOD FOR SYSTEM_TIME ([SysStart], [SysEnd]),
    
    CONSTRAINT [PK_ProfilerRecommendation] PRIMARY KEY CLUSTERED 
([ProfilerRecommendationId])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON   [PRIMARY], 
    CONSTRAINT [FK_ProfilerRecommendation_ProfileDatesMaster] FOREIGN KEY ([ProfileDateId]) REFERENCES [Profiler].[ProfileDates]([ProfileDateId]),
    CONSTRAINT [FK_ProfilerRecommendation_RecommendationType] FOREIGN KEY ([RecommendationTypes]) REFERENCES [Profiler].[RecommendationType]([RecommendationTypeId])
) ON  [PRIMARY]
WITH (SYSTEM_VERSIONING = ON(HISTORY_TABLE=[Profiler].[ProfilerRecommendation_HISTORY], DATA_CONSISTENCY_CHECK=ON))

GO

CREATE NONCLUSTERED INDEX [Ix_ProfilerRecommendation_MandateId_ForPaymentCycle]ON [Profiler].[ProfilerRecommendation] ([MandateId],[ForPaymentCycle])INCLUDE ([CollectionMethod],[TrackingDays],[MandateDRI],[RecommendationTypes],[ProfileDateId],[InsertedOn])

GO

GO
CREATE NONCLUSTERED INDEX [IX_ProfilerRecommendation_ForPaymentCycle]
ON [Profiler].[ProfilerRecommendation] ([ForPaymentCycle])
INCLUDE ([MandateId],[CollectionMethod],[TrackingDays],[MandateDRI],[RecommendationTypes],[ProfileDateId],[SourcePaymentId],[InsertedOn])

GO
CREATE NONCLUSTERED INDEX [IX_ProfilerRecommendation_ProfileDateId_ForPaymentCycle]
ON [Profiler].[ProfilerRecommendation] ([ProfileDateId],[ForPaymentCycle])
INCLUDE ([MandateId],[CollectionMethod],[TrackingDays],[MandateDRI],[RecommendationTypes],[InsertedOn])
WITH (ONLINE=ON)
