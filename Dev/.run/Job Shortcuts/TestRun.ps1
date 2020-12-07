param(
  [Parameter(Mandatory=$true)]
  [String[]] $Projects, 
  [String] $Category,
  [String[]] $ExcludeProjects = @(), 
  [String[]] $ExcludeCategories,
  [String] $TestsToRun="${bamboo.TestsToRun}",
  [String] $VSTestPath="${bamboo.capability.system.builder.devenv.Visual Studio 2019}",
  [int] $RetryCount=${bamboo.RetryCount},
  [String] $PreTestRunScript,
  [switch] $RunInDocker,
  [switch] $Coverage
)
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script expects to be run as Administrator. (Right click run as administrator)"
    exit 1
}
if ($PreTestRunScript) {
	if (!(Test-Path .\StartAs.ps1)) {
		Write-Error -Message "This script expects to be run from a bin directoy that includes a Warewolf Server or Studio."
		exit 1
	}
	if ($Coverage.IsPresent -and !($PreTestRunScript.Contains("-Coverage"))) {
		$PreTestRunScript += " -Coverage"
	}
}
$TestResultsPath = ".\TestResults"
if (Test-Path "$TestResultsPath") {
    Remove-Item -Force -Recurse "$TestResultsPath"
}
if ($VSTestPath -eq $null -or $VSTestPath -eq "" -or !(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
	$VSTestPath = ".\Microsoft.TestPlatform\tools\net451\common7\ide"
} else {
	if ($RunInDocker.IsPresent) {
		Write-Error "Cannot use VSTestPath paramemter with the RunInDocker paramemter."
		exit 1
	}
}
if (!($RunInDocker.IsPresent) -and (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe") -or ($Coverage.IsPresent -and !(Test-Path ".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe")))) {
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
		Write-Host NuGet not found. Download from: https://dist.nuget.org/win-x86-commandline/latest/nuget.exe to adirectory in the PATH environment variable like c:\windows\nuget.exe. Or use the -NuGet switch.
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
if ($RunInDocker.IsPresent) {
    $ContainerName = $Projects.ToLower().replace(" ", "-");
    if ($Category -ne $null -and $Category -ne "") {
        $ContainerName += "-$Category";
    }
    if ("${bamboo.repository.git.branch}" -ne $null -and "${bamboo.repository.git.branch}" -ne "") {
        $ContainerName += "-${bamboo.repository.git.branch}".replace("/\","-")
    }
}
for ($LoopCounter=0; $LoopCounter -le $RetryCount; $LoopCounter++) {
	if ($RunInDocker.IsPresent) {
		docker rm -f $ContainerName
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
	New-Item -ItemType Directory "$TestResultsPath" -ErrorAction SilentlyContinue
	if (Test-Path "$TestResultsPath\RunTests.ps1") {
		Move-Item "$TestResultsPath\RunTests.ps1" "$TestResultsPath\RunTests($LoopCounter).ps1"
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
		if ($RunInDocker.IsPresent) {
			$AssembliesArg = ".\BuildUnderTest\" + ($AssembliesList -join " .\BuildUnderTest\")
		} else {
			$AssembliesArg = ".\" + ($AssembliesList -join " .\")
		}
	}
	if ($TestsToRun) {
		if ($PreTestRunScript) {
            "&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii
			"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /Parallel /logger:trx $AssembliesArg /Tests:`"$TestsToRun`"" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		} else {
			if ($Coverage.IsPresent -and !($PreTestRunScript)) {
				"  <TargetArguments>/Parallel /logger:trx $AssembliesArg /Tests:`"$TestsToRun`"</TargetArguments>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
			} else {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /Parallel /logger:trx $AssembliesArg /Tests:`"$TestsToRun`"" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii
			}
		}
	} else {
		$CategoryArg = ""
		if ($ExcludeCategories -ne $null -and $ExcludeCategories -ne @()) {
			$CategoryArg = "/TestCaseFilter:`"(TestCategory!="
			$CategoryArg += $ExcludeCategories -join ")&(TestCategory!="
			$CategoryArg += ")`""
		} else {
            if ($Category -ne $null -and $Category -ne "") {
			    $CategoryArg = "/TestCaseFilter:`"(TestCategory=" + $Category + ")`""
            }
		}
		if ($PreTestRunScript) {
            "&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii
			"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg $CategoryArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		} else {
			if ($Coverage.IsPresent -and !($PreTestRunScript)) {
				"  <TargetArguments>/Parallel /logger:trx $AssembliesArg $CategoryArg</TargetArguments>" | Out-File "$TestResultsPath\DotCover Runner.xml" -Append
			} else {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /Parallel /logger:trx $AssembliesArg $CategoryArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii
			}
		}
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
		if ($RunInDocker.IsPresent) {
			"&`".\BuildUnderTest\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe`" cover `".\BuildUnderTest\TestResults\DotCover Runner.xml`" /LogFile=`".\BuildUnderTest\TestResults\DotCover.log`" --DisableNGen" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii
		} else {
			"&`".\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe`" cover `"$TestResultsPath\DotCover Runner.xml`" /LogFile=`"$TestResultsPath\DotCover.log`" --DisableNGen" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii
		}
    }
    if (!($RunInDocker.IsPresent)) {
        Get-Content "$TestResultsPath\RunTests.ps1"
        &"$TestResultsPath\RunTests.ps1"
        if ($PreTestRunScript) {
			sc.exe stop "Warewolf Server"
			Wait-Process -Name "Warewolf Server" -ErrorAction SilentlyContinue
			Move-Item "C:\programdata\warewolf\Server Log\warewolf-server.log" "$TestResultsPath\warewolf-server($LoopCounter).log"
		}
		if ($Coverage.IsPresent) {
			Wait-Process -Name "DotCover" -ErrorAction SilentlyContinue
		}
    } else {
        docker create --name=$ContainerName --entrypoint="powershell -File .\BuildUnderTest\TestResults\RunTests.ps1" -P registry.gitlab.com/warewolf/vstest
        docker cp . ${ContainerName}:.\BuildUnderTest
        docker start -a $ContainerName
		if ($Coverage.IsPresent -and !($PreTestRunScript)) {
			docker cp ${ContainerName}:\BuildUnderTest\TestResults .
			docker cp ${ContainerName}:\BuildUnderTest\Microsoft.TestPlatform\tools\net451\common7\ide\Extensions\TestPlatform\TestResults .
		} else {
			docker cp ${ContainerName}:\TestResults .
		}
        if ($PreTestRunScript) {
            docker cp ${ContainerName}:"\programdata\warewolf\Server Log\warewolf-server.log" .\TestResults\warewolf-server`($LoopCounter`).log
        }
    }
	if (Test-Path "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx") {
		Move-Item "$VSTestPath\Extensions\TestPlatform\TestResults\*" "$TestResultsPath"
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