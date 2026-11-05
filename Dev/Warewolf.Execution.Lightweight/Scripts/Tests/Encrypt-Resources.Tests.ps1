<#
    Pester 5 suite for Encrypt-Resources.ps1.

        Invoke-Pester -Path ./Tests/Encrypt-Resources.Tests.ps1

    The script WFAES-encrypts the <Source> connection strings of a resources folder with key
    material passed in by the caller (CI/CD without az), into a separate output folder or in
    place, all or nothing. Runs offline: `az` is shadowed with a stub that only counts calls, so
    a test can prove the script never reaches it.

    Round-trips are checked with Encrypt-Config.ps1 -Decrypt -KVSecret, the product's own decrypt.
#>

BeforeAll {
    $scriptsDir = Split-Path $PSScriptRoot -Parent
    $script:ResourcesScript = Join-Path $scriptsDir 'Encrypt-Resources.ps1'
    $script:EncryptScript   = Join-Path $scriptsDir 'Encrypt-Config.ps1'

    $script:KeyB64      = [Convert]::ToBase64String([byte[]](1..32))
    $script:KeyMaterial =
        '{"version":1,"keyId":"11111111-2222-3333-4444-555555555555","key":"' +
        $script:KeyB64 + '","created":"2026-01-01T00:00:00Z"}'
    $script:OtherKeyMaterial =
        '{"version":1,"keyId":"99999999-8888-7777-6666-555555555555","key":"' +
        [Convert]::ToBase64String([byte[]](101..132)) + '","created":"2026-01-01T00:00:00Z"}'

    $script:DbConnectionString = 'Data Source=db01;User Id=sa;Password=Pa55word!'

    $global:AzCallCount = 0
    function global:az { $global:AzCallCount++; $global:LASTEXITCODE = 0; return '' }

    # A resources tree shaped like the ProfilerServices repo after token replacement.
    function script:New-Fixture {
        $root = Join-Path $TestDrive ([Guid]::NewGuid().ToString('N'))
        $src  = Join-Path $root 'src'
        $files = [ordered]@{
            # A source: its ConnectionString is the only value that gets encrypted.
            'Sources\Db.bite'      = "<Source ID=`"0f1c0a1e-0000-4000-8000-000000000001`" Name=`"Db`" ResourceType=`"SqlDatabase`" ConnectionString=`"$($script:DbConnectionString)`" Type=`"SqlDatabase`">`r`n  <DisplayName>Db</DisplayName>`r`n</Source>"
            # Globals: API keys in workflow FieldValue attributes stay as they are.
            'Sources\Globals.bite' = '<Service ID="0f1c0a1e-0000-4000-8000-000000000002" Name="Globals" ResourceType="WorkflowService"><Action><Variable FieldName="ApiKey" FieldValue="api-key-123" /></Action></Service>'
            # A workflow with a <Source> element but the ConnectionString on an activity: not a source value.
            'Flows\Nested\Main.bite' = '<Service ID="0f1c0a1e-0000-4000-8000-000000000003" Name="Main" ResourceType="WorkflowService"><Source ID="0f1c0a1e-0000-4000-8000-000000000004" /><Action Name="Sql" ConnectionString="Data Source=plain-activity" /></Service>'
            'Sources\auditingSettings.json' = '{"Payload":"{\"Host\":\"es01\",\"Port\":\"9200\"}"}'
            'Engine\RabbitMqBroker.txt'     = '{"Host":"mq01","UserName":"u","Password":"rabbit-pw"}'
            '.git\HEAD'                     = 'ref: refs/heads/develop'
        }
        foreach ($rel in $files.Keys) {
            $path = Join-Path $src $rel
            $null = New-Item -ItemType Directory -Force -Path (Split-Path $path -Parent)
            Set-Content -LiteralPath $path -Value $files[$rel] -Encoding UTF8 -NoNewline
        }
        $null = New-Item -ItemType Directory -Force -Path (Join-Path $src 'Empty')
        return [pscustomobject]@{ Root = $root; Source = $src; Output = (Join-Path $root 'out') }
    }

    # Relative path -> SHA256 for every file and folder (folders hash to 'dir').
    function script:Get-TreeState ([string]$dir) {
        $state = @{}
        if (-not (Test-Path -LiteralPath $dir)) { return $state }
        foreach ($i in Get-ChildItem -LiteralPath $dir -Recurse -Force) {
            $rel = [System.IO.Path]::GetRelativePath($dir, $i.FullName)
            $state[$rel] = $i.PSIsContainer ? 'dir' : (Get-FileHash -LiteralPath $i.FullName -Algorithm SHA256).Hash
        }
        return $state
    }

    function script:Get-SourceConnectionString ([string]$file) {
        ([xml](Get-Content -LiteralPath $file -Raw)).Source.ConnectionString
    }

    # Runs the script with every stream captured. ExitCode, the summary object, and all text.
    function script:Invoke-Resources {
        param([string] $Source, [string] $Output, [string] $KVSecret = $script:KeyMaterial, [switch] $NoSecret)
        $p = @{ SourceDirectory = $Source; OutputDirectory = $Output }
        if (-not $NoSecret) { $p['KVSecret'] = $KVSecret }
        $global:LASTEXITCODE = 0
        $all  = @(& $script:ResourcesScript @p *>&1)
        $code = $LASTEXITCODE
        [pscustomobject]@{
            ExitCode = $code
            Summary  = $all | Where-Object { $_.PSObject.Properties['SourcesEncrypted'] } | Select-Object -First 1
            Text     = $all | Out-String
        }
    }

    function script:Get-StagingFolders {
        @(Get-ChildItem -LiteralPath ([System.IO.Path]::GetTempPath()) -Filter 'ww-encrypt-resources-*' -Directory -ErrorAction SilentlyContinue)
    }
}

