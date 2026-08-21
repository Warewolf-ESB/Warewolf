-- Provisions the dbo.jobs1 / dbo.jobs2 ShovelBridge load-test logging schema (tables,
-- indexes, and the 8 usp_jobs{1,2}_* procedures each) plus sp_TestEntraConnectivity,
-- consumed by Resources/rabbit/RabbitProcess.bite / RabbitProcess2.bite / "Simple MSSQL
-- Test.bite" via the shared NewSqlServerSource.bite (SourceId
-- b9184f70-64ea-4dc5-b23b-02fcd5f91082).
--
-- WOLF-8510: this schema previously existed only as live, undocumented state on
-- WarewolfEntraTestDb (Azure SQL, server warewolf-dev2-mcgeaj) -- see
-- docs/ShovelBridge-Architecture.md's "no provisioning script for it in the repo" note.
-- That database permanently exhausted Azure SQL's monthly free-limit allowance and was
-- deleted; this script was reverse-engineered from its live schema immediately beforehand
-- so the replacement database (WarewolfDevOpsTestDb, same server/login, NOT free-limit
-- enrolled) could be provisioned identically and any future replacement can be too.
--
-- Faithfully reproduces the schema AS IT WAS LIVE, including the pre-existing jobs1/jobs2
-- discrepancy: jobs1 has 8 CHECK constraints, jobs2 has none. Idempotent: safe to re-run
-- against an already-provisioned database (tables are skipped if they exist; procedures
-- use CREATE OR ALTER).
--
-- Usage: run against the target database as a principal with CREATE TABLE/PROCEDURE
-- rights (e.g. the AAD admin, or dbo), then grant EXECUTE/VIEW DEFINITION on schema dbo to
-- whichever login the source's connection string uses (devops_warewolf on the current
-- Azure SQL Database -- see Resources/rabbit/NewSqlServerSource.bite).

IF OBJECT_ID('dbo.jobs1', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.jobs1
    (
        JobLogId                  BIGINT IDENTITY(1,1) NOT NULL,
        MessageContent            NVARCHAR(MAX)   NOT NULL,
        MessageContentHash        NVARCHAR(64)    NULL,
        PayloadSizeBytes          NVARCHAR(20)    NULL,
        AttemptNumber             NVARCHAR(20)    NOT NULL,
        AdditionalContext         NVARCHAR(MAX)   NULL,
        QueueName                 NVARCHAR(200)   NULL,
        ExecutionEngineInstanceId NVARCHAR(200)   NULL,
        MachineName               NVARCHAR(200)   NULL,
        Status                    NVARCHAR(30)    NOT NULL,
        StartedAtUtc              NVARCHAR(50)    NULL,
        ProcessingAtUtc           NVARCHAR(50)    NULL,
        FinishedAtUtc             NVARCHAR(50)    NULL,
        DurationMs                NVARCHAR(20)    NULL,
        HttpStatusCode            NVARCHAR(20)    NULL,
        ErrorCode                 NVARCHAR(100)   NULL,
        ErrorMessage              NVARCHAR(MAX)   NULL,
        ErrorStackTrace           NVARCHAR(MAX)   NULL,
        CreatedAtUtc              NVARCHAR(50)    NOT NULL,
        ModifiedAtUtc             NVARCHAR(50)    NOT NULL,
        RowVer                    ROWVERSION      NOT NULL,
        CONSTRAINT PK_jobs1 PRIMARY KEY CLUSTERED (JobLogId),
        CONSTRAINT UQ_jobs1_ContentHash_AttemptNumber UNIQUE NONCLUSTERED (MessageContentHash, AttemptNumber),
        CONSTRAINT CK_jobs1_Status CHECK (Status IN ('STARTED','PROCESSING','FINISHED','FINISHED_WITH_ERROR')),
        CONSTRAINT CK_jobs1_AttemptNumber_IsInt CHECK (TRY_CAST(AttemptNumber AS INT) IS NOT NULL),
        CONSTRAINT CK_jobs1_HttpStatusCode_IsInt CHECK (HttpStatusCode IS NULL OR TRY_CAST(HttpStatusCode AS INT) IS NOT NULL),
        CONSTRAINT CK_jobs1_StartedAtUtc_IsDate CHECK (StartedAtUtc IS NULL OR TRY_CONVERT(DATETIME2(3), StartedAtUtc, 127) IS NOT NULL),
        CONSTRAINT CK_jobs1_ProcessingAtUtc_IsDate CHECK (ProcessingAtUtc IS NULL OR TRY_CONVERT(DATETIME2(3), ProcessingAtUtc, 127) IS NOT NULL),
        CONSTRAINT CK_jobs1_FinishedAtUtc_IsDate CHECK (FinishedAtUtc IS NULL OR TRY_CONVERT(DATETIME2(3), FinishedAtUtc, 127) IS NOT NULL),
        CONSTRAINT CK_jobs1_CreatedAtUtc_IsDate CHECK (TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) IS NOT NULL),
        CONSTRAINT CK_jobs1_ModifiedAtUtc_IsDate CHECK (TRY_CONVERT(DATETIME2(3), ModifiedAtUtc, 127) IS NOT NULL)
    );
    CREATE NONCLUSTERED INDEX IX_jobs1_ContentHash ON dbo.jobs1 (Status, AttemptNumber, MessageContentHash);
    CREATE NONCLUSTERED INDEX IX_jobs1_Status_CreatedAtUtc ON dbo.jobs1 (Status, CreatedAtUtc);
