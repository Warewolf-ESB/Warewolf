CREATE TABLE [dbo].[CollectionRecommendation](
	[CollectionRecommendationId] [uniqueidentifier] NOT NULL,
	[MandateId] [uniqueidentifier] NOT NULL,
	[InsertedOn] [datetime] NOT NULL,
	[CollectionMonth] [int] NOT NULL,
	[DebitRunID] [int] NOT NULL,
	[PresentForCollectionOn] [datetime] NOT NULL,
	[Profile] [varchar](100) NOT NULL,
	[CollectionMethod] [varchar](50) NOT NULL,
	[TrackingDays] [varchar](2) NOT NULL,
	[IsOverride] [bit] NOT NULL,
	[IsValid] [bit] NOT NULL,
	[SentOn] [datetime] NOT NULL,
	[ApplicableMonth] [int] NOT NULL,
	[ApplicableYear] INT NULL, 
    [ProfilerTypeId] INT NULL,
	[ProfilerDRI] INT NULL,
	CONSTRAINT [FK_CollectionRecommendation_ProfilerType] FOREIGN KEY ([ProfilerTypeId]) REFERENCES [dbo].[ProfilerTypes] ([ProfilerTypeId]),
    CONSTRAINT [PK_CollectionRecommendation] PRIMARY KEY CLUSTERED 
(
	[CollectionRecommendationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE NONCLUSTERED INDEX [IX_CollectionRecommendation_MandateId]
ON [dbo].[CollectionRecommendation] ([MandateId])
INCLUDE ([ProfilerTypeId],[ProfilerDRI],[InsertedOn],[CollectionMonth],[DebitRunID],[PresentForCollectionOn],[Profile],[CollectionMethod],[TrackingDays],[IsOverride],[IsValid],[SentOn],[ApplicableMonth],[ApplicableYear])
GO

CREATE NONCLUSTERED INDEX [IX_CollectionRecommendation_Profile_Include]
ON [dbo].[CollectionRecommendation] ([Profile])
INCLUDE ([MandateId],[InsertedOn],[CollectionMonth],[DebitRunID],[PresentForCollectionOn],[CollectionMethod],[TrackingDays],[IsOverride],[IsValid],[SentOn],[ApplicableMonth],[ApplicableYear],[ProfilerTypeId],[ProfilerDRI])
GO 

CREATE NONCLUSTERED INDEX [IX_CollectionRecommendation_PresentForCollectionOn]
ON [dbo].[CollectionRecommendation] ([PresentForCollectionOn])
INCLUDE ([MandateId],[ApplicableMonth])
GO

CREATE NONCLUSTERED INDEX [IX_CollectionRecommendation_InsertedOn]
ON [dbo].[CollectionRecommendation] ([InsertedOn])
INCLUDE ([MandateId],[PresentForCollectionOn],[CollectionMethod],[ApplicableMonth],[ApplicableYear])
GO