#Requires -Version 7.0
<#
    Pester 5 integration suite for Profiler.usp_ScoreMandateCollectionDates, as defined by
    Resources/ProfilerServices/Provision-ProfilerSchema.sql.

    Runs the REAL provisioning script and the REAL procedure against a throwaway SQL Server in Docker
    (the container is removed afterwards). Skipped when Docker is unavailable.

    The invariant: scoring a result writes exactly one Profiler.MandateScores row per
    (ProfileId, DebitRunId), even when the [ResultPresentedOnDate, ResultDate] window holds several
    ProfileDates for that profile and debit run - which happens once the window spans two payment
    cycles (CECB-2050 removed the old same-cycle filter). Before the fix the first MERGE inserted one
    row per ProfileDate and failed on PK_MandateProfileScores (SQL error 2627), or, when the row
    already existed, on error 8672. UAT load-test build 31052 dead-lettered 77 of 20000 messages that
    way (LastDay-ClearsInTwoBankDays, DebitRunId 31, Sep 2 and Oct 2).

    The business rule: a Success scores +1 when ANY candidate ProfileDate equals ResultDate, otherwise
    -1; a non-Success scores -1. Candidates are the ProfileDates in [ResultPresentedOnDate, ResultDate],
    so ResultDate can only ever equal the LATEST of them - there is no "ResultDate is the earlier
    candidate" case to test, and "earliest wins" would score a correct Success as -1.

    All fixtures use 2099 dates and payment cycles 2099xx so they cannot collide with seeded data.
#>

BeforeDiscovery {
    $script:DockerAvailable = $false
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker info --format '{{.ServerVersion}}' 2>&1 | Out-Null
        $script:DockerAvailable = ($LASTEXITCODE -eq 0)
    }
}

