CREATE PROCEDURE [Profiler].[usp_SetScoresForWeekdayFailure]
	@MandateId uniqueidentifier
AS
	UPDATE OtherMandatesDRIs
	SET OtherMandatesDRIs.Score = -1
	FROM Profiler.MandateScores OtherMandatesDRIs with (rowlock,updlock)
	INNER JOIN Profiler.MandateScores FailedWeekdays with (nolock)
		on FailedWeekdays.MandateId = OtherMandatesDRIs.MandateId 
		and FailedWeekdays.ProfileId = 'Weekday'	
		and FailedWeekdays.DebitRunId = OtherMandatesDRIs.DebitRunId 
		and FailedWeekdays.ProfileId <> OtherMandatesDRIs.ProfileId 
		and FailedWeekdays.Score = -1 		
	where OtherMandatesDRIs.MandateId = @MandateId
	and OtherMandatesDRIs.ProfileId <> 'FullyCovered'	
	and OtherMandatesDRIs.Score > 0
RETURN 0
