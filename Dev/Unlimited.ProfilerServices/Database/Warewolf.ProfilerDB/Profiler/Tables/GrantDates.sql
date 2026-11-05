CREATE TABLE [Profiler].[GrantDates]
(
	[GrantDateId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Date] DATE NOT NULL, 
    [PaymentCycle] INT NOT NULL,
    [GrantTypeId] INT NOT NULL, 
    CONSTRAINT [FK_GrantDates_ToGrantType] FOREIGN KEY ([GrantTypeId]) REFERENCES [Profiler].[GrantType]([GrantTypeId]) 
)