BeforeAll {
    # BeforeDiscovery variables do not exist in the run phase, so detect again here.
    $script:DockerAvailable = $false
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker info --format '{{.ServerVersion}}' 2>&1 | Out-Null
        $script:DockerAvailable = ($LASTEXITCODE -eq 0)
    }
    $script:Image     = if ($env:WW_SCORING_TEST_SQL_IMAGE) { $env:WW_SCORING_TEST_SQL_IMAGE } else { 'mcr.microsoft.com/mssql/server:2022-latest' }
    $script:Container = "ww-scoring-test-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
    # Random, policy-compliant (upper, lower, digit, symbol), never logged or persisted.
    $script:SaPassword = 'Aa1!' + [guid]::NewGuid().ToString('N')
    $script:SchemaPath = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) 'Resources/ProfilerServices/Provision-ProfilerSchema.sql'

    function Invoke-ScoringSql {
        param([Parameter(Mandatory)][string] $Query, [string] $Database = 'ScoringTest')
        $out = & docker exec $script:Container /opt/mssql-tools18/bin/sqlcmd -C `
            -S localhost -U sa -P $script:SaPassword -d $Database -b -h -1 -W -s '|' -Q $Query 2>&1
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed (exit $LASTEXITCODE) for [$Query]: $($out -join [Environment]::NewLine)" }
        @($out | ForEach-Object { "$_".Trim() } | Where-Object { $_ })
    }

    # Runs the procedure and returns 'OK' or 'ERR<number>' - the procedure's own error never escapes.
    function Invoke-Score {
        param(
            [Parameter(Mandatory)][string] $MandateId,
            [Parameter(Mandatory)][int]    $PaymentCycle,
            [Parameter(Mandatory)][string] $PresentedOn,
            [Parameter(Mandatory)][string] $ResultDate,
            [string] $ResultType = 'Success'
        )
        $q = "SET NOCOUNT ON; BEGIN TRY EXEC Profiler.usp_ScoreMandateCollectionDates " +
             "@MandateId='$MandateId', @ResultPaymentCycle=$PaymentCycle, @ResultPresentedOnDate='$PresentedOn', " +
             "@ResultDate='$ResultDate', @ResultType='$ResultType', @ResultMethod='DebiCheck', @MandateCanTrack=1; " +
             "SELECT 'OK'; END TRY BEGIN CATCH SELECT 'ERR' + CAST(ERROR_NUMBER() AS VARCHAR(10)); END CATCH"
        @(Invoke-ScoringSql -Query $q)[-1]
    }

    # 'Profile:DRI:Score' rows for one mandate, e.g. 'LastDay-ClearsInTwoBankDays:31:1'.
    function Get-Scores {
        param([Parameter(Mandatory)][string] $MandateId)
        @(Invoke-ScoringSql -Query ("SET NOCOUNT ON; SELECT ProfileId + ':' + CAST(DebitRunId AS VARCHAR(3)) + ':' + CAST(Score AS VARCHAR(5)) " +
            "FROM Profiler.MandateScores WHERE MandateId='$MandateId' ORDER BY ProfileId, DebitRunId"))
    }

    if ($script:DockerAvailable) {
        docker run -d --name $script:Container -e 'ACCEPT_EULA=Y' -e "MSSQL_SA_PASSWORD=$($script:SaPassword)" $script:Image | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "docker run failed for $($script:Image) (exit $LASTEXITCODE)." }

        $ready = $false
        foreach ($i in 1..60) {
            docker exec $script:Container /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P $script:SaPassword -Q 'SELECT 1' 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) { $ready = $true; break }
            Start-Sleep -Seconds 2
        }
        if (-not $ready) { throw 'SQL Server did not become ready within 120s.' }

        Invoke-ScoringSql -Database master -Query 'CREATE DATABASE ScoringTest' | Out-Null
        docker cp $script:SchemaPath "$($script:Container):/tmp/Provision-ProfilerSchema.sql" | Out-Null
        $prov = & docker exec $script:Container /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P $script:SaPassword `
            -d ScoringTest -b -i /tmp/Provision-ProfilerSchema.sql 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Provision-ProfilerSchema.sql failed (exit $LASTEXITCODE): $($prov -join [Environment]::NewLine)" }

        # Fixtures. The profile and debit-run rows are inserted only if the seed did not already.
        # Cycle 209901 / 209902 hold the straddling pair (the Sep 2 / Oct 2 shape from build 31052);
        # 209903 holds a single Weekday date for the one-candidate regression cases.
        Invoke-ScoringSql -Query @"
IF NOT EXISTS (SELECT 1 FROM Profiler.Profiles WHERE ProfileId='LastDay-ClearsInTwoBankDays') INSERT Profiler.Profiles (ProfileId, Description) VALUES ('LastDay-ClearsInTwoBankDays', 'fixture');
IF NOT EXISTS (SELECT 1 FROM Profiler.Profiles WHERE ProfileId='Weekday') INSERT Profiler.Profiles (ProfileId, Description) VALUES ('Weekday', 'fixture');
IF NOT EXISTS (SELECT 1 FROM Profiler.DebitRunIds WHERE DebitRunId=31) INSERT Profiler.DebitRunIds (DebitRunId, Description) VALUES (31, 'fixture');
IF NOT EXISTS (SELECT 1 FROM Profiler.DebitRunIds WHERE DebitRunId=1) INSERT Profiler.DebitRunIds (DebitRunId, Description) VALUES (1, 'fixture');
INSERT Profiler.ProfileDates (CollectionDate, DebitRunId, PaymentCycle, ProfileId) VALUES
  ('2099-01-04', 31, 209901, 'LastDay-ClearsInTwoBankDays'),
  ('2099-02-02', 31, 209902, 'LastDay-ClearsInTwoBankDays'),
  ('2099-03-03',  1, 209903, 'Weekday');
"@ | Out-Null
    }

    # Distinct mandate per case so cases are independent and order-free.
    $script:M = @{
        Later    = '00000000-0000-0000-0000-000000000001'
        Neither  = '00000000-0000-0000-0000-000000000003'
        Failure  = '00000000-0000-0000-0000-000000000004'
        SingleHit  = '00000000-0000-0000-0000-000000000005'
        SingleMiss = '00000000-0000-0000-0000-000000000006'
        Existing = '00000000-0000-0000-0000-000000000007'
        Covered  = '00000000-0000-0000-0000-000000000008'
    }
}

