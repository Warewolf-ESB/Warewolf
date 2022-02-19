IF [%1] NEQ [] (
mkdir "%~dp0..\..\..\TestResults"
echo Set-Location C:\BuildUnderTest>"%~dp0..\..\..\TestResults\RunTestsEntrypoint.ps1"
echo ^&".\Job Shortcuts\TestRun.ps1" -RetryCount 6 -Projects Dev2.Activities.Specs -Category StudioTestFrameworkWithHTTPWebDeleteTools -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath ServerTests" -PostTestRunScript ReverseDeployLog.ps1 -Coverage>>"%~dp0..\..\..\TestResults\RunTestsEntrypoint.ps1"
docker run -i --rm --memory 4g -v "%~dp0..\..\..\TestResults:C:\BuildUnderTest\TestResults" registry.gitlab.com/warewolf/vstest:%1 powershell -File C:\BuildUnderTest\TestResults\RunTestsEntrypoint.ps1
) ELSE (
mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryCount 6 -Projects Dev2.Activities.Specs -Category StudioTestFrameworkWithHTTPWebDeleteTools -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath ServerTests" -PostTestRunScript ReverseDeployLog.ps1 -Coverage -InContainer
)