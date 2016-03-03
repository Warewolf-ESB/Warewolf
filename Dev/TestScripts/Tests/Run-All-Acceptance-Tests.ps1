﻿$TestSettingsFile = "$PSScriptRoot\LocalAcceptanceTesting.testsettings"
$SolutionDir = (get-item $PSScriptRoot ).parent.parent.FullName
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding=`"UTF-8`"?>
<TestSettings name=`"Local Acceptance Run`" id=`"3264dd0f-6fc1-4cb9-b44f-c649fef29609`" xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>These are default test settings for a local acceptance test run.</Description>
    <Deployment>
      <!--Server and Studio under test-->
		  <DeploymentItem filename=`"Dev2.Server\bin\Debug\`" outputDirectory=`"Server`" />
      <!--Missing test assembly dependencies-->
      <DeploymentItem filename=`"packages\FSharp.Core.3.0.0.2\lib\net40\FSharp.Core.dll`" />
      <DeploymentItem filename=`"packages\FSharp.Core.3.0.0.2\lib\net40\policy.2.3.FSharp.Core.dll`" />
    </Deployment>
  <NamingScheme baseName=`"AcceptanceTesting`" appendTimeStamp=`"false`" useDefault=`"false`" />
    <Scripts setupScript=`"TestScripts\Server\Startup.bat`" cleanupScript=`"TestScripts\Server\Cleanup.bat`" />
  <RemoteController name=`"rsaklfsvrtfsbld`" />
  <Execution hostProcessPlatform=`"MSIL`">
    <Timeouts testTimeout=`"180000`" />
    <TestTypeSpecific>
      <UnitTestRunConfig testTypeId=`"13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b`">
        <AssemblyResolution>
          <TestDirectory useLoadContext=`"true`" />
        </AssemblyResolution>
      </UnitTestRunConfig>
      <WebTestRunConfiguration testTypeId=`"4e7599fa-5ecb-43e9-a887-cd63cf72d207`">
        <Browser name=`"Internet Explorer 9.0`" MaxConnections=`"6`">
          <Headers>
            <Header name=`"User-Agent`" value=`"Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)`" />
            <Header name=`"Accept`" value=`"*/*`" />
            <Header name=`"Accept-Language`" value=`"{{`$IEAcceptLanguage}}`" />
            <Header name=`"Accept-Encoding`" value=`"GZIP`" />
          </Headers>
        </Browser>
      </WebTestRunConfiguration>
    </TestTypeSpecific>
    <AgentRule name=`"LocalMachineDefaultRole`">
    </AgentRule>
  </Execution>
</TestSettings>
"@)

$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir | ? {$_.PSIsContainer -and ((($_.Name.StartsWith("Dev2.") -or $_.Name.StartsWith("Warewolf.")) -and $_.Name.EndsWith(".Specs")) -or $_.Name.StartsWith("Warewolf.AcceptanceTesting.")) -and $_.Name -ne "Dev2.Installer.Specs" -and $_.Name -ne "Warewolf.AcceptanceTesting.Scheduler"} ) {
    $TestAssembliesList = "`"$SolutionDir\" + $file.Name + "\bin\Debug\" + $file.Name + ".dll`" " + $TestAssembliesList 
}
$FullArgsList = $TestAssembliesList + "/logger:trx /Settings:`"" + $TestSettingsFile + "`""
Write-Host $SolutionDir> `"$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`" $FullArgsList
Start-Process -FilePath "$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList @($FullArgsList) -verb RunAs -WorkingDirectory $SolutionDir -Wait