END
GO

IF OBJECT_ID('dbo.jobs2', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.jobs2
    (
        JobLogId                  BIGINT IDENTITY(1,1) NOT NULL,
        MessageContent            NVARCHAR(MAX)   NOT NULL,
        MessageContentHash        NVARCHAR(64)    NULL,
        PayloadSizeBytes          NVARCHAR(20)    NULL,
        AttemptNumber             NVARCHAR(20)    NOT NULL,
        AdditionalContext         NVARCHAR(MAX)   NULL,
        QueueName                 NVARCHAR(200)   NULL,
        ExecutionEngineInstanceId NVARCHAR(200)   NULL,
        MachineName               NVARCHAR(200)   NULL,
        Status                    NVARCHAR(30)    NOT NULL,
        StartedAtUtc              NVARCHAR(50)    NULL,
        ProcessingAtUtc           NVARCHAR(50)    NULL,
        FinishedAtUtc             NVARCHAR(50)    NULL,
        DurationMs                NVARCHAR(20)    NULL,
        HttpStatusCode            NVARCHAR(20)    NULL,
        ErrorCode                 NVARCHAR(100)   NULL,
        ErrorMessage              NVARCHAR(MAX)   NULL,
        ErrorStackTrace           NVARCHAR(MAX)   NULL,
        CreatedAtUtc              NVARCHAR(50)    NOT NULL,
        ModifiedAtUtc             NVARCHAR(50)    NOT NULL,
        RowVer                    ROWVERSION      NOT NULL,
        CONSTRAINT PK_jobs2 PRIMARY KEY CLUSTERED (JobLogId),
        CONSTRAINT UQ_jobs2_ContentHash_AttemptNumber UNIQUE NONCLUSTERED (MessageContentHash, AttemptNumber)
        -- No CHECK constraints -- matches the live jobs1/jobs2 discrepancy described above.
    );
    CREATE NONCLUSTERED INDEX IX_jobs2_ContentHash ON dbo.jobs2 (Status, AttemptNumber, MessageContentHash);
    CREATE NONCLUSTERED INDEX IX_jobs2_Status_CreatedAtUtc ON dbo.jobs2 (Status, CreatedAtUtc);
END
GO

GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_LogStart
(
    @MessageContent               NVARCHAR(MAX),
    @ExecutionEngineInstanceId    NVARCHAR(200) = NULL,
    @MachineName                  NVARCHAR(200) = NULL,
    @QueueName                    NVARCHAR(200) = NULL,
    @AdditionalContext            NVARCHAR(MAX) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @MessageContent IS NULL OR LEN(@MessageContent) = 0
        THROW 51005, 'usp_jobs1_LogStart: @MessageContent is required — it is the only identifying detail available.', 1;

    DECLARE @hash          NVARCHAR(64);
    DECLARE @attemptInt    INT;
    DECLARE @newJobLogId   BIGINT;

    SET @hash = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @MessageContent), 2);

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @lockResult INT;
        EXEC @lockResult = sp_getapplock
             @Resource    = @hash,                 -- 64-char hex string, well under the 255-char sp_getapplock resource limit
             @LockMode    = 'Exclusive',
             @LockOwner   = 'Transaction',
             @LockTimeout = 15000;

        IF @lockResult < 0
            THROW 51000, 'usp_jobs1_LogStart: could not acquire correlation lock for this message content (timeout or deadlock victim).', 1;

        -- cast the existing text AttemptNumber values to INT to find the true numeric max
        -- (a plain string MAX would wrongly say '9' > '10')
        SELECT @attemptInt = ISNULL(MAX(TRY_CONVERT(INT, AttemptNumber)), 0) + 1
        FROM dbo.jobs1
        WHERE MessageContentHash = @hash;

        INSERT INTO dbo.jobs1
        (
            MessageContent, MessageContentHash, AttemptNumber,
            AdditionalContext, QueueName, ExecutionEngineInstanceId, MachineName,
            Status, StartedAtUtc,
            CreatedAtUtc, ModifiedAtUtc
        )
        VALUES
        (
            @MessageContent, @hash, CONVERT(NVARCHAR(20), @attemptInt),
            @AdditionalContext, @QueueName, @ExecutionEngineInstanceId, @MachineName,
            'STARTED', CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
            CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127), CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127)
        );

        SET @newJobLogId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    -- no OUTPUT parameters: hand the new attempt's identity back as a result set instead
    SELECT
        CONVERT(NVARCHAR(20), @newJobLogId) AS JobLogId,
        CONVERT(NVARCHAR(20), @attemptInt)  AS AttemptNumber;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_LogProcessing
