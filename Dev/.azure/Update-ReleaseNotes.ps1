<#
.SYNOPSIS
    Archives the current "latest release" block in release-notes.php into the
    Previous Versions section, then writes in a new latest version/date/notes/links.

.DESCRIPTION
    Operates on the RAW PHP source of release-notes.php (fetch/write it over FTP,
    never over HTTP - HTTP would return PHP-rendered output, not the editable
    template). Relies on stable markers in the file:
        <!-- RELEASE:LATEST:START -->  ... <!-- RELEASE:LATEST:END -->
        <!-- RELEASE:VERSION -->X.X.X.X<!-- /RELEASE:VERSION -->
        <!-- RELEASE:DATE -->yyyy/MM/dd<!-- /RELEASE:DATE -->
        <!-- RELEASE:NOTES:START --> <ul>...</ul> <!-- RELEASE:NOTES:END -->
        <!-- RELEASE:PREVIOUS:INSERT -->
        data-role="engine-zip-link" / data-role="scripts-zip-link" on the <a> tags
    If $NewVersion already matches the version currently marked as latest, the
    file is left untouched (idempotent - re-running a release publish is safe).

.EXAMPLE
    .\Update-ReleaseNotes.ps1 -Path .\release-notes.php -NewVersion 3.0.2.90 `
        -NotesBullets @('Added X', 'Fixed Y') `
        -EngineZipUrl 'https://storageaccountwarew83c6.blob.core.windows.net/releases/AzureFunctionsPackage-3.0.2.90.zip' `
        -ScriptsZipUrl 'https://storageaccountwarew83c6.blob.core.windows.net/releases/Warewolf-ExecutionEngine-Deployment-Scripts-v2.0.zip'
#>
param(
    [Parameter(Mandatory)] [string]$Path,
    [Parameter(Mandatory)] [string]$NewVersion,
    [string]$NewDate = (Get-Date -Format 'yyyy/MM/dd'),
    [Parameter(Mandatory)] [string[]]$NotesBullets,
    [Parameter(Mandatory)] [string]$EngineZipUrl,
    [Parameter(Mandatory)] [string]$ScriptsZipUrl
)

$ErrorActionPreference = 'Stop'

function Get-Marked {
    param([string]$Text, [string]$StartTag, [string]$EndTag)
    $pattern = [regex]::Escape($StartTag) + '(.*?)' + [regex]::Escape($EndTag)
    $m = [regex]::Match($Text, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $m.Success) { throw "Marker pair not found in '$Path': $StartTag ... $EndTag" }
    return $m
}

$content = Get-Content -LiteralPath $Path -Raw

$oldVersion = (Get-Marked $content '<!-- RELEASE:VERSION -->' '<!-- /RELEASE:VERSION -->').Groups[1].Value.Trim()
$oldDate    = (Get-Marked $content '<!-- RELEASE:DATE -->' '<!-- /RELEASE:DATE -->').Groups[1].Value.Trim()
$oldNotes   = (Get-Marked $content '<!-- RELEASE:NOTES:START -->' '<!-- RELEASE:NOTES:END -->').Groups[1].Value.Trim()

$engineHrefMatch  = [regex]::Match($content, 'data-role="engine-zip-link"[^>]*href="([^"]+)"')
$scriptsHrefMatch = [regex]::Match($content, 'data-role="scripts-zip-link"[^>]*href="([^"]+)"')
if (-not $engineHrefMatch.Success)  { throw "Could not find data-role=`"engine-zip-link`" href in '$Path'." }
if (-not $scriptsHrefMatch.Success) { throw "Could not find data-role=`"scripts-zip-link`" href in '$Path'." }
$oldEngineHref  = $engineHrefMatch.Groups[1].Value
$oldScriptsHref = $scriptsHrefMatch.Groups[1].Value

if ($oldVersion -eq $NewVersion) {
    Write-Warning "release-notes.php already shows '$NewVersion' as the latest version - leaving the file unchanged (idempotent no-op)."
    return
}

$bullets = $NotesBullets | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() }
if ($bullets.Count -eq 0) {
    $bullets = @('See the Warewolf.Execution.Lightweight changelog for this release.')
}
$newNotesUl = "<ul class=`"baseline-large`">`n" + (($bullets | ForEach-Object { "`t`t`t`t`t<li>$_</li>" }) -join "`n") + "`n`t`t`t`t</ul>"

# Archive the OLD latest block as a new <article> immediately after the insert marker.
$archiveArticle = @"

	<article class="release-notes" data-version="$oldVersion">
		<div class="container">
		<h2>$oldVersion</h2>
		<h3>$oldDate</h3>
		$oldNotes
		<p>
		<a class="btn btn-primary" href="$oldEngineHref">Download Execution Zip</a>
		<a class="btn btn-primary" href="$oldScriptsHref">Download Scripts</a>
	</div>
	</article>
"@
$content = $content.Replace('<!-- RELEASE:PREVIOUS:INSERT -->', '<!-- RELEASE:PREVIOUS:INSERT -->' + $archiveArticle)

# Write in the new latest version/date/notes/links using literal (non-regex) replacement
# of each old marked value, so no PowerShell/.NET regex-group escaping is needed at all.
$content = $content.Replace(
    "<!-- RELEASE:VERSION -->$oldVersion<!-- /RELEASE:VERSION -->",
    "<!-- RELEASE:VERSION -->$NewVersion<!-- /RELEASE:VERSION -->")
$content = $content.Replace(
    "<!-- RELEASE:DATE -->$oldDate<!-- /RELEASE:DATE -->",
    "<!-- RELEASE:DATE -->$NewDate<!-- /RELEASE:DATE -->")

$oldNotesBlockPattern = [regex]::Escape('<!-- RELEASE:NOTES:START -->') + '.*?' + [regex]::Escape('<!-- RELEASE:NOTES:END -->')
$newNotesBlock = "<!-- RELEASE:NOTES:START -->`n`t`t`t`t$newNotesUl`n`t`t`t`t<!-- RELEASE:NOTES:END -->"
$content = [regex]::Replace($content, $oldNotesBlockPattern, { param($m) $newNotesBlock }, [System.Text.RegularExpressions.RegexOptions]::Singleline)

$content = $content.Replace(
    "data-role=`"engine-zip-link`" href=`"$oldEngineHref`"",
    "data-role=`"engine-zip-link`" href=`"$EngineZipUrl`"")
$content = $content.Replace(
    "data-role=`"scripts-zip-link`" href=`"$oldScriptsHref`"",
    "data-role=`"scripts-zip-link`" href=`"$ScriptsZipUrl`"")

Set-Content -LiteralPath $Path -Value $content -NoNewline -Encoding UTF8
Write-Host "release-notes.php updated: $oldVersion -> $NewVersion"
