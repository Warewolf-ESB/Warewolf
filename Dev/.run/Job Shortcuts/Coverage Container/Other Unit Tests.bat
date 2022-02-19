IF [%1] NEQ [] (
mkdir "%~dp0..\..\..\TestResults"
echo Set-Location C:\BuildUnderTest>"%~dp0..\..\..\TestResults\RunTestsEntrypoint.ps1"
echo ^&".\Job Shortcuts\TestRun.ps1" -RetryCount 6 -Projects Dev2.*.Tests,Warewolf.*.Tests,Warewolf.UIBindingTests.* -ExcludeProjects Dev2.Integration.Tests,Dev2.Studio.Core.Tests,Dev2.RunTime.Tests,Dev2.Infrastructure.Tests,Warewolf.UI.Tests,Warewolf.Logger.Tests,Warewolf.Studio.ViewModels.Tests,Warewolf.Web.UI.Tests,Warewolf.Storage.Tests,Warewolf.Auditing.Tests -ExcludeCategories CannotParallelize,ResourceCatalog_LoadTests,PluginRuntimeHandler,GatherSystemInformation -Coverage>>"%~dp0..\..\..\TestResults\RunTestsEntrypoint.ps1"
docker run -i --rm --memory 4g -v "%~dp0..\..\..\TestResults:C:\BuildUnderTest\TestResults" registry.gitlab.com/warewolf/vstest:%1 powershell -File C:\BuildUnderTest\TestResults\RunTestsEntrypoint.ps1
) ELSE (
mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryCount 6 -Projects Dev2.*.Tests,Warewolf.*.Tests,Warewolf.UIBindingTests.* -ExcludeProjects Dev2.Integration.Tests,Dev2.Studio.Core.Tests,Dev2.RunTime.Tests,Dev2.Infrastructure.Tests,Warewolf.UI.Tests,Warewolf.Logger.Tests,Warewolf.Studio.ViewModels.Tests,Warewolf.Web.UI.Tests,Warewolf.Storage.Tests,Warewolf.Auditing.Tests -ExcludeCategories CannotParallelize,ResourceCatalog_LoadTests,PluginRuntimeHandler,GatherSystemInformation -Coverage -InContainer
)