(
    @JobLogId                   NVARCHAR(20),
    @ExecutionEngineInstanceId  NVARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id BIGINT = TRY_CONVERT(BIGINT, @JobLogId);
    IF @id IS NULL
        THROW 51007, 'usp_jobs1_LogProcessing: @JobLogId does not parse as a valid identifier.', 1;

    UPDATE dbo.jobs1
    SET Status                    = 'PROCESSING',
        ProcessingAtUtc            = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ModifiedAtUtc              = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ExecutionEngineInstanceId  = COALESCE(@ExecutionEngineInstanceId, ExecutionEngineInstanceId)
    WHERE JobLogId = @id
      AND Status   = 'STARTED';

    IF @@ROWCOUNT = 0
        THROW 51001, 'usp_jobs1_LogProcessing: invalid state transition — row not found in STARTED status for this JobLogId.', 1;

    SELECT
        CONVERT(NVARCHAR(20), JobLogId) AS JobLogId,
        Status,
        ProcessingAtUtc
    FROM dbo.jobs1
    WHERE JobLogId = @id;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_LogFinished
(
    @JobLogId          NVARCHAR(20),
    @HttpStatusCode    NVARCHAR(20) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id BIGINT = TRY_CONVERT(BIGINT, @JobLogId);
    IF @id IS NULL
        THROW 51008, 'usp_jobs1_LogFinished: @JobLogId does not parse as a valid identifier.', 1;

    IF @HttpStatusCode IS NOT NULL AND TRY_CONVERT(INT, @HttpStatusCode) IS NULL
        THROW 51009, 'usp_jobs1_LogFinished: @HttpStatusCode must be numeric text (e.g. ''200'').', 1;

    UPDATE dbo.jobs1
    SET Status          = 'FINISHED',
        FinishedAtUtc   = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ModifiedAtUtc   = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        HttpStatusCode  = @HttpStatusCode
    WHERE JobLogId = @id
      AND Status IN ('STARTED', 'PROCESSING');

    IF @@ROWCOUNT = 0
        THROW 51002, 'usp_jobs1_LogFinished: invalid state transition — row not in STARTED/PROCESSING status for this JobLogId.', 1;

    SELECT
        CONVERT(NVARCHAR(20), JobLogId) AS JobLogId,
        Status,
        StartedAtUtc,
        FinishedAtUtc,
        DurationMs
    FROM dbo.jobs1
    WHERE JobLogId = @id;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_LogFinishedWithError
(
    @JobLogId          NVARCHAR(20),
    @HttpStatusCode    NVARCHAR(20)   = NULL,
    @ErrorCode         NVARCHAR(100)  = NULL,
    @ErrorMessage      NVARCHAR(MAX)  = NULL,
    @ErrorStackTrace   NVARCHAR(MAX)  = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id BIGINT = TRY_CONVERT(BIGINT, @JobLogId);
    IF @id IS NULL
        THROW 51010, 'usp_jobs1_LogFinishedWithError: @JobLogId does not parse as a valid identifier.', 1;

    IF @HttpStatusCode IS NOT NULL AND TRY_CONVERT(INT, @HttpStatusCode) IS NULL
        THROW 51011, 'usp_jobs1_LogFinishedWithError: @HttpStatusCode must be numeric text (e.g. ''500'').', 1;

    UPDATE dbo.jobs1
    SET Status           = 'FINISHED_WITH_ERROR',
        FinishedAtUtc    = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ModifiedAtUtc    = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        HttpStatusCode   = @HttpStatusCode,
        ErrorCode        = @ErrorCode,
        ErrorMessage     = @ErrorMessage,
        ErrorStackTrace  = @ErrorStackTrace
    WHERE JobLogId = @id
      AND Status IN ('STARTED', 'PROCESSING');

    IF @@ROWCOUNT = 0
        THROW 51003, 'usp_jobs1_LogFinishedWithError: invalid state transition — row not in STARTED/PROCESSING status for this JobLogId.', 1;

    SELECT
        CONVERT(NVARCHAR(20), JobLogId) AS JobLogId,
        Status,
        StartedAtUtc,
        FinishedAtUtc,
        DurationMs,
        ErrorCode,
        ErrorMessage
    FROM dbo.jobs1
    WHERE JobLogId = @id;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_GetDuplicateSuccesses
(
    @FromUtc NVARCHAR(50) = NULL,
    @ToUtc   NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @FromUtc, 127) IS NULL
        THROW 51013, 'usp_jobs1_GetDuplicateSuccesses: @FromUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;
    IF @ToUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @ToUtc, 127) IS NULL
        THROW 51014, 'usp_jobs1_GetDuplicateSuccesses: @ToUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;

    DECLARE @from DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @FromUtc, 127);
    DECLARE @to   DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @ToUtc, 127);

    SELECT
        MessageContentHash,
        MIN(MessageContent)                                                                   AS SampleMessageContent,
        COUNT(*)                                                                               AS SuccessCount,
        CONVERT(NVARCHAR(50), MIN(TRY_CONVERT(DATETIME2(3), FinishedAtUtc, 127)), 127)         AS FirstSuccessAtUtc,
        CONVERT(NVARCHAR(50), MAX(TRY_CONVERT(DATETIME2(3), FinishedAtUtc, 127)), 127)         AS LastSuccessAtUtc
    FROM dbo.jobs1
    WHERE Status = 'FINISHED'
      AND (@from IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) >= @from)
      AND (@to   IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) <  @to)
    GROUP BY MessageContentHash
    HAVING COUNT(*) > 1
    ORDER BY SuccessCount DESC, MessageContentHash;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_GetMessageHistory
