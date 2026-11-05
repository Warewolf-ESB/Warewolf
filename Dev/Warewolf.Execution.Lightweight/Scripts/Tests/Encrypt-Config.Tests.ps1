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

    Also: -KVSecret, where the caller supplies the key material (CI/CD without az) and Key Vault
    is never contacted.

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

    # Counted so the -KVSecret tests can prove az is never called.
    $global:AzCallCount = 0
    function global:az {
        $global:AzCallCount++
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

    # -KVSecret replaces -VaultName/-SecretName. The script's exit code is left in $LASTEXITCODE.
    function script:Invoke-Encrypt {
        param([string] $Path, [switch] $WholeFile, [switch] $VerifyOnly, [switch] $Decrypt, [string] $OutputDirectory,
              [string] $KVSecret, [switch] $GenerateKeys)
        $p = @{ FilePath = $Path; NonInteractive = $true; NoBackup = $true }
        if ($KVSecret)   { $p['KVSecret'] = $KVSecret } else { $p['VaultName'] = 'kv'; $p['SecretName'] = 'sec' }
        if ($WholeFile)  { $p['WholeFile']  = $true }
        if ($VerifyOnly) { $p['VerifyOnly'] = $true }
        if ($Decrypt)    { $p['Decrypt']    = $true }
        if ($GenerateKeys) { $p['GenerateKeys'] = $true }
        if ($OutputDirectory) { $p['OutputDirectory'] = $OutputDirectory }
        $global:LASTEXITCODE = 0
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

Describe 'Encrypt-Config -KVSecret — key material from the caller, no Key Vault' {
    # For CI/CD steps that cannot run az: the key arrives as a pipeline secret variable. Every
    # test asserts the az stub was never reached.

    BeforeAll {
        $script:OtherKeyMaterial =
            '{"version":1,"keyId":"99999999-8888-7777-6666-555555555555","key":"' +
            [Convert]::ToBase64String([byte[]](101..132)) + '","created":"2026-01-01T00:00:00Z"}'
    }

    BeforeEach { $global:AzCallCount = 0 }

    It 'encrypts a source ConnectionString to WFAES:: without calling az' {
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'sources\s1.bite'
            $null = script:Invoke-Encrypt -Path $file -KVSecret $global:FakeKeyMaterial

            $LASTEXITCODE | Should -Be 0
            ([xml](Get-Content -LiteralPath $file -Raw)).Source.ConnectionString | Should -Match '^WFAES::'
            $global:AzCallCount | Should -Be 0
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'round-trips through -Decrypt with the same key' {
        $dir = script:New-Sandbox
        try {
            $file   = Join-Path $dir 'sources\s1.bite'
            $before = ([xml](Get-Content -LiteralPath $file -Raw)).Source.ConnectionString
            $null = script:Invoke-Encrypt -Path $file -KVSecret $global:FakeKeyMaterial

            $out = Join-Path $dir 'plain'
            $null = script:Invoke-Encrypt -Path $file -KVSecret $global:FakeKeyMaterial -Decrypt -OutputDirectory $out

            $LASTEXITCODE | Should -Be 0
            ([xml](Get-Content -LiteralPath (Join-Path $out 's1.bite') -Raw)).Source.ConnectionString | Should -Be $before
            $global:AzCallCount | Should -Be 0
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It '-VerifyOnly passes with the key that encrypted, and fails with any other key' {
        $dir = script:New-Sandbox
        try {
            $file = Join-Path $dir 'sources\s1.bite'
            $null = script:Invoke-Encrypt -Path $file -KVSecret $global:FakeKeyMaterial

            $ok = script:Invoke-Encrypt -Path $file -KVSecret $global:FakeKeyMaterial -VerifyOnly
            $LASTEXITCODE | Should -Be 0
            $ok | Should -Match 'Decrypt OK \(in-memory\)\s*:\s*1'

            $bad = script:Invoke-Encrypt -Path $file -KVSecret $script:OtherKeyMaterial -VerifyOnly
            $LASTEXITCODE | Should -Be 1
            $bad | Should -Match 'Verify FAILED'
            $global:AzCallCount | Should -Be 0
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'round-trips a JSON trigger in -WholeFile mode' {
        $dir = script:New-Sandbox
        try {
            $file   = Join-Path $dir 'triggers\t1.bite'
            $before = (Get-Content -LiteralPath $file -Raw).Trim()
            $null = script:Invoke-Encrypt -Path $file -WholeFile -KVSecret $global:FakeKeyMaterial
            (Get-Content -LiteralPath $file -Raw).Trim() | Should -Match '^WFAES::'

            $out = Join-Path $dir 'plain'
            $null = script:Invoke-Encrypt -Path $file -WholeFile -Decrypt -OutputDirectory $out -KVSecret $global:FakeKeyMaterial

            (Get-Content -LiteralPath (Join-Path $out 't1.bite') -Raw).Trim() | Should -Be $before
            $global:AzCallCount | Should -Be 0
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'rejects -GenerateKeys, which would have to store the new key in Key Vault' {
        $dir = script:New-Sandbox
        try {
            $file   = Join-Path $dir 'sources\s1.bite'
            $before = Get-Content -LiteralPath $file -Raw
            $out = script:Invoke-Encrypt -Path $file -KVSecret $global:FakeKeyMaterial -GenerateKeys

            $LASTEXITCODE | Should -Be 1
            $out | Should -Match '-KVSecret and -GenerateKeys cannot be used together'
            Get-Content -LiteralPath $file -Raw | Should -Be $before
            $global:AzCallCount | Should -Be 0
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'fails without echoing the secret when it is <Case>' -ForEach @(
        @{ Case = 'not JSON';          Secret = 'not-json-key-material-s3cr3t' }
        @{ Case = 'a key of 16 bytes'; Secret = '{"version":1,"keyId":"k","key":"' + [Convert]::ToBase64String([byte[]](1..16)) + '","created":"c"}' }
    ) {
        $dir = script:New-Sandbox
        try {
            $file   = Join-Path $dir 'sources\s1.bite'
            $before = Get-Content -LiteralPath $file -Raw
            $out = script:Invoke-Encrypt -Path $file -KVSecret $Secret

            $LASTEXITCODE | Should -Be 1
            $out | Should -Not -Match ([regex]::Escape($Secret))
            Get-Content -LiteralPath $file -Raw | Should -Be $before
            $global:AzCallCount | Should -Be 0
        }
        finally { Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue }
    }
}
