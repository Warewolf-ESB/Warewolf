CREATE TABLE [Profiler].[Profiles](
	[ProfileId] VARCHAR(50) NOT NULL,
	[Description] [varchar](100) NOT NULL, 
    [IsDeleted] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_ProfileMaster] PRIMARY KEY ([ProfileId])
) ON [PRIMARY]
GO
