<#
    Pester 5 suite for Remove-UnreferencedReleases.ps1.

        Invoke-Pester -Path ./Remove-UnreferencedReleases.Tests.ps1

    Operates entirely on local temp files - no Azure, no network. The script only decides WHICH
    blobs are unreferenced; the pipeline does the deleting.
#>

BeforeAll {
    $script:Prune = Join-Path $PSScriptRoot 'Remove-UnreferencedReleases.ps1'
    $script:Base  = 'https://stwwreleases.blob.core.windows.net/releases/'

    function script:New-Page([string[]]$Names) {
        $path = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + '.php')
        $links = ($Names | ForEach-Object { "<a class=`"btn btn-primary`" href=`"$($script:Base)$_`">Download</a>" }) -join "`n"
        Set-Content -LiteralPath $path -Value "<html><body>`n$links`n</body></html>"
        $path
    }

    function script:Invoke-Prune([string]$Page, [string[]]$Blobs, [string[]]$Expected) {
        @(& $script:Prune -ReleaseNotesPath $Page -BaseUrl $script:Base -BlobName $Blobs -ExpectedBlobName $Expected)
    }
}

Describe 'Remove-UnreferencedReleases.ps1' {

    It 'keeps blobs mentioned anywhere on the page and returns the rest' {
        $page = New-Page @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip', 'AzureFunctionsPackage-linux-x64-3.0.0.1.zip')
        $result = Invoke-Prune $page `
            @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip', 'AzureFunctionsPackage-linux-x64-3.0.0.1.zip', 'AzureFunctionsPackage-linux-x64-2.9.0.0.zip') `
            @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip')
        $result | Should -Be @('AzureFunctionsPackage-linux-x64-2.9.0.0.zip')
    }

    It 'keeps a pre-split release that only has a single engine link' {
        $page = New-Page @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip', 'AzureFunctionsPackage-1.0.0.1.zip')
        $result = Invoke-Prune $page `
            @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip', 'AzureFunctionsPackage-1.0.0.1.zip') `
            @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip')
        $result | Should -BeNullOrEmpty
    }

    It 'matches blob names exactly, so a name that is a prefix of a kept one is still pruned' {
        $page = New-Page @('Warewolf-QueueProcessor-3.0.2.90.zip')
        $result = Invoke-Prune $page `
            @('Warewolf-QueueProcessor-3.0.2.90.zip', 'Warewolf-QueueProcessor-3.0.2.9.zip') `
            @('Warewolf-QueueProcessor-3.0.2.90.zip')
        $result | Should -Be @('Warewolf-QueueProcessor-3.0.2.9.zip')
    }

    It 'compares case-sensitively, as blob names are' {
        $page = New-Page @('Warewolf-QueueProcessor-3.0.2.90.zip')
        $result = Invoke-Prune $page `
            @('Warewolf-QueueProcessor-3.0.2.90.zip', 'warewolf-queueprocessor-3.0.2.90.zip') `
            @('Warewolf-QueueProcessor-3.0.2.90.zip')
        $result | Should -BeNullOrEmpty   # the lower-case name matches no release prefix, so it is left alone
    }

    It 'never returns a blob outside the release naming patterns' {
        $page = New-Page @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip')
        $result = Invoke-Prune $page `
            @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip', 'notes.txt', 'backup.zip', 'AzureFunctionsPackage-old.txt') `
            @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip')
        $result | Should -BeNullOrEmpty
    }

    It 'throws when the page mentions no blobs at all' {
        $page = Join-Path $TestDrive 'empty.php'
        Set-Content -LiteralPath $page -Value '<html><body>nothing here</body></html>'
        { Invoke-Prune $page @('AzureFunctionsPackage-1.0.0.1.zip') @('AzureFunctionsPackage-1.0.0.1.zip') } |
            Should -Throw '*mentions no blobs*'
    }

    It 'throws when the page does not mention a just-published blob' {
        $page = New-Page @('AzureFunctionsPackage-linux-x64-3.0.0.1.zip')
        { Invoke-Prune $page @('AzureFunctionsPackage-linux-x64-3.0.0.1.zip') @('AzureFunctionsPackage-linux-x64-3.0.0.2.zip') } |
            Should -Throw '*just-published*'
    }

    It 'decodes percent-encoded URLs before comparing' {
        $page = New-Page @('Warewolf-QueueProcessor-3.0.2.90%2Bbeta.zip')
        $result = Invoke-Prune $page `
            @('Warewolf-QueueProcessor-3.0.2.90+beta.zip') `
            @('Warewolf-QueueProcessor-3.0.2.90+beta.zip')
        $result | Should -BeNullOrEmpty
    }

    It 'returns nothing for an empty container listing' {
        $page = New-Page @('AzureFunctionsPackage-1.0.0.1.zip')
        { & $script:Prune -ReleaseNotesPath $page -BaseUrl $script:Base -BlobName @() -ExpectedBlobName @('AzureFunctionsPackage-1.0.0.1.zip') } |
            Should -Not -Throw
    }

    It 'throws when the release notes file does not exist' {
        { Invoke-Prune (Join-Path $TestDrive 'missing.php') @() @('x.zip') } | Should -Throw '*not found*'
    }
}
