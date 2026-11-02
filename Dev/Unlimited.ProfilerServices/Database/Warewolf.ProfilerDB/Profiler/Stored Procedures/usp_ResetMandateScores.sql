CREATE PROCEDURE [Profiler].[usp_ResetMandateScores]
	@ScoreAdjustmentReason varchar(200)
	,@MandateId uniqueIdentifier
AS

Update [Profiler].[MandateScores]
set 
	 [Score] = 0
	,[ScoreAdjustmentReason] = @ScoreAdjustmentReason
	,[ScoreAdjustedOn] = getdate()
where [MandateId] = @MandateId
and coalesce([Score],0) <> 0

RETURN 0