AfterAll {
    Remove-Item Function:\az -ErrorAction SilentlyContinue
    Remove-Item Env:\WW_KV_SECRET -ErrorAction SilentlyContinue
}

Describe 'Encrypt-Resources — separate output folder' {

    BeforeAll {
        $script:F      = script:New-Fixture
        $script:Before = script:Get-TreeState $script:F.Source
        $script:Run    = script:Invoke-Resources -Source $script:F.Source -Output $script:F.Output
        $script:After  = script:Get-TreeState $script:F.Output
    }

    It 'succeeds' {
        $script:Run.ExitCode | Should -Be 0 -Because $script:Run.Text
    }

    It 'writes every file and folder of the source, hidden ones and empty folders included' {
        ($script:After.Keys | Sort-Object) | Should -Be ($script:Before.Keys | Sort-Object)
    }

    It 'copies non-.bite files and .bite files without a source value byte for byte' {
        foreach ($rel in 'Sources\auditingSettings.json', 'Engine\RabbitMqBroker.txt', '.git\HEAD',
                         'Sources\Globals.bite', 'Flows\Nested\Main.bite') {
            $script:After[$rel] | Should -Be $script:Before[$rel] -Because "$rel must be copied unchanged"
        }
        Get-Content -LiteralPath (Join-Path $script:F.Output 'Flows\Nested\Main.bite') -Raw |
            Should -Match 'ConnectionString="Data Source=plain-activity"' -Because 'only <Source> values are encrypted'
    }

    It 'encrypts the Source ConnectionString, and it decrypts back to the original' {
        $outFile = Join-Path $script:F.Output 'Sources\Db.bite'
        script:Get-SourceConnectionString $outFile | Should -Match '^WFAES::'

        $plain = Join-Path $script:F.Root 'plain'
        $null = & $script:EncryptScript -FilePath $outFile -KVSecret $script:KeyMaterial -Decrypt -OutputDirectory $plain -NonInteractive 6>&1
        script:Get-SourceConnectionString (Join-Path $plain 'Db.bite') | Should -Be $script:DbConnectionString
    }

    It 'leaves the source folder untouched' {
        $now = script:Get-TreeState $script:F.Source
        ($now.Keys | Sort-Object) | Should -Be ($script:Before.Keys | Sort-Object)
        foreach ($k in $script:Before.Keys) { $now[$k] | Should -Be $script:Before[$k] -Because $k }
    }

    It 'returns a summary of what it did' {
        $s = $script:Run.Summary
        $s.Mode             | Should -Be 'Copy'
        $s.FilesCopied      | Should -Be 6
        $s.BiteFiles        | Should -Be 3
        $s.SourcesEncrypted | Should -Be 1
        $s.AlreadyEncrypted | Should -Be 0
        $s.Unchanged        | Should -Be 2
    }
}

