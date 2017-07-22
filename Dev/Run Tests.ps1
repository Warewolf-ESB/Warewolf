#Requires -RunAsAdministrator
Param(
  [switch]$StartServer,
  [switch]$StartStudio,
  [string]$ServerPath,
  [string]$StudioPath,
  [string]$TestsPath=$PSScriptRoot,
  [string]$TestsResultsPath="$TestsPath\TestResults",
  [string]$ResourcesType,
  [switch]$VSTest,
  [switch]$MSTest,
  [switch]$DotCover,
  [string]$VSTestPath="$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe",
  [string]$MSTestPath="$env:vs140comntools..\IDE\MSTest.exe",
  [string]$DotCoverPath="$env:LocalAppData\JetBrains\Installations\dotCover08\dotCover.exe",
  [string]$ServerUsername,
  [string]$ServerPassword,
  [string]$JobName="",
  [switch]$RunAllJobs,
  [switch]$Cleanup,
  [switch]$AssemblyFileVersionsTest,
  [switch]$DisableTimeouts,
  [switch]$RecordScreen,
  [switch]$Parallelize,
  [string]$Category,
  [string]$ProjectName,
  [string]$TestList,
  [switch]$RunAllUnitTests,
  [switch]$RunAllServerTests,
  [switch]$RunAllReleaseResourcesTests,
  [switch]$RunAllCodedUITests,
  [switch]$RunWarewolfServiceTests
)
$JobSpecs = @{}
#CI
$JobSpecs["Other Specs"]		 				= "Dev2.*.Specs,Warewolf.*.Specs", "(TestCategory!=ExampleWorkflowExecution)&(TestCategory!=WorkflowExecution)&(TestCategory!=SubworkflowExecution)"
$JobSpecs["Example Workflow Execution Specs"] 	= "Dev2.*.Specs,Warewolf.*.Specs", "(TestCategory=ExampleWorkflowExecution)"
$JobSpecs["Subworkflow Execution Specs"]		= "Dev2.*.Specs,Warewolf.*.Specs", "(TestCategory=SubworkflowExecution)"
$JobSpecs["Workflow Execution Specs"]		 	= "Dev2.*.Specs,Warewolf.*.Specs", "(TestCategory=WorkflowExecution)"
$JobSpecs["Other Unit Tests"] 				 	= "Dev2.*.Tests,Warewolf.*.Tests"
$JobSpecs["COMIPC Unit Tests"]				 	= "Warewolf.COMIPC.Tests"
$JobSpecs["Studio View Models Unit Tests"]	 	= "Warewolf.Studio.ViewModels.Tests"
$JobSpecs["Activity Designers Unit Tests"]	 	= "Dev2.Activities.Designers.Tests"
$JobSpecs["Activities Unit Tests"]				= "Dev2.Activities.Tests"
$JobSpecs["Tools Specs"]		 				= "Warewolf.ToolsSpecs"
$JobSpecs["UI Binding Tests"] 				 	= "Warewolf.UIBindingTests.*"
$JobSpecs["Integration Tests"]				 	= "Dev2.IntegrationTests"
#Coded UI
$JobSpecs["Other UI Tests"]					    = "Warewolf.UITests", "(TestCategory!=Tools)&(TestCategory!=Data Tools)&(TestCategory!=Database Tools)&(TestCategory!=Dropbox Tools)&(TestCategory!=File Tools)&(TestCategory!=HTTP Tools)&(TestCategory!=Recordset Tools)&(TestCategory!=Sharepoint Tools)&(TestCategory!=Utility Tools)&(TestCategory!=Explorer)&(TestCategory!=Tabs and Panes)&(TestCategory!=Deploy)&(TestCategory!=Debug Input)&(TestCategory!=Workflow Testing)&(TestCategory!=Default Layout)&(TestCategory!=Resource Tools)&(TestCategory!=Save Dialog)&(TestCategory!=Shortcut Keys)&(TestCategory!=Settings)&(TestCategory!=Dependency Graph)&(TestCategory!=Variables)&(TestCategory!=Email Tools)&(TestCategory!=Plugin Sources)&(TestCategory!=Web Sources)&(TestCategory!=Database Sources)&(TestCategory!=Workflow Mocking Tests)&(TestCategory!=Assign Tool)&(TestCategory!=Control Flow Tools)&(TestCategory!=DotNet Connector Mocking Tests)&(TestCategory!=DotNet Connector Tool)&(TestCategory!=Hello World Mocking Tests)&(TestCategory!=Server Sources)&(TestCategory!=Source Wizards)"
$JobSpecs["Other UI Specs"]					    = "Warewolf.UISpecs", "(TestCategory!=DBConnector)&(TestCategory!=PluginConnector)&(TestCategory!=WebConnector)&(TestCategory!=Explorer)&(TestCategory!=Deploy)&(TestCategory!=SaveDialog)"
$JobSpecs["Assign Tool UI Tests"]				= "Warewolf.UITests", "(TestCategory=Assign Tool)"
$JobSpecs["Control Flow Tools UI Tests"]		= "Warewolf.UITests", "(TestCategory=Control Flow Tools)"
$JobSpecs["Database Sources UI Tests"]			= "Warewolf.UITests", "(TestCategory=Database Sources)"
$JobSpecs["Database Tools UI Tests"]			= "Warewolf.UITests", "(TestCategory=Database Tools)"
$JobSpecs["Data Tools UI Tests"]				= "Warewolf.UITests", "(TestCategory=Data Tools)"
$JobSpecs["DB Connector UI Specs"]				= "Warewolf.UISpecs", "(TestCategory=DBConnector)"
$JobSpecs["Debug Input UI Tests"]				= "Warewolf.UITests", "(TestCategory=Debug Input)"
$JobSpecs["Default Layout UI Tests"]			= "Warewolf.UITests", "(TestCategory=Default Layout)"
$JobSpecs["Dependency Graph UI Tests"]			= "Warewolf.UITests", "(TestCategory=Dependency Graph)"
$JobSpecs["Deploy UI Specs"]					= "Warewolf.UISpecs", "(TestCategory=Deploy)"
$JobSpecs["Deploy UI Tests"]					= "Warewolf.UITests", "(TestCategory=Deploy)"
$JobSpecs["DotNet Connector Mocking UI Tests"]	= "Warewolf.UITests", "(TestCategory=DotNet Connector Mocking Tests)"
$JobSpecs["DotNet Connector Tool UI Tests"]	    = "Warewolf.UITests", "(TestCategory=DotNet Connector Tool)"
$JobSpecs["Dropbox Tools UI Tests"]			    = "Warewolf.UITests", "(TestCategory=Dropbox Tools)"
$JobSpecs["Email Tools UI Tests"]				= "Warewolf.UITests", "(TestCategory=Email Tools)"
$JobSpecs["Explorer UI Specs"]					= "Warewolf.UISpecs", "(TestCategory=Explorer)"
$JobSpecs["Explorer UI Tests"]					= "Warewolf.UITests", "(TestCategory=Explorer)"
$JobSpecs["File Tools UI Tests"]				= "Warewolf.UITests", "(TestCategory=File Tools)"
$JobSpecs["Hello World Mocking UI Tests"]		= "Warewolf.UITests", "(TestCategory=Hello World Mocking Tests)"
$JobSpecs["HTTP Tools UI Tests"]				= "Warewolf.UITests", "(TestCategory=HTTP Tools)"
$JobSpecs["Plugin Sources UI Tests"]			= "Warewolf.UITests", "(TestCategory=Plugin Sources)"
$JobSpecs["Recordset Tools UI Tests"]			= "Warewolf.UITests", "(TestCategory=Recordset Tools)"
$JobSpecs["Resource Tools UI Tests"]			= "Warewolf.UITests", "(TestCategory=Resource Tools)"
$JobSpecs["Save Dialog UI Specs"]				= "Warewolf.UISpecs", "(TestCategory=SaveDialog)"
$JobSpecs["Save Dialog UI Tests"]				= "Warewolf.UITests", "(TestCategory=Save Dialog)"
$JobSpecs["Server Sources UI Tests"]			= "Warewolf.UITests", "(TestCategory=Server Sources)"
$JobSpecs["Settings UI Tests"]					= "Warewolf.UITests", "(TestCategory=Settings)"
$JobSpecs["Sharepoint Tools UI Tests"]			= "Warewolf.UITests", "(TestCategory=Sharepoint Tools)"
$JobSpecs["Shortcut Keys UI Tests"]			    = "Warewolf.UITests", "(TestCategory=Shortcut Keys)"
$JobSpecs["Source Wizards UI Tests"]			= "Warewolf.UITests", "(TestCategory=Source Wizards)"
$JobSpecs["Tabs And Panes UI Tests"]			= "Warewolf.UITests", "(TestCategory=Tabs and Panes)"
$JobSpecs["Tools UI Tests"]					    = "Warewolf.UITests", "(TestCategory=Tools)"
$JobSpecs["Utility Tools UI Tests"]			    = "Warewolf.UITests", "(TestCategory=Utility Tools)"
$JobSpecs["Variables UI Tests"]				    = "Warewolf.UITests", "(TestCategory=Variables)"
$JobSpecs["Web Connector UI Specs"]			    = "Warewolf.UISpecs", "(TestCategory=WebConnector)"
$JobSpecs["Web Sources UI Tests"]				= "Warewolf.UITests", "(TestCategory=Web Sources)"
$JobSpecs["Workflow Mocking Tests UI Tests"]	= "Warewolf.UITests", "(TestCategory=Workflow Mocking Tests)"
$JobSpecs["Workflow Testing UI Tests"]			= "Warewolf.UITests", "(TestCategory=Workflow Testing)"
#Security
$JobSpecs["Conflicting Contribute View And Execute Permissions Security Specs"] = "Warewolf.SecuritySpecs", "(TestCategory=ConflictingContributeViewExecutePermissionsSecurity)"
$JobSpecs["Conflicting Execute Permissions Security Specs"]					    = "Warewolf.SecuritySpecs", "(TestCategory=ConflictingExecutePermissionsSecurity)"
$JobSpecs["Conflicting View And Execute Permissions Security Specs"]			= "Warewolf.SecuritySpecs", "(TestCategory=ConflictingViewExecutePermissionsSecurity)"
$JobSpecs["Conflicting View Permissions Security Specs"]						= "Warewolf.SecuritySpecs", "(TestCategory=ConflictingViewPermissionsSecurity)"
$JobSpecs["No Conflicting Permissions Security Specs"]							= "Warewolf.SecuritySpecs", "(TestCategory=NoConflictingPermissionsSecurity)"
$JobSpecs["Overlapping User Groups Permissions Security Specs"]					= "Warewolf.SecuritySpecs", "(TestCategory=OverlappingUserGroupsPermissionsSecurity)"
$JobSpecs["Resource Permissions Security Specs"]								= "Warewolf.SecuritySpecs", "(TestCategory=ResourcePermissionsSecurity)"
$JobSpecs["Server Permissions Security Specs"]									= "Warewolf.SecuritySpecs", "(TestCategory=ServerPermissionsSecurity)"

