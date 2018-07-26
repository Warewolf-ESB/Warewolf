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
  [string]$VSTestPath="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe",
  [string]$MSTestPath="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\MSTest.exe",
  [string]$DotCoverPath,
  [string]$ServerUsername,
  [string]$ServerPassword,
  [string]$JobNames="",
  [string]$JobName="",
  [switch]$Cleanup,
  [switch]$AssemblyFileVersionsTest,
  [switch]$RecordScreen,
  [switch]$Parallelize,
  [string]$Category,
  [string]$ProjectName,
  [string]$TestList="",
  [switch]$RunWarewolfServiceTests,
  [string]$MergeDotCoverSnapshotsInDirectory="",
  [switch]${Startmy.warewolf.io},
  [string]$sendRecordedMediaForPassedTestCase="false",
  [string]$JobContainerVersion="",
  [switch]$JobContainers,
  [switch]$IsInContainer,
  [string]$ContainerRemoteApiHost,
  [switch]$StartServerAsService,
  [switch]$StartServerAsConsole,
  [switch]$ServerContainer,
  [string]$ContainerRegistryHost
)
$JobSpecs = @{}
#Unit Tests
$JobSpecs["Other Unit Tests"] 				 	= "Dev2.*.Tests,Warewolf.*.Tests"
$JobSpecs["Server Proxy Layer Tests"] 			= "Warewolf.Studio.ServerProxyLayer.Tests"
$JobSpecs["Infrastructure Unit Tests"] 			= "Dev2.Infrastructure.Tests"
$JobSpecs["Runtime Unit Tests"] 				= "Dev2.Runtime.Tests"
$JobSpecs["Core Unit Tests"] 					= "Dev2.Core.Tests"
$JobSpecs["Data Unit Tests"] 					= "Dev2.Data.Tests"
$JobSpecs["Parsing Unit Tests"] 				= "Warewolf.Parsing.Tests"
$JobSpecs["Storage Unit Tests"] 				= "Warewolf.Storage.Tests"
$JobSpecs["Studio Core Unit Tests"] 			= "Dev2.Studio.Core.Tests"
$JobSpecs["COMIPC Unit Tests"]				 	= "Warewolf.COMIPC.Tests"
$JobSpecs["Studio View Models Unit Tests"]	 	= "Warewolf.Studio.ViewModels.Tests"
$JobSpecs["Request Service Name View Models Unit Tests"] = "Warewolf.Studio.ViewModels.Tests", "RequestServiceNameViewModel"
$JobSpecs["Activity Designers Unit Tests"]	 	= "Dev2.Activities.Designers.Tests"
$JobSpecs["Activities Unit Tests"]				= "Dev2.Activities.Tests"
$JobSpecs["Other Tools Specs"]		 			= "Warewolf.Tools.Specs"
$JobSpecs["Scripting Tools Specs"]		 		= "Warewolf.Tools.Specs", "Scripting"
$JobSpecs["Storage Tools Specs"]		 		= "Warewolf.Tools.Specs", "Storage"
$JobSpecs["Utility Tools Specs"]		 		= "Warewolf.Tools.Specs", "Utility"
$JobSpecs["Control Flow Tools Specs"]		 	= "Warewolf.Tools.Specs", "ControlFlow"
$JobSpecs["Data Tools Specs"]		 			= "Warewolf.Tools.Specs", "Data"
$JobSpecs["Database Tools Specs"]		 		= "Warewolf.Tools.Specs", "Database"
$JobSpecs["Email Tools Specs"]		 			= "Warewolf.Tools.Specs", "Email"
$JobSpecs["File And Folder Copy Tool Specs"]	= "Warewolf.Tools.Specs", "FileAndFolderCopy"
$JobSpecs["File And Folder Create Tool Specs"]	= "Warewolf.Tools.Specs", "FileAndFolderCreate"
$JobSpecs["File And Folder Delete Tool Specs"]	= "Warewolf.Tools.Specs", "FileAndFolderDelete"
$JobSpecs["File And Folder Move Tool Specs"]	= "Warewolf.Tools.Specs", "FileAndFolderMove"
$JobSpecs["Folder Read Tool Specs"]		 		= "Warewolf.Tools.Specs", "ReadFolder"$JobSpecs["New Folder Read Tool Specs"]		 	= "Warewolf.Tools.Specs", "NewReadFolder"
$JobSpecs["File Read Tool Specs"]		 		= "Warewolf.Tools.Specs", "ReadFile"
$JobSpecs["File And Folder Rename Tool Specs"]	= "Warewolf.Tools.Specs", "FileAndFolderRename"
$JobSpecs["Unzip Tool Specs"]		 			= "Warewolf.Tools.Specs", "Unzip"
$JobSpecs["Write File Tool Specs"]		 		= "Warewolf.Tools.Specs", "WriteFile"
$JobSpecs["Zip Tool Specs"]		 				= "Warewolf.Tools.Specs", "Zip"
$JobSpecs["Loop Construct Tools Specs"]			= "Warewolf.Tools.Specs", "LoopConstructs"
$JobSpecs["Recordset Tools Specs"]		 		= "Warewolf.Tools.Specs", "Recordset"
$JobSpecs["Resource Tools Specs"]		 		= "Warewolf.Tools.Specs", "Resources"
$JobSpecs["UI Binding Tests"] 				 	= "Warewolf.UIBindingTests.*"
#Server Tests
$JobSpecs["Integration Tests"]				 									= "Dev2.Integration.Tests"
$JobSpecs["Other Specs"]		 												= "Dev2.*.Specs,Warewolf.*.Specs"
$JobSpecs["Other Activities Specs"]		 										= "Dev2.Activities.Specs"
$JobSpecs["Composition Load Tests"]		 										= "Dev2.Activities.Specs", "CompositionLoadTests"
$JobSpecs["Remote Server Specs"]		 										= "Dev2.Activities.Specs", "RemoteServer"
$JobSpecs["Workflow Merging Specs"]		 										= "Dev2.Activities.Specs", "WorkflowMerging"
$JobSpecs["Example Workflow Execution Specs"] 									= "Dev2.Activities.Specs", "ExampleWorkflowExecution"
$JobSpecs["Subworkflow Execution Specs"]										= "Dev2.Activities.Specs", "SubworkflowExecution"
$JobSpecs["Workflow Execution Specs"]		 									= "Dev2.Activities.Specs", "WorkflowExecution"
$JobSpecs["Assign Workflow Execution Specs"]		 							= "Dev2.Activities.Specs", "AssignWorkflowExecution"
$JobSpecs["Studio Test Framework Specs"]		 								= "Dev2.Activities.Specs", "StudioTestFramework"
$JobSpecs["Studio Test Framework With Data Tools Specs"]		 				= "Dev2.Activities.Specs", "StudioTestFrameworkWithDataTools"
$JobSpecs["Studio Test Framework With Database Tools Specs"]					= "Dev2.Activities.Specs", "StudioTestFrameworkWithDatabaseTools"
$JobSpecs["Studio Test Framework With Deleted Resources Specs"]					= "Dev2.Activities.Specs", "StudioTestFrameworkWithDeletedResources"
$JobSpecs["Studio Test Framework With Dropbox Tools Specs"]		 				= "Dev2.Activities.Specs", "StudioTestFrameworkWithDropboxTools"
$JobSpecs["Studio Test Framework With File And Folder Tools Specs"]				= "Dev2.Activities.Specs", "StudioTestFrameworkWithFileAndFolderTools"
$JobSpecs["Studio Test Framework With Hello World Workflow Specs"]				= "Dev2.Activities.Specs", "StudioTestFrameworkWithHelloWorldWorkflow"
$JobSpecs["Studio Test Framework With HTTP Web Tools Specs"]					= "Dev2.Activities.Specs", "StudioTestFrameworkWithHTTPWebTools"
$JobSpecs["Studio Test Framework With Scripting Tools Specs"]					= "Dev2.Activities.Specs", "StudioTestFrameworkWithScriptingTools"
$JobSpecs["Studio Test Framework With Subworkflow Specs"]		 				= "Dev2.Activities.Specs", "StudioTestFrameworkWithSubworkflow"
$JobSpecs["Studio Test Framework With Utility Tools Specs"]		 				= "Dev2.Activities.Specs", "StudioTestFrameworkWithUtilityTools"
$JobSpecs["Other Security Specs"] 												= "Warewolf.Security.Specs"
$JobSpecs["Conflicting Contribute View And Execute Permissions Security Specs"] = "Warewolf.Security.Specs", "ConflictingContributeViewExecutePermissionsSecurity"
$JobSpecs["Conflicting Execute Permissions Security Specs"]					    = "Warewolf.Security.Specs", "ConflictingExecutePermissionsSecurity"
$JobSpecs["Conflicting View And Execute Permissions Security Specs"]			= "Warewolf.Security.Specs", "ConflictingViewExecutePermissionsSecurity"
$JobSpecs["Conflicting View Permissions Security Specs"]						= "Warewolf.Security.Specs", "ConflictingViewPermissionsSecurity"
$JobSpecs["No Conflicting Permissions Security Specs"]							= "Warewolf.Security.Specs", "NoConflictingPermissionsSecurity"
$JobSpecs["Overlapping User Groups Permissions Security Specs"]					= "Warewolf.Security.Specs", "OverlappingUserGroupsPermissionsSecurity"
$JobSpecs["Resource Permissions Security Specs"]								= "Warewolf.Security.Specs", "ResourcePermissionsSecurity"
$JobSpecs["Server Permissions Security Specs"]									= "Warewolf.Security.Specs", "ServerPermissionsSecurity"
#Web UI Tests
$JobSpecs["Other Web UI Tests"]													= "Warewolf.Web.UI.Tests"
$JobSpecs["Execution Logging Web UI Tests"]										= "Warewolf.Web.UI.Tests", "ExecutionLogging"
$JobSpecs["No Warewolf Server Web UI Tests"]									= "Warewolf.Web.UI.Tests", "NoWarewolfServer"
#Desktop UI Tests                                               	
$JobSpecs["Other UI Tests"]					    			= "Warewolf.UI.Tests"
$JobSpecs["Other UI Specs"]					    			= "Warewolf.UI.Specs"
$JobSpecs["Assign Tool UI Tests"]							= "Warewolf.UI.Tests", "Assign Tool"
$JobSpecs["Control Flow Tools UI Tests"]					= "Warewolf.UI.Tests", "Control Flow Tools"
$JobSpecs["Database Sources UI Tests"]						= "Warewolf.UI.Tests", "Database Sources"
$JobSpecs["Database Tools UI Tests"]						= "Warewolf.UI.Tests", "Database Tools"
$JobSpecs["Data Tools UI Tests"]							= "Warewolf.UI.Tests", "Data Tools"
$JobSpecs["DB Connector UI Specs"]							= "Warewolf.UI.Specs", "DBConnector"
$JobSpecs["Debug Input UI Tests"]							= "Warewolf.UI.Tests", "Debug Input"
$JobSpecs["Default Layout UI Tests"]						= "Warewolf.UI.Tests", "Default Layout"
$JobSpecs["Studio Shutdown UI Tests"]						= "Warewolf.UI.Tests", "Studio Shutdown"
$JobSpecs["Dependency Graph UI Tests"]						= "Warewolf.UI.Tests", "Dependency Graph"
$JobSpecs["Deploy UI Specs"]								= "Warewolf.UI.Specs", "Deploy"
$JobSpecs["Deploy Security UI Specs"]						= "Warewolf.UI.Specs", "DeploySecurity"
$JobSpecs["Deploy UI Tests"]								= "Warewolf.UI.Tests", "Deploy"
$JobSpecs["Deploy from Explorer UI Tests"]					= "Warewolf.UI.Tests", "Deploy from Explorer"
$JobSpecs["Deploy from Remote UI Tests"]					= "Warewolf.UI.Tests", "Deploy from Remote"
$JobSpecs["Deploy Filtering UI Tests"]						= "Warewolf.UI.Tests", "Deploy Filtering"
$JobSpecs["Deploy Hello World UI Tests"]					= "Warewolf.UI.Tests", "Deploy Hello World"
$JobSpecs["Deploy Select Dependencies UI Tests"]			= "Warewolf.UI.Tests", "Deploy Select Dependencies"
$JobSpecs["DotNet Connector Mocking UI Tests"]				= "Warewolf.UI.Tests", "DotNet Connector Mocking Tests"
$JobSpecs["DotNet Connector Tool UI Tests"]	    			= "Warewolf.UI.Tests", "DotNet Connector Tool"
$JobSpecs["Dropbox Tools UI Tests"]			    			= "Warewolf.UI.Tests", "Dropbox Tools"
$JobSpecs["Email Tools UI Tests"]							= "Warewolf.UI.Tests", "Email Tools"
$JobSpecs["Explorer UI Specs"]								= "Warewolf.UI.Specs", "Explorer"
$JobSpecs["Explorer UI Tests"]								= "Warewolf.UI.Tests", "Explorer"
$JobSpecs["File Tools UI Tests"]							= "Warewolf.UI.Tests", "File Tools"
$JobSpecs["Hello World Mocking UI Tests"]					= "Warewolf.UI.Tests", "Hello World Mocking Tests"
$JobSpecs["HTTP Tools UI Tests"]							= "Warewolf.UI.Tests", "HTTP Tools"
$JobSpecs["Plugin Sources UI Tests"]						= "Warewolf.UI.Tests", "Plugin Sources"
$JobSpecs["Recordset Tools UI Tests"]						= "Warewolf.UI.Tests", "Recordset Tools"
$JobSpecs["Resource Tools UI Tests"]						= "Warewolf.UI.Tests", "Resource Tools"
$JobSpecs["Save Dialog UI Specs"]							= "Warewolf.UI.Specs", "SaveDialog"
$JobSpecs["Save Dialog UI Tests"]							= "Warewolf.UI.Tests", "Save Dialog"
$JobSpecs["Server Sources UI Tests"]						= "Warewolf.UI.Tests", "Server Sources"
$JobSpecs["Settings UI Tests"]								= "Warewolf.UI.Tests", "Settings"
$JobSpecs["Sharepoint Tools UI Tests"]						= "Warewolf.UI.Tests", "Sharepoint Tools"
$JobSpecs["Shortcut Keys UI Tests"]			    			= "Warewolf.UI.Tests", "Shortcut Keys"
$JobSpecs["Source Wizards UI Tests"]						= "Warewolf.UI.Tests", "Source Wizards"
$JobSpecs["Tabs And Panes UI Tests"]						= "Warewolf.UI.Tests", "Tabs and Panes"
$JobSpecs["Tools UI Tests"]					    			= "Warewolf.UI.Tests", "Tools"
$JobSpecs["Utility Tools UI Tests"]			    			= "Warewolf.UI.Tests", "Utility Tools"
$JobSpecs["Variables UI Tests"]				    			= "Warewolf.UI.Tests", "Variables"
$JobSpecs["Web Connector UI Specs"]			    			= "Warewolf.UI.Specs", "WebConnector"
$JobSpecs["Web Sources UI Tests"]							= "Warewolf.UI.Tests", "Web Sources"
$JobSpecs["Workflow Mocking Tests UI Tests"]				= "Warewolf.UI.Tests", "Workflow Mocking Tests"
$JobSpecs["Workflow Testing UI Tests"]						= "Warewolf.UI.Tests", "Workflow Testing"
$JobSpecs["Workflow Merge with All Tools Conflicting"]		= "Warewolf.UI.Tests", "Merge All Tools Conflicts"
$JobSpecs["Workflow Merge with Assign Tools Conflicting"]	= "Warewolf.UI.Tests", "Merge Assign Conflicts"
$JobSpecs["Workflow Merge with Decision Tools Conflicting"]	= "Warewolf.UI.Tests", "Merge Decision Conflicts"
$JobSpecs["Workflow Merge with Foreach Tools Conflicting"]	= "Warewolf.UI.Tests", "Merge Foreach"
$JobSpecs["Workflow Merge with Sequence Tools Conflicting"]	= "Warewolf.UI.Tests", "Merge Sequence Conflicts"
$JobSpecs["Workflow Merge with Simple Tools Conflicting"]	= "Warewolf.UI.Tests", "Merge Simple Tools Conflicts"
$JobSpecs["Workflow Merge with Switch Tools Conflicting"]	= "Warewolf.UI.Tests", "Merge Switch Conflicts"
$JobSpecs["Workflow Merge with Variables Conflicting"]		= "Warewolf.UI.Tests", "Merge Variable Conflicts"
$JobSpecs["Search UI Tests"]								= "Warewolf.UI.Tests", "Search"
$JobSpecs["Input Search UI Tests"]							= "Warewolf.UI.Tests", "Input Search"
$JobSpecs["Output Search UI Tests"]							= "Warewolf.UI.Tests", "Output Search"
$JobSpecs["Test Name Search UI Tests"]						= "Warewolf.UI.Tests", "Test Name Search"
$JobSpecs["Scalar Search UI Tests"]							= "Warewolf.UI.Tests", "Scalar Search"
$JobSpecs["Recordset Search UI Tests"]						= "Warewolf.UI.Tests", "Recordset Search"
$JobSpecs["Object Search UI Tests"]							= "Warewolf.UI.Tests", "Object Search"
$JobSpecs["Service Search UI Tests"]						= "Warewolf.UI.Tests", "Service Search"
$JobSpecs["Title Search UI Tests"]							= "Warewolf.UI.Tests", "Title Search"
#Load Tests
$JobSpecs["UI Load Specs"]	= "Warewolf.UI.Load.Specs"
$JobSpecs["Load Tests"]		= "Dev2.Integration.Tests", "Load Tests"

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