Describe 'Encrypt-Resources — in place (output is the source folder)' {

    It 'encrypts the source file and leaves every other file untouched' {
        $f      = script:New-Fixture
        $before = script:Get-TreeState $f.Source

        $run = script:Invoke-Resources -Source $f.Source -Output $f.Source

        $run.ExitCode | Should -Be 0 -Because $run.Text
        script:Get-SourceConnectionString (Join-Path $f.Source 'Sources\Db.bite') | Should -Match '^WFAES::'
        $after = script:Get-TreeState $f.Source
        ($after.Keys | Sort-Object) | Should -Be ($before.Keys | Sort-Object)
        foreach ($k in $before.Keys | Where-Object { $_ -ne 'Sources\Db.bite' }) {
            $after[$k] | Should -Be $before[$k] -Because $k
        }
        $run.Summary.Mode        | Should -Be 'InPlace'
        $run.Summary.FilesCopied | Should -Be 1
    }
}

Describe 'Encrypt-Resources — re-running' {

    It 'succeeds and changes nothing when the input is already encrypted' {
        $f = script:New-Fixture
        $null = script:Invoke-Resources -Source $f.Source -Output $f.Output
        $once = script:Get-TreeState $f.Output

        $run = script:Invoke-Resources -Source $f.Output -Output $f.Output

        $run.ExitCode | Should -Be 0 -Because $run.Text
        $twice = script:Get-TreeState $f.Output
        foreach ($k in $once.Keys) { $twice[$k] | Should -Be $once[$k] -Because "$k must not be encrypted twice" }
        $run.Summary.SourcesEncrypted | Should -Be 0
        $run.Summary.AlreadyEncrypted | Should -Be 1
        $run.Summary.FilesCopied      | Should -Be 0
    }
}

Describe 'Encrypt-Resources — an output folder that already has files' {

    It 'overwrites files the source also has, and keeps files only the output has' {
        $f = script:New-Fixture
        $null = New-Item -ItemType Directory -Force -Path (Join-Path $f.Output 'Sources')
        Set-Content -LiteralPath (Join-Path $f.Output 'Sources\auditingSettings.json') -Value 'old' -NoNewline
        Set-Content -LiteralPath (Join-Path $f.Output 'stale.txt') -Value 'only in output' -NoNewline

        $run = script:Invoke-Resources -Source $f.Source -Output $f.Output

        $run.ExitCode | Should -Be 0 -Because $run.Text
        (Get-FileHash (Join-Path $f.Output 'Sources\auditingSettings.json')).Hash |
            Should -Be (Get-FileHash (Join-Path $f.Source 'Sources\auditingSettings.json')).Hash
        Get-Content -LiteralPath (Join-Path $f.Output 'stale.txt') -Raw | Should -Be 'only in output'
    }
}

Describe 'Encrypt-Resources — nested folders' {

    It 'rejects an output folder inside the source, writing nothing' {
        $f      = script:New-Fixture
        $before = script:Get-TreeState $f.Source
        $out    = Join-Path $f.Source 'Sources\encrypted'

        $run = script:Invoke-Resources -Source $f.Source -Output $out

        $run.ExitCode | Should -Be 1
        $run.Text     | Should -Match 'are nested'
        Test-Path -LiteralPath $out | Should -BeFalse
        (script:Get-TreeState $f.Source).Count | Should -Be $before.Count
    }

    It 'rejects a source folder inside the output' {
        $f   = script:New-Fixture
        $run = script:Invoke-Resources -Source (Join-Path $f.Source 'Sources') -Output $f.Source

        $run.ExitCode | Should -Be 1
        $run.Text     | Should -Match 'are nested'
    }
}

Describe 'Encrypt-Resources — key material' {

    AfterEach { Remove-Item Env:\WW_KV_SECRET -ErrorAction SilentlyContinue }

    It 'fails before writing anything when neither -KVSecret nor WW_KV_SECRET is set' {
        $f = script:New-Fixture
        Remove-Item Env:\WW_KV_SECRET -ErrorAction SilentlyContinue

        $run = script:Invoke-Resources -Source $f.Source -Output $f.Output -NoSecret

        $run.ExitCode | Should -Be 1
        $run.Text     | Should -Match 'No key material'
        Test-Path -LiteralPath $f.Output | Should -BeFalse
    }

    It 'reads the key material from WW_KV_SECRET when -KVSecret is omitted' {
        $f = script:New-Fixture
        $env:WW_KV_SECRET = $script:KeyMaterial

        $run = script:Invoke-Resources -Source $f.Source -Output $f.Output -NoSecret

        $run.ExitCode | Should -Be 0 -Because $run.Text
        script:Get-SourceConnectionString (Join-Path $f.Output 'Sources\Db.bite') | Should -Match '^WFAES::'
    }
}

