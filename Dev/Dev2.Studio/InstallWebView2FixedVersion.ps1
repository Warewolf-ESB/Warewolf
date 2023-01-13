if (!(Test-Path "$PSScriptRoot\Microsoft.WebView2.FixedVersionRuntime.95.0.1020.44.x64")) {
	if (!(Test-Path "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab")) {
		Invoke-WebRequest -Uri https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/925c3045-c587-4eff-ad34-fb9daf903e47/Microsoft.WebView2.FixedVersionRuntime.108.0.1462.76.arm64.cab -OutFile "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab"
	}
	expand "$env:UserProfile\.nuget\packages\Microsoft.WebView2.FixedVersionRuntime.cab" -F:* "$PSScriptRoot"
}