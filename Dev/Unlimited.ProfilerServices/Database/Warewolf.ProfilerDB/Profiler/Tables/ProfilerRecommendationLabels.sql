CREATE TABLE [Profiler].[ProfilerRecommendationLabels]
(
	[ProfilerRecommendationLabelId] INT NOT NULL IDENTITY , 
    [ProfilerRecommendationld] INT NOT NULL , 
    [Label] VARCHAR(100) NOT NULL, 
    CONSTRAINT [FK_ProfilerRecommendationLabels_ProfilerRecommendation] FOREIGN KEY ([ProfilerRecommendationld]) REFERENCES [Profiler].[ProfilerRecommendation]([ProfilerRecommendationId]), 
    CONSTRAINT [PK_ProfilerRecommendationLabels] PRIMARY KEY ([ProfilerRecommendationld], [ProfilerRecommendationLabelId]) 
)