(
    @MessageContent      NVARCHAR(MAX) = NULL,
    @MessageContentHash  NVARCHAR(64)  = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @MessageContentHash IS NULL AND @MessageContent IS NULL
        THROW 51006, 'usp_jobs1_GetMessageHistory: supply either @MessageContent or @MessageContentHash.', 1;

    DECLARE @hash NVARCHAR(64) = ISNULL(@MessageContentHash, CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @MessageContent), 2));

    SELECT *
    FROM dbo.jobs1
    WHERE MessageContentHash = @hash
    ORDER BY TRY_CONVERT(INT, AttemptNumber);   -- numeric order, not text order ('2' before '10')
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_GetStuckMessages
(
    @StuckThresholdMinutes NVARCHAR(10) = '15'
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @minutes INT = TRY_CONVERT(INT, @StuckThresholdMinutes);
    IF @minutes IS NULL
        THROW 51012, 'usp_jobs1_GetStuckMessages: @StuckThresholdMinutes must be numeric text (e.g. ''15'').', 1;

    SELECT j.*
    FROM dbo.jobs1 j
    INNER JOIN
    (
        SELECT MessageContentHash, MAX(TRY_CONVERT(INT, AttemptNumber)) AS LastAttempt
        FROM dbo.jobs1
        GROUP BY MessageContentHash
    ) latest
        ON latest.MessageContentHash = j.MessageContentHash
       AND latest.LastAttempt = TRY_CONVERT(INT, j.AttemptNumber)
    WHERE j.Status IN ('STARTED', 'PROCESSING')
      AND TRY_CONVERT(DATETIME2(3), j.ModifiedAtUtc, 127) < DATEADD(MINUTE, -@minutes, SYSUTCDATETIME())
    ORDER BY TRY_CONVERT(DATETIME2(3), j.ModifiedAtUtc, 127) ASC;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs1_Summary
(
    @FromUtc                NVARCHAR(50) = NULL,
    @ToUtc                  NVARCHAR(50) = NULL,
    @StuckThresholdMinutes  NVARCHAR(10) = '15'
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @FromUtc, 127) IS NULL
        THROW 51015, 'usp_jobs1_Summary: @FromUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;
    IF @ToUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @ToUtc, 127) IS NULL
        THROW 51016, 'usp_jobs1_Summary: @ToUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;

    DECLARE @from    DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @FromUtc, 127);
    DECLARE @to      DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @ToUtc, 127);
    DECLARE @minutes INT          = TRY_CONVERT(INT, @StuckThresholdMinutes);

    IF @minutes IS NULL
        THROW 51017, 'usp_jobs1_Summary: @StuckThresholdMinutes must be numeric text (e.g. ''15'').', 1;

    ;WITH MsgAgg AS
    (
        SELECT
            MessageContentHash,
            SUM(CASE WHEN Status = 'FINISHED'             THEN 1 ELSE 0 END) AS SuccessCount,
            SUM(CASE WHEN Status = 'FINISHED_WITH_ERROR'   THEN 1 ELSE 0 END) AS ErrorCount,
            SUM(CASE WHEN Status IN ('STARTED','PROCESSING') THEN 1 ELSE 0 END) AS PendingCount,
            MAX(CASE WHEN Status IN ('STARTED','PROCESSING') THEN TRY_CONVERT(DATETIME2(3), ModifiedAtUtc, 127) END) AS LastPendingActivityUtc
        FROM dbo.jobs1
        WHERE (@from IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) >= @from)
          AND (@to   IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) <  @to)
        GROUP BY MessageContentHash
    )
    SELECT
        COUNT(*)                                                                              AS TotalMessages,
        SUM(CASE WHEN SuccessCount >= 1                              THEN 1 ELSE 0 END)         AS SucceededTotal,
        SUM(CASE WHEN SuccessCount = 1 AND ErrorCount = 0            THEN 1 ELSE 0 END)         AS SucceededFirstTry,
        SUM(CASE WHEN SuccessCount = 1 AND ErrorCount >= 1           THEN 1 ELSE 0 END)         AS SucceededAfterRetry_CorrectBehavior,
        SUM(CASE WHEN SuccessCount > 1                               THEN 1 ELSE 0 END)         AS DuplicateSuccess_ReliabilityBug,
        SUM(CASE WHEN SuccessCount = 0 AND ErrorCount >= 1 AND PendingCount = 0
                 THEN 1 ELSE 0 END)                                                             AS FailedAllAttempts,
        SUM(CASE WHEN SuccessCount = 0 AND PendingCount > 0          THEN 1 ELSE 0 END)         AS StillInProcess_Incomplete,
        SUM(CASE WHEN SuccessCount = 0 AND PendingCount > 0
                      AND LastPendingActivityUtc < DATEADD(MINUTE, -@minutes, SYSUTCDATETIME())
                 THEN 1 ELSE 0 END)                                                             AS StillInProcess_StuckPastThreshold
    FROM MsgAgg;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_LogStart
