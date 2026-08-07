<#
    Pester 5 suite for Encrypt-Config.ps1.

        Invoke-Pester -Path ./Tests/Encrypt-Config.Tests.ps1

    Focus: the -WholeFile mode added for QUEUE-TRIGGER files.

    Why it exists: the default (attribute) mode rewrites the ConnectionString attribute of a
    <Source> element. A queue trigger is JSON with no <Source> element, so attribute mode
    SILENTLY SKIPS it — which would leave the trigger (including its stored UserName/Password)
    plaintext inside the container image, and would leave a whole-file DPAPI-encrypted trigger
    unreadable on Linux. -WholeFile produces the same WFAES:: payload the worker's
    TriggerBiteReader already decrypts, under the same Key Vault key as the sources.

    `az` is shadowed with a function so the suite runs offline: no Key Vault, no login. Functions
    win over applications in PowerShell's command resolution, so the script under test calls the
    stub without knowing.
#>

BeforeAll {
    $script:EncryptScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Encrypt-Config.ps1'

    # Deterministic 32-byte AES key, so encrypt/decrypt round-trips inside one run.
    # $global:, not $script: — the stub below is invoked from INSIDE Encrypt-Config.ps1, where
    # $script: resolves to that script's scope, not this test file's.
    $global:FakeKeyB64 = [Convert]::ToBase64String([byte[]](1..32))
    $global:FakeKeyMaterial =
        '{"version":1,"keyId":"11111111-2222-3333-4444-555555555555","key":"' +
        $global:FakeKeyB64 + '","created":"2026-01-01T00:00:00Z"}'

    function global:az {
        $joined = $args -join ' '
        $global:LASTEXITCODE = 0
        if ($joined -match 'account show')        { return '{"user":{"name":"test@example.com"},"name":"Test Sub","id":"sub-1"}' }
        if ($joined -match 'keyvault secret show'){ return $global:FakeKeyMaterial }
        if ($joined -match 'keyvault secret set') { return $global:FakeKeyMaterial }
        return ''
    }

    $script:TriggerJson = @'
{
    "$id": "1",
    "$type": "Warewolf.Trigger.Queue.TriggerQueue, Warewolf.Trigger.Queue",
    "TriggerId": "03fb9052-7fe4-4e8b-ac18-53779b0ebcba",
    "Name": "OrderQueue",
    "QueueName": "order-success-queue",
    "UserName": "wwengineworker",
    "Password": "Test@123"
}
'@

    $script:SourceXml = @'
<Source ID="fa5f49d7-f6d7-422c-b08e-17b094d82f1d" Name="Src" ResourceType="RabbitMQSource" ConnectionString="HostName=h;Port=5672;UserName=u;Password=p;VirtualHost=/" Type="RabbitMQSource">
  <DisplayName>Src</DisplayName>
</Source>
'@

    function script:New-Sandbox {
        $dir = Join-Path ([System.IO.Path]::GetTempPath()) ("encfg-" + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Force -Path (Join-Path $dir 'triggers') | Out-Null
        New-Item -ItemType Directory -Force -Path (Join-Path $dir 'sources')  | Out-Null
        Set-Content -LiteralPath (Join-Path $dir 'triggers\t1.bite') -Value $script:TriggerJson -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $dir 'sources\s1.bite')  -Value $script:SourceXml   -Encoding UTF8 -NoNewline
        return $dir
    }

    function script:Invoke-Encrypt {
        param([string] $Path, [switch] $WholeFile, [switch] $VerifyOnly, [switch] $Decrypt, [string] $OutputDirectory)
        $p = @{ FilePath = $Path; VaultName = 'kv'; SecretName = 'sec'; NonInteractive = $true; NoBackup = $true }
        if ($WholeFile)  { $p['WholeFile']  = $true }
        if ($VerifyOnly) { $p['VerifyOnly'] = $true }
        if ($Decrypt)    { $p['Decrypt']    = $true }
        if ($OutputDirectory) { $p['OutputDirectory'] = $OutputDirectory }
        # 6>&1 redirects the INFORMATION stream: the script reports through Write-Host, which
        # does not flow to stdout, so `2>&1` alone captures nothing.
        & $script:EncryptScript @p 6>&1 2>&1 | Out-String
    }
}

AfterAll { Remove-Item Function:\az -ErrorAction SilentlyContinue }