Describe 'Encrypt-Resources — all or nothing' {

    It 'fails, changing neither folder, when a source is already encrypted under a different key' {
        $f = script:New-Fixture
        $null = & $script:EncryptScript -FilePath (Join-Path $f.Source 'Sources\Db.bite') -KVSecret $script:OtherKeyMaterial -NonInteractive -NoBackup 6>&1
        $before = script:Get-TreeState $f.Source

        $run = script:Invoke-Resources -Source $f.Source -Output $f.Output

        $run.ExitCode | Should -Be 1
        $run.Text     | Should -Match 'does not decrypt with this key'
        Test-Path -LiteralPath $f.Output | Should -BeFalse
        $after = script:Get-TreeState $f.Source
        foreach ($k in $before.Keys) { $after[$k] | Should -Be $before[$k] -Because $k }
    }

    It 'rolls the output back when a write fails partway' -Skip:(-not $IsWindows) {
        # A file opened with FileShare.None cannot be read or replaced on Windows. 'zz-locked.bite'
        # sorts last, so the JSON/.txt files and the other .bite files are written before it fails.
        $f = script:New-Fixture
        Set-Content -LiteralPath (Join-Path $f.Source 'zz-locked.bite') -Value '<Service Name="zz" />' -NoNewline
        $null = New-Item -ItemType Directory -Force -Path (Join-Path $f.Output 'Sources')
        Set-Content -LiteralPath (Join-Path $f.Output 'Sources\auditingSettings.json') -Value 'old' -NoNewline
        Set-Content -LiteralPath (Join-Path $f.Output 'zz-locked.bite') -Value 'locked' -NoNewline
        $before = script:Get-TreeState $f.Output

        $lock = [System.IO.File]::Open((Join-Path $f.Output 'zz-locked.bite'), 'Open', 'ReadWrite', 'None')
        try   { $run = script:Invoke-Resources -Source $f.Source -Output $f.Output }
        finally { $lock.Dispose() }

        $run.ExitCode | Should -Be 1
        $run.Text     | Should -Match 'Output restored'
        $after = script:Get-TreeState $f.Output
        ($after.Keys | Sort-Object) | Should -Be ($before.Keys | Sort-Object) -Because 'new files and folders are removed'
        foreach ($k in $before.Keys) { $after[$k] | Should -Be $before[$k] -Because "$k is restored" }
    }
}

Describe 'Encrypt-Resources — staging, secrecy and az' {

    BeforeEach { $global:AzCallCount = 0 }

    It 'removes its staging folder after a successful run' {
        $f      = script:New-Fixture
        $staged = (script:Get-StagingFolders).Count

        $run = script:Invoke-Resources -Source $f.Source -Output $f.Output

        $run.ExitCode | Should -Be 0 -Because $run.Text
        (script:Get-StagingFolders).Count | Should -Be $staged
    }

    It 'removes its staging folder after a failed run' {
        $f = script:New-Fixture
        $null = & $script:EncryptScript -FilePath (Join-Path $f.Source 'Sources\Db.bite') -KVSecret $script:OtherKeyMaterial -NonInteractive -NoBackup 6>&1
        $staged = (script:Get-StagingFolders).Count

        $run = script:Invoke-Resources -Source $f.Source -Output $f.Output

        $run.ExitCode | Should -Be 1
        (script:Get-StagingFolders).Count | Should -Be $staged
    }

    It 'never writes the key material to any output stream, and never calls az' {
        $f   = script:New-Fixture
        $run = script:Invoke-Resources -Source $f.Source -Output $f.Output

        $run.ExitCode | Should -Be 0 -Because $run.Text
        $run.Text | Should -Not -Match ([regex]::Escape($script:KeyB64))
        $run.Text | Should -Not -Match ([regex]::Escape($script:KeyMaterial))
        $global:AzCallCount | Should -Be 0
    }
}