$UnitTestJobNames = "Other Unit Tests,COMIPC Unit Tests,Studio View Models Unit Tests,Activity Designers Unit Tests,Activities Unit Tests,Tools Specs,UI Binding Tests"
$ServerTestJobNames = "Other Specs,Subworkflow Execution Specs,Workflow Execution Specs,Integration Tests"
$ReleaseResourcesJobNames = "Example Workflow Execution Specs,Conflicting Contribute View And Execute Permissions Security Specs,Conflicting Execute Permissions Security Specs,Conflicting View And Execute Permissions Security Specs,Conflicting View Permissions Security Specs,No Conflicting Permissions Security Specs,Overlapping User Groups Permissions Security Specs,Resource Permissions Security Specs,Server Permissions Security Specs"
$UITestJobNames = "Other UI Tests,Other UI Specs,Assign Tool UI Tests,Control Flow Tools UI Tests,Database Sources UI Tests,Database Tools UI Tests,Data Tools UI Tests,DB Connector UI Specs,Debug Input UI Tests,Default Layout UI Tests,Dependency Graph UI Tests,Deploy UI Specs,Deploy UI Tests,DotNet Connector Mocking UI Tests,DotNet Connector Tool UI Tests,Dropbox Tools UI Tests,Email Tools UI Tests,Explorer UI Specs,Explorer UI Tests,File Tools UI Tests,Hello World Mocking UI Tests,HTTP Tools UI Tests,Plugin Sources UI Tests,Recordset Tools UI Tests,Resource Tools UI Tests,Save Dialog UI Specs,Save Dialog UI Tests,Server Sources UI Tests,Settings UI Tests,Sharepoint Tools UI Tests,Shortcut Keys UI Tests,Source Wizards UI Tests,Tabs And Panes UI Tests,Tools UI Tests,Utility Tools UI Tests,Variables UI Tests,Web Connector UI Specs,Web Sources UI Tests,Workflow Mocking Tests UI Tests,Workflow Testing UI Tests"

if ($RunAllUnitTests.IsPresent) {
    $JobName = $UnitTestJobNames
}
if ($RunAllServerTests.IsPresent) {
    $JobName = $ServerTestJobNames
}
if ($RunAllReleaseResourcesTests.IsPresent) {
    $JobName = $ReleaseResourcesJobNames
}
if ($RunAllCodedUITests.IsPresent) {
    $JobName = $UITestJobNames
}

$ServerExeName = "Warewolf Server.exe"
$ServerPathSpecs = @()
$ServerPathSpecs += $ServerExeName
$ServerPathSpecs += "Server\" + $ServerExeName
$ServerPathSpecs += "DebugServer\" + $ServerExeName
$ServerPathSpecs += "ReleaseServer\" + $ServerExeName
$ServerPathSpecs += "Dev2.Server\bin\Debug\" + $ServerExeName
$ServerPathSpecs += "Bin\Server\" + $ServerExeName
$ServerPathSpecs += "Dev2.Server\bin\Release\" + $ServerExeName
$ServerPathSpecs += "*Server.zip"

$StudioExeName = "Warewolf Studio.exe"
$StudioPathSpecs = @()
$StudioPathSpecs += $StudioExeName
$StudioPathSpecs += "Studio\" + $StudioExeName
$StudioPathSpecs += "DebugStudio\" + $StudioExeName
$StudioPathSpecs += "ReleaseStudio\" + $StudioExeName
$StudioPathSpecs += "Dev2.Studio\bin\Debug\" + $StudioExeName
$StudioPathSpecs += "Bin\Studio\" + $StudioExeName
$StudioPathSpecs += "Dev2.Studio\bin\Release\" + $StudioExeName
$StudioPathSpecs += "*Studio.zip"

if ($JobName.Contains(" DotCover")) {
    $ApplyDotCover = $true
    $JobName = $JobName.Replace(" DotCover", "")
} else {
    $ApplyDotCover = $DotCover.IsPresent
}


If (!(Test-Path "$TestsResultsPath")) {
    New-Item "$TestsResultsPath" -ItemType Directory
}

function FindFile-InParent([string[]]$FileSpecs,[int]$NumberOfParentsToSearch=7) {
	$NumberOfParentsSearched = -1
    $FilePath = ""
	while ($FilePath -eq "" -and $NumberOfParentsSearched++ -lt $NumberOfParentsToSearch -and $CurrentDirectory -ne "") {
        $NumberOfFileSpecsSearched = -1
        while ($FilePath -eq "" -and ++$NumberOfFileSpecsSearched -lt $FileSpecs.Length) {
            $FileSpec = $FileSpecs[$NumberOfFileSpecsSearched]
			if ($FileSpec -ne (Split-Path -Path $FileSpec -NoQualifier)) {
                if (!$CurrentDirectory) {
				    $CurrentDirectory = Split-Path -Path $FileSpec
                }
                $FileSpec = Split-Path -Path $FileSpec -leaf
			} else {
                if (!$CurrentDirectory) {
	                $CurrentDirectory = $PSScriptRoot
                }
            }
		    if (Test-Path "$CurrentDirectory\$FileSpec") {
                $FilePath = "$CurrentDirectory\$FileSpec"                    
		    }
        }
        if ($CurrentDirectory -ne $null -and $CurrentDirectory -ne "" -and (Split-Path -Path $CurrentDirectory -NoQualifier) -ne "\") {
		    if ($FilePath -eq "") {
			    $CurrentDirectory = (Get-Item $CurrentDirectory).Parent.FullName
		    }
        } else {
            $CurrentDirectory = ""
        }
    }
    $FilePath
}

