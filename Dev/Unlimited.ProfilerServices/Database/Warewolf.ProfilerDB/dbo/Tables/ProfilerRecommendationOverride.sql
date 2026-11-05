
CREATE TABLE [dbo].[ProfilerRecommendationOverride](
 [ProfilerRecommendationOverrideId] INT NOT NULL IDENTITY,
	[MandateId] UNIQUEIDENTIFIER NOT NULL,
	[DebitMonth] [int] NOT NULL,
	[DebitMethod] [varchar](20) NOT NULL,
	[DebitRunID] [int] NOT NULL,
	[DebitDate] [date] NOT NULL,
	[ProfileName] [varchar](50) NOT NULL,
	[Comment] [varchar](500) NULL,
	[InsertedOn] [datetime] NULL,
	[SysStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START NOT NULL default getutcdate(),
	[SysEnd] DATETIME2 (7) GENERATED ALWAYS AS ROW END NOT NULL DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
    PERIOD FOR SYSTEM_TIME ([SysStart], [SysEnd]),
    CONSTRAINT [PK_ProfilerRecommendationOverride] PRIMARY KEY CLUSTERED ([ProfilerRecommendationOverrideId])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
    CONSTRAINT UK_ProfilerRecormmendationOverride UNIQUE([MandateId],DebitMonth) ) ON [PRIMARY]
    
		 

 WITH (SYSTEM_VERSIONING = ON(HISTORY_TABLE=[dbo].[ProfilerRecormmendationOverride_HISTORY], DATA_CONSISTENCY_CHECK=ON))
GO


CREATE NONCLUSTERED INDEX [Ix_ProfilerRecommendationOverride_MandateId_DebitMonth_Include] ON [dbo].[ProfilerRecommendationOverride]
(
	[MandateId] ASC,
	[DebitMonth] ASC
)
INCLUDE([DebitMethod],[DebitRunID],[DebitDate],[ProfileName],[Comment],[InsertedOn]) 
GO