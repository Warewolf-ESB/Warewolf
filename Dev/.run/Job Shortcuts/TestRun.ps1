param(
  [Parameter(Mandatory=$true)]
  [String[]] $Projects, 
  [String] $Category,
  [String[]] $Categories,
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
if ($ExcludeProjects.Length -eq 1 -and $ExcludeProjects[0].Contains(",")) {
    $SplitExcludeProjects = $ExcludeProjects[0]
    $ExcludeProjects = New-Object string[] $SplitExcludeProjects.Split(",").Count
    for ($j=0; $j -le $SplitExcludeProjects.Split(",").Count-1; $j++) {
        $ExcludeProjects[$j] = $SplitExcludeProjects.Split(",")[$j]
    }
}
if ($Projects.Length -eq 1 -and $Projects[0].Contains(",")) {
    $SplitProjects = $Projects[0]
    $Projects = New-Object string[] $SplitProjects.Split(",").Count
    for ($j=0; $j -le $SplitProjects.Split(",").Count-1; $j++) {
        $Projects[$j] = $SplitProjects.Split(",")[$j]
    }
}
if ($ExcludeCategories.Length -eq 1 -and $ExcludeCategories[0].Contains(",")) {
    $SplitExcludeCategories = $ExcludeCategories[0]
    $ExcludeCategories = New-Object string[] $SplitExcludeCategories.Split(",").Count
    for ($j=0; $j -le $SplitExcludeCategories.Split(",").Count-1; $j++) {
        $ExcludeCategories[$j] = $SplitExcludeCategories.Split(",")[$j]
    }
}
if ($Categories.Length -eq 1 -and $Categories[0].Contains(",")) {
    $SplitCategories = $Categories[0]
    $Categories = New-Object string[] $SplitCategories.Split(",").Count
    for ($j=0; $j -le $SplitCategories.Split(",").Count-1; $j++) {
        $Categories[$j] = $SplitCategories.Split(",")[$j]
    }
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
if (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
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
if ($Coverage.IsPresent) {
	Write-Host Removing existing Coverage Reports
	if (Test-Path "$TestResultsPath\Merged.coveragexml") {
		Remove-Item "$TestResultsPath\Merged.coveragexml"
	}
	if (Test-Path "$TestResultsPath\Cobertura.xml") {
		Remove-Item "$TestResultsPath\Cobertura.xml"
	}
}
for ($LoopCounter=0; $LoopCounter -le $RetryCount; $LoopCounter++) {
    if ($RetryRebuild.IsPresent) {
		if (Test-Path "$PWD\..\..\Compile.ps1") {
			&"$PWD\..\..\Compile.ps1" "-AcceptanceTesting -NuGet `"$NuGet`" -MSBuildPath `"$MSBuildPath`""
		} else {
			if (Test-Path "$PWD\Compile.ps1") {
				&"$PWD\Compile.ps1" "-AcceptanceTesting -NuGet `"$NuGet`" -MSBuildPath `"$MSBuildPath`""
				Set-Location "$PWD\bin\AcceptanceTesting"
			}
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
	$AssembliesArg = ".\" + ($AssembliesList -join " .\")
	if ($UNCPassword) {
		"net use \\DEVOPSPDC.premier.local\FileSystemShareTestingSite /user:Administrator $UNCPassword" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
	}
	if ($TestsToRun) {
		if ($PreTestRunScript) {
			"&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg /Tests:`"$TestsToRun`"" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		} else {
			if ($Coverage.IsPresent -and !($PreTestRunScript)) {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg /Tests:`"$TestsToRun`" /EnableCodeCoverage" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			} else {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg /Tests:`"$TestsToRun`"" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
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
			} else {
				if ($Categories -ne $null -and $Categories.Count -ne 0) {
					$CategoryArg = "/TestCaseFilter:`"(TestCategory="
					$CategoryArg += $Categories -join ")|(TestCategory="
					$CategoryArg += ")`""
				}
			}
		}
		if ($PreTestRunScript) {
			"&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg $CategoryArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		} else {
			if ($Coverage.IsPresent -and !($PreTestRunScript)) {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg $CategoryArg /EnableCodeCoverage" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			} else {
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx $AssembliesArg $CategoryArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			}
		}
	}
	if ($PostTestRunScript) {
		"&.\$PostTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
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
	$MergedSnapshotPath = "$TestResultsPath\Merged.coveragexml"
	$CoverageToolPath = ".\Microsoft.TestPlatform\tools\net451\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe"
	$GetSnapshots = Get-ChildItem "$TestResultsPath\**\*.coverage"
	Write-Host `&`"$CoverageToolPath`" analyze /output:`"$MergedSnapshotPath`" @GetSnapshots
	&"$CoverageToolPath" analyze /output:"$MergedSnapshotPath" @GetSnapshots
	$reportGeneratorExecutable = ".\reportgenerator.exe"

    if (!(Test-Path "$reportGeneratorExecutable")) {
        dotnet tool install dotnet-reportgenerator-globaltool --tool-path "."
    }
    
    $reportGeneratorCoberturaParams = @(
        "-reports:$MergedSnapshotPath",
        "-targetdir:$TestResultsPath",
        "-reporttypes:Cobertura"
    )
    
    Write-Output "Executing Report Generator with following parameters: $reportGeneratorCoberturaParams."
    &"$reportGeneratorExecutable" @reportGeneratorCoberturaParams
}