function Cleanup-ServerStudio([bool]$Force=$true) {
    if ($Force) {
        $WaitForCloseTimeout = 10
        $WaitForCloseRetryCount = 1
    } else {
        $WaitForCloseTimeout = 1800
        $WaitForCloseRetryCount = 10
    }
    #Stop Studio
    $Output = ""
    taskkill /im "Warewolf Studio.exe"  2>&1 | %{$Output = $_}

    #Soft Kill
    [int]$i = 0
    [string]$WaitTimeoutMessage = "This command stopped operation because process "
    [string]$WaitOutput = $WaitTimeoutMessage
    while (!($Output.ToString().StartsWith("ERROR: ")) -and $WaitOutput.ToString().StartsWith($WaitTimeoutMessage) -and $i -lt $WaitForCloseRetryCount) {
	    $i += 1
	    Write-Host $Output.ToString()
	    Wait-Process "Warewolf Studio" -Timeout ([math]::Round($WaitForCloseTimeout/$WaitForCloseRetryCount))  2>&1 | %{$WaitOutput = $_}
        $FormatWaitForCloseTimeoutMessage = $WaitOutput.ToString().replace($WaitTimeoutMessage, "")
        if ($FormatWaitForCloseTimeoutMessage -ne "" -and !($FormatWaitForCloseTimeoutMessage.StartsWith("Cannot find a process with the name "))) {
            Write-Host $FormatWaitForCloseTimeoutMessage
        }
	    taskkill /im "Warewolf Studio.exe"  2>&1 |  %{$Output = $_}
    }

    #Force Kill
    taskkill /im "Warewolf Studio.exe" /f  2>&1 | %{if (!($_.ToString().StartsWith("ERROR: "))) {Write-Host $_}}

    #Stop Server
    $ServiceOutput = ""
    sc.exe stop "Warewolf Server" 2>&1 | %{$ServiceOutput += "`n" + $_}
    if ($ServiceOutput -ne "`n[SC] ControlService FAILED 1062:`n`nThe service has not been started.`n") {
        Write-Host $ServiceOutput.TrimStart("`n")
        Wait-Process "Warewolf Server" -Timeout $WaitForCloseTimeout  2>&1 | out-null
    }
    taskkill /im "Warewolf Server.exe" /f  2>&1 | out-null

    #Delete All Studio and Server Resources Except Logs
    $ToClean = "$env:LOCALAPPDATA\Warewolf\DebugData\PersistSettings.dat",
               "$env:LOCALAPPDATA\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml",
               "$env:PROGRAMDATA\Warewolf\Resources",
               "$env:PROGRAMDATA\Warewolf\Tests",
               "$env:PROGRAMDATA\Warewolf\Workspaces",
               "$env:PROGRAMDATA\Warewolf\Server Settings"

    [int]$ExitCode = 0
    foreach ($FileOrFolder in $ToClean) {
	    Remove-Item $FileOrFolder -Recurse -ErrorAction SilentlyContinue
	    if (Test-Path $FileOrFolder) {
		    Write-Error -Message "Cannot delete $FileOrFolder"
		    $ExitCode = 1
	    }	
    }
    if ($ExitCode -eq 1) {
        sleep 30
        exit 1
    }
}

function Copy-On-Write([string]$FilePath) {
    if (Test-Path $FilePath) {
        $num = 1
        $FileExtention = [System.IO.Path]::GetExtension($FilePath)
        $FilePathWithoutExtention = $FilePath.Substring(0, $FilePath.LastIndexOf('.'))
        while(Test-Path "$FilePathWithoutExtention.$num$FileExtention")
        {
            $num += 1
        }
        $FilePath | Move-Item -Destination "$FilePathWithoutExtention.$num$FileExtention"
    }
}

function Move-File-To-TestResults([string]$SourceFilePath, [string]$DestinationFileName) {
    $DestinationFilePath = "$TestsResultsPath\$DestinationFileName"
    if (Test-Path $SourceFilePath) {
        Copy-On-Write $DestinationFilePath
        Move-Item "$SourceFilePath" "$DestinationFilePath"
    }
}

