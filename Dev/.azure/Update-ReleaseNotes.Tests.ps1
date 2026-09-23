<#
    Pester 5 suite for Update-ReleaseNotes.ps1.

        Invoke-Pester -Path ./Update-ReleaseNotes.Tests.ps1

    Focus: the linux/windows engine-zip split (engine-linux-zip-link / engine-windows-zip-link,
    converted from the legacy engine-zip-link on first run) and the -QueueWorkerZipUrl / data-role="queueworker-zip-link" support added alongside
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
        # -EngineLayout: 'Legacy' = one pre-split engine-zip-link anchor (what the live template
        # carried before the linux/windows split); 'Split' = engine-linux-zip-link +
        # engine-windows-zip-link, i.e. a page this script has already converted.
        param([switch] $WithQueueWorkerLink, [ValidateSet('Legacy', 'Split')] [string] $EngineLayout = 'Legacy')

        $queueWorkerAnchor = if ($WithQueueWorkerLink) {
            "`n    <a class=`"btn btn-primary`" data-role=`"queueworker-zip-link`" href=`"https://stwwreleases.blob.core.windows.net/releases/Warewolf-QueueProcessor-1.0.0.1.zip`">Download QueueProcessor</a>"
        } else { '' }

        $engineAnchors = if ($EngineLayout -eq 'Split') {
            "<a class=`"btn btn-primary`" data-role=`"engine-linux-zip-link`" href=`"https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-linux-x64-1.0.0.1.zip`">Download Execution Zip (Linux)</a>`n    " +
            "<a class=`"btn btn-primary`" data-role=`"engine-windows-zip-link`" href=`"https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-win-x64-1.0.0.1.zip`">Download Execution Zip (Windows)</a>"
        } else {
            "<a class=`"btn btn-primary`" data-role=`"engine-zip-link`" href=`"https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-1.0.0.1.zip`">Download Execution Zip</a>"
        }

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
    $engineAnchors
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
              [string] $EngineLinuxZipUrl = 'https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-linux-x64-2.0.0.1.zip',
              [string] $EngineWindowsZipUrl = 'https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-win-x64-2.0.0.1.zip',
              [string] $ScriptsZipUrl = 'https://stwwreleases.blob.core.windows.net/releases/Warewolf-ExecutionEngine-Deployment-Scripts-v1.0.zip',
              [string] $QueueWorkerZipUrl = 'https://stwwreleases.blob.core.windows.net/releases/Warewolf-QueueProcessor-2.0.0.1.zip')
        & $script:UpdateScript -Path $Path -NewVersion $NewVersion `
            -EngineLinuxZipUrl $EngineLinuxZipUrl -EngineWindowsZipUrl $EngineWindowsZipUrl -ScriptsZipUrl $ScriptsZipUrl -QueueWorkerZipUrl $QueueWorkerZipUrl
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

            $content | Should -Match 'data-role="engine-linux-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/AzureFunctionsPackage-linux-x64-2\.0\.0\.1\.zip"'
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

Describe 'Update-ReleaseNotes - engine linux/windows links' {

    It 'converts a legacy engine-zip-link into engine-linux-zip-link and inserts engine-windows-zip-link after it' {
        $path = script:New-ReleaseNotesFixture
        try {
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $content = Get-Content -LiteralPath $path -Raw

            $content | Should -Not -Match 'data-role="engine-zip-link"'
            $content | Should -Match '<a class="btn btn-primary" data-role="engine-linux-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/AzureFunctionsPackage-linux-x64-2\.0\.0\.1\.zip">Download Execution Zip \(Linux\)</a>'
            # Cloned from the linux anchor (same class list) and placed straight after it.
            $content | Should -Match 'data-role="engine-linux-zip-link"[^>]*>[^<]*</a>\s*<a class="btn btn-primary" data-role="engine-windows-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/AzureFunctionsPackage-win-x64-2\.0\.0\.1\.zip">Download Execution Zip \(Windows\)</a>'
            ($content | Select-String -Pattern 'data-role="engine-windows-zip-link"' -AllMatches).Matches.Count | Should -Be 1
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'archives a pre-split release with its single unlabelled engine link and fabricates no windows link' {
        $path = script:New-ReleaseNotesFixture
        try {
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $content = Get-Content -LiteralPath $path -Raw

            $archive = [regex]::Match($content.Substring($content.IndexOf('<!-- RELEASE:PREVIOUS:INSERT -->')), '<article class="release-notes" data-version="1\.0\.0\.1">\s*<div[\s\S]*?</article>').Value
            $archive | Should -Match 'href="https://stwwreleases\.blob\.core\.windows\.net/releases/AzureFunctionsPackage-1\.0\.0\.1\.zip">Download Execution Zip</a>'
            $archive | Should -Not -Match '\(Windows\)'
            $archive | Should -Not -Match 'data-role='
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'replaces both hrefs in place on an already-split page and archives both old links' {
        $path = script:New-ReleaseNotesFixture -EngineLayout Split
        try {
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            $content = Get-Content -LiteralPath $path -Raw

            $content | Should -Match 'data-role="engine-linux-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/AzureFunctionsPackage-linux-x64-2\.0\.0\.1\.zip"'
            $content | Should -Match 'data-role="engine-windows-zip-link" href="https://stwwreleases\.blob\.core\.windows\.net/releases/AzureFunctionsPackage-win-x64-2\.0\.0\.1\.zip"'
            ($content | Select-String -Pattern 'data-role="engine-windows-zip-link"' -AllMatches).Matches.Count | Should -Be 1

            $archive = [regex]::Match($content.Substring($content.IndexOf('<!-- RELEASE:PREVIOUS:INSERT -->')), '<article class="release-notes" data-version="1\.0\.0\.1">\s*<div[\s\S]*?</article>').Value
            $archive | Should -Match 'AzureFunctionsPackage-linux-x64-1\.0\.0\.1\.zip">Download Execution Zip \(Linux\)</a>'
            $archive | Should -Match 'AzureFunctionsPackage-win-x64-1\.0\.0\.1\.zip">Download Execution Zip \(Windows\)</a>'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'converges - a second release after the conversion replaces rather than re-inserts the engine links' {
        $path = script:New-ReleaseNotesFixture
        try {
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1'
            script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '3.0.0.1' `
                -EngineLinuxZipUrl 'https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-linux-x64-3.0.0.1.zip' `
                -EngineWindowsZipUrl 'https://stwwreleases.blob.core.windows.net/releases/AzureFunctionsPackage-win-x64-3.0.0.1.zip'
            $content = Get-Content -LiteralPath $path -Raw

            ($content | Select-String -Pattern 'data-role="engine-linux-zip-link"' -AllMatches).Matches.Count | Should -Be 1
            ($content | Select-String -Pattern 'data-role="engine-windows-zip-link"' -AllMatches).Matches.Count | Should -Be 1
            $content | Should -Match 'data-role="engine-windows-zip-link" href="[^"]*win-x64-3\.0\.0\.1\.zip"'
            # 2.0.0.1 was published split, so its archive entry carries both links.
            $archive = [regex]::Match($content.Substring($content.IndexOf('<!-- RELEASE:PREVIOUS:INSERT -->')), '<article class="release-notes" data-version="2\.0\.0\.1">\s*<div[\s\S]*?</article>').Value
            $archive | Should -Match 'win-x64-2\.0\.0\.1\.zip">Download Execution Zip \(Windows\)</a>'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'fails clearly when the page has no engine link of either kind' {
        $path = script:New-ReleaseNotesFixture
        try {
            (Get-Content -LiteralPath $path -Raw) -replace 'data-role="engine-zip-link" ', '' | Set-Content -LiteralPath $path -NoNewline
            { script:Invoke-UpdateReleaseNotes -Path $path -NewVersion '2.0.0.1' } | Should -Throw '*engine-linux-zip-link*'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }
}
