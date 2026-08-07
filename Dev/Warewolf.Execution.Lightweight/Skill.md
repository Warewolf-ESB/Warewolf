# Skill — Deploy the Warewolf Lightweight Execution Engine to Azure Functions

> **Audience:** an LLM agent (or an engineer driving one) that needs to deploy a built
> `Warewolf.Execution.Lightweight` package to Azure Functions.
>
> **Single source of truth:** the **Warewolf Azure Execution Engine deployment scripts zip**,
> published from [warewolf.io/release-notes.php](https://warewolf.io/release-notes.php). This
> file is a pointer, not a manual. Every parameter, phase, environment variable, role
> requirement, and troubleshooting step lives in that zip — primarily its `Scripts/README.md` —
> and changes as the scripts change. If this file and the zip ever disagree, **the zip wins**.
> Fixes belong in the zip's own docs, not here.

---

## 1. Get the deployment scripts

Always resolve the download from the release-notes page — **never hard-code a storage URL**;
it changes on every release.

```
https://warewolf.io/release-notes.php  →  "Warewolf Azure Execution Engine deployment scripts" link
```

At the time of writing this resolves to
`https://stwwreleases.blob.core.windows.net/releases/Warewolf-ExecutionEngine-Deployment-Scripts-v2.1.zip`,
but treat that as an example only — always re-resolve the current link from `release-notes.php`
rather than reusing a URL you've seen before or find in old chat history.

If you need the built engine binaries (rather than building from source), the same page has a
separate "Download Zip" link for the **release package** — the Function App publish output,
which bundles this `Skill.md` and `Protect-LicenseConfig.ps1`, but not the `Scripts/` folder.

Programmatic bootstrap — safe to run in an empty folder, resolves both links automatically:

```powershell
$ErrorActionPreference = 'Stop'
$ReleaseNotesUri = 'https://warewolf.io/release-notes.php'
$Root        = Join-Path (Get-Location) 'warewolf-lightweight'
$Downloads   = Join-Path $Root 'downloads'
$PublishPath = Join-Path $Root 'publish'
New-Item -ItemType Directory -Force -Path $Downloads, $PublishPath | Out-Null

$html = (Invoke-WebRequest -Uri $ReleaseNotesUri -UseBasicParsing).Content
$zipLinks = [regex]::Matches($html, 'href="(?<href>[^"]+\.zip)"[^>]*>(?<text>[^<]+)</a>', 'IgnoreCase') |
    ForEach-Object {
        [pscustomobject]@{
            Href = [System.Net.WebUtility]::HtmlDecode($_.Groups['href'].Value)
            Text = [System.Net.WebUtility]::HtmlDecode($_.Groups['text'].Value).Trim()
        }
    }

$deploymentScriptsUrl = ($zipLinks |
    Where-Object { $_.Text -match 'Deployment Scripts' -or $_.Href -match 'Deployment-Scripts' } |
    Select-Object -First 1).Href
$releasePackageUrl = ($zipLinks |
    Where-Object { $_.Text -match 'Download Zip' -or $_.Href -match 'Lightweight|Package' } |
    Select-Object -First 1).Href

if (-not $deploymentScriptsUrl) { throw "No deployment scripts zip link found on $ReleaseNotesUri" }

$scriptsZip = Join-Path $Downloads (Split-Path $deploymentScriptsUrl -Leaf)
Invoke-WebRequest -Uri $deploymentScriptsUrl -OutFile $scriptsZip
Expand-Archive -Path $scriptsZip -DestinationPath $Root -Force

if ($releasePackageUrl) {
    $packageZip = Join-Path $Downloads (Split-Path $releasePackageUrl -Leaf)
    Invoke-WebRequest -Uri $releasePackageUrl -OutFile $packageZip
    Expand-Archive -Path $packageZip -DestinationPath $PublishPath -Force
}

$ScriptsPath = Join-Path $Root 'Scripts'
Write-Host "Deployment scripts: $ScriptsPath"
Write-Host "Next: read $ScriptsPath\README.md, then run Deploy-WwExecutionEngine.ps1."
```

If you're already inside a source checkout of this repo, the same scripts live at
[`Scripts/`](./Scripts) — build with `dotnet publish -c Release -o <path>` instead of downloading
the release package.

---

## 2. Everything else lives in the zip — start with `Scripts/README.md`

Once extracted, `Scripts/README.md` is the index for the whole toolkit: what each script does,
required parameters, phases, Azure/Entra role requirements, and troubleshooting. Read it before
running anything, and re-read the copy that ships with the zip you just downloaded — do not rely
on a cached or remembered copy, since scripts and docs change together between releases.

Golden path (see `Scripts/README.md` for the full parameter reference and every option):

1. `dotnet publish -c Release -o <path>` your `Warewolf.Execution.Lightweight` build (or use the
   release package downloaded in §1).
2. Extract the deployment scripts zip from §1.
3. Run `Scripts/Deploy-WwExecutionEngine.ps1 -DryRun` first to preview every action, then re-run
   without `-DryRun` to apply it.
4. Follow `Scripts/README.md`'s own verification guidance to confirm the deployment is licensed,
   secured (`secure.config`), and logging correctly before calling the deployment done.

---

## 3. If this file and the zip disagree

Trust the zip. This file can go stale between releases; the scripts and their `README.md` are
updated in lockstep with actual behaviour and are the authority for how to deploy.