Describe 'Encrypt-Config -WholeFile — trigger files' {

    It 'encrypts a JSON trigger to a WFAES:: payload, hiding the stored credentials' {
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'triggers\t1.bite'
            $null = script:Invoke-Encrypt -Path $file -WholeFile

            $body = (Get-Content -LiteralPath $file -Raw).Trim()
            $body | Should -Match '^WFAES::'
            $body | Should -Not -Match 'Test@123' -Because 'the stored password must not survive in the image'
            $body | Should -Not -Match 'order-success-queue'
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'round-trips back to the exact original JSON' {
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'triggers\t1.bite'
            $before = (Get-Content -LiteralPath $file -Raw).Trim()
            $null = script:Invoke-Encrypt -Path $file -WholeFile

            $out = Join-Path $dir 'plain'
            $null = script:Invoke-Encrypt -Path $file -WholeFile -Decrypt -OutputDirectory $out

            $restored = (Get-Content -LiteralPath (Join-Path $out 't1.bite') -Raw).Trim()
            $restored | Should -Be $before
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'is idempotent — a second run skips instead of double-encrypting' {
        # Re-running a deploy must not wrap the payload twice, which would make it undecryptable
        # by a single unwrap and fail at cold start.
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'triggers\t1.bite'
            $null  = script:Invoke-Encrypt -Path $file -WholeFile
            $once  = (Get-Content -LiteralPath $file -Raw).Trim()
            $out2  = script:Invoke-Encrypt -Path $file -WholeFile
            $twice = (Get-Content -LiteralPath $file -Raw).Trim()

            $twice | Should -Be $once
            $out2  | Should -Match 'already WFAES'
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'reports success from -VerifyOnly only after actually verifying the payload' {
        # The trap this guards: without a whole-file branch, -VerifyOnly would skip every trigger
        # and still report success — verifying nothing.
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'triggers\t1.bite'
            $null = script:Invoke-Encrypt -Path $file -WholeFile
            $before = (Get-Content -LiteralPath $file -Raw).Trim()

            $out = script:Invoke-Encrypt -Path $file -WholeFile -VerifyOnly

            $out | Should -Match 'OK \(whole file\)'
            $out | Should -Match 'Files with encrypted values\s*:\s*1'
            (Get-Content -LiteralPath $file -Raw).Trim() | Should -Be $before -Because '-VerifyOnly must not write'
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'detects tampering, because the payload is authenticated (AES-GCM)' {
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'triggers\t1.bite'
            $null = script:Invoke-Encrypt -Path $file -WholeFile

            $body = (Get-Content -LiteralPath $file -Raw).Trim()
            $b64  = $body.Substring('WFAES::'.Length)
            $data = [Convert]::FromBase64String($b64)
            $data[$data.Length - 1] = $data[$data.Length - 1] -bxor 0xFF     # flip a tag bit
            Set-Content -LiteralPath $file -Value ('WFAES::' + [Convert]::ToBase64String($data)) -Encoding UTF8 -NoNewline

            $out = script:Invoke-Encrypt -Path $file -WholeFile -VerifyOnly
            $out | Should -Match 'FAIL \(whole file\)'
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

Describe 'Encrypt-Config — attribute mode is unchanged' {

    It 'encrypts the ConnectionString of an XML source' {
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'sources\s1.bite'
            $null = script:Invoke-Encrypt -Path $file

            $cs = ([xml](Get-Content -LiteralPath $file -Raw)).Source.ConnectionString
            $cs | Should -Match '^WFAES::'
            (Get-Content -LiteralPath $file -Raw) | Should -Match '<Source' -Because 'the XML structure must survive'
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'SKIPS a JSON trigger — the reason -WholeFile had to exist' {
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'triggers\t1.bite'
            $out  = script:Invoke-Encrypt -Path $file          # no -WholeFile

            $out | Should -Match 'SKIP \(no <Source ConnectionString'
            (Get-Content -LiteralPath $file -Raw) | Should -Match 'Test@123' `
                -Because 'attribute mode leaves the trigger plaintext, which is the defect -WholeFile fixes'
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'handles a folder containing exactly ONE .bite file' {
        # Regression: under Set-StrictMode -Version Latest, Get-ChildItem returns a bare [string]
        # for a single match and $files.Count threw
        # "The property 'Count' cannot be found on this object."
        # A one-source folder is the normal case for a per-trigger staging tree.
        $dir = script:New-Sandbox
        try {
            $out = script:Invoke-Encrypt -Path (Join-Path $dir 'sources')

            $out | Should -Match 'Found 1 \.bite file'
            $out | Should -Not -Match "property 'Count' cannot be found"
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }
}