(
    @MessageContent               NVARCHAR(MAX),
    @ExecutionEngineInstanceId    NVARCHAR(200) = NULL,
    @MachineName                  NVARCHAR(200) = NULL,
    @QueueName                    NVARCHAR(200) = NULL,
    @AdditionalContext            NVARCHAR(MAX) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @MessageContent IS NULL OR LEN(@MessageContent) = 0
        THROW 51005, 'usp_jobs2_LogStart: @MessageContent is required — it is the only identifying detail available.', 1;

    DECLARE @hash          NVARCHAR(64);
    DECLARE @attemptInt    INT;
    DECLARE @newJobLogId   BIGINT;

    SET @hash = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @MessageContent), 2);

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @lockResult INT;
        EXEC @lockResult = sp_getapplock
             @Resource    = @hash,                 -- 64-char hex string, well under the 255-char sp_getapplock resource limit
             @LockMode    = 'Exclusive',
             @LockOwner   = 'Transaction',
             @LockTimeout = 15000;

        IF @lockResult < 0
            THROW 51000, 'usp_jobs2_LogStart: could not acquire correlation lock for this message content (timeout or deadlock victim).', 1;

        -- cast the existing text AttemptNumber values to INT to find the true numeric max
        -- (a plain string MAX would wrongly say '9' > '10')
        SELECT @attemptInt = ISNULL(MAX(TRY_CONVERT(INT, AttemptNumber)), 0) + 1
        FROM dbo.jobs2
        WHERE MessageContentHash = @hash;

        INSERT INTO dbo.jobs2
        (
            MessageContent, MessageContentHash, AttemptNumber,
            AdditionalContext, QueueName, ExecutionEngineInstanceId, MachineName,
            Status, StartedAtUtc,
            CreatedAtUtc, ModifiedAtUtc
        )
        VALUES
        (
            @MessageContent, @hash, CONVERT(NVARCHAR(20), @attemptInt),
            @AdditionalContext, @QueueName, @ExecutionEngineInstanceId, @MachineName,
            'STARTED', CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
            CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127), CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127)
        );

        SET @newJobLogId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    -- no OUTPUT parameters: hand the new attempt's identity back as a result set instead
    SELECT
        CONVERT(NVARCHAR(20), @newJobLogId) AS JobLogId,
        CONVERT(NVARCHAR(20), @attemptInt)  AS AttemptNumber;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_LogProcessing
