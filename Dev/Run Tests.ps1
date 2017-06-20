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
  [switch]$NoTestSettings,
  [switch]$RecordScreen
)
$JobSpecs = @{}
#CI
$JobSpecs["Other Unit Tests"] 				 	= "Dev2.*.Tests,Warewolf.*.Tests"
$JobSpecs["Other Specs"]		 				= "Dev2.*.Specs,Warewolf.*.Specs", "(TestCategory!=ExampleWorkflowExecution)&(TestCategory!=WorkflowExecution)&(TestCategory!=SubworkflowExecution)"
$JobSpecs["Tools Specs"]		 				= "Warewolf.ToolsSpecs"
$JobSpecs["UI Binding Tests"] 				 	= "Warewolf.UIBindingTests.*"
$JobSpecs["COMIPC Unit Tests"]				 	= "Warewolf.COMIPC.Tests"
$JobSpecs["Example Workflow Execution Specs"] 	= "Dev2.*.Specs,Warewolf.*.Specs", "ExampleWorkflowExecution"
$JobSpecs["Studio View Models Unit Tests"]	 	= "Warewolf.Studio.ViewModels.Tests"
$JobSpecs["Subworkflow Execution Specs"]		= "Dev2.*.Specs,Warewolf.*.Specs", "SubworkflowExecution"
$JobSpecs["Workflow Execution Specs"]		 	= "Dev2.*.Specs,Warewolf.*.Specs", "WorkflowExecution"
$JobSpecs["Activity Designers Unit Tests"]	 	= "Dev2.Activities.Designers.Tests"
$JobSpecs["Activity Unit Tests"]				= "Dev2.Activities.Tests"
$JobSpecs["Integration Tests"]				 	= "Dev2.IntegrationTests"
#Coded UI
$JobSpecs["Other UI Tests"]					    = "Warewolf.UITests", "(TestCategory!=Tools)&(TestCategory!=Data Tools)&(TestCategory!=Database Tools)&(TestCategory!=Dropbox Tools)&(TestCategory!=File Tools)&(TestCategory!=HTTP Tools)&(TestCategory!=Recordset Tools)&(TestCategory!=Sharepoint Tools)&(TestCategory!=Utility Tools)&(TestCategory!=Explorer)&(TestCategory!=Tabs and Panes)&(TestCategory!=Deploy)&(TestCategory!=Debug Input)&(TestCategory!=Workflow Testing)&(TestCategory!=Default Layout)&(TestCategory!=Resource Tools)&(TestCategory!=Save Dialog)&(TestCategory!=Shortcut Keys)&(TestCategory!=Settings)&(TestCategory!=Dependency Graph)&(TestCategory!=Variables)&(TestCategory!=Email Tools)&(TestCategory!=Plugin Sources)&(TestCategory!=Web Sources)&(TestCategory!=Database Sources)&(TestCategory!=Workflow Mocking Tests)&(TestCategory!=Assign Tool)&(TestCategory!=Control Flow Tools)&(TestCategory!=DotNet Connector Mocking Tests)&(TestCategory!=DotNet Connector Tool)&(TestCategory!=Hello World Mocking Tests)&(TestCategory!=Server Sources)&(TestCategory!=Source Wizards)"
$JobSpecs["Other UI Specs"]					    = "Warewolf.UISpecs", "(TestCategory!=DBConnector)&(TestCategory!=PluginConnector)&(TestCategory!=WebConnector)&(TestCategory!=Explorer)&(TestCategory!=Deploy)&(TestCategory!=SaveDialog)"
$JobSpecs["Assign Tool UI Tests"]				= "Warewolf.UITests", "Assign Tool"
$JobSpecs["Control Flow Tools UI Tests"]		= "Warewolf.UITests", "Control Flow Tools"
$JobSpecs["Database Sources UI Tests"]			= "Warewolf.UITests", "Database Sources"
$JobSpecs["Database Tools UI Tests"]			= "Warewolf.UITests", "Database Tools"
$JobSpecs["Data Tools UI Tests"]				= "Warewolf.UITests", "Data Tools"
$JobSpecs["DB Connector UI Specs"]				= "Warewolf.UISpecs", "DBConnector"
$JobSpecs["Debug Input UI Tests"]				= "Warewolf.UITests", "Debug Input"
$JobSpecs["Default Layout UI Tests"]			= "Warewolf.UITests", "Default Layout"
$JobSpecs["Dependency Graph UI Tests"]			= "Warewolf.UITests", "Dependency Graph"
$JobSpecs["Deploy UI Specs"]					= "Warewolf.UISpecs", "Deploy"
$JobSpecs["Deploy UI Tests"]					= "Warewolf.UITests", "Deploy"
$JobSpecs["DotNet Connector Mocking UI Tests"]	= "Warewolf.UITests", "DotNet Connector Mocking Tests"
$JobSpecs["DotNet Connector Tool UI Tests"]	    = "Warewolf.UITests", "DotNet Connector Tool"
$JobSpecs["Dropbox Tools UI Tests"]			    = "Warewolf.UITests", "Dropbox Tools"
$JobSpecs["Email Tools UI Tests"]				= "Warewolf.UITests", "Email Tools"
$JobSpecs["Explorer UI Specs"]					= "Warewolf.UISpecs", "Explorer"
$JobSpecs["Explorer UI Tests"]					= "Warewolf.UITests", "Explorer"
$JobSpecs["File Tools UI Tests"]				= "Warewolf.UITests", "File Tools"
$JobSpecs["Hello World Mocking UI Tests"]		= "Warewolf.UITests", "Hello World Mocking Tests"
$JobSpecs["HTTP Tools UI Tests"]				= "Warewolf.UITests", "HTTP Tools"
$JobSpecs["Plugin Sources UI Tests"]			= "Warewolf.UITests", "Plugin Sources"
$JobSpecs["Recordset Tools UI Tests"]			= "Warewolf.UITests", "Recordset Tools"
$JobSpecs["Resource Tools UI Tests"]			= "Warewolf.UITests", "Resource Tools"
$JobSpecs["Save Dialog UI Specs"]				= "Warewolf.UISpecs", "Save Dialog"
$JobSpecs["Save Dialog UI Tests"]				= "Warewolf.UITests", "SaveDialog"
$JobSpecs["Server Sources UI Tests"]			= "Warewolf.UITests", "Server Sources"
$JobSpecs["Settings UI Tests"]					= "Warewolf.UITests", "Settings"
$JobSpecs["Sharepoint Tools UI Tests"]			= "Warewolf.UITests", "Sharepoint Tools"
$JobSpecs["Shortcut Keys UI Tests"]			    = "Warewolf.UITests", "Shortcut Keys"
$JobSpecs["Source Wizards UI Tests"]			= "Warewolf.UITests", "Source Wizards"
$JobSpecs["Tabs And Panes UI Tests"]			= "Warewolf.UITests", "Tabs and Panes"
$JobSpecs["Tools UI Tests"]					    = "Warewolf.UITests", "Tools"
$JobSpecs["Utility Tools UI Tests"]			    = "Warewolf.UITests", "Utility Tools"
$JobSpecs["Variables UI Tests"]				    = "Warewolf.UITests", "Variables"
$JobSpecs["Web Connector UI Specs"]			    = "Warewolf.UISpecs", "WebConnector"
$JobSpecs["Web Sources UI Tests"]				= "Warewolf.UITests", "Web Sources"
$JobSpecs["Workflow Mocking Tests UI Tests"]	= "Warewolf.UITests", "Workflow Mocking Tests"
$JobSpecs["Workflow Testing UI Tests"]			= "Warewolf.UITests", "Workflow Testing"
#Security
$JobSpecs["Conflicting Contribute View And Execute Permissions Security Specs"] = "Warewolf.SecuritySpecs", "ConflictingContributeViewExecutePermissionsSecurity"
$JobSpecs["Conflicting Execute Permissions Security Specs"]					    = "Warewolf.SecuritySpecs", "ConflictingExecutePermissionsSecurity"
$JobSpecs["Conflicting View And Execute Permissions Security Specs"]			= "Warewolf.SecuritySpecs", "ConflictingViewExecutePermissionsSecurity"
$JobSpecs["Conflicting View Permissions Security Specs"]						= "Warewolf.SecuritySpecs", "ConflictingViewPermissionsSecurity"
$JobSpecs["No Conflicting Permissions Security Specs"]							= "Warewolf.SecuritySpecs", "NoConflictingPermissionsSecurity"
$JobSpecs["OverlappingUserGroupsPermissions Security Specs"]					= "Warewolf.SecuritySpecs", "OverlappingUserGroupsPermissionsSecurity"
$JobSpecs["ResourcePermissions Security Specs"]								    = "Warewolf.SecuritySpecs", "ResourcePermissionsSecurity"
$JobSpecs["ServerPermissions Security Specs"]									= "Warewolf.SecuritySpecs", "ServerPermissionsSecurity"

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

