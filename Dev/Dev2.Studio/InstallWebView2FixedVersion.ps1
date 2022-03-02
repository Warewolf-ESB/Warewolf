if (!(Test-Path "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab")) {
    Write-Host Running reachability test:
    ping warewolfreleases.s3.eu-west-1.amazonaws.com
	Invoke-WebRequest -Uri https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/f93ee51c-3968-426b-9c3e-4f3cba8b1a97/Microsoft.WebView2.FixedVersionRuntime.95.0.1020.44.x64.cab -OutFile "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab"
}
expand "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab" -F:* "$PSScriptRoot"