(
    @JobLogId                   NVARCHAR(20),
    @ExecutionEngineInstanceId  NVARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id BIGINT = TRY_CONVERT(BIGINT, @JobLogId);
    IF @id IS NULL
        THROW 51007, 'usp_jobs2_LogProcessing: @JobLogId does not parse as a valid identifier.', 1;

    UPDATE dbo.jobs2
    SET Status                    = 'PROCESSING',
        ProcessingAtUtc            = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ModifiedAtUtc              = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ExecutionEngineInstanceId  = COALESCE(@ExecutionEngineInstanceId, ExecutionEngineInstanceId)
    WHERE JobLogId = @id
      AND Status   = 'STARTED';

    IF @@ROWCOUNT = 0
        THROW 51001, 'usp_jobs2_LogProcessing: invalid state transition — row not found in STARTED status for this JobLogId.', 1;

    SELECT
        CONVERT(NVARCHAR(20), JobLogId) AS JobLogId,
        Status,
        ProcessingAtUtc
    FROM dbo.jobs2
    WHERE JobLogId = @id;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_LogFinished
(
    @JobLogId          NVARCHAR(20),
    @HttpStatusCode    NVARCHAR(20) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id BIGINT = TRY_CONVERT(BIGINT, @JobLogId);
    IF @id IS NULL
        THROW 51008, 'usp_jobs2_LogFinished: @JobLogId does not parse as a valid identifier.', 1;

    IF @HttpStatusCode IS NOT NULL AND TRY_CONVERT(INT, @HttpStatusCode) IS NULL
        THROW 51009, 'usp_jobs2_LogFinished: @HttpStatusCode must be numeric text (e.g. ''200'').', 1;

    UPDATE dbo.jobs2
    SET Status          = 'FINISHED',
        FinishedAtUtc   = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ModifiedAtUtc   = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        HttpStatusCode  = @HttpStatusCode
    WHERE JobLogId = @id
      AND Status IN ('STARTED', 'PROCESSING');

    IF @@ROWCOUNT = 0
        THROW 51002, 'usp_jobs2_LogFinished: invalid state transition — row not in STARTED/PROCESSING status for this JobLogId.', 1;

    SELECT
        CONVERT(NVARCHAR(20), JobLogId) AS JobLogId,
        Status,
        StartedAtUtc,
        FinishedAtUtc,
        DurationMs
    FROM dbo.jobs2
    WHERE JobLogId = @id;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_LogFinishedWithError
(
    @JobLogId          NVARCHAR(20),
    @HttpStatusCode    NVARCHAR(20)   = NULL,
    @ErrorCode         NVARCHAR(100)  = NULL,
    @ErrorMessage      NVARCHAR(MAX)  = NULL,
    @ErrorStackTrace   NVARCHAR(MAX)  = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id BIGINT = TRY_CONVERT(BIGINT, @JobLogId);
    IF @id IS NULL
        THROW 51010, 'usp_jobs2_LogFinishedWithError: @JobLogId does not parse as a valid identifier.', 1;

    IF @HttpStatusCode IS NOT NULL AND TRY_CONVERT(INT, @HttpStatusCode) IS NULL
        THROW 51011, 'usp_jobs2_LogFinishedWithError: @HttpStatusCode must be numeric text (e.g. ''500'').', 1;

    UPDATE dbo.jobs2
    SET Status           = 'FINISHED_WITH_ERROR',
        FinishedAtUtc    = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        ModifiedAtUtc    = CONVERT(NVARCHAR(50), SYSUTCDATETIME(), 127),
        HttpStatusCode   = @HttpStatusCode,
        ErrorCode        = @ErrorCode,
        ErrorMessage     = @ErrorMessage,
        ErrorStackTrace  = @ErrorStackTrace
    WHERE JobLogId = @id
      AND Status IN ('STARTED', 'PROCESSING');

    IF @@ROWCOUNT = 0
        THROW 51003, 'usp_jobs2_LogFinishedWithError: invalid state transition — row not in STARTED/PROCESSING status for this JobLogId.', 1;

    SELECT
        CONVERT(NVARCHAR(20), JobLogId) AS JobLogId,
        Status,
        StartedAtUtc,
        FinishedAtUtc,
        DurationMs,
        ErrorCode,
        ErrorMessage
    FROM dbo.jobs2
    WHERE JobLogId = @id;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_GetDuplicateSuccesses
(
    @FromUtc NVARCHAR(50) = NULL,
    @ToUtc   NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @FromUtc, 127) IS NULL
        THROW 51013, 'usp_jobs2_GetDuplicateSuccesses: @FromUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;
    IF @ToUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @ToUtc, 127) IS NULL
        THROW 51014, 'usp_jobs2_GetDuplicateSuccesses: @ToUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;

    DECLARE @from DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @FromUtc, 127);
    DECLARE @to   DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @ToUtc, 127);

    SELECT
        MessageContentHash,
        MIN(MessageContent)                                                                   AS SampleMessageContent,
        COUNT(*)                                                                               AS SuccessCount,
        CONVERT(NVARCHAR(50), MIN(TRY_CONVERT(DATETIME2(3), FinishedAtUtc, 127)), 127)         AS FirstSuccessAtUtc,
        CONVERT(NVARCHAR(50), MAX(TRY_CONVERT(DATETIME2(3), FinishedAtUtc, 127)), 127)         AS LastSuccessAtUtc
    FROM dbo.jobs2
    WHERE Status = 'FINISHED'
      AND (@from IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) >= @from)
      AND (@to   IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) <  @to)
    GROUP BY MessageContentHash
    HAVING COUNT(*) > 1
    ORDER BY SuccessCount DESC, MessageContentHash;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_GetMessageHistory
