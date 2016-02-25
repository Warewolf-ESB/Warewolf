$TestAssembliesList = ''
foreach ($file in Get-ChildItem $PSScriptRoot\.. | ? {$_.PSIsContainer -and (($_.Name.StartsWith("Dev2.") -or $_.Name.StartsWith("Warewolf.")) -and $_.Name.EndsWith(".Tests"))} ) {
    $TestAssembliesList = "`"$PSScriptRoot\..\" + $file.Name + "\bin\Debug\" + $file.Name + ".dll`" " + $TestAssembliesList 
}
$TestAssembliesList = $TestAssembliesList + "/logger:trx"
#Write-Host $TestAssembliesList
#Start-Process -FilePath "$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList $TestAssembliesList -verb RunAs -WorkingDirectory $PSScriptRoot -PassThru

$pinfo = New-Object System.Diagnostics.ProcessStartInfo
$pinfo.FileName = "$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe"
$pinfo.RedirectStandardError = $true
$pinfo.RedirectStandardOutput = $true
$pinfo.UseShellExecute = $false
$pinfo.Arguments = $TestAssembliesList
$pinfo.Verb = "RunAs"
$pinfo.CreateNoWindow = $true
$p = New-Object System.Diagnostics.Process
$p.StartInfo = $pinfo
$p.Start() | Out-Null
$StreamingStandardOutput = $p.StandardOutput
while (-not $p.HasExited) {
    if (-not $StreamingStandardOutput.EndOfStream) {
        Write-Host $StreamingStandardOutput.ReadLine()
        start-sleep 2
    }
}
Write-Host $p.StandardOutput.ReadToEnd()
Write-Host $p.StandardError.ReadToEnd()
Write-Host exit code: $p.ExitCode