$JobName = $JobName.TrimEnd("1234567890 ")

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

function Cleanup-ServerStudio {
    if ($Args.length > 0) {
        [int]$WaitForCloseTimeout = $Args[0]
    } else {
        [int]$WaitForCloseTimeout = 1800
    }
    if ($Args.length > 1) {
	    [int]$WaitForCloseRetryCount = $Args[1]
    } else {
	    [int]$WaitForCloseRetryCount = 10
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
		    Write-Host Cannot delete $FileOrFolder
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
        while(Test-Path -Path "$FilePath.$num")
        {
            $num += 1
        }
        $FilePath | Move-Item -Destination "$FilePath.$num"
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
    
    Move-File-To-TestResults "$TestsResultsPath\..\RunTests.bat" "Run $JobName.bat"
    Move-File-To-TestResults "$TestsResultsPath\RunTests.testsettings" "$JobName.testsettings"

    # Write failing tests playlistfunction Move-Artifacts-T.
    Write-Host Writing all test failures in `"$TestsResultsPath`" to a playlist file

    Get-ChildItem "$TestsResultsPath" -Filter *.trx | Rename-Item -NewName {$_.name -replace ' ','_' }

    $PlayList = "<Playlist Version=`"1.0`">"
    Get-ChildItem "$TestsResultsPath" -Filter *.trx | `
    Foreach-Object{
	    [xml]$trxContent = Get-Content $_.FullName
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
        &"$DotCoverPath" "merge" "/Source=`"$TestsResultsPath\$JobName Server DotCover.dcvr`";`"$TestsResultsPath\$JobName Studio DotCover.dcvr`"" "/Output=`"$PSScriptRoot\$JobName DotCover.dcvr`"" "/LogFile=`"$TestsResultsPath\ServerAndStudioDotCoverSnapshotMerge.log`""
    }
}

function Move-ScreenRecordings-To-TestResults {
    [string]$TestsResultsPath = "$TestsResultsPath"
    Write-Host Getting UI test screen recordings from `"$TestsResultsPath`"

    $screenRecordingsFolder = "$TestsResultsPath\ScreenRecordings"
    New-Item $screenRecordingsFolder -Force -ItemType Directory
    if (Test-Path $TestsResultsPath\UI\In\*) {
        Copy-Item $TestsResultsPath\UI\In\* $screenRecordingsFolder -Recurse -Force
        $pngFiles = Get-ChildItem -Path "$screenRecordingsFolder" -Filter *.png -Recurse
        foreach ($file in $pngFiles)
        {
	        if (-not $file.name.Contains($file.Directory.Name))
	        {
		        Rename-Item -Path $file.FullName -NewName ( $file.Directory.Name + " " + $file.name )
	        }
        }
        $trmxFiles = Get-ChildItem -Path "$screenRecordingsFolder" -Filter *.png -Recurse
        foreach ($file in $trmxFiles)
        {
            $newRecordingFileName = $file.name -replace ".png",".wmv"
            Rename-Item -Path ($file.DirectoryName + "\ScreenCapture.wmv") -NewName $newRecordingFileName
        }
        $files = Get-ChildItem -Path "$screenRecordingsFolder" -Include *.png, *.wmv -Recurse | where {$_.PSIScontainer -eq $false}
        foreach ($file in $files)
	    {
		    $destinationFolder = (Get-Item $file).Directory.parent.parent.FullName
		    Move-Item $file.FullName $destinationFolder
        }
        Remove-Item -Recurse -Force $screenRecordingsFolder\* -Exclude "*.png","*.wmv"
    } else {
        Write-Host $TestsResultsPath\UI\In\* not found.
    }
}

function Install-Server {
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

    if ($ServerPath -eq "") {
        $ServerPath = FindFile-InParent $ServerPathSpecs
        if ($ServerPath.EndsWith(".zip")) {
			    Expand-Archive "$PSScriptRoot\*Server.zip" "$CurrentDirectory\Server" -Force
			    $ServerPath = "$PSScriptRoot\Server\" + $ServerExeName
		    }
        if ($ServerPath -eq "") {
            Write-Host Cannot find Warewolf Server.exe. Please provide a path to that file as a commandline parameter like this: -ServerPath
            sleep 30
            exit 1
        }
    }

    $ServerService = Get-Service "Warewolf Server" -ErrorAction SilentlyContinue
    if (!$DotCover.IsPresent) {
        if ($ServerService -eq $null) {
            New-Service -Name "Warewolf Server" -BinaryPathName "$ServerPath" -StartupType Manual
        } else {    
		    Write-Host Configuring service to $ServerPath
		    sc.exe config "Warewolf Server" binPath= "$ServerPath"
        }
    } else {
        $ServerBinDir = (Get-Item $ServerPath).Directory.FullName 
        $RunnerXML = @"
<AnalyseParams>
<TargetExecutable>$ServerPath</TargetExecutable>
<TargetArguments></TargetArguments>
<Output>$env:ProgramData\Warewolf\Server Log\dotCover.dcvr</Output>
<Scope>
	<ScopeEntry>$ServerBinDir\**\*.dll</ScopeEntry>
	<ScopeEntry>$ServerBinDir\**\*.exe</ScopeEntry>
</Scope>
</AnalyseParams>
"@

        Out-File -LiteralPath "$ServerBinDir\DotCoverRunner.xml" -Encoding default -InputObject $RunnerXML
        $BinPathWithDotCover = "\`"" + $DotCoverPath + "\`" cover \`"" + $ServerBinDir + "\DotCoverRunner.xml\`" /LogFile=\`"$env:ProgramData\Warewolf\Server Log\dotCover.log\`""
        if ($ServerService -eq $null) {
            New-Service -Name "Warewolf Server" -BinaryPathName "$BinPathWithDotCover" -StartupType Manual
	    } else {
		    Write-Host Configuring service to $BinPathWithDotCover
		    sc.exe config "Warewolf Server" binPath= "$BinPathWithDotCover"
	    }
    }
    if ($ServerUsername -ne "" -and $ServerPassword -eq "") {
        sc.exe config "Warewolf Server" obj= "$ServerUsername"
    }
    if ($ServerUsername -ne "" -and $ServerPassword -ne "") {
        sc.exe config "Warewolf Server" obj= "$ServerUsername" password= "$ServerPassword"
    }

    $ResourcePathSpecs = @()
    foreach ($ServerPathSpec in $ServerPathSpecs) {
        if ($ServerPathSpec.EndsWith($ServerExeName)) {
            $ResourcePathSpecs += $ServerPathSpec.Replace($ServerExeName, "Resources - $ResourcesType")
        }
    }
    $ResourcesDirectory = FindFile-InParent $ResourcePathSpecs
    if ($ResourcesPath -ne "" -and $ResourcesDirectory -ne (Get-Item $ServerPath).Directory.FullName + "\" + (Get-Item $ResourcesDirectory).Name ) {
        Copy-Item -Path "$ResourcesDirectory" -Destination (Get-Item $ServerPath).Directory.FullName -Recurse -Force
    }

    Write-Host Cleaning up old resources in Warewolf ProgramData and copying in new resources from ((Get-Item $ServerPath).Directory.FullName + "\Resources - $ResourcesType\*").
    Cleanup-ServerStudio 10 1
    Copy-Item -Path ((Get-Item $ServerPath).Directory.FullName + "\Resources - $ResourcesType\*") -Destination "$env:ProgramData\Warewolf" -Recurse -Force
}

function Start-Server {
    Start-Service "Warewolf Server"
    Write-Host Server has started.

    #Check if started
    $Output = @()
    sc.exe interrogate "Warewolf Server" 2>&1 | %{$Output += $_}
    if ($Output.Length -lt 4 -or !($Output[3].EndsWith("RUNNING "))) {
        sc.exe start "Warewolf Server"
    }
}

function Start-Studio {
    $StudioPath = FindFile-InParent $StudioPathSpecs
    if ($StudioPath.EndsWith(".zip")) {
	    Expand-Archive "$StudioPath" "$PSScriptRoot\Studio" -Force
	    $StudioPath = "$PSScriptRoot\Studio\" + $StudioExeName
    }
	if ($StudioPath -eq "") {
		Write-Host Cannot find Warewolf Studio. To run the studio provide a path to the Warewolf Studio exe file as a commandline parameter like this: -StudioPath
        sleep 30
		exit 1
	}
    $StudioLogFile = "$env:LocalAppData\Warewolf\Studio Logs\Warewolf Studio.log"
    Copy-On-Write $StudioLogFile
	if (!$DotCover.IsPresent) {
		Start-Process "$StudioPath"
	} else {
        $StudioBinDir = (Get-Item $StudioPath).Directory.FullName 
        $RunnerXML = @"
<AnalyseParams>
<TargetExecutable>$StudioPath</TargetExecutable>
<TargetArguments></TargetArguments>
<LogFile>$env:LocalAppData\Warewolf\Studio Logs\dotCover.log</LogFile>
<Output>$env:LocalAppData\Warewolf\Studio Logs\dotCover.dcvr</Output>
<Scope>
	<ScopeEntry>$StudioBinDir\**\*.dll</ScopeEntry>
	<ScopeEntry>$StudioBinDir\**\*.exe</ScopeEntry>
</Scope>
</AnalyseParams>
"@

        Out-File -LiteralPath "$StudioBinDir\DotCoverRunner.xml" -Encoding default -InputObject $RunnerXML
		Start-Process $DotCoverPath "cover `"$StudioBinDir\DotCoverRunner.xml`" /LogFile=`"$env:LocalAppData\Warewolf\Studio Logs\dotCover.log`""
    }
    while (!(Test-Path $StudioLogFile)){
        Write-Warning 'Waiting for Studio to start...'
        Sleep 3
    }
	Write-Host Studio has started.
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

if ($StartServer.IsPresent -or $StartStudio.IsPresent) {
    if ($ServerPath -ne "" -and !(Test-Path $ServerPath)) {
        Write-Host Server path not found: $ServerPath
        sleep 30
        exit 1
    }
    if ($StudioPath -ne "" -and !(Test-Path $StudioPath)) {
        Write-Host Studio path not found: $StudioPath
        sleep 30
        exit 1
    }
    if ($DotCoverPath -ne "" -and !(Test-Path $DotCoverPath)) {
        Write-Host DotCover path not found: $DotCoverPath
        sleep 30
        exit 1
    }
    Install-Server
}

#Unpack jobs
$JobNames = @()
$JobAssemblySpecs = @()
$JobCategories = @()
if ($JobName -ne $null -and $JobName -ne "") {
    foreach ($Job in $JobName.Split(",")) {
        $JobNames += $Job
        if ($JobSpecs.ContainsKey($Job)) {
            if ($JobSpecs[$Job].Count -eq 1) {
                $JobAssemblySpecs += $JobSpecs[$Job]
                $JobCategories += ""
            } else {
                $JobAssemblySpecs += $JobSpecs[$Job][0]
                $JobCategories += $JobSpecs[$Job][1]
            }
        }
    }
} else {
    if ($RunAllJobs.IsPresent) {
        $JobSpecs.Keys.ForEach({
            $JobNames += $_
            if ($JobSpecs[$_].Count -eq 1) {
                $JobAssemblySpecs += $JobSpecs[$_]
                $JobCategories += ""
            } else {
                $JobAssemblySpecs += $JobSpecs[$_][0]
                $JobCategories += $JobSpecs[$_][1]
            }
        })
    }
}
$TotalNumberOfJobsToRun = $JobNames.length
if ($TotalNumberOfJobsToRun -gt 0) {
    If (!(Test-Path "$TestsResultsPath")) {
        New-Item "$TestsResultsPath" -ItemType Directory
    }
    if (!(Test-Path $VSTestPath) -and !(Test-Path $MSTestPath)) {
        Write-Host Error cannot find VSTest.console.exe or MSTest.exe. Use either -VSTestPath `'`' or -MSTestPath `'`' parameters to pass paths to one of those files.
        sleep 30
        exit 1
    }
    if ($DotCover.IsPresent -and !(Test-Path $DotCoverPath)) {
        Write-Host Error cannot find dotcover.exe. Use -DotCoverPath `'`' parameter to pass a path to that file.
        sleep 30
        exit 1
    }

    if (!$MSTest.IsPresent) {
        # Read playlists and args.
        $TestList = ""
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
    } else {
        $TestList = ""
        if ($Args.Count -gt 0) {
            $TestList = $Args.ForEach({ "," + $_ })
        } else {
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
        if ($StartServer.IsPresent -or $StartStudio.IsPresent) {
            Start-Server
        }
        if ($StartStudio.IsPresent) {
            Start-Studio
        }
        $ProjectSpec = $JobAssemblySpecs[$_].ToString()
        $JobName = $JobNames[$_].ToString()
        $TestCategories = $JobCategories[$_].ToString()
        $TestAssembliesList = ""
        $TestAssembliesDirectories = @()
        if (!($TestsPath.EndsWith("\"))) { $TestsPath += "\" }
        foreach ($Project in $ProjectSpec.Split(",")) {
            $TestAssembliesFileSpecs = @()
            $TestAssembliesFileSpecs += $TestsPath + $Project + ".dll"
            $TestAssembliesFileSpecsInParent = FindFile-InParent $TestAssembliesFileSpecs
            if ($TestAssembliesFileSpecsInParent -ne "") {
                foreach ($file in Get-ChildItem $TestAssembliesFileSpecsInParent) {
                    $AssemblyNameToCheck = $file.Name.replace($file.extension, "")
                    if (!$TestAssembliesFileSpecsInParent.Contains("*") -or (AssemblyIsNotAlreadyDefinedWithoutWildcards $AssemblyNameToCheck)) {
                        if ((Test-Path $VSTestPath) -and !$MSTest.IsPresent) {
		                    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
                        } else {
		                    $TestAssembliesList = $TestAssembliesList + " /testcontainer:`"" + $file.FullName + "`""
                        }
                        if (!$TestAssembliesDirectories.Contains($file.Directory.FullName)) {
                            $TestAssembliesDirectories += $file.Directory.FullName
                        }
	                }
                }
            }
            if ($TestAssembliesList -eq "") {
                $ProjectFolderSpec = @()
                $ProjectFolderSpec += $TestsPath + $Project
                $SolutionFolderPath = FindFile-InParent $ProjectFolderSpec
                if ($SolutionFolderPath -ne "") {
                    if (!$TestAssembliesDirectories.Contains($SolutionFolderPath + "\bin\Debug")) {
                        $TestAssembliesDirectories += $SolutionFolderPath + "\bin\Debug"
                    }
                    if ((Test-Path $VSTestPath) -and !$MSTest.IsPresent) {
		                $TestAssembliesList = $TestAssembliesList + " `"" + $SolutionFolderPath + "\bin\Debug\" + (Get-Item $SolutionFolderPath).Name + ".dll`""
                    } else {
		                $TestAssembliesList = $TestAssembliesList + " /testcontainer:`"" + $SolutionFolderPath + "\bin\Debug\" + (Get-Item $SolutionFolderPath).Name + ".dll`""
                    }
                }
            }
        }
        if ($TestAssembliesList -eq "") {
	        Write-Host Cannot find any $ProjectSpec project folders or assemblies at $TestsPath.
	        exit 1
        }

        # Create test settings.
        $TestSettingsFile = ""
        if ($RecordScreen.IsPresent) {
            $TestSettingsFile = "$TestsResultsPath\RunTests.testsettings"
            [system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings
  id=`"
"@ + [guid]::NewGuid() + @"
`"
  name=`"$JobName`"
  enableDefaultDataCollectors=`"false`"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run Tests With Timeout And Screen Recordings.</Description>
  <Deployment enabled=`"false`" />
  <NamingScheme baseName=`"UI`" appendTimeStamp=`"false`" useDefault=`"false`" />
  <Execution>
    <Timeouts testTimeout=`"600000`" />
    <AgentRule name=`"LocalMachineDefaultRole`">
      <DataCollectors>
        <DataCollector uri=`"datacollector://microsoft/VideoRecorder/1.0`" assemblyQualifiedName=`"Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder.VideoRecorderDataCollector, Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`" friendlyName=`"Screen and Voice Recorder`">
          <Configuration>
            <MediaRecorder sendRecordedMediaForPassedTestCase=`"false`" xmlns="" />
          </Configuration>
        </DataCollector>
      </DataCollectors>
    </AgentRule>
  </Execution>
</TestSettings>
"@)
        } else {
            if (!$NoTestSettings.IsPresent) {
                $TestSettingsFile = "$TestsResultsPath\RunTests.testsettings"
                [system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings
  id=`"
"@ + [guid]::NewGuid() + @"
`"
  name=`"$JobName`"
  enableDefaultDataCollectors=`"false`"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run Tests With Timeout.</Description>
  <Deployment enabled=`"false`" />
  <Execution>
    <Timeouts testTimeout=`"180000`" />
  </Execution>
</TestSettings>
"@)
            }
        }
        if (!$MSTest.IsPresent) {
            # Create full VSTest argument string.
            if ($TestCategories -ne "") {
                $TestCategories = " /TestCaseFilter:`"$TestCategories`""
            }
            if($TestSettingsFile -ne "") {
                $TestSettings =  " /Settings:`"" + $TestSettingsFile + "`""
            }
            $FullArgsList = $TestAssembliesList + " /logger:trx " + $TestList + $TestSettings + $TestCategories

            # Write full command including full argument string.
            Out-File -LiteralPath "$TestsResultsPath\..\RunTests.bat" -Encoding default -InputObject `"$VSTestPath`"$FullArgsList
        } else {
            #Resolve test results file name
            $TestResultsFile = $TestsResultsPath + "\" + $JobName + " Results.trx"

            # Create full MSTest argument string.
            if ($TestCategories -ne "") {
                $TestCategories = $TestCategories.Replace("(TestCategory", "").Replace("=", "").Replace(")", "")
                $TestCategories = " /category:`"$TestCategories`""
            }
            if($TestSettingsFile -ne "") {
                $TestSettings =  " /testsettings:`"" + $TestSettingsFile + "`""
            }
            $FullArgsList = $TestAssembliesList + " /resultsfile:`"" + $TestResultsFile + "`" " + $TestList + $TestSettings + $TestCategories

            # Write full command including full argument string.
            Out-File -LiteralPath "$TestsResultsPath\..\RunTests.bat" -Encoding default -InputObject `"$MSTestPath`"$FullArgsList
        }
        if (Test-Path "$TestsResultsPath\..\RunTests.bat") {
            if ($DotCover.IsPresent -and !$StartServer.IsPresent -and !$StartStudio.IsPresent) {
                # Write DotCover Runner XML 
                $DotCoverArgs = @"
<AnalyseParams>
	<TargetExecutable>$TestsResultsPath\..\RunTests.bat</TargetExecutable>
	<TargetArguments></TargetArguments>
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
                Out-File -LiteralPath "$TestsResultsPath\DotCoverRunner.xml" -Encoding default -InputObject $DotCoverArgs

                #Write DotCover Runner Batch File
                Out-File -LiteralPath $TestsResultsPath\RunDotCover.bat -Encoding default -InputObject "`"$DotCoverPath`" cover `"$TestsResultsPath\DotCoverRunner.xml`" /LogFile=\`"$TestsResultsPath\DotCoverRunner.xml.log\`""

                #Run DotCover Runner Batch File
                &"$TestsResultsPath\RunDotCover.bat"
                Cleanup-ServerStudio 1800 10
                Move-File-To-TestResults "$TestsResultsPath\RunDotCover.bat" "Run $JobName DotCover.bat"
                Move-File-To-TestResults "$TestsResultsPath\DotCoverRunner.xml" "$JobName DotCover Runner.xml"
                Move-File-To-TestResults "$TestsResultsPath\DotCoverRunner.xml.log" "$JobName DotCover Runner.xml.log"
            } else {
                &"$TestsResultsPath\..\RunTests.bat"
                Cleanup-ServerStudio 10 1
            }
            Move-Artifacts-To-TestResults $DotCover.IsPresent ($StartServer.IsPresent -or $StartStudio.IsPresent) $StartStudio.IsPresent
            if ($RecordScreen.IsPresent) {
                Move-ScreenRecordings-To-TestResults
            }
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

if ($Cleanup.IsPresent) {
    if ($DotCover.IsPresent) {
        Cleanup-ServerStudio 1800 10
    } else {
        Cleanup-ServerStudio 10 1
    }
    Move-Artifacts-To-TestResults $DotCover.IsPresent (Test-Path "$env:ProgramData\Warewolf\Server Log\wareWolf-Server.log") (Test-Path "$env:LocalAppData\Warewolf\Studio Logs\Warewolf Studio.log")
}

if (!$Cleanup.IsPresent -and !$AssemblyFileVersionsTest.IsPresent -and !$RunAllJobs.IsPresent -and $JobName -eq "") {
    Install-Server
    Start-Server
    if (!$StartServer.IsPresent) {
        Start-Studio
    }
}