(
    @MessageContent      NVARCHAR(MAX) = NULL,
    @MessageContentHash  NVARCHAR(64)  = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @MessageContentHash IS NULL AND @MessageContent IS NULL
        THROW 51006, 'usp_jobs2_GetMessageHistory: supply either @MessageContent or @MessageContentHash.', 1;

    DECLARE @hash NVARCHAR(64) = ISNULL(@MessageContentHash, CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @MessageContent), 2));

    SELECT *
    FROM dbo.jobs2
    WHERE MessageContentHash = @hash
    ORDER BY TRY_CONVERT(INT, AttemptNumber);   -- numeric order, not text order ('2' before '10')
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_GetStuckMessages
(
    @StuckThresholdMinutes NVARCHAR(10) = '15'
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @minutes INT = TRY_CONVERT(INT, @StuckThresholdMinutes);
    IF @minutes IS NULL
        THROW 51012, 'usp_jobs2_GetStuckMessages: @StuckThresholdMinutes must be numeric text (e.g. ''15'').', 1;

    SELECT j.*
    FROM dbo.jobs2 j
    INNER JOIN
    (
        SELECT MessageContentHash, MAX(TRY_CONVERT(INT, AttemptNumber)) AS LastAttempt
        FROM dbo.jobs2
        GROUP BY MessageContentHash
    ) latest
        ON latest.MessageContentHash = j.MessageContentHash
       AND latest.LastAttempt = TRY_CONVERT(INT, j.AttemptNumber)
    WHERE j.Status IN ('STARTED', 'PROCESSING')
      AND TRY_CONVERT(DATETIME2(3), j.ModifiedAtUtc, 127) < DATEADD(MINUTE, -@minutes, SYSUTCDATETIME())
    ORDER BY TRY_CONVERT(DATETIME2(3), j.ModifiedAtUtc, 127) ASC;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_jobs2_Summary
(
    @FromUtc                NVARCHAR(50) = NULL,
    @ToUtc                  NVARCHAR(50) = NULL,
    @StuckThresholdMinutes  NVARCHAR(10) = '15'
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @FromUtc, 127) IS NULL
        THROW 51015, 'usp_jobs2_Summary: @FromUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;
    IF @ToUtc IS NOT NULL AND TRY_CONVERT(DATETIME2(3), @ToUtc, 127) IS NULL
        THROW 51016, 'usp_jobs2_Summary: @ToUtc must be ISO-8601 UTC text (e.g. ''2026-08-10T00:00:00.000Z'').', 1;

    DECLARE @from    DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @FromUtc, 127);
    DECLARE @to      DATETIME2(3) = TRY_CONVERT(DATETIME2(3), @ToUtc, 127);
    DECLARE @minutes INT          = TRY_CONVERT(INT, @StuckThresholdMinutes);

    IF @minutes IS NULL
        THROW 51017, 'usp_jobs2_Summary: @StuckThresholdMinutes must be numeric text (e.g. ''15'').', 1;

    ;WITH MsgAgg AS
    (
        SELECT
            MessageContentHash,
            SUM(CASE WHEN Status = 'FINISHED'             THEN 1 ELSE 0 END) AS SuccessCount,
            SUM(CASE WHEN Status = 'FINISHED_WITH_ERROR'   THEN 1 ELSE 0 END) AS ErrorCount,
            SUM(CASE WHEN Status IN ('STARTED','PROCESSING') THEN 1 ELSE 0 END) AS PendingCount,
            MAX(CASE WHEN Status IN ('STARTED','PROCESSING') THEN TRY_CONVERT(DATETIME2(3), ModifiedAtUtc, 127) END) AS LastPendingActivityUtc
        FROM dbo.jobs2
        WHERE (@from IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) >= @from)
          AND (@to   IS NULL OR TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) <  @to)
        GROUP BY MessageContentHash
    )
    SELECT
        COUNT(*)                                                                              AS TotalMessages,
        SUM(CASE WHEN SuccessCount >= 1                              THEN 1 ELSE 0 END)         AS SucceededTotal,
        SUM(CASE WHEN SuccessCount = 1 AND ErrorCount = 0            THEN 1 ELSE 0 END)         AS SucceededFirstTry,
        SUM(CASE WHEN SuccessCount = 1 AND ErrorCount >= 1           THEN 1 ELSE 0 END)         AS SucceededAfterRetry_CorrectBehavior,
        SUM(CASE WHEN SuccessCount > 1                               THEN 1 ELSE 0 END)         AS DuplicateSuccess_ReliabilityBug,
        SUM(CASE WHEN SuccessCount = 0 AND ErrorCount >= 1 AND PendingCount = 0
                 THEN 1 ELSE 0 END)                                                             AS FailedAllAttempts,
        SUM(CASE WHEN SuccessCount = 0 AND PendingCount > 0          THEN 1 ELSE 0 END)         AS StillInProcess_Incomplete,
        SUM(CASE WHEN SuccessCount = 0 AND PendingCount > 0
                      AND LastPendingActivityUtc < DATEADD(MINUTE, -@minutes, SYSUTCDATETIME())
                 THEN 1 ELSE 0 END)                                                             AS StillInProcess_StuckPastThreshold
    FROM MsgAgg;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_TestEntraConnectivity
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'Entra Managed Identity connectivity test succeeded' AS Result, SUSER_SNAME() AS ConnectedAs, SYSUTCDATETIME() AS TimestampUtc;
END



