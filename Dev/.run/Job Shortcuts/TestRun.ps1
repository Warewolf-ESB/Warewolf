
param(
  [Parameter(Mandatory=$true)]
  [String[]] $Projects, 
  [String] $Category,
  [String[]] $ExcludeProjects = @(), 
  [String[]] $ExcludeCategories,
  [String] $TestsToRun="${bamboo.TestsToRun}",
  [String] $VSTestPath="${bamboo.capability.system.builder.devenv.Visual Studio 2019}",
  [String] $NuGet="${bamboo.capability.system.builder.command.NuGet}",
  [String] $MSBuildPath="${bamboo.capability.system.builder.msbuild.MSBuild v16.0}",
  [int] $RetryCount=${bamboo.RetryCount},
  [String] $PreTestRunScript,
  [String] $PostTestRunScript,
  [switch] $InContainer,
  [switch] $Coverage,
  [switch] $RetryRebuild,
  [String] $UNCPassword
)
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
	Write-Error "This script expects to be run as Administrator. (Right click run as administrator)"
	exit 1
}
if ($PreTestRunScript -and $Coverage.IsPresent -and !($PreTestRunScript.Contains("-Coverage"))) {
	$PreTestRunScript += " -Coverage"
}
if ($PostTestRunScript -and $Coverage.IsPresent -and !($PostTestRunScript.Contains("-Coverage"))) {
	$PostTestRunScript += " -Coverage"
}
$TestResultsPath = ".\TestResults"
if (Test-Path "$TestResultsPath") {
	Remove-Item -Force -Recurse "$TestResultsPath"
}
if ($VSTestPath -eq $null -or $VSTestPath -eq "" -or !(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
	$VSTestPath = ".\Microsoft.TestPlatform\tools\net451\common7\ide"
} else {
	if ($InContainer.IsPresent) {
		Write-Warning -Message "Ignoring VSTestPath parameter because it cannot be used with the -InContainer parameter."
		$VSTestPath = ".\Microsoft.TestPlatform\tools\net451\Common7\IDE"
	}
}
if (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe") -or ($Coverage.IsPresent -and !(Test-Path ".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe"))) {
	#Find NuGet
	if ("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) {
		$NuGetCommand = Get-Command NuGet -ErrorAction SilentlyContinue
		if ($NuGetCommand) {
			$NuGet = $NuGetCommand.Path
		}
	}
	if (("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) -and (Test-Path "$env:windir")) {
		wget "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "$env:windir\nuget.exe"
		$NuGet = "$env:windir\nuget.exe"
	}
	if ("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) {
		Write-Host NuGet not found. Download from: https://dist.nuget.org/win-x86-commandline/latest/nuget.exe to a directory in the PATH environment variable like c:\windows\nuget.exe. Or use the -NuGet switch.
		sleep 10
		exit 1
	}
}
if (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
	&"nuget.exe" "install" "Microsoft.TestPlatform" "-ExcludeVersion" "-NonInteractive" "-OutputDirectory" "."
	if (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
		Write-Error "Cannot install test runner using nuget."
		exit 1
	}
}
if ($Coverage.IsPresent -and !(Test-Path ".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe")) {
	&"nuget.exe" "install" "JetBrains.dotCover.CommandLineTools" "-ExcludeVersion" "-NonInteractive" "-OutputDirectory" "."
	if (!(Test-Path ".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe")) {
		Write-Error "Cannot install coverage runner using nuget."
		exit 1
	}
}
if ($Coverage.IsPresent) {
	Write-Host Removing existing DotCover Coverage Report
	if (Test-Path "$TestResultsPath\MergedDotCover.dcvr") {
		Remove-Item "$TestResultsPath\MergedDotCover.dcvr"
	}
	if (Test-Path "$TestResultsPath\DotCover-Coverage-Report.html") {
		Remove-Item "$TestResultsPath\DotCover-Coverage-Report.html"
	}
	if (Test-Path "$TestResultsPath\DotCover-Coverage-Report") {
		Remove-Item "$TestResultsPath\DotCover-Coverage-Report"
	}
}
for ($LoopCounter=0; $LoopCounter -le $RetryCount; $LoopCounter++) {
    if ($RetryRebuild.IsPresent) {
		if (!(Test-Path "$PWD\*tests.dll") -or $LoopCounter -gt 0) {
			Get-ChildItem -Path  "$PWD" -Recurse |
Select -ExpandProperty FullName |
Where {$_ -notlike "$PWD\TestResults*" -and $_ -notlike "$PWD\Microsoft.TestPlatform*" -and $_ -notlike "$PWD\JetBrains.dotCover.CommandLineTools*"} |
sort length -Descending |
Remove-Item -force -recurse
			&..\..\Compile.ps1 "-AcceptanceTesting -NuGet `"$NuGet`" -MSBuildPath `"$MSBuildPath`""
		}
	} else {
		if (!(Test-Path "$PWD\*tests.dll")) {
			Write-Error "This script expects to be run from a directory containing test assemblies. (Files with names that end in tests.dll)"
			exit 1
		}
	}
	$AllAssemblies = @()
	foreach ($project in $Projects) {
		$AllAssemblies += @(Get-ChildItem ".\$project.dll" -Recurse)
	}
	if ($AllAssemblies.Count -le 0) {
		$ShowError = "Could not find any assemblies in the current environment directory matching the project definition of: " + ($Projects -join ",")
		Write-Error $ShowError
	}
	$AssembliesList = @()
	for ($i = 0; $i -lt $AllAssemblies.Count; $i++) 
	{
		if ([array]::indexof($ExcludeProjects, $AllAssemblies[$i].Name.TrimEnd(".dll")) -eq -1) {
			$AssembliesList += @($AllAssemblies[$i].Name)
		}
	}
    if (Test-Path "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx") {
        Remove-Item "$VSTestPath\Extensions\TestPlatform\TestResults" -Force -Recurse
    }
	New-Item -ItemType Directory "$TestResultsPath" -ErrorAction SilentlyContinue
	if (Test-Path "$TestResultsPath\RunTests.ps1") {
		Move-Item "$TestResultsPath\RunTests.ps1" "$TestResultsPath\RunTests($LoopCounter).ps1"
	}
	if (Test-Path "$TestResultsPath\warewolf-server.log") {
		Move-Item "$TestResultsPath\warewolf-server.log" "$TestResultsPath\warewolf-server($LoopCounter).ps1"
	}
	if ($Coverage.IsPresent -and !($PreTestRunScript)) {
		if (Test-Path "$TestResultsPath\DotCover Runner.xml") {
			Move-Item "$TestResultsPath\DotCover Runner.xml" "$TestResultsPath\DotCover Runner($LoopCounter).xml"
		}
		if (Test-Path "$TestResultsPath\DotCover.dcvr") {
			Move-Item "$TestResultsPath\DotCover.dcvr" "$TestResultsPath\DotCover($LoopCounter).dcvr"
		}
		if (Test-Path "$TestResultsPath\DotCover.log") {
			Move-Item "$TestResultsPath\DotCover.log" "$TestResultsPath\DotCover($LoopCounter).log"
		}
		"<AnalyseParams>" | Out-File "$TestResultsPath\DotCover Runner.xml"
		$HandleRelativePath = $VSTestPath
		if ($VSTestPath.StartsWith(".\")) {
			$HandleRelativePath = "." + $VSTestPath
		}
		"  <TargetExecutable>$HandleRelativePath\Extensions\TestPlatform\vstest.console.exe</TargetExecutable>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		$AssembliesArg = "..\" + ($AssembliesList -join " ..\")
	} else {
		$AssembliesArg = ".\" + ($AssembliesList -join " .\")
	}
	if ($UNCPassword) {
		"net use \\DEVOPSPDC.premier.local\FileSystemShareTestingSite /user:Administrator $UNCPassword" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
	}
	if ($TestsToRun) {
		if ($PreTestRunScript) {
			"&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /Parallel /logger:trx $AssembliesArg /Tests:`"$TestsToRun`"" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		} else {
			if ($Coverage.IsPresent -and !($PreTestRunScript)) {
				"  <TargetArguments>/logger:trx $AssembliesArg /Tests:`"$TestsToRun`"</TargetArguments>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
			} else {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /Parallel /logger:trx $AssembliesArg /Tests:`"$TestsToRun`"" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			}
		}
	} else {
		$CategoryArg = ""
		if ($ExcludeCategories -ne $null -and $ExcludeCategories -ne @()) {
			if ($ExcludeCategories.Count -eq 1 -and $ExcludeCategories[0].Contains(",")) {
				$ExcludeCategories = $ExcludeCategories[0] -split ","
			}
			$CategoryArg = "/TestCaseFilter:`"(TestCategory!="
			$CategoryArg += $ExcludeCategories -join ")&(TestCategory!="
			$CategoryArg += ")`""
		} else {
			if ($Category -ne $null -and $Category -ne "") {
			    $CategoryArg = "/TestCaseFilter:`"(TestCategory=" + $Category + ")`""
			}
		}
		if ($PreTestRunScript) {
			"&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg $CategoryArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		} else {
			if ($Coverage.IsPresent) {
				$XmlSafeCategory = $CategoryArg -replace "`&","`&amp;"
				"  <TargetArguments>/Parallel /logger:trx $AssembliesArg $XmlSafeCategory</TargetArguments>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
			} else {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /Parallel /logger:trx $AssembliesArg $CategoryArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			}
		}
	}
	if ($PostTestRunScript) {
		"&.\$PostTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
	}
	if ($Coverage.IsPresent -and !($PreTestRunScript)) {
		"  <Output>.$TestResultsPath\DotCover.dcvr</Output>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"  <Scope>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"    <ScopeEntry>..\Warewolf*.dll</ScopeEntry>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"    <ScopeEntry>..\Warewolf*.exe</ScopeEntry>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"    <ScopeEntry>..\Dev2.*.dll</ScopeEntry>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"  </Scope>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"  <Filters>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"    <ExcludeFilters>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"      <FilterEntry>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"        <ModuleMask>*tests</ModuleMask>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"        <ModuleMask>*specs</ModuleMask>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"        <ModuleMask>*Tests</ModuleMask>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"        <ModuleMask>*Specs</ModuleMask>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"        <ModuleMask>Warewolf.UIBindingTests*</ModuleMask>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"      </FilterEntry>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"    </ExcludeFilters>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"    <AttributeFilters>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"      <AttributeFilterEntry>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"        <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"      </AttributeFilterEntry>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"    </AttributeFilters>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"  </Filters>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		"</AnalyseParams>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
		Get-Content "$TestResultsPath\DotCover Runner.xml"
		"&`".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe`" cover `"$TestResultsPath\DotCover Runner.xml`" /LogFile=`"$TestResultsPath\DotCover.log`" --DisableNGen" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
	}
	if ($Coverage.IsPresent) {
		"Wait-Process -Name `"DotCover`" -ErrorAction SilentlyContinue" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
	}
	if ($UNCPassword) {
		"net use \\DEVOPSPDC.premier.local\FileSystemShareTestingSite /delete" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
	}
	Get-Content "$TestResultsPath\RunTests.ps1"
	if (!($InContainer.IsPresent)) {
		&"$TestResultsPath\RunTests.ps1"
	} else {
		docker run -i --rm -v "${PWD}:C:\BuildUnderTest" --entrypoint="powershell -Command Set-Location .\BuildUnderTest;&.\TestResults\RunTests.ps1" registry.gitlab.com/warewolf/vstest
	}
    if (Test-Path "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx") {
        Copy-Item "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx" "$TestResultsPath" -Force -Recurse
    }
	if (Test-Path "$TestResultsPath\*.trx") {
		[System.Collections.ArrayList]$getXMLFiles = @(Get-ChildItem "$TestResultsPath\*.trx")
		if ($getXMLFiles.Count -gt 1) {
			$MaxCount = 0
			$MaxCountIndex = 0
			$CountIndex = 0
			$getXMLFiles | % {
				$getXML = [xml](Get-Content $_.FullName)
				if ([int]$getXML.TestRun.ResultSummary.Counters.total -gt $MaxCount) {
					$MaxCount = $getXML.TestRun.ResultSummary.Counters.total
					$MaxCountIndex = $CountIndex
				}
				$CountIndex++
			}
			$BaseXMLFile = $getXMLFiles[$MaxCountIndex]
			$getBaseXML = [xml](Get-Content $BaseXMLFile.FullName)
			$getXMLFiles.RemoveAt($MaxCountIndex)
			$getXMLFiles | % {
				$getXML = [xml](Get-Content $_.FullName)
				$getXMl.TestRun.Results.UnitTestResult | % {
					$RetryUnitTestResult = $_
					$getBaseXMl.TestRun.Results.UnitTestResult | % {
						if ($RetryUnitTestResult.testName -eq $_.testName -and $RetryUnitTestResult.outcome -eq 'Passed' -and $_.outcome -eq 'Failed') {
							[void]$_.ParentNode.AppendChild($getBaseXMl.ImportNode($RetryUnitTestResult, $true))
							$_.ParentNode.ParentNode.ResultSummary.Counters.SetAttribute("passed", [int]($_.ParentNode.ParentNode.ResultSummary.Counters.passed) + 1)
							$_.ParentNode.ParentNode.ResultSummary.Counters.SetAttribute("failed", [int]($_.ParentNode.ParentNode.ResultSummary.Counters.failed) - 1)
							[void]$_.ParentNode.RemoveChild($_)
						}
					}
				}
				Remove-Item $_.FullName
			}
			$getBaseXML.Save($BaseXMLFile.FullName)
		} else {
			$getBaseXML = [xml](Get-Content $getXMLFiles[0].FullName)
		}
		if ($getBaseXML.TestRun.ResultSummary.Counters.passed -ne $getBaseXML.TestRun.ResultSummary.Counters.executed) {
			$TestsToRun = ($getBaseXML.TestRun.Results.UnitTestResult | Where-Object {$_.outcome -ne "Passed"}).testName -join ","
		} else {
			break
		}
	} else {
		Write-Error "No test results found."
		exit 1
	}
}
if ($Coverage.IsPresent) {
	$MergedSnapshotPath = "$TestResultsPath\MergedDotCover.dcvr"
	&".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe" merge --Sources="$TestResultsPath\DotCover*.dcvr" --Output=$MergedSnapshotPath
	&".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe" report --Source=$MergedSnapshotPath --Output="$TestResultsPath\DotCover-Coverage-Report.html" --ReportType=HTML
}