if (!(Test-Path "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab")) {
    Write-Host Downloading  Microsoft.WebView2.FixedVersionRuntime.cab
    Write-Host Running reachability test:
    ping msedge.sf.dl.delivery.mp.microsoft.com
    Write-Host Downloading to $env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab
    Write-Host running free space test:
    Get-CimInstance -ClassName Win32_LogicalDisk | Select-Object -Property DeviceID,@{'Name' = 'FreeSpace (GB)'; Expression= { [int]($_.FreeSpace / 1GB) }}
	Invoke-WebRequest -Uri https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/f93ee51c-3968-426b-9c3e-4f3cba8b1a97/Microsoft.WebView2.FixedVersionRuntime.95.0.1020.44.x64.cab -OutFile "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab"
}
expand "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab" -F:* "$PSScriptRoot"