function Move-Artifacts-To-TestResults([bool]$DotCover, [bool]$Server, [bool]$Studio) {
    if (Test-Path "$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\TestResults\*.trx") {
        Move-Item "$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\TestResults\*.trx" "$TestsResultsPath"
        Write-Host Moved loose TRX files from VS install directory into TestResults.
    }

    # Write failing tests playlistfunction Move-Artifacts-T.
    Write-Host Writing all test failures in `"$TestsResultsPath`" to a playlist file

    $PlayList = "<Playlist Version=`"1.0`">"
    Get-ChildItem "$TestsResultsPath" -Filter *.trx | `
    Foreach-Object{
        $FullTRXFilePath = $_.FullName
	    [xml]$trxContent = Get-Content "$FullTRXFilePath"
	    if ($trxContent.TestRun.Results.UnitTestResult.count -gt 0) {
	        foreach($TestResult in $trxContent.TestRun.Results.UnitTestResult) {
		        if ($TestResult.outcome -eq "Failed") {
		            if ($trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.count -gt 0) {
		                foreach($TestDefinition in $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod) {
			                if ($TestDefinition.name -eq $TestResult.testName) {
				                $PlayList += "<Add Test=`"" + $TestDefinition.className + "." + $TestDefinition.name + "`" />"
			                }
		                }
                    } else {
			            Write-Host Error parsing TestRun.TestDefinitions.UnitTest.TestMethod from trx file at $_.FullName
			            Continue
		            }
		        }
	        }
	    } elseif ($trxContent.TestRun.Results.UnitTestResult.outcome -eq "Failed") {
            $PlayList += "<Add Test=`"" + $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.className + "." + $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.name + "`" />"
        } else {
		    Write-Host Error parsing TestRun.Results.UnitTestResult from trx file at $_.FullName
        }
    }
    $PlayList += "</Playlist>"
    $OutPlaylistPath = $TestsResultsPath + "\" + $JobName + " Failures.playlist"
    Copy-On-Write $OutPlaylistPath
    $PlayList | Out-File -LiteralPath $OutPlaylistPath -Encoding utf8 -Force
    Write-Host Playlist file written to `"$OutPlaylistPath`".
    if ($Server) {
        Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\wareWolf-Server.log" "$JobName Server.log"
    }
    if ($Studio) {
        Move-File-To-TestResults "$env:LocalAppData\Warewolf\Studio Logs\Warewolf Studio.log" "$JobName Studio.log"
    }
    if ($Server -and $DotCover) {
        $ServerSnapshot = "$env:ProgramData\Warewolf\Server Log\dotCover.dcvr"
        Write-Host Trying to move Server coverage snapshot file from $ServerSnapshot to $TestsResultsPath\$JobName Server DotCover.dcvr
        while (!(Test-Path $ServerSnapshot) -and $Timeout++ -lt 10) {
            sleep 10
        }
        $locked = $true
        $RetryCount = 0
        while($locked -and $RetryCount -lt 12) {
            $RetryCount++
            try {
                [IO.File]::OpenWrite($ServerSnapshot).close()
                $locked = $false
            } catch {
                Sleep 10
            }
        }
        if (!($locked)) {
            Write-Host Moving Server coverage snapshot file from $ServerSnapshot to $TestsResultsPath\$JobName Server DotCover.dcvr
            Move-File-To-TestResults $ServerSnapshot "$JobName Server DotCover.dcvr"
        } else {
            Write-Host Server Coverage Snapshot File still locked after retrying for 2 minutes.
        }
        if (Test-Path "$env:ProgramData\Warewolf\Server Log\dotCover.log") {
            Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\dotCover.log" "$JobName Server DotCover.log"
        }
    }
    if ($Studio -and $DotCover) {
        $StudioSnapshot = "$env:LocalAppData\Warewolf\Studio Logs\dotCover.dcvr"
        Write-Host Trying to move Studio coverage snapshot file from $StudioSnapshot to $TestsResultsPath\$JobName Studio DotCover.dcvr
        while (!(Test-Path $StudioSnapshot) -and $Timeout++ -lt 10) {
            sleep 10
        }
        if (Test-Path $StudioSnapshot) {
            $locked = $true
            $RetryCount = 0
            while($locked -and $RetryCount -lt 12) {
                $RetryCount++
                try {
                    [IO.File]::OpenWrite($StudioSnapshot).close()
                    $locked = $false
                } catch {
                    Sleep 10
                }
            }
            if (!($locked)) {
                Write-Host Moving Studio coverage snapshot file from $StudioSnapshot to $TestsResultsPath\$JobName Studio DotCover.dcvr
                Move-Item $StudioSnapshot "$TestsResultsPath\$JobName Studio DotCover.dcvr" -force
            } else {
                Write-Host Studio Coverage Snapshot File is locked.
            }
        } else {
            Write-Host Studio coverage snapshot not found at $StudioSnapshot
        }
        if (Test-Path "$env:LocalAppData\Warewolf\Studio Logs\dotCover.log") {
            Move-File-To-TestResults "$env:LocalAppData\Warewolf\Studio Logs\dotCover.log" "$JobName Studio DotCover.log"
        }
    }
    if ($Server -and $Studio -and $DotCover) {
		$MergedSnapshot = "$TestsResultsPath\$JobName Merged Server and Studio DotCover.dcvr"
		Copy-On-Write "$MergedSnapshot"
        &"$DotCoverPath" "merge" "/Source=`"$TestsResultsPath\$JobName Server DotCover.dcvr`";`"$TestsResultsPath\$JobName Studio DotCover.dcvr`"" "/Output=`"$MergedSnapshot`"" "/LogFile=`"$TestsResultsPath\ServerAndStudioDotCoverSnapshotMerge.log`""
    }
    if ($RecordScreen.IsPresent) {
        Move-ScreenRecordings-To-TestResults
    }
    if (Test-Path "$TestResultsPath\..\*.bat") {
        foreach ($testRunner in (Get-ChildItem "$TestResultsPath\..\*.bat")) {
	        Move-File-To-TestResults $testRunner.FullName $testRunner.Name
        }
    }
}

function Move-ScreenRecordings-To-TestResults {
    Write-Host Getting UI test screen recordings from `"$TestsResultsPath`"
    $ScreenRecordingsFolder = "$TestsResultsPath\ScreenRecordings"
    if (Test-Path $ScreenRecordingsFolder\In) {
        Move-Item $ScreenRecordingsFolder\In\* $ScreenRecordingsFolder -Force
        Remove-Item $ScreenRecordingsFolder\In
    } else {
        Write-Host $ScreenRecordingsFolder\In not found.
    }
}

function Install-Server([string]$ServerPath,[string]$ResourcesType) {
    if ($ServerPath -eq "" -or !(Test-Path $ServerPath)) {
        $ServerPath = FindFile-InParent $ServerPathSpecs
        if ($ServerPath.EndsWith(".zip")) {
			Expand-Archive "$ServerPath" "$TestsResultsPath\Server" -Force
			$ServerPath = "$TestsResultsPath\Server\" + $ServerExeName
		}
        if ($ServerPath -eq "" -or !(Test-Path $ServerPath)) {
            Write-Error -Message "Cannot find Warewolf Server.exe. Please provide a path to that file as a commandline parameter like this: -ServerPath"
            sleep 30
            exit 1
        }
    }
    if ($ResourcesType -eq "") {
	    $title = "Server Resources"
	    $message = "What type of resources would you like to install the server with?"

	    $UITest = New-Object System.Management.Automation.Host.ChoiceDescription "&UITest", `
		    "Uses these resources for running UI Tests."

	    $ServerTest = New-Object System.Management.Automation.Host.ChoiceDescription "&ServerTests", `
		    "Uses these resources for running everything except unit tests and Coded UI tests."

	    $Release = New-Object System.Management.Automation.Host.ChoiceDescription "&Release", `
		    "Uses these resources for Warewolf releases."

	    $options = [System.Management.Automation.Host.ChoiceDescription[]]($UITest, $ServerTest, $Release)

	    $result = $host.ui.PromptForChoice($title, $message, $options, 0) 

	    switch ($result)
		    {
			    0 {$ResourcesType = "UITests"}
			    1 {$ResourcesType = "ServerTests"}
			    2 {$ResourcesType = "Release"}
		    }
    }

    $ServerService = Get-Service "Warewolf Server" -ErrorAction SilentlyContinue
    if (!$ApplyDotCover) {
        if ($ServerService -eq $null) {
            New-Service -Name "Warewolf Server" -BinaryPathName "$ServerPath" -StartupType Manual
        } else {    
		    Write-Host Configuring service to $ServerPath
		    $ServiceOuput = sc.exe config "Warewolf Server" binPath= "$ServerPath"
        }
    } else {
        $ServerBinDir = (Get-Item $ServerPath).Directory.FullName 
        $RunnerXML = @"
<AnalyseParams>
    <TargetExecutable>$ServerPath</TargetExecutable>
    <Output>$env:ProgramData\Warewolf\Server Log\dotCover.dcvr</Output>
    <Scope>
	    <ScopeEntry>$ServerBinDir\**\*.dll</ScopeEntry>
	    <ScopeEntry>$ServerBinDir\**\*.exe</ScopeEntry>
    </Scope>
</AnalyseParams>
"@

        if (!$JobName) {
			if ($ProjectName) {
				$JobName = $ProjectName
			} else {
				$JobName = "Manual Tests"
			}
        }
        $DotCoverRunnerXMLPath = "$TestsResultsPath\Server DotCover Runner.xml"
        Copy-On-Write $DotCoverRunnerXMLPath
        Out-File -LiteralPath "$DotCoverRunnerXMLPath" -Encoding default -InputObject $RunnerXML
        $BinPathWithDotCover = "\`"" + $DotCoverPath + "\`" cover \`"$DotCoverRunnerXMLPath\`" /LogFile=\`"$TestsResultsPath\ServerDotCover.log\`""
        if ($ServerService -eq $null) {
            New-Service -Name "Warewolf Server" -BinaryPathName "$BinPathWithDotCover" -StartupType Manual
	    } else {
		    Write-Host Configuring service to $BinPathWithDotCover
		    $ServiceOuput = sc.exe config "Warewolf Server" binPath= "$BinPathWithDotCover"
	    }
    }
    if ($ServerUsername -ne "" -and $ServerPassword -eq "") {
        $ServiceOuput = sc.exe config "Warewolf Server" obj= "$ServerUsername"
    }
    if ($ServerUsername -ne "" -and $ServerPassword -ne "") {
        $ServiceOuput = sc.exe config "Warewolf Server" obj= "$ServerUsername" password= "$ServerPassword"
    }

    $ResourcePathSpecs = @()
    foreach ($ServerPathSpec in $ServerPathSpecs) {
        if ($ServerPathSpec.EndsWith($ServerExeName)) {
            $ResourcePathSpecs += $ServerPathSpec.Replace($ServerExeName, "Resources - $ResourcesType")
        }
    }
    $ResourcesDirectory = FindFile-InParent $ResourcePathSpecs
    if ($ResourcesDirectory -ne "" -and $ResourcesDirectory -ne (Get-Item $ServerPath).Directory.FullName + "\" + (Get-Item $ResourcesDirectory).Name ) {
        Copy-Item -Path "$ResourcesDirectory" -Destination (Get-Item $ServerPath).Directory.FullName -Recurse -Force
    }
    $ServerPath,$ResourcesType
}

function Start-Server([string]$ServerPath,[string]$ResourcesType) {
    Write-Host Cleaning up old resources in Warewolf ProgramData and copying in new resources from ((Get-Item $ServerPath).Directory.FullName + "\Resources - $ResourcesType\*").
    Cleanup-ServerStudio
    Copy-Item -Path ((Get-Item $ServerPath).Directory.FullName + "\Resources - $ResourcesType\*") -Destination "$env:ProgramData\Warewolf" -Recurse -Force
	
    Start-Service "Warewolf Server"
    Write-Host Server has started.

    #Check if started
    $Output = @()
    sc.exe interrogate "Warewolf Server" 2>&1 | %{$Output += $_}
    if ($Output.Length -lt 4 -or !($Output[3].EndsWith("RUNNING "))) {
        sc.exe start "Warewolf Server"
    }

    #Wait for the ServerStarted file to appear.
    $TimeoutCounter = 0
    $ServerStartedFilePath = (Get-Item $ServerPath).Directory.FullName + "\ServerStarted"
    while (!(Test-Path $ServerStartedFilePath) -and $TimeoutCounter++ -lt 100) {
        sleep 3
    }
    if (!(Test-Path $ServerStartedFilePath)) {
        Write-Error -Message "Server Cannot Start."
        sleep 30
        exit 1
    }
}

function Start-Studio {
    if ($StudioPath -eq "" -or !(Test-Path $StudioPath)) {
        $StudioPath = FindFile-InParent $StudioPathSpecs
        if ($StudioPath.EndsWith(".zip")) {
	        Expand-Archive "$StudioPath" "$TestsResultsPath\Studio" -Force
	        $StudioPath = "$TestsResultsPath\Studio\" + $StudioExeName
        }
        if ($ServerPath -eq "" -or !(Test-Path $StudioPath)) {
            Write-Error -Message "Studio path not found: $StudioPath"
            sleep 30
            exit 1
        }
    }
	if ($StudioPath -eq "") {
		Write-Error -Message "Cannot find Warewolf Studio. To run the studio provide a path to the Warewolf Studio exe file as a commandline parameter like this: -StudioPath"
        sleep 30
		exit 1
	}
    $StudioLogFile = "$env:LocalAppData\Warewolf\Studio Logs\Warewolf Studio.log"
    Copy-On-Write $StudioLogFile
	if (!$ApplyDotCover) {
		Start-Process "$StudioPath"
	} else {
        $StudioBinDir = (Get-Item $StudioPath).Directory.FullName 
        $RunnerXML = @"
<AnalyseParams>
    <TargetExecutable>$StudioPath</TargetExecutable>
    <Output>$env:LocalAppData\Warewolf\Studio Logs\dotCover.dcvr</Output>
    <Scope>
    	<ScopeEntry>$StudioBinDir\**\*.dll</ScopeEntry>
    	<ScopeEntry>$StudioBinDir\**\*.exe</ScopeEntry>
    </Scope>
</AnalyseParams>
"@
        $DotCoverRunnerXMLPath = "$TestsResultsPath\Studio DotCover Runner.xml"
        Copy-On-Write $DotCoverRunnerXMLPath
        Out-File -LiteralPath "$DotCoverRunnerXMLPath" -Encoding default -InputObject $RunnerXML
		Start-Process $DotCoverPath "cover `"$DotCoverRunnerXMLPath`" /LogFile=`"$TestsResultsPath\StudioDotCover.log`""
    }
    $i = 0
    while (!(Test-Path $StudioLogFile) -and $i++ -lt 10){
        Write-Warning "Waiting for Studio to start..."
        Sleep 3
    }
    if (Test-Path $StudioLogFile) {
	    Write-Host Studio has started.
    } else {
		Write-Error -Message "Warewolf studio failed to start before the timeout."
        sleep 30
		exit 1
    }
}

function AssemblyIsNotAlreadyDefinedWithoutWildcards([string]$AssemblyNameToCheck) {
    $JobAssemblySpecs = @()
    foreach ($Job in $JobSpecs.Values) {
        if ($Job.Count -eq 2) {
            $Job = $Job[0]
        }
        if (!$Job.Contains("*") -and !$JobAssemblySpecs.Contains($Job)) {
            $JobAssemblySpecs += $Job
        }
    }
    !$JobAssemblySpecs.Contains($AssemblyNameToCheck)
}

function Resolve-Project-Folder-Specs([string]$ProjectFolderSpec) {
    $TestAssembliesList = ""
    $ProjectFolderSpecInParent = FindFile-InParent $ProjectFolderSpec
    if ($ProjectFolderSpecInParent -ne "") {
        if ($ProjectFolderSpecInParent.Contains("*")) {
            foreach ($projectFolder in Get-ChildItem $ProjectFolderSpecInParent -Directory) {
                if ((Test-Path $VSTestPath) -and !$MSTest.IsPresent) {
		            $TestAssembliesList += " `"" + $projectFolder.FullName + "\bin\Debug\" + $projectFolder.Name + ".dll`""
                } else {
		            $TestAssembliesList += " /testcontainer:`"" + $projectFolder.FullName + "\bin\Debug\" + $projectFolder.Name + ".dll`""
                }
                if (!$TestAssembliesDirectories.Contains($projectFolder.FullName + "\bin\Debug")) {
                    $TestAssembliesDirectories += $projectFolder.FullName + "\bin\Debug"
                }
            }
        } else {
            if ((Test-Path $VSTestPath) -and !$MSTest.IsPresent) {
		        $TestAssembliesList += " `"" + $ProjectFolderSpecInParent + "\bin\Debug\" + (Get-Item $ProjectFolderSpecInParent).Name + ".dll`""
            } else {
		        $TestAssembliesList += " /testcontainer:`"" + $ProjectFolderSpecInParent + "\bin\Debug\" + (Get-Item $ProjectFolderSpecInParent).Name + ".dll`""
            }
            if (!$TestAssembliesDirectories.Contains($ProjectFolderSpecInParent + "\bin\Debug")) {
                $TestAssembliesDirectories += $ProjectFolderSpecInParent + "\bin\Debug"
            }
        }
        $TestAssembliesList,$TestAssembliesDirectories
    }
}

function Resolve-Test-Assembly-File-Specs([string]$TestAssemblyFileSpecs) {
    $TestAssembliesList = ""
    $TestAssembliesDirectories = @()
    $TestAssembliesFileSpecsInParent = FindFile-InParent $TestAssemblyFileSpecs
    if ($TestAssembliesFileSpecsInParent -ne "") {
        foreach ($file in Get-ChildItem $TestAssembliesFileSpecsInParent) {
            $AssemblyNameToCheck = $file.Name.replace($file.extension, "")
            if (!$TestAssembliesFileSpecsInParent.Contains("*") -or (AssemblyIsNotAlreadyDefinedWithoutWildcards $AssemblyNameToCheck)) {
                if (!$MSTest.IsPresent) {
		            $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
                } else {
		            $TestAssembliesList += " /testcontainer:`"" + $file.FullName + "`""
                }
                if (!$TestAssembliesDirectories.Contains($file.Directory.FullName)) {
                    $TestAssembliesDirectories += $file.Directory.FullName
                }
	        }
        }
        $TestAssembliesList,$TestAssembliesDirectories
    }
}

#Unpack jobs
$JobNames = @()
$JobAssemblySpecs = @()
$JobCategories = @()
if ($JobName -ne $null -and $JobName -ne "") {
    foreach ($Job in $JobName.Split(",")) {
        $Job = $Job.TrimEnd("1234567890 ")
        if ($JobSpecs.ContainsKey($Job)) {
            $JobNames += $Job
            if ($JobSpecs[$Job].Count -eq 1) {
                $JobAssemblySpecs += $JobSpecs[$Job]
                $JobCategories += ""
            } else {
                $JobAssemblySpecs += $JobSpecs[$Job][0]
                $JobCategories += $JobSpecs[$Job][1]
            }
        } else {
            Write-Warning "Unrecognized Job $Job was ignored from the run"
        }
    }
}
if ($ProjectName) {
    $JobNames += $ProjectName
    $JobAssemblySpecs += $ProjectName
    if ($Category -ne $null -and $Category -ne "") {
        $JobCategories += $Category
    } else {
        $JobCategories += ""
    }
}
$TotalNumberOfJobsToRun = $JobNames.length
if ($TotalNumberOfJobsToRun -gt 0) {
    if (!(Test-Path $VSTestPath) -and !(Test-Path $MSTestPath)) {
        Write-Error -Message "Error cannot find VSTest.console.exe or MSTest.exe. Use either -VSTestPath `'`' or -MSTestPath `'`' parameters to pass paths to one of those files."
        sleep 30
        exit 1
    }

    if ($ApplyDotCover -and $DotCoverPath -ne "" -and !(Test-Path $DotCoverPath)) {
        Write-Error -Message "Error cannot find dotcover.exe. Use -DotCoverPath `'`' parameter to pass a path to that file."
        sleep 30
        exit 1
    }

    if (Test-Path "$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\TestResults\*.trx") {
        Remove-Item "$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\TestResults\*.trx"
        Write-Host Removed loose TRX files from VS install directory.
    }

    if (($StartServer.IsPresent -or $StartStudio.IsPresent) -and !$Parallelize.IsPresent) {
        $ServerPath,$ResourcesType = Install-Server $ServerPath $ResourcesType
    }

    if (!$MSTest.IsPresent) {
        # Read playlists and args.
        if ($TestList = "") {
            if ($Args.Count -gt 0) {
                $TestList = $Args.ForEach({ "," + $_ })
            } else {
                Get-ChildItem "$PSScriptRoot" -Filter *.playlist | `
                Foreach-Object{
	                [xml]$playlistContent = Get-Content $_.FullName
	                if ($playlistContent.Playlist.Add.count -gt 0) {
	                    foreach( $TestName in $playlistContent.Playlist.Add) {
		                    $TestList += "," + $TestName.Test.SubString($TestName.Test.LastIndexOf(".") + 1)
	                    }
	                } else {        
                        if ($playlistContent.Playlist.Add.Test -ne $null) {
                            $TestList = " /Tests:" + $playlistContent.Playlist.Add.Test.SubString($playlistContent.Playlist.Add.Test.LastIndexOf(".") + 1)
                        } else {
	                        Write-Host Error parsing Playlist.Add from playlist file at $_.FullName
                        }
                    }
                }
            }
            if ($TestList.StartsWith(",")) {
	            $TestList = $TestList -replace "^.", " /Tests:"
            }
        }
    } else {
        if ($TestList = "") {
            Get-ChildItem "$PSScriptRoot" -Filter *.playlist | `
            Foreach-Object{
	            [xml]$playlistContent = Get-Content $_.FullName
	            if ($playlistContent.Playlist.Add.count -gt 0) {
	                foreach( $TestName in $playlistContent.Playlist.Add) {
		                $TestList += " /test:" + $TestName.Test.SubString($TestName.Test.LastIndexOf(".") + 1)
	                }
	            } else {        
                    if ($playlistContent.Playlist.Add.Test -ne $null) {
                        $TestList = " /test:" + $playlistContent.Playlist.Add.Test.SubString($playlistContent.Playlist.Add.Test.LastIndexOf(".") + 1)
                    } else {
	                    Write-Host Error parsing Playlist.Add from playlist file at $_.FullName
                    }
                }
            }
        }
    }
    foreach ($_ in 0..($TotalNumberOfJobsToRun-1)) {
        $JobName = $JobNames[$_].ToString()
        $ProjectSpec = $JobAssemblySpecs[$_].ToString()
        $TestCategories = $JobCategories[$_].ToString()
        $TestAssembliesList = ""
        $TestAssembliesDirectories = @()
        if (!($TestsPath.EndsWith("\"))) { $TestsPath += "\" }
        foreach ($Project in $ProjectSpec.Split(",")) {
            $TestAssembliesFileSpecs = @()
            $TestAssembliesFileSpecs += $TestsPath + $Project + ".dll"
            $UnPackTestAssembliesList,$UnPackTestAssembliesDirectories = Resolve-Test-Assembly-File-Specs $TestAssembliesFileSpecs
            $TestAssembliesList += $UnPackTestAssembliesList
            if ($UnPackTestAssembliesDirectories.Count -gt 0) {
                $TestAssembliesDirectories += $UnPackTestAssembliesDirectories
            }
            if ($TestAssembliesList -eq "") {
                $ProjectFolderSpec = @()
                $ProjectFolderSpec += $TestsPath + $Project
                $UnPackTestAssembliesList,$UnPackTestAssembliesDirectories = Resolve-Project-Folder-Specs $ProjectFolderSpec
                $TestAssembliesList += $UnPackTestAssembliesList
                if ($UnPackTestAssembliesDirectories.Count -gt 0) {
                    $TestAssembliesDirectories += $UnPackTestAssembliesDirectories
                }
            }
        }
        if ($TestAssembliesList -eq $null -or $TestAssembliesList -eq "") {
	        Write-Host Cannot find any $ProjectSpec project folders or assemblies at $TestsPath.
	        exit 1
        }

        # Setup for remote execution
		$TestSettingsId = [guid]::NewGuid()
        $ControllerNameTag = ""
        $RemoteExecutionAttribute = ""
        $AgentRoleTags = ""
        $AgentRuleNameValue = "LocalMachineDefaultRole"
        $TestTypeSpecificTags = ""
        $DeploymentTags = "`n  <Deployment enabled=`"false`" />"
        $ScriptsTag = ""
        $DataCollectorTags = ""
        $NamingSchemeTag = ""
        $TestRunName = "Run $JobName With Timeout"
		$DeploymentTimeoutAttribute = ""
		$BucketsTag = ""
        if ($StartStudio.IsPresent) {
            $TestsTimeout = "360000"
        } else {
            $TestsTimeout = "180000"
        }
        $HardcodedTestController = "rsaklfsvrdev:6901"
        if ($RecordScreen.IsPresent) {
            $DataCollectorTags = @"

      <DataCollectors>
        <DataCollector uri="datacollector://microsoft/VideoRecorder/1.0" assemblyQualifiedName="Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder.VideoRecorderDataCollector, Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" friendlyName="Screen and Voice Recorder">
          <Configuration>
            <MediaRecorder sendRecordedMediaForPassedTestCase="false" xmlns="" />
          </Configuration>
        </DataCollector>
      </DataCollectors>
"@
            $NamingSchemeTag = "`n  <NamingScheme baseName=`"ScreenRecordings`" appendTimeStamp=`"false`" useDefault=`"false`" />"
            $TestRunName += " and Screen Recording"
        }
        if ($Parallelize.IsPresent) {
            $CleanupScriptPath = "$TestsResultsPath\cleanup.bat"
            $ThisComputerHostname = Hostname            $ResultsPathAsAdminShare = $TestsResultsPath.Replace(":","$")
            $ReverseDeploymentScript = "`nxcopy /S /Y `"%ResultsDirectory%`" `"\\$ThisComputerHostname\$ResultsPathAsAdminShare`"`nrmdir /S /Q `"%DeploymentDirectory%`""                   
			Copy-On-Write $CleanupScriptPath
			New-Item -Force -Path "$CleanupScriptPath" -ItemType File -Value "$ReverseDeploymentScriptLine"
            $ScriptsTag = "`n  <Scripts cleanupScript=`"$CleanupScriptPath`" />"
            $ControllerNameTag = "`n  <RemoteController name=`"$HardcodedTestController`" />"
            $RemoteExecutionAttribute = " location=`"Remote`""
            $AgentRuleNameValue = "Remote"
            $TestTypeSpecificTags = @"

    <TestTypeSpecific>
      <UnitTestRunConfig testTypeId="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b">
        <AssemblyResolution>
          <TestDirectory useLoadContext="true" />
        </AssemblyResolution>
      </UnitTestRunConfig>
    </TestTypeSpecific>
"@
            $DeploymentTags = "`n  <Deployment enabled=`"true`" />"
			$DeploymentTimeoutAttribute = " deploymentTimeout=`"600000`" agentNotRespondingTimeout=`"600000`""
            if ($StartStudio.IsPresent -or $StartServer.IsPresent) {
			    New-Item -Force -Path "$CleanupScriptPath" -ItemType File -Value "powershell -Command `"&'%DeploymentDirectory%\Run Tests.ps1' -Cleanup`"`n$ReverseDeploymentScriptLine"
                if ($ServerUsername -ne "") {
                    $ServerUsernameParam = " -ServerUsername '" + $ServerUsername + "'"
                } else {
                    $ServerUsernameParam = ""
                }
                if ($ServerPassword -ne "") {
                    $ServerPasswordParam = " -ServerPassword '" + $ServerPassword + "'"
                } else {
                    $ServerPasswordParam = ""
                }
                $StartupScriptPath = "$TestsResultsPath\startup.bat"
                $ScriptsTag = "`n  <Scripts setupScript=`"$StartupScriptPath`" cleanupScript=`"$CleanupScriptPath`" />"
                if ($StartStudio.IsPresent) {
                    $AgentRoleTags = @"

      <SelectionCriteria>
        <AgentProperty name="UI" value="" />
      </SelectionCriteria>
"@
                    $DeploymentTags = @"

  <Deployment>
    <DeploymentItem filename="..\DebugServer.zip" />
    <DeploymentItem filename="..\DebugStudio.zip" />
    <DeploymentItem filename="DebugServer.zip" />
    <DeploymentItem filename="DebugStudio.zip" />
    <DeploymentItem filename="..\Server.zip" />
    <DeploymentItem filename="..\Studio.zip" />
    <DeploymentItem filename="Server.zip" />
    <DeploymentItem filename="Studio.zip" />
    <DeploymentItem filename="Run Tests.ps1" />
  </Deployment>
"@
                    Copy-On-Write $StartupScriptPath
                    New-Item -Force -Path "$StartupScriptPath" -ItemType File -Value "powershell -Command `"&'%DeploymentDirectory%\Run Tests.ps1' -StartStudio -ResourcesType $ResourcesType$ServerUsernameParam$ServerPasswordParam`""
					$BucketsTag = @"

    <Buckets size="1" threshold="1"/>
"@
                } else {
                    $DeploymentTags = @"

  <Deployment>
    <DeploymentItem filename="..\DebugServer.zip" />
    <DeploymentItem filename="DebugServer.zip" />
    <DeploymentItem filename="..\Server.zip" />
    <DeploymentItem filename="Server.zip" />
    <DeploymentItem filename="Run Tests.ps1" />
  </Deployment>
"@
                    Copy-On-Write $StartupScriptPath
                    New-Item -Force -Path "$StartupScriptPath" -ItemType File -Value "powershell -Command `"&'%DeploymentDirectory%\Run Tests.ps1' -StartServer -ResourcesType $ResourcesType$ServerUsernameParam$ServerPasswordParam`""
                }
            } else {

            }
        }

        # Create test settings.
        $TestSettingsFile = ""
        if (!$DisableTimeouts.IsPresent) {
            $TestSettingsFile = "$TestsResultsPath\$JobName.testsettings"
            Copy-On-Write $TestSettingsFile
            [system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings id="$TestSettingsId" name="$JobName" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010" abortRunOnError="false">
  <Description>$TestRunName.</Description>$DeploymentTags$NamingSchemeTag$ScriptsTag$ControllerNameTag
  <Execution$RemoteExecutionAttribute>$BucketsTag
    <Timeouts testTimeout="$TestsTimeout"$DeploymentTimeoutAttribute/>$TestTypeSpecificTags
    <AgentRule name="$AgentRuleNameValue">$AgentRoleTags$DataCollectorTags
    </AgentRule>
  </Execution>
</TestSettings>
"@)
        }
        if (!$MSTest.IsPresent) {
            #Resolve test results file name
            Set-Location -Path "$TestsResultsPath\.."

            # Create full VSTest argument string.
            if ($TestCategories -ne "") {
				if (!$TestCategories.StartsWith("(TestCategory")) {
					$TestCategories = "(TestCategory=" + $TestCategories  + ")"
				}
                $TestCategories = " /TestCaseFilter:`"$TestCategories`""
            }
            if($TestSettingsFile -ne "") {
                $TestSettings =  " /Settings:`"" + $TestSettingsFile + "`""
            }
            if ($Parallelize.IsPresent -and !$StartStudio.IsPresent -and $DisableTimeouts.IsPresent) {
                $ParallelSwitch = " /Parallel"
            } else {
                $ParallelSwitch = ""
            }
            $FullArgsList = $TestAssembliesList + " /logger:trx" + $TestList + $TestSettings + $TestCategories + $ParallelSwitch

            # Write full command including full argument string.
            $TestRunnerPath = "$TestsResultsPath\..\Run $JobName.bat"
            Copy-On-Write "$TestRunnerPath"
            Out-File -LiteralPath "$TestRunnerPath" -Encoding default -InputObject `"$VSTestPath`"$FullArgsList
        } else {
            #Resolve test results file name
            $TestResultsFile = $TestsResultsPath + "\" + $JobName + " Results.trx"
            Copy-On-Write $TestResultsFile

            # Create full MSTest argument string.
            if ($TestCategories -ne "") {
				if ($TestCategories.StartsWith("(TestCategory")) {
					$TestCategories = $TestCategories.Replace("(TestCategory", "").Replace("=", "").Replace(")", "")
				}
                $TestCategories = " /category:`"$TestCategories`""
            }
            if($TestSettingsFile -ne "") {
                $TestSettings =  " /testsettings:`"" + $TestSettingsFile + "`""
            }
            $FullArgsList = $TestAssembliesList + " /resultsfile:`"" + $TestResultsFile + "`"" + $TestList + $TestSettings + $TestCategories

            # Write full command including full argument string.
            $TestRunnerPath = "$TestsResultsPath\..\Run $JobName.bat"
            Copy-On-Write "$TestRunnerPath"
            Out-File -LiteralPath "$TestRunnerPath" -Encoding default -InputObject `"$MSTestPath`"$FullArgsList
        }
        if (Test-Path "$TestsResultsPath\..\Run $JobName.bat") {
            if (($StartServer.IsPresent -or $StartStudio.IsPresent) -and !$Parallelize.IsPresent) {
                Start-Server $ServerPath $ResourcesType
                if ($StartStudio.IsPresent) {
                    Start-Studio
                }
            }
            if ($ApplyDotCover -and !$StartServer.IsPresent -and !$StartStudio.IsPresent) {
                # Write DotCover Runner XML 
                $DotCoverArgs = @"
<AnalyseParams>
	<TargetExecutable>$TestsResultsPath\..\Run $JobName.bat</TargetExecutable>
	<Output>$TestsResultsPath\$JobName DotCover Output.dcvr</Output>
	<Scope>
"@
                foreach ($TestAssembliesDirectory in $TestAssembliesDirectories) {
                    $DotCoverArgs += @"

        <ScopeEntry>$TestAssembliesDirectory\**\*.dll</ScopeEntry>
        <ScopeEntry>$TestAssembliesDirectory\**\*.exe</ScopeEntry>
"@
                }
                $DotCoverArgs += @"

    </Scope>
</AnalyseParams>
"@
                $DotCoverRunnerXMLPath = "$TestsResultsPath\$JobName DotCover Runner.xml"
                Copy-On-Write $DotCoverRunnerXMLPath
                Out-File -LiteralPath $DotCoverRunnerXMLPath -Encoding default -InputObject $DotCoverArgs
                
                # Create full DotCover argument string.
                $DotCoverLogFile = "$TestsResultsPath\DotCoverRunner.xml.log"
                Copy-On-Write $DotCoverLogFile
                $FullArgsList = " cover `"$DotCoverRunnerXMLPath`" /LogFile=`"$DotCoverLogFile`""

                #Write DotCover Runner Batch File
                $DotCoverRunnerPath = "$TestsResultsPath\Run $JobName DotCover.bat"
                Copy-On-Write $DotCoverRunnerPath
                Out-File -LiteralPath "$DotCoverRunnerPath" -Encoding default -InputObject `"$DotCoverPath`"$FullArgsList
                
                #Run DotCover Runner Batch File
                &"$DotCoverRunnerPath"
                if ($StartServer.IsPresent -or $StartStudio.IsPresent) {
                    Cleanup-ServerStudio $false
                }
            } else {
                &"$TestRunnerPath"
                if (($StartServer.IsPresent -or $StartStudio.IsPresent) -and !$Parallelize.IsPresent) {
                    Cleanup-ServerStudio
                }
            }
            Move-Artifacts-To-TestResults $ApplyDotCover ($StartServer.IsPresent -or $StartStudio.IsPresent) $StartStudio.IsPresent
        }
    }
}

if ($AssemblyFileVersionsTest.IsPresent) {
    Write-Host Testing Warewolf assembly file versions...
    $HighestReadVersion = "0.0.0.0"
    $LastReadVersion = "0.0.0.0"
    foreach ($file in Get-ChildItem -recurse $TestsPath) {
	    if (($file.Name.EndsWith(".dll") -or ($file.Name.EndsWith(".exe") -and -Not $file.Name.EndsWith(".vshost.exe"))) -and ($file.Name.StartsWith("Dev2.") -or $file.Name.StartsWith("Warewolf.") -or $file.Name.StartsWith("WareWolf"))) {
		    # Get version.
		    $ReadVersion = [system.diagnostics.fileversioninfo]::GetVersionInfo($file.FullName).FileVersion
		
		    # Find highest version
		    $SeperateVersionNumbers = $ReadVersion.split(".")
		    $SeperateVersionNumbersHighest = $HighestReadVersion.split(".")
		    if ([convert]::ToInt32($SeperateVersionNumbers[0], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[0], 10)`
		    -or [convert]::ToInt32($SeperateVersionNumbers[1], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[1], 10)`
		    -or [convert]::ToInt32($SeperateVersionNumbers[2], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[2], 10)`
		    -or [convert]::ToInt32($SeperateVersionNumbers[3], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[3], 10)){
			    $HighestReadVersion = $ReadVersion
		    }

            # Check for invalid.
            if ($ReadVersion.StartsWith("0.0.") -or $ReadVersion.EndsWith(".0") -or ($LastReadVersion -ne $ReadVersion -and $LastReadVersion -ne "0.0.0.0")) {
			    $getFullPath = $file.FullName
	            Write-Host ERROR! Invalid version! $getFullPath $ReadVersion $LastReadVersion
	            throw "ERROR! `"$getFullPath $ReadVersion`" is either an invalid version or not equal to `"$LastReadVersion`". All Warewolf assembly versions in `"$TestsPath`" must conform and cannot start with 0.0. or end with .0"
            }
            $LastReadVersion = $ReadVersion
	    }
    }
    Out-File -LiteralPath FullVersionString -InputObject "FullVersionString=$HighestReadVersion" -Encoding default
}

if ($RunWarewolfServiceTests.IsPresent) {
    if ($ServerPath -eq "") {
        $ServerPath = "http://localhost:3142"
    }
    $WarewolfServerURL = "$ServerPath/secure/apis.json"
    if ($ServerUsername -eq "") {
        $Headers = @{}
        $ServerUsername = "Unknown User"
    } else {
        $pair = "$($ServerUsername):$($ServerPassword)"
        $encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
        $basicAuthValue = "Basic $encodedCreds"
        $Headers = @{
            Authorization = $basicAuthValue
        }
    }
    Write-Warning "Connecting to $WarewolfServerURL"
    $TestStartDateTime = Get-Date -Format o
    if (!$DisableTimeouts.IsPresent) {
        $ConnectTimeout = 180
    } else {
        $ConnectTimeout = 0
    }
    try {
        $ConnectToWarewolfServer = wget $WarewolfServerURL -Headers $Headers -TimeoutSec $ConnectTimeout -UseBasicParsing
    } catch {
        throw $_.Exception
    }
    try {
        $TryGetWarewolfServerVersion = wget "$ServerPath/secure/getserverversion.json" -Headers $Headers -TimeoutSec $ConnectTimeout -UseBasicParsing
    } catch {
        Write-Warning $_.Exception
    }
    $WarewolfServerVersion = "0.0.0.0"
    if ($TryGetWarewolfServerVersion.StatusCode -eq 200) {
        $WarewolfServerVersion = $TryGetWarewolfServerVersion.Content.Trim("`"")
    }

    $WarewolfServiceData = (ConvertFrom-Json $ConnectToWarewolfServer).Apis
    $WarewolfServiceTestData = @()
    foreach ($WarewolfService in $WarewolfServiceData) {
        $WarewolfServiceTestURL = "http://" + $WarewolfService.BaseUrl.Replace(".json", ".tests")
        Write-Warning "Connecting to $WarewolfServiceTestURL"
        try {
            if (!$DisableTimeouts.IsPresent) {
                $TestTimeout = 180
            } else {
                $TestTimeout = 0
            }
            $TestStart = Get-Date
            $ServiceTestResults = ConvertFrom-Json (wget $WarewolfServiceTestURL -Headers $Headers -TimeoutSec $TestTimeout -UseBasicParsing)
            $ServiceTestDuration = New-TimeSpan -start $TestStart -end (Get-Date)
            if ($ServiceTestResults -ne $null -and $ServiceTestResults -ne "" -and $ServiceTestResults.Count -gt 0) {
                [double]$TestDurationSeconds = $ServiceTestDuration.TotalSeconds / $ServiceTestResults.Count
                if ($TestDurationSeconds -ge 60) {
                    $TestDuration = New-TimeSpan -Seconds $TestDurationSeconds
                } else {
                    $TestDuration = "00:00:" + $TestDurationSeconds.ToString("00.0000000")
                }
                $ServiceTestResults | Foreach-object { 
                    $_.'Test Name' = $WarewolfService.Name.Replace(" ", "_") + "_" + $_.'Test Name'.Replace(" ", "_")
                    $_.Result = $_.Result.Replace("Invalid", "Failed")
                    $_ | Add-Member -MemberType noteproperty -Name "ID" -Value ([guid]::NewGuid()) -PassThru
                    $_ | Add-Member -MemberType noteproperty -Name "ExecutionID" -Value ([guid]::NewGuid()) -PassThru
                    $_ | Add-Member -MemberType noteproperty -Name "Duration" -Value $TestDuration.ToString() -PassThru
                    $_ | Add-Member -MemberType noteproperty -Name "StartTime" -Value (Get-Date $TestStart -Format o) -PassThru
                    $TestEnd = $TestStart + $TestDuration
                    $_ | Add-Member -MemberType noteproperty -Name "EndTime" -Value (Get-Date $TestEnd -Format o) -PassThru
                }
                $WarewolfServiceTestData += $ServiceTestResults
            }
        } catch {
            Write-Warning $_.Exception
        }
    }

    $TestListID = [guid]::NewGuid().ToString()
    $TRXFileContents = @"
<?xml version="1.0" encoding="UTF-8"?>
<TestRun id="
"@ + [guid]::NewGuid() + @"
" name="Warewolf Service Tests" runUser="$ServerUsername" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Times creation="
"@ + $TestStartDateTime + @"
" queuing="
"@ + $TestStartDateTime + @"
" start="
"@ + $TestStartDateTime + @"
" finish="
"@ + (Get-Date -Format o) + @"
" />
  <ResultSummary outcome="Completed">
    <Counters total="
"@ + $WarewolfServiceTestData.Count + @"
" executed="
"@ + $WarewolfServiceTestData.Count + @"
" passed="
"@ + $WarewolfServiceTestData.Result.Where({($_ -eq "Passed")}, 'Split')[0].Count + @"
" error="0" failed="
"@ + $WarewolfServiceTestData.Result.Where({($_ -eq "Failed")}, 'Split')[0].Count + @"
" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
  </ResultSummary>
  <TestDefinitions>
"@
    foreach ($TestResult in $WarewolfServiceTestData) {
        $TRXFileContents += @"

    <UnitTest name="
"@ + $TestResult.'Test Name' + @"
" storage="
"@ + $TestResult.'Test Name' + @"
.dll" id="
"@ + $TestResult.ID + @"
">
      <Execution id="
"@ + $TestResult.ExecutionID + @"
" />
      <TestMethod codeBase="
"@ + $TestResult.'Test Name' + @"
.dll" adapterTypeName="Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" className="
"@ + $TestResult.'Test Name' + @"
, Version=$WarewolfServerVersion, Culture=neutral, PublicKeyToken=null" name="
"@ + $TestResult.'Test Name' + @"
" />
    </UnitTest>
"@
                }
                $TRXFileContents += @"

  </TestDefinitions>
  <TestLists>
    <TestList name="Results Not in a List" id="$TestListID" />
    <TestList name="All Loaded Results" id="19431567-8539-422a-85d7-44ee4e166bda"/>
  </TestLists>
  <TestEntries>
"@
    foreach ($TestResult in $WarewolfServiceTestData) {
        $TRXFileContents += @"

    <TestEntry testId="
"@ + $TestResult.ID + @"
" executionId="
"@ + $TestResult.ExecutionID + @"
" testListId="$TestListID" />
"@
    }
    $TRXFileContents += @"

  </TestEntries>
  <Results>
"@
    foreach ($TestResult in $WarewolfServiceTestData) {
        $TRXFileContents += @"

    <UnitTestResult executionId="
"@ + $TestResult.ExecutionID + @"
" testId="
"@ + $TestResult.ID + @"
" testName="
"@ + $TestResult.'Test Name' + @"
" computerName="$ServerPath" duration="
"@ + $TestResult.Duration + @"
" startTime="
"@ + $TestResult.StartTime + @"
" endTime="
"@ + $TestResult.EndTime + @"
" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="
"@ + $TestResult.Result + @"
" testListId="$TestListID" relativeResultsDirectory="ca6d373f-8816-4969-8999-3dac700d7626">
"@
	    if ($TestResult.Result -eq "Failed") {
            Add-Type -AssemblyName System.Web
            $TestResultMessage = [System.Web.HttpUtility]::HtmlEncode($TestResult.Message)		    
		    $TRXFileContents += @"
      <Output>
        <ErrorInfo>
          <Message>$TestResultMessage</Message>
          <StackTrace></StackTrace>
        </ErrorInfo>
      </Output>
"@
	    }
	    $TRXFileContents += @"
    </UnitTestResult>
"@
    }
    $TRXFileContents += @"

  </Results>
</TestRun>
"@
    Copy-On-Write "$TestsResultsPath\TestResults.trx"
    New-Item -Force -Path "$TestsResultsPath\TestResults.trx" -ItemType File -Value $TRXFileContents
}

if ($Cleanup.IsPresent) {
    if ($ApplyDotCover) {
        Cleanup-ServerStudio $false
    } else {
        Cleanup-ServerStudio
    }
	if (!$JobName) {
		if ($ProjectName) {
			$JobName = $ProjectName
		} else {
			$JobName = "Manual Tests"
		}
	}
    Move-Artifacts-To-TestResults $ApplyDotCover (Test-Path "$env:ProgramData\Warewolf\Server Log\wareWolf-Server.log") (Test-Path "$env:LocalAppData\Warewolf\Studio Logs\Warewolf Studio.log")
}

if ($RunAllJobs.IsPresent) {
    Invoke-Expression -Command ("&'$PSCommandPath' -JobName '$UnitTestJobNames' -DisableTimeouts -Parallelize")
    Invoke-Expression -Command ("&'$PSCommandPath' -JobName '$ServerTestJobNames' -StartServer -ResourcesType ServerTests")
    Invoke-Expression -Command ("&'$PSCommandPath' -JobName '$ReleaseResourcesJobNames' -StartServer -ResourcesType Release")
    Invoke-Expression -Command ("&'$PSCommandPath' -JobName '$RunAllCodedUITests' -StartStudio -ResourcesType UITests")
}

if (!$RunAllJobs.IsPresent -and !$Cleanup.IsPresent -and !$AssemblyFileVersionsTest.IsPresent -and !$RunAllUnitTests.IsPresent -and !$RunAllServerTests.IsPresent -and !$RunAllCodedUITests.IsPresent -and $JobName -eq "" -and !$RunWarewolfServiceTests.IsPresent) {
    $ServerPath,$ResourcesType = Install-Server $ServerPath $ResourcesType
    Start-Server $ServerPath $ResourcesType
    if (!$StartServer.IsPresent) {
        Start-Studio
    }
}