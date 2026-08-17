<#
    Pester 5 suite for Update-ReleaseNotes.ps1.

        Invoke-Pester -Path ./Update-ReleaseNotes.Tests.ps1

    Focus: the -QueueWorkerZipUrl / data-role="queueworker-zip-link" support added alongside
    the QueueProcessor release zip (see pipeline-CLOUD.yml's Release stage). Unlike
    engine-zip-link/scripts-zip-link, that anchor is NOT assumed to already exist on the live
    template - it has to be inserted the first time this script runs against a page that
    predates it, then simply replaced on every run after that. Both paths are covered here,
    plus that neither one disturbs the pre-existing engine/scripts behaviour.

    Operates entirely on local temp files - no FTP, no network - since the script itself only
    ever touches the $Path it is given.
#>

BeforeAll {
    $script:UpdateScript = Join-Path $PSScriptRoot 'Update-ReleaseNotes.ps1'

    function script:New-ReleaseNotesFixture {
        param([switch] $WithQueueWorkerLink)

        $queueWorkerAnchor = if ($WithQueueWorkerLink) {
            "`n    <a class=`"btn btn-primary`" data-role=`"queueworker-zip-link`" href=`"https://stwwreleases.blob.core.windows.net/releases/Warewolf-QueueProcessor-1.0.0.1.zip`">Download QueueProcessor</a>"
        } else { '' }

        $html = @"
<html><body>
<!-- RELEASE:LATEST:START -->
<article class="release-notes" data-version="1.0.0.1">
  <div class="container">
  <h2><!-- RELEASE:VERSION -->1.0.0.1<!-- /RELEASE:VERSION --></h2>
  <h3><!-- RELEASE:DATE -->2026/01/01<!-- /RELEASE:DATE --></h3>
  <!-- RELEASE:NOTES:START -->
  <ul class="baseline-large">
    <li>Old note</li>
  </ul>
  <!-- RELEASE:NOTES:END -->
  <p>
    <a class="btn btn-primary" data-role="engine-zip-link" href="https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-1.0.0.1.zip">Download Execution Zip</a>
    <a class="btn btn-primary" data-role="scripts-zip-link" href="https://stwwreleases.blob.core.windows.net/releases/Warewolf-ExecutionEngine-Deployment-Scripts-v1.0.zip">Download Scripts</a>$queueWorkerAnchor
  </p>
  </div>
</article>
<!-- RELEASE:LATEST:END -->
<!-- RELEASE:PREVIOUS:INSERT -->
</body></html>
"@
        $path = Join-Path ([System.IO.Path]::GetTempPath()) ("release-notes-" + [Guid]::NewGuid().ToString('N') + '.php')
        Set-Content -LiteralPath $path -Value $html -Encoding UTF8 -NoNewline
        return $path
    }

    function script:Invoke-UpdateReleaseNotes {
        param([string] $Path, [string] $NewVersion,
              [string] $EngineZipUrl = 'https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-2.0.0.1.zip',
              [string] $ScriptsZipUrl = 'https://stwwreleases.blob.core.windows.net/releases/Warewolf-ExecutionEngine-Deployment-Scripts-v1.0.zip',
              [string] $QueueWorkerZipUrl = 'https://stwwreleases.blob.core.windows.net/releases/Warewolf-QueueProcessor-2.0.0.1.zip')
        & $script:UpdateScript -Path $Path -NewVersion $NewVersion `
            -EngineZipUrl $EngineZipUrl -ScriptsZipUrl $ScriptsZipUrl -QueueWorkerZipUrl $QueueWorkerZipUrl
    }
}

Describe 'Update-ReleaseNotes - queueworker-zip-link' {

    It 'replaces the queueworker href in place when the anchor already exists, and archives the old one' {
        $path = script:New-ReleaseNotesFixture -WithQueueWorkerLink
        try {
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $content = Get-Content -LiteralPath $path -Raw

            $content | Should -Match 'data-role="queueworker-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/Warewolf-QueueProcessor-2\.0\.0\.1\.zip"'
            $content | Should -Not -Match 'Warewolf-QueueProcessor-1\.0\.0\.1\.zip"[^>]*data-role="queueworker-zip-link"'

            # Archived block for the superseded 1.0.0.1 release keeps its own (old) queueworker link.
            $content | Should -Match 'data-version="1\.0\.0\.1"[\s\S]*?Download QueueProcessor</a>[\s\S]*?</article>'
            $content | Should -Match 'href="https://stwwreleases\.blob\.core\.windows\.net/releases/Warewolf-QueueProcessor-1\.0\.0\.1\.zip">Download QueueProcessor</a>'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'inserts a new queueworker-zip-link anchor, styled like the scripts-zip-link anchor, when absent' {
        $path = script:New-ReleaseNotesFixture
        try {
            $before = Get-Content -LiteralPath $path -Raw
            $before | Should -Not -Match 'queueworker-zip-link'

            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $content = Get-Content -LiteralPath $path -Raw

            $content | Should -Match 'data-role="queueworker-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/Warewolf-QueueProcessor-2\.0\.0\.1\.zip"'
            # Cloned from the scripts-zip-link anchor, so it carries the same class list.
            $content | Should -Match '<a class="btn btn-primary" data-role="queueworker-zip-link"'

            # The superseded release never had one, so nothing was fabricated for it.
            $content | Should -Match 'data-version="1\.0\.0\.1"[\s\S]*?</article>'
            ($content | Select-String -Pattern 'Download QueueProcessor</a>' -AllMatches).Matches.Count | Should -Be 1
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'leaves the engine and scripts links working exactly as before, unaffected by the new link' {
        $path = script:New-ReleaseNotesFixture
        try {
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $content = Get-Content -LiteralPath $path -Raw

            $content | Should -Match 'data-role="engine-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/AzureFunctionsPackage-2\.0\.0\.1\.zip"'
            $content | Should -Match 'data-role="scripts-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/Warewolf-ExecutionEngine-Deployment-Scripts-v1\.0\.zip"'
            $content | Should -Match '<!-- RELEASE:VERSION -->2\.0\.0\.1<!-- /RELEASE:VERSION -->'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'is idempotent - re-running with the same version that is already latest changes nothing' {
        $path = script:New-ReleaseNotesFixture -WithQueueWorkerLink
        try {
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $once = Get-Content -LiteralPath $path -Raw

            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $twice = Get-Content -LiteralPath $path -Raw

            $twice | Should -Be $once
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }
}
