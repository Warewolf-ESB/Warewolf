CREATE PROCEDURE [Profiler].[usp_GetGrantDatesByPaymentCycle] (
	@FromPaymentCycle INT
	,@ToPaymentCycle INT
	)
AS
BEGIN
	SET NOCOUNT ON;

	WITH PaymentCycleData
	AS (
		-- Convert input '202509' to a DATE: '2025-09-01'
		SELECT CAST(CONCAT (
					LEFT(@FromPaymentCycle, 4)
					,'-'
					,RIGHT(@FromPaymentCycle, 2)
					,'-01'
					) AS DATE) AS PaymentCycle
		
		UNION ALL
		
		-- Recursively add one month
		SELECT DATEADD(MONTH, 1, PaymentCycle)
		FROM PaymentCycleData
		WHERE FORMAT(PaymentCycle, 'yyyyMM') < @ToPaymentCycle -- Change this to generate more months
		)
	SELECT GT.GrantTypeId
		,GT.GrantName
		,FORMAT(PCD.PaymentCycle, 'yyyyMM') AS PaymentCycle
		,FORMAT(G.[Date], 'yyyy-MM-dd') AS GrantDate
	FROM [Profiler].[GrantType] AS GT WITH (NOLOCK)
	CROSS JOIN PaymentCycleData AS PCD
	LEFT OUTER JOIN [Profiler].[GrantDates] AS G WITH (NOLOCK) ON G.PaymentCycle = CAST(FORMAT(PCD.PaymentCycle, 'yyyyMM') AS INT)
		AND G.GrantTypeId = GT.GrantTypeId;
END