-- ── Grants ──────────────────────────────────────────────────────────────────────
-- Grant to the login the deployed source's connection string authenticates as
-- (devops_warewolf for the current Azure SQL Database source -- see
-- Resources/rabbit/NewSqlServerSource.bite). CONNECT + db_datareader/db_datawriter cover
-- normal execution; EXECUTE + VIEW DEFINITION on schema dbo are required in addition --
-- see docs/ShovelBridge-Architecture.md's "connecting principal holds EXECUTE but not
-- VIEW DEFINITION" root-cause note (SQL error 15197 from sp_helptext otherwise).
IF DATABASE_PRINCIPAL_ID('devops_warewolf') IS NULL
    CREATE USER devops_warewolf FOR LOGIN devops_warewolf;
GO
ALTER ROLE db_datareader ADD MEMBER devops_warewolf;
ALTER ROLE db_datawriter ADD MEMBER devops_warewolf;
GRANT EXECUTE ON SCHEMA::dbo TO devops_warewolf;
GRANT VIEW DEFINITION ON SCHEMA::dbo TO devops_warewolf;
GO

-- ── Entra ID (Azure AD) login for the engine's own Managed Identity ─────────────
-- WOLF-8510: `DatabaseServiceExecution`/`MssqlSqlExecution` has the Lightweight engine
-- itself try to authenticate as its own Function App Managed Identity for an internal
-- sp_helptext/definition probe before falling back to the source's configured SQL auth
-- (devops_warewolf, above) -- see ShovelBridge-Architecture.md's "connecting principal
-- holds EXECUTE but not VIEW DEFINITION" / "WarewolfServer-UAT does not exist as a
-- database user" notes. Without this, that probe silently falls through every time.
-- This block requires running as the server's AAD admin (CREATE USER ... FROM EXTERNAL
-- PROVIDER cannot be run as a SQL login). Repeat for any other Function App (e.g. a
-- future non-UAT engine) that needs to reach this database, substituting its own
-- system-assigned Managed Identity's Function App name below.
IF DATABASE_PRINCIPAL_ID('WarewolfServer-UAT') IS NULL
    CREATE USER [WarewolfServer-UAT] FROM EXTERNAL PROVIDER;
GO
GRANT EXECUTE ON SCHEMA::dbo TO [WarewolfServer-UAT];
GRANT VIEW DEFINITION ON SCHEMA::dbo TO [WarewolfServer-UAT];
GO