AfterAll {
    if ($script:Container) { docker rm -f $script:Container 2>&1 | Out-Null }
}

Describe 'usp_ScoreMandateCollectionDates - two candidate dates for one profile and debit run' -Skip:(-not $script:DockerAvailable) {

    It 'scores +1 once when ResultDate is the LATER candidate date, with no error' {
        Invoke-Score -MandateId $script:M.Later -PaymentCycle 209901 -PresentedOn '2099-01-04' -ResultDate '2099-02-02' | Should -Be 'OK'
        Get-Scores -MandateId $script:M.Later | Should -Be @('LastDay-ClearsInTwoBankDays:31:1')
    }

    It 'scores -1 once when a Success ResultDate matches neither candidate date' {
        Invoke-Score -MandateId $script:M.Neither -PaymentCycle 209901 -PresentedOn '2099-01-04' -ResultDate '2099-02-03' | Should -Be 'OK'
        Get-Scores -MandateId $script:M.Neither | Should -Be @('LastDay-ClearsInTwoBankDays:31:-1')
    }

    It 'scores -1 once for a non-Success result, even when ResultDate equals a candidate date' {
        Invoke-Score -MandateId $script:M.Failure -PaymentCycle 209901 -PresentedOn '2099-01-04' -ResultDate '2099-02-02' -ResultType 'Failure' | Should -Be 'OK'
        Get-Scores -MandateId $script:M.Failure | Should -Be @('LastDay-ClearsInTwoBankDays:31:-1')
    }

    It 'updates an existing score exactly once (no SQL error 8672) when two candidate dates match its key' {
        # Prior-cycle row with score 2: the MATCHED branch must see ONE source row and add 1.
        Invoke-ScoringSql -Query ("INSERT Profiler.MandateScores (MandateId, ProfileId, DebitRunId, PaymentCycle, ScoredOn, Score, ScoreAdjustedOn, ScoreAdjustmentReason) " +
            "VALUES ('$($script:M.Existing)', 'LastDay-ClearsInTwoBankDays', 31, 209812, GETDATE(), 2, GETDATE(), 'fixture')") | Out-Null
        Invoke-Score -MandateId $script:M.Existing -PaymentCycle 209901 -PresentedOn '2099-01-04' -ResultDate '2099-02-02' | Should -Be 'OK'
        Get-Scores -MandateId $script:M.Existing | Should -Be @('LastDay-ClearsInTwoBankDays:31:3')
    }
}

Describe 'usp_ScoreMandateCollectionDates - behaviour that must not change' -Skip:(-not $script:DockerAvailable) {

    It 'one candidate date, ResultDate equal to it: scores +1' {
        Invoke-Score -MandateId $script:M.SingleHit -PaymentCycle 209903 -PresentedOn '2099-03-03' -ResultDate '2099-03-03' | Should -Be 'OK'
        Get-Scores -MandateId $script:M.SingleHit | Should -Contain 'Weekday:1:1'
    }

    It 'one candidate date, ResultDate later than it: scores -1' {
        Invoke-Score -MandateId $script:M.SingleMiss -PaymentCycle 209903 -PresentedOn '2099-03-03' -ResultDate '2099-03-05' | Should -Be 'OK'
        Get-Scores -MandateId $script:M.SingleMiss | Should -Contain 'Weekday:1:-1'
    }

    It 'a fully scored debit run still gets its FullyCovered row at -1 (second MERGE untouched)' {
        # Weekday DRI 1 has a single 209903 date; a miss scores it -1, so ScoredCount = ProfileDateCount
        # and the FullyCovered MERGE inserts a -1 row for that debit run.
        Invoke-Score -MandateId $script:M.Covered -PaymentCycle 209903 -PresentedOn '2099-03-03' -ResultDate '2099-03-05' | Should -Be 'OK'
        Get-Scores -MandateId $script:M.Covered | Should -Be @('FullyCovered:1:-1', 'Weekday:1:-1')
    }
}