if ($JobName -ne "") {
    $JobNames = $JobName
    $JobName = ""
}

if ($ContainerRemoteApiHost -ne "" -and -not $ContainerRemoteApiHost.StartsWith("-H ")) {
    $ContainerRemoteApiHost = "-H " + $ContainerRemoteApiHost
    if ($ContainerRemoteApiHost.Contains(",")) {
        $ContainerRemoteApiHost = $ContainerRemoteApiHost.Replace(",", ",-H ") 
    }
}

if (!$StartServerAsConsole.IsPresent) {
    [bool]$ConsoleServer = $False
} else {
    [bool]$ConsoleServer = $True
}

[bool]$ApplyDotCover = $false
if ($JobNames.Contains(" DotCover")) {
    [bool]$ApplyDotCover = $True
    $JobNames = $JobNames.Replace(" DotCover", "")
} else {
    if ($DotCoverPath -ne "") {
        if (!(Test-Path $DotCoverPath)) {
            Write-Error -Message "Cannot find DotCover.exe. Please provide a path to that file as a commandline parameter like this: -DotCoverPath"
            exit 1
        }
        [bool]$ApplyDotCover = $true
    }
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
	                $CurrentDirectory = $TestsPath
                }
            }
		    if (Test-Path "$CurrentDirectory\$FileSpec") {
                $FilePath = Join-Path $CurrentDirectory $FileSpec                   
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

function Copy-On-Write([string]$FilePath) {
    if (Test-Path $FilePath) {
        $num = 1
        $FileExtention = (Get-Item $FilePath -ErrorAction Stop).Extension
        $FilePathWithoutExtention = $FilePath.Substring(0, $FilePath.LastIndexOf('.')+1)
        while(Test-Path "$FilePathWithoutExtention$num$FileExtention")
        {
            $num += 1
        }
        $FilePath | Move-Item -Destination "$FilePathWithoutExtention$num$FileExtention"
    }
}

function Move-File-To-TestResults([string]$SourceFilePath, [string]$DestinationFileName) {
    $DestinationFilePath = "$TestsResultsPath\$DestinationFileName"
    if (Test-Path $SourceFilePath) {
        Copy-On-Write $DestinationFilePath
        Write-Host Moving `"$SourceFilePath`" to `"$DestinationFilePath`"
        Move-Item "$SourceFilePath" "$DestinationFilePath"
    }
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

    #Stop my.warewolf.io
    taskkill /im iisexpress.exe /f  2>&1 | %{if (!($_.ToString().StartsWith("ERROR: "))) {Write-Host $_}}

    if (!$ConsoleServer) {
        #Stop Server
        $ServiceOutput = ""
        sc.exe stop "Warewolf Server" 2>&1 | %{$ServiceOutput += "`n" + $_}
        if ($ServiceOutput -ne "`n[SC] ControlService FAILED 1062:`n`nThe service has not been started.`n") {
            Write-Host $ServiceOutput.TrimStart("`n")
            Wait-Process "Warewolf Server" -Timeout $WaitForCloseTimeout  2>&1 | out-null
        }
    }
    taskkill /im "Warewolf Server.exe" /f  2>&1 | out-null
    taskkill /im "operadriver.exe" /f  2>&1 | out-null
    taskkill /im "geckodriver.exe" /f  2>&1 | out-null
    taskkill /im "IEDriverServer.exe" /f  2>&1 | out-null

    #Delete Certain Studio and Server Resources
    $ToClean = "$env:LOCALAPPDATA\Warewolf\DebugData\PersistSettings.dat",
               "$env:LOCALAPPDATA\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml",
               "$env:PROGRAMDATA\Warewolf\Workspaces",
               "$env:PROGRAMDATA\Warewolf\Server Settings",
               "$env:PROGRAMDATA\Warewolf\VersionControl",
               "$env:PROGRAMDATA\Warewolf\Resources",
               "$env:PROGRAMDATA\Warewolf\Tests"

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

function Get-ContainerName([string]$JobName) {
    $JobName.Replace(" ", "_") + "_Container" + (&{If("$JobContainerVersion" -eq "") {""} Else {"_" + $JobContainerVersion.ToLower().SubString(0,8)}})
}

function Stop-JobContainer([string]$ContainerName) {
    foreach($JobContainerRemoteApiHost in $ContainerRemoteApiHost.Split(",")) {
        $ResultsDirectory = $TestsResultsPath + "\" + $ContainerName
        if ($(docker $JobContainerRemoteApiHost container ls --format 'table {{.Names}}' | % { $_ -eq $ContainerName }) -eq $true) {
            docker $JobContainerRemoteApiHost exec -d $ContainerName -Cleanup
        }
        if ($(docker $JobContainerRemoteApiHost container ls -a --format 'table {{.Names}}' | % { $_ -eq $ContainerName }) -eq $true) {
		    docker $JobContainerRemoteApiHost cp $($ContainerName + ":C:\Build\TestResults") "$ResultsDirectory" 2>&1
		    docker $JobContainerRemoteApiHost container rm $ContainerName 2>&1
            Write-Host $ContainerName Removed from $JobContainerRemoteApiHost See $ResultsDirectory
        }
    }
}

function Stop-JobContainers {
    foreach ($Job in $JobNames.Split(",")) {
        $JobContainerName = Get-ContainerName $Job
        Stop-JobContainer $JobContainerName
    }
}

function Cleanup-JobContainers {
    foreach($JobContainerRemoteApiHost in $ContainerRemoteApiHost.Split(",")) {
        foreach ($Job in $JobNames.Split(",")) {
            $JobContainerName = Get-ContainerName $Job
            if ($(docker $JobContainerRemoteApiHost container ls --format 'table {{.Names}}' | % { $_ -eq $JobContainerName }) -eq $true) {
                Write-Host Waiting for $JobContainerName on $JobContainerRemoteApiHost
                docker $JobContainerRemoteApiHost container logs --follow $JobContainerName
            }
	    }
    }
    Stop-JobContainers
}

function Cleanup-ServerContainer {
    if ($(docker $ContainerRemoteApiHost container ls --format 'table {{.Names}}' | % { $_ -eq "warewolfserver" }) -eq $true) {
        Write-Host Recovering Warewolf server container program data to $TestsResultsPath
        docker $ContainerRemoteApiHost stop "warewolfserver"
    }
    if ($(docker $ContainerRemoteApiHost container ls -a --format 'table {{.Names}}' | % { $_ -eq "warewolfserver" }) -eq $true) {
        Write-Host Recovering Warewolf server container program data to $TestsResultsPath
		docker $ContainerRemoteApiHost cp "warewolfserver:C:\ProgramData\Warewolf" "$TestsResultsPath" 2>&1
		docker $ContainerRemoteApiHost container rm "warewolfserver" 2>&1
    }
    if ($(docker $ContainerRemoteApiHost images --format 'table {{.Repository}}' | % { $_ -eq "warewolfserver" }) -eq $true) {
        docker $ContainerRemoteApiHost rmi "warewolfserver"
    }
}

function Timeout-JobContainers {
    $ContainerUpTimes = docker $ContainerRemoteApiHost container ls -a --format 'table {{.Names}} {{.Status}}'
    foreach ($Job in $JobNames.Split(",")) {
        $JobContainerName = Get-ContainerName $Job
        foreach ($UpTime in $ContainerUpTimes) {
            if ($UpTime.Split(" ").Count -ge 4) {
                $ContainerName = $UpTime.Split(" ")[0]
                $Status = $UpTime.Split(" ")[1]
                $Duration = $UpTime.Split(" ")[2]
                $DurationUnit = $UpTime.Split(" ")[3]
                if ($ContainerName -eq $JobContainerName -and $Status -eq "Up" -and (($DurationUnit -eq "minutes" -and $Duration -is [int32] -and [int32]$Duration -gt 30) -or $DurationUnit -eq "hours")) {
                    Stop-JobContainer $ContainerName
                }
            }
        }
    }
}

function Wait-For-FileUnlock([string]$FilePath) {
    $locked = $true
    $RetryCount = 0
    while($locked -and $RetryCount -lt 12) {
        $RetryCount++
        try {
            [IO.File]::OpenWrite($FilePath).close()
            $locked = $false
        } catch {
            Write-Host Still waiting for $FilePath file to unlock.
            Sleep 10
        }
    }
    return $locked
}

function Wait-For-FileExist([string]$FilePath) {
    $exists = $false
    $RetryCount = 0
    while(!($exists) -and $RetryCount -lt 12) {
        $RetryCount++
        if (Test-Path $FilePath) {
            $exists = $true
        } else {
            Write-Host Still waiting for $FilePath file to exist.
            Sleep 10
        }
    }
    return $exists
}

function Merge-DotCover-Snapshots($DotCoverSnapshots, [string]$DestinationFilePath, [string]$LogFilePath) {
    if ($DotCoverSnapshots -ne $null) {
        if ($DotCoverSnapshots.Count -gt 1) {
            $DotCoverSnapshotsString = $DotCoverSnapshots -join "`";`""
            Copy-On-Write "$LogFilePath.merge.log"
            Copy-On-Write "$LogFilePath.report.log"
            Copy-On-Write "$DestinationFilePath.dcvr"
            Copy-On-Write "$DestinationFilePath.html"
            &"$DotCoverPath" "merge" "/Source=`"$DotCoverSnapshotsString`"" "/Output=`"$DestinationFilePath.dcvr`"" "/LogFile=`"$LogFilePath.merge.log`""
        }
        if ($DotCoverSnapshots.Count -eq 1) {
            $LoneSnapshot = $DotCoverSnapshots[0].FullName
            if ($DotCoverSnapshots.Count -eq 1 -and (Test-Path "$LoneSnapshot")) {
                &"$DotCoverPath" "report" "/Source=`"$LoneSnapshot`"" "/Output=`"$DestinationFilePath\DotCover Report.html`"" "/ReportType=HTML" "/LogFile=`"$LogFilePath.report.log`""
                Write-Host DotCover report written to $DestinationFilePath\DotCover Report.html
            }
        }
    }
    if (Test-Path "$DestinationFilePath.dcvr") {
        &"$DotCoverPath" "report" "/Source=`"$DestinationFilePath.dcvr`"" "/Output=`"$DestinationFilePath\DotCover Report.html`"" "/ReportType=HTML" "/LogFile=`"$LogFilePath.report.log`""
        Write-Host DotCover report written to $DestinationFilePath\DotCover Report.html
    }
}

function Move-Artifacts-To-TestResults([bool]$DotCover, [bool]$Server, [bool]$Studio, [string]$JobName) {
    if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\TestResults\*.trx") {
        Move-Item "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\TestResults\*.trx" "$TestsResultsPath"
        Write-Host Moved loose TRX files from VS install directory into TestResults.
    }

    if (!($Cleanup.IsPresent)) {
        # Write failing tests playlist.
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
            } elseif ($trxContent.TestRun.Results.UnitTestResult -eq $null) {
		        Write-Host Error parsing TestRun.Results.UnitTestResult from trx file at $_.FullName
            }
        }
        $PlayList += "</Playlist>"
        $OutPlaylistPath = $TestsResultsPath + "\" + $JobName + " Failures.playlist"
        Copy-On-Write $OutPlaylistPath
        $PlayList | Out-File -LiteralPath $OutPlaylistPath -Encoding utf8 -Force
        Write-Host Playlist file written to `"$OutPlaylistPath`".
    }

    if ($Studio) {
        Move-File-To-TestResults "$env:LocalAppData\Warewolf\Studio Logs\Warewolf Studio.log" "$JobName Studio.log"
    }
    if ($Studio -and $DotCover) {
        $StudioSnapshot = "$env:LocalAppData\Warewolf\Studio Logs\dotCover.dcvr"
        Write-Host Trying to move Studio coverage snapshot file from $StudioSnapshot to $TestsResultsPath\$JobName Studio DotCover.dcvr
        $exists = Wait-For-FileExist $StudioSnapshot
        if ($exists) {
            $locked = Wait-For-FileUnlock $StudioSnapshot
            if (!($locked)) {
                Write-Host Moving Studio coverage snapshot file from $StudioSnapshot to $TestsResultsPath\$JobName Studio DotCover.dcvr
                Move-Item $StudioSnapshot "$TestsResultsPath\$JobName Studio DotCover.dcvr" -force
            } else {
                Write-Host Studio Coverage Snapshot File is locked.
            }
        } else {
		    Write-Error -Message "Studio coverage snapshot not found at $StudioSnapshot"
        }
        if (Test-Path "$env:LocalAppData\Warewolf\Studio Logs\dotCover.log") {
            Move-File-To-TestResults "$env:LocalAppData\Warewolf\Studio Logs\dotCover.log" "$JobName Studio DotCover.log"
        }
    }
    if ($Server) {
        Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\wareWolf-Server.log" "$JobName Server.log"
        Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\my.warewolf.io.log" "$JobName my.warewolf.io Server.log"
        Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\my.warewolf.io.errors.log" "$JobName my.warewolf.io Server Errors.log"
    }
    if ($Server -and $DotCover) {
        $ServerSnapshot = "$env:ProgramData\Warewolf\Server Log\dotCover.dcvr"
        Write-Host Trying to move Server coverage snapshot file from $ServerSnapshot to $TestsResultsPath\$JobName Server DotCover.dcvr
        $exists = Wait-For-FileExist $ServerSnapshot
        if ($exists) {
            $locked = Wait-For-FileUnlock $ServerSnapshot
            if (!($locked)) {
                Write-Host Moving Server coverage snapshot file from $ServerSnapshot to $TestsResultsPath\$JobName Server DotCover.dcvr
                Move-File-To-TestResults $ServerSnapshot "$JobName Server DotCover.dcvr"
            } else {
                Write-Host Server Coverage Snapshot File still locked after retrying for 2 minutes.
            }
        }
        if (Test-Path "$env:ProgramData\Warewolf\Server Log\dotCover.log") {
            Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\dotCover.log" "$JobName Server DotCover.log"
        }
        if (Test-Path "$env:ProgramData\Warewolf\Server Log\my.warewolf.io.log") {
            Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\my.warewolf.io.log" "$JobName my.warewolf.io.log"
        }
        if (Test-Path "$env:ProgramData\Warewolf\Server Log\my.warewolf.io.errors.log") {
            Move-File-To-TestResults "$env:ProgramData\Warewolf\Server Log\my.warewolf.io.errors.log" "$JobName my.warewolf.io Errors.log"
        }
    }
    if ($Server -and $Studio -and $DotCover) {
        Merge-DotCover-Snapshots @("$TestsResultsPath\$JobName Server DotCover.dcvr", "$TestsResultsPath\$JobName Studio DotCover.dcvr") "$TestsResultsPath\$JobName Merged Server and Studio DotCover" "$TestsResultsPath\ServerAndStudioDotCoverSnapshot"
    }
    if ($RecordScreen.IsPresent) {
        Move-ScreenRecordings-To-TestResults
    }
    if (Test-Path "$TestsResultsPath\..\Run *.bat") {
        foreach ($testRunner in (Get-ChildItem "$TestsResultsPath\..\Run *.bat")) {
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

function Find-Warewolf-Server-Exe {
    $ServerPath = FindFile-InParent $ServerPathSpecs
    if ($ServerPath.EndsWith(".zip")) {
		Expand-Archive "$ServerPath" "$TestsResultsPath\Server" -Force
		$ServerPath = "$TestsResultsPath\Server\" + $ServerExeName
	}
    return $ServerPath
}

if (($ServerPath -eq $null -or $ServerPath -eq "" -or !(Test-Path $ServerPath)) -and !($RunWarewolfServiceTests.IsPresent)) {
    $ServerPath = Find-Warewolf-Server-Exe
}

function Install-Server {
    if ($ServerPath -eq $null -or $ServerPath -eq "" -or !(Test-Path $ServerPath)) {
        Write-Error -Message "Cannot find Warewolf Server.exe. Please provide a path to that file as a commandline parameter like this: -ServerPath"
        exit 1
    }
    Write-Warning "Will now stop any currently running Warewolf servers and studios. Resources will be backed up to $TestsResultsPath."
    if ($ResourcesType -eq "") {
	    $title = "Server Resources"
	    $message = "What type of resources would you like to install the server with?"

	    $UITest = New-Object System.Management.Automation.Host.ChoiceDescription "&UITest", `
		    "Uses these resources for running UI Tests."

	    $ServerTest = New-Object System.Management.Automation.Host.ChoiceDescription "&ServerTests", `
		    "Uses these resources for running everything except unit tests and Coded UI tests."

	    $Release = New-Object System.Management.Automation.Host.ChoiceDescription "&Release", `
		    "Uses these resources for Warewolf releases."

	    $UILoad = New-Object System.Management.Automation.Host.ChoiceDescription "&Load", `
		    "Uses these resources for Studio UI Load Testing."

	    $options = [System.Management.Automation.Host.ChoiceDescription[]]($UITest, $ServerTest, $Release, $UILoad)

	    $result = $host.ui.PromptForChoice($title, $message, $options, 0) 

	    switch ($result)
		    {
			    0 {$ResourcesType = "UITests"}
			    1 {$ResourcesType = "ServerTests"}
			    2 {$ResourcesType = "Release"}
			    3 {$ResourcesType = "Load"}
		    }
    }

    if (!$ConsoleServer) {
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
	    <ScopeEntry>$ServerBinDir\*.dll</ScopeEntry>
	    <ScopeEntry>$ServerBinDir\*.exe</ScopeEntry>
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>
"@

            if (!$JobNames) {
			    if ($ProjectName) {
				    $JobNames = $ProjectName
			    } else {
				    $JobNames = "Manual Tests"
			    }
            }
            $DotCoverRunnerXMLPath = "$TestsResultsPath\Server DotCover Runner.xml"
            Copy-On-Write $DotCoverRunnerXMLPath
            Out-File -LiteralPath "$DotCoverRunnerXMLPath" -Encoding default -InputObject $RunnerXML
            if ($IsInContainer.IsPresent) {
                $BinPathWithDotCover = "`"" + $DotCoverPath + "`" cover `"$DotCoverRunnerXMLPath`" /LogFile=`"$TestsResultsPath\ServerDotCover.log`""
            } else {
                $BinPathWithDotCover = "\`"" + $DotCoverPath + "\`" cover \`"$DotCoverRunnerXMLPath\`" /LogFile=\`"$TestsResultsPath\ServerDotCover.log\`""
            }
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
    $ResourcesType
}

function Start-Server {
    $ServerFolderPath = (Get-Item $ServerPath).Directory.FullName
    Write-Host Deploying New resources from $ServerFolderPath\Resources - $ResourcesType\*
    if (!(Test-Path "$env:ProgramData\Warewolf")) {
        New-Item "$env:ProgramData\Warewolf" -ItemType Directory
    }
    Copy-Item -Path ($ServerFolderPath + "\Resources - $ResourcesType\*") -Destination "$env:ProgramData\Warewolf" -Recurse -Force
	
    if (!$ConsoleServer) {
        Start-Service "Warewolf Server"

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
        Write-Host Server has started.
    } else {
        Write-Host Server has started.
        Start-Process -FilePath "$ServerPath" -ArgumentList "--interactive" -Verb RunAs -Wait
    }
}

function Start-my.warewolf.io {
    if ($TestsPath.EndsWith("\")) {
        $WebsPath = $TestsPath + "_PublishedWebsites\Dev2.Web"
    } else {
        $WebsPath = $TestsPath + "\_PublishedWebsites\Dev2.Web"
    }
    Write-Host Starting my.warewolf.io from $WebsPath
    if (!(Test-Path $WebsPath)) {
        if ($ServerPath -eq $null -or $ServerPath -eq "" -or !(Test-Path $ServerPath)) {
            Write-Error -Message "Cannot find Warewolf Server.exe. Please provide a path to that file as a commandline parameter like this: -ServerPath"
            exit 1
        }
        $WebsPath = (Get-Item $ServerPath).Directory.FullName + "\_PublishedWebsites\Dev2.Web"
    }
    Cleanup-ServerStudio
    if (Test-Path $WebsPath) {
        $IISExpressPath = "C:\Program Files (x86)\IIS Express\iisexpress.exe"
        if (!(Test-Path $IISExpressPath)) {
            Write-Warning "my.warewolf.io cannot be hosted. $IISExpressPath not found."
        } else {
            Write-Host `"$IISExpressPath`" /path:`"$WebsPath`" /port:18405 /trace:error
            Start-Process -FilePath $IISExpressPath -ArgumentList "/path:`"$WebsPath`" /port:18405 /trace:error" -NoNewWindow -PassThru -RedirectStandardOutput "$env:programdata\Warewolf\Server Log\my.warewolf.io.log" -RedirectStandardError "$env:programdata\Warewolf\Server Log\my.warewolf.io.errors.log"
            Write-Host my.warewolf.io has started.
        }
    } else {
        Write-Warning "my.warewolf.io cannot be hosted. Webs not found at $TestsPath\_PublishedWebsites\Dev2.Web or at $ServerFolderPath\_PublishedWebsites\Dev2.Web"
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
    	<ScopeEntry>$StudioBinDir\*.dll</ScopeEntry>
    	<ScopeEntry>$StudioBinDir\*.exe</ScopeEntry>
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>
"@
        $DotCoverRunnerXMLPath = "$TestsResultsPath\Studio DotCover Runner.xml"
        Copy-On-Write $DotCoverRunnerXMLPath
        Out-File -LiteralPath "$DotCoverRunnerXMLPath" -Encoding default -InputObject $RunnerXML
		Start-Process $DotCoverPath "cover `"$DotCoverRunnerXMLPath`" /LogFile=`"$TestsResultsPath\StudioDotCover.log`""
    }
    Write-Host "Waiting for Studio at $StudioPath to start..."
    $TimeoutCounter = 0
    $StudioStartedFilePath = (Get-Item $StudioPath).Directory.FullName + "\StudioStarted"
    while (!(Test-Path $StudioStartedFilePath) -and $TimeoutCounter++ -lt 200) {
        Write-Warning "Waiting for Studio to start..."
        sleep 3
    }
    if (!(Test-Path $StudioStartedFilePath)) {
		Write-Error -Message "Warewolf studio failed to start within 10 minutes."
        exit 1
    }
    Write-Host Studio has started.
}

function Start-ServerContainer {
    Cleanup-ServerContainer
    $ServerFolderPath = (Get-Item $ServerPath).Directory.FullName
    Out-File -LiteralPath "$ServerFolderPath\dockerfile" -Encoding default -InputObject @"
FROM microsoft/windowsservercore

RUN NET user WarewolfAdmin W@rEw0lf@dm1n /ADD
RUN NET localgroup "Administrators" WarewolfAdmin /ADD
RUN NET localgroup "Warewolf Administrators" /ADD
RUN NET localgroup "Warewolf Administrators" WarewolfAdmin /ADD
EXPOSE 3142
EXPOSE 3143

SHELL ["powershell"]
RUN New-Item -Path Build -ItemType Directory
ADD . Build
ENV SCRIPT_PATH "Build\Run Tests.ps1"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"

ENTRYPOINT & `$env:SCRIPT_PATH
CMD ["-StartServerAsConsole", "-ResourcesType", "Release"]
"@
    docker $ContainerRemoteApiHost build -t warewolfserver "$ServerFolderPath"
    docker $ContainerRemoteApiHost container inspect warewolfserver
    docker $ContainerRemoteApiHost run --name "warewolfserver" -di warewolfserver -StartServerAsConsole -ResourcesType `'$ResourcesType`'
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

function AllCategoriesDefinedForProject([string]$AssemblyNameToCheck) {
    $JobCategorySpecs = @()
    foreach ($Job in $JobSpecs.Values) {
        if ($Job.Count -gt 1 -and $AssemblyNameToCheck -eq $Job[0]) {
            $JobCategorySpecs += $Job[1]
        }
    }
    $JobCategorySpecs
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

function Pick-TestAgent {
    $Timeout = 30
    while($Timeout-- -gt 0) {
        foreach($JobContainerRemoteApiHost in $ContainerRemoteApiHost.Split(",")) {
            if ([int]((docker $JobContainerRemoteApiHost info --format '{{json .}}' | ConvertFrom-Json).MemTotal /2147483648) - 
                [int]((docker $JobContainerRemoteApiHost info --format '{{json .}}' | ConvertFrom-Json).ContainersRunning) -ge 0) {
                return $JobContainerRemoteApiHost
            }
        }
        sleep 10
    }
    Write-Error -Message "Test Controller timed out waiting for a test agent to be available."
    exit 1
}

#Unpack jobs
$JobNamesList = @()
$JobAssemblySpecs = @()
$JobCategories = @()
if ($JobNames -ne $null -and $JobNames -ne "" -and $MergeDotCoverSnapshotsInDirectory -eq "" -and $Cleanup.IsPresent -eq $false) {
    foreach ($Job in $JobNames.Split(",")) {
        $Job = $Job.TrimEnd("1234567890 ")
        if ($JobSpecs.ContainsKey($Job)) {
            $JobNamesList += $Job
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
    $JobNamesList += $ProjectName
    $JobAssemblySpecs += $ProjectName
    if ($Category -ne $null -and $Category -ne "") {
        $JobCategories += $Category
    } else {
        $JobCategories += ""
    }
}
$TotalNumberOfJobsToRun = $JobNamesList.length
if ($TotalNumberOfJobsToRun -gt 0) {
    if ($VSTestPath -ne "" -and !(Test-Path "$VSTestPath" -ErrorAction SilentlyContinue)) {
        if (Test-Path $VSTestPath.Replace("Enterprise", "Professional")) {
            $VSTestPath = $VSTestPath.Replace("Enterprise", "Professional")
        }
        if (Test-Path $VSTestPath.Replace("Enterprise", "Community")) {
            $VSTestPath = $VSTestPath.Replace("Enterprise", "Community")
        }
        if (Test-Path $VSTestPath.Replace("Enterprise", "TestAgent")) {
            $VSTestPath = $VSTestPath.Replace("Enterprise", "TestAgent")
        }
    }
    if ($MSTestPath -ne "" -and !(Test-Path "$MSTestPath" -ErrorAction SilentlyContinue)) {
        if (Test-Path $MSTestPath.Replace("Enterprise", "Professional")) {
            $MSTestPath = $MSTestPath.Replace("Enterprise", "Professional")
        }
        if (Test-Path $MSTestPath.Replace("Enterprise", "Community")) {
            $MSTestPath = $MSTestPath.Replace("Enterprise", "Community")
        }
        if (Test-Path $MSTestPath.Replace("Enterprise", "TestAgent")) {
            $MSTestPath = $MSTestPath.Replace("Enterprise", "TestAgent")
        }
    }

    if(!$JobContainers.IsPresent) {
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

        if ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent -or $StartStudio.IsPresent) {
            $ResourcesType = Install-Server
        }
    }

    if (!$MSTest.IsPresent) {
        # Read playlists and args.
        if ($TestList -eq "") {
            Get-ChildItem "$TestsPath" -Filter *.playlist | `
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
            if ($TestList.StartsWith(",")) {
	            $TestList = $TestList -replace "^.", " /Tests:"
            }
        }
    } else {
        if ($TestList -eq "") {
            Get-ChildItem "$TestsPath" -Filter *.playlist | `
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
    if ($JobContainers.IsPresent) {
        Cleanup-JobContainers
    }
    foreach ($_ in 0..($TotalNumberOfJobsToRun-1)) {
        $JobName = $JobNamesList[$_].ToString()
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
        if ($JobContainers.IsPresent) {
            $JobContainerRemoteApiHost = $ContainerRemoteApiHost
            if ($ContainerRemoteApiHost.Contains(",")) {
                $JobContainerRemoteApiHost = Pick-TestAgent
            }
            $TestEnvironmentImageName = "warewolftestenvironment"
            if ("$ContainerRegistryHost" -ne "") {
                $TestEnvironmentImageName = $ContainerRegistryHost + "/" + $TestEnvironmentImageName
            }
            if (($(docker $JobContainerRemoteApiHost images) | ConvertFrom-String | ? {  $_.P1 -eq $TestEnvironmentImageName -and $_.P2 -eq $JobContainerVersion }) -eq $null -and ($(docker $JobContainerRemoteApiHost images) | ConvertFrom-String | ? {  $_.P1 -eq $TestEnvironmentImageName }) -eq $null) {
                Write-Host Image $TestEnvironmentImageName missing from $JobContainerRemoteApiHost
                docker $JobContainerRemoteApiHost pull $TestEnvironmentImageName 2>&1
                if (($(docker $JobContainerRemoteApiHost images) | ConvertFrom-String | ? {  $_.P1 -eq $TestEnvironmentImageName }) -eq $null) {
                    Write-Host Image $TestEnvironmentImageName still missing from $JobContainerRemoteApiHost after pull
                    $DockerfileContent = @"
FROM microsoft/windowsservercore

ENV chocolateyUseWindowsCompression=false
RUN @powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin"

RUN choco install visualstudio2017testagent --package-parameters "--passive --locale en-US --includeOptional" --confirm --limit-output --timeout 216000

SHELL ["powershell"]
RUN if (!(Test-Path \"`C:\Program Files (x86)\Microsoft Visual Studio\2017\TestAgent\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe\")) {Write-Host VSTest did not install correctly; exit 1}
"@
                    Out-File -LiteralPath "$TestsPath\dockerfile" -Encoding default -InputObject $DockerfileContent
                    Write-Host Docker dockerfile written as:`n$DockerfileContent
                    Write-Host Docker $JobContainerRemoteApiHost build -t $TestEnvironmentImageName "$TestsPath"
                    docker $JobContainerRemoteApiHost build -t $TestEnvironmentImageName "$TestsPath"
                }
            }
            $ImageName = "jobsenvironment"
            if ("$ContainerRegistryHost" -ne "") {
                $ImageName = $ContainerRegistryHost + "/" + $ImageName
            }
            if (($(docker $JobContainerRemoteApiHost images) | ConvertFrom-String | ? {  $_.P1 -eq $ImageName -and $_.P2 -eq $JobContainerVersion }) -eq $null) {
                Write-Host Version $JobContainerVersion of image $ImageName missing from $JobContainerRemoteApiHost
                if ("$JobContainerVersion" -ne "") {
                    docker $JobContainerRemoteApiHost pull ($ImageName + ":" + $JobContainerVersion) 2>&1
                } else {
                    docker $JobContainerRemoteApiHost pull $ImageName 2>&1
                }
                if (($(docker $JobContainerRemoteApiHost images) | ConvertFrom-String | ? {  $_.P1 -eq $ImageName -and $_.P2 -eq $JobContainerVersion }) -eq $null) {
                    Write-Host Version $JobContainerVersion of image $ImageName still missing from $JobContainerRemoteApiHost after pull
                    $DockerfileContent = @"
FROM warewolftestenvironment
SHELL ["powershell"]

RUN New-Item -Path Build -ItemType Directory
ADD . Build
ENV SCRIPT_PATH "Build\Run Tests.ps1"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"

ENTRYPOINT & `$env:SCRIPT_PATH
"@
                    Out-File -LiteralPath "$TestsPath\dockerfile" -Encoding default -InputObject $DockerfileContent
                    $DockerIgnorefileContent = @"
dockerfile
TestResults/**/*
TestResults
"@
                    Out-File -LiteralPath "$TestsPath\.dockerignore" -Encoding default -InputObject $DockerIgnorefileContent
                    Write-Host Docker dockerfile written as:`n$DockerfileContent
                    Write-Host `nDocker ignore file written as:`n$DockerIgnorefileContent
                    Write-Host docker $JobContainerRemoteApiHost build -t $ImageName "$TestsPath"
                    docker $JobContainerRemoteApiHost build -t $ImageName "$TestsPath"
                    if ("$JobContainerVersion" -ne "") {
                        docker $JobContainerRemoteApiHost tag $ImageName $JobContainerVersion
                        docker $JobContainerRemoteApiHost push $ImageName
                    }
                }
            }
            if ("$JobContainerVersion" -ne "") {
                $ImageName = $ImageName + ":" + $JobContainerVersion
            }
            $JobContainerName = Get-ContainerName $JobName
            if ((docker $JobContainerRemoteApiHost node ls 2>&1).GetType() -eq [System.Management.Automation.ErrorRecord]) {
                $JobContainerResult = "", "Insufficient system resources exist to complete the requested service. The paging file is too small for this operation to complete."
                while(([string]$JobContainerResult[1]).Contains("The paging file is too small for this operation to complete.") -or ([string]$JobContainerResult[1]).Contains("Insufficient system resources exist to complete the requested service.") -or ([string]$JobContainerResult[1]).Contains("This operation returned because the timeout period expired. (0x5b4).")) {
                    if ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent) {
                        $JobContainerResult = docker $JobContainerRemoteApiHost run --memory="1500m" --name $JobContainerName -di $ImageName -JobName `'$JobName`' -TestList `'$TestList`' -DotCoverPath `'$DotCoverPath`' -IsInContainer -StartServer -ServerPath `'C:\Build\Warewolf Server.exe`' -ResourcesType `'$ResourcesType`' 2>&1
                    } else {
                        $JobContainerResult = docker $JobContainerRemoteApiHost run --memory="700m" --name $JobContainerName -di $ImageName -JobName `'$JobName`' -TestList `'$TestList`' -DotCoverPath `'$DotCoverPath`' -IsInContainer 2>&1
                    }
                    if (([string]$JobContainerResult[1]).Contains("The paging file is too small for this operation to complete.") -or ([string]$JobContainerResult[1]).Contains("Insufficient system resources exist to complete the requested service.") -or ([string]$JobContainerResult[1]).Contains("This operation returned because the timeout period expired. (0x5b4).")) {
                        docker $JobContainerRemoteApiHost container rm $JobContainerName 2>&1
                        Write-Host Out of memory. Timing out containers and waiting 30s before trying to start $JobContainerName again.
                        Timeout-JobContainers
                        sleep 30
                    } else {
                        Write-Host Started $JobContainerName as $JobContainerResult on $JobContainerRemoteApiHost
                    }
                }
            } else {
                if ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent) {
                    $JobContainerResult = docker $JobContainerRemoteApiHost service create --replicas 1 --restart-condition none --limit-memory="1500m" --name $JobContainerName $ImageName -JobName `'$JobName`' -TestList `'$TestList`' -DotCoverPath `'$DotCoverPath`' -IsInContainer -StartServer -ServerPath `'C:\Build\Warewolf Server.exe`' -ResourcesType `'$ResourcesType`' 2>&1
                } else {
                    $JobContainerResult = docker $JobContainerRemoteApiHost service create --replicas 1 --restart-condition none --limit-memory="700m" --name $JobContainerName $ImageName -JobName `'$JobName`' -TestList `'$TestList`' -DotCoverPath `'$DotCoverPath`' -IsInContainer 2>&1
                }
                Write-Host Started $JobContainerName as $JobContainerResult on $JobContainerRemoteApiHost
            }
        } else {
            if ($TestAssembliesList -eq $null -or $TestAssembliesList -eq "") {
	            Write-Host Cannot find any $ProjectSpec project folders or assemblies at $TestsPath.
	            exit 1
            }

            # Setup for screen recording
            if ($RecordScreen.IsPresent) {
		        $TestSettingsId = [guid]::NewGuid()
                $NamingSchemeTag = "`n"

                # Create test settings.
                $TestSettingsFile = "$TestsResultsPath\$JobName.testsettings"
                Copy-On-Write $TestSettingsFile
                [system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings id="$TestSettingsId" name="$JobName" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
    <Description>Run $JobName With Screen Recording.</Description>
    <NamingScheme baseName="ScreenRecordings" appendTimeStamp="false" useDefault="false" />
    <Execution>
    <AgentRule name="LocalMachineDefaultRole">
        <DataCollectors>
        <DataCollector uri="datacollector://microsoft/VideoRecorder/1.0" assemblyQualifiedName="Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder.VideoRecorderDataCollector, Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" friendlyName="Screen and Voice Recorder">
            <Configuration>
            <MediaRecorder sendRecordedMediaForPassedTestCase="$sendRecordedMediaForPassedTestCase" xmlns="" />
            </Configuration>
        </DataCollector>
        </DataCollectors>
    </AgentRule>
    </Execution>
</TestSettings>
"@)
            }

            if (!$MSTest.IsPresent) {
                #Resolve test results file name
                Set-Location -Path "$TestsResultsPath\.."

                # Create full VSTest argument string.
                if ($TestList -eq "") {
                    if ($TestCategories -ne "") {
                        $TestCategories = " /TestCaseFilter:`"(TestCategory=" + $TestCategories  + ")`""
                    } else {
                        $DefinedCategories = AllCategoriesDefinedForProject $ProjectSpec
                        if ($DefinedCategories.Count -gt 0) {
                            $TestCategories = $DefinedCategories -join ")&(TestCategory!="
                            $TestCategories = " /TestCaseFilter:`"(TestCategory!=$TestCategories)`""
                        }
                    }
                } else {
                    $TestCategories = ""
                    if (!($TestList.StartsWith(" /Tests:"))) {
                        $TestList = " /Tests:" + $TestList
                    }
                }
                if($RecordScreen.IsPresent) {
                    $TestSettings =  " /Settings:`"" + $TestSettingsFile + "`""
                } else {
                    $TestSettings = ""
                }

                if ($Parallelize.IsPresent) {
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

                if($RecordScreen.IsPresent) {
                    $TestSettings =  " /Settings:`"" + $TestSettingsFile + "`""
                } else {
                    $TestSettings = ""
                }

                # Create full MSTest argument string.
                if ($TestList -eq "") {
                    if ($TestCategories -ne "") {
                        $TestCategories = " /category:`"$TestCategories`""
                    } else {
                        $DefinedCategories = AllCategoriesDefinedForProject $ProjectSpec
                        if ($DefinedCategories.Count -gt 0) {
                            $TestCategories = $DefinedCategories -join "&!"
                            $TestCategories = " /category:`"!$TestCategories`""
                        }
                    }
                } else {
                    $TestCategories = ""
                    if (!($TestList.StartsWith(" /test:"))) {
                        $TestNames = $TestList.Split(",") -join " /test:"
                        $TestList = " /test:" + $TestNames
                    }
                }
                $FullArgsList = $TestAssembliesList + " /resultsfile:`"" + $TestResultsFile + "`"" + $TestList + $TestSettings + $TestCategories

                # Write full command including full argument string.
                $TestRunnerPath = "$TestsResultsPath\..\Run $JobName.bat"
                Copy-On-Write "$TestRunnerPath"
                Out-File -LiteralPath "$TestRunnerPath" -Encoding default -InputObject `"$MSTestPath`"$FullArgsList
            }
            if (Test-Path "$TestsResultsPath\..\Run $JobName.bat") {
                if ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent -or $StartStudio.IsPresent -or ${Startmy.warewolf.io}.IsPresent) {
                    Start-my.warewolf.io
                    if ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent -or $StartStudio.IsPresent) {
                        Start-Server
                        if ($StartStudio.IsPresent) {
                            Start-Studio
                        }
                    }
                }
                if ($ApplyDotCover -and !$StartServerAsConsole.IsPresent -and !$StartServerAsService.IsPresent -and !$StartServer.IsPresent -and !$StartStudio.IsPresent) {
                    # Write DotCover Runner XML 
                    $DotCoverSnapshotFile = "$TestsResultsPath\$JobName DotCover Output.dcvr"
                    Copy-On-Write $DotCoverSnapshotFile
                    $DotCoverArgs = @"
<AnalyseParams>
	<TargetExecutable>$TestsResultsPath\..\Run $JobName.bat</TargetExecutable>
	<Output>$DotCoverSnapshotFile</Output>
	<Scope>
"@
                    foreach ($TestAssembliesDirectory in $TestAssembliesDirectories) {
                        $DotCoverArgs += @"

        <ScopeEntry>$TestAssembliesDirectory\*.dll</ScopeEntry>
        <ScopeEntry>$TestAssembliesDirectory\*.exe</ScopeEntry>
"@
                    }
                    $DotCoverArgs += @"

    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>
"@
                    $DotCoverRunnerXMLPath = "$TestsResultsPath\$JobName DotCover Runner.xml"
                    Copy-On-Write $DotCoverRunnerXMLPath
                    Out-File -LiteralPath $DotCoverRunnerXMLPath -Encoding default -InputObject $DotCoverArgs
                
                    # Create full DotCover argument string.
                    $DotCoverLogFile = "$TestsResultsPath\DotCover.xml.log"
                    Copy-On-Write $DotCoverLogFile
                    $FullArgsList = " cover `"$DotCoverRunnerXMLPath`" /LogFile=`"$DotCoverLogFile`""

                    #Write DotCover Runner Batch File
                    $DotCoverRunnerPath = "$TestsResultsPath\Run $JobName DotCover.bat"
                    Copy-On-Write $DotCoverRunnerPath
                    Out-File -LiteralPath "$DotCoverRunnerPath" -Encoding default -InputObject `"$DotCoverPath`"$FullArgsList
                
                    #Run DotCover Runner Batch File
                    &"$DotCoverRunnerPath"
                    if ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent -or $StartStudio.IsPresent -or ${Startmy.warewolf.io}.IsPresent) {
                        Cleanup-ServerStudio $false
                    }
                } else {
                    &"$TestRunnerPath"
                    if ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent -or $StartStudio.IsPresent -or ${Startmy.warewolf.io}.IsPresent) {
                        Cleanup-ServerStudio (!$ApplyDotCover)
                    }
                }
                Move-Artifacts-To-TestResults $ApplyDotCover ($StartServerAsConsole.IsPresent -or $StartServerAsService.IsPresent -or $StartServer.IsPresent -or $StartStudio.IsPresent) $StartStudio.IsPresent $JobName
            }
        }
        if ($ApplyDotCover -and $TotalNumberOfJobsToRun -gt 1 -and !$JobContainers.IsPresent) {
            Invoke-Expression -Command ("&'$PSCommandPath' -JobName '$JobName' -MergeDotCoverSnapshotsInDirectory '$TestsResultsPath' -DotCoverPath '$DotCoverPath'")
        }
    }
    if ($JobContainers.IsPresent) {
        if ($ApplyDotCover -and $TotalNumberOfJobsToRun -gt 1) {
            Invoke-Expression -Command ("&'$PSCommandPath' -JobName '$JobName' -MergeDotCoverSnapshotsInDirectory '$TestsResultsPath' -DotCoverPath '$DotCoverPath'")
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
            if ($ReadVersion.StartsWith("0.0.") -or ($LastReadVersion -ne $ReadVersion -and $LastReadVersion -ne "0.0.0.0")) {
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
    Write-Warning "Connecting to $WarewolfServerURL"
    $TestStartDateTime = Get-Date -Format o
    $ConnectTimeout = 180
    try {
        $ConnectToWarewolfServer = wget $WarewolfServerURL -TimeoutSec $ConnectTimeout -UseDefaultCredentials -UseBasicParsing
    } catch {
        throw $_.Exception
    }
    try {
        $TryGetWarewolfServerVersion = wget "$ServerPath/secure/getserverversion.json" -TimeoutSec $ConnectTimeout -UseDefaultCredentials -UseBasicParsing
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
            $TestTimeout = 180
            $TestStart = Get-Date
            $ServiceTestResults = ConvertFrom-Json (wget $WarewolfServiceTestURL -TimeoutSec $TestTimeout -UseDefaultCredentials -UseBasicParsing)
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

if ($MergeDotCoverSnapshotsInDirectory -ne "") {
    $DotCoverSnapshots = Get-ChildItem $MergeDotCoverSnapshotsInDirectory\*.dcvr -Recurse
    if ($JobNames -eq "") {
        $JobNames = "DotCover"
    }
    $MergedSnapshotFileName = $JobNames.Split(",")[0]
    $MergedSnapshotFileName = "Merged $MergedSnapshotFileName Snapshots"
    Merge-DotCover-Snapshots $DotCoverSnapshots "$MergeDotCoverSnapshotsInDirectory\$MergedSnapshotFileName" "$MergeDotCoverSnapshotsInDirectory\DotCover"
}

if ($Cleanup.IsPresent) {
    if ($JobContainers.IsPresent) {
        Cleanup-JobContainers
    } else {
        if ($ServerContainer.IsPresent) {
            Cleanup-ServerContainer
        } else {
            if ($ApplyDotCover) {
                Cleanup-ServerStudio $false
            } else {
                Cleanup-ServerStudio
            }
	        if (!$JobNames -or $JobNames.Contains(",")) {
		        if ($ProjectName) {
			        $JobNames = $ProjectName
		        } else {
			        $JobNames = "Manual Tests"
		        }
	        }
            Move-Artifacts-To-TestResults $ApplyDotCover (Test-Path "$env:ProgramData\Warewolf\Server Log\wareWolf-Server.log") (Test-Path "$env:LocalAppData\Warewolf\Studio Logs\Warewolf Studio.log") $JobNames
        }
    }
}

if ($ServerContainer.IsPresent) {
    Start-ServerContainer
}

if (!$Cleanup.IsPresent -and !$AssemblyFileVersionsTest.IsPresent -and $JobNames -eq "" -and !$RunWarewolfServiceTests.IsPresent -and $MergeDotCoverSnapshotsInDirectory -eq "" -and !$ServerContainer.IsPresent) {
    Start-my.warewolf.io
    if (!${Startmy.warewolf.io}.IsPresent) {
        $ResourcesType = Install-Server
        Start-Server
        if (!$StartServerAsConsole.IsPresent -and !$StartServerAsService.IsPresent -and !$StartServer.IsPresent -and !${Startmy.warewolf.io}.IsPresent) {
            Start-Studio
        }
    }
}