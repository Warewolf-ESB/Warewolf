Param(
	[string]$FromPath, 
	[string]$ToPath, 
	[string]$Suffixes, 
	[string]$RenameToAppConfig, 
	[string]$PathToAppend = "")

$Location = $PSScriptRoot;
$Root = $(Resolve-Path "$PSScriptRoot\..");
$Path = [System.Io.Path]::Combine($Location, $FromPath);
$From = New-Object -TypeName System.Io.DirectoryInfo -ArgumentList $Path
$Path = [System.Io.Path]::Combine($Location, $ToPath);
$To = New-Object -TypeName System.Io.DirectoryInfo -ArgumentList $Path

$Files = Get-ChildItem -File -Path $From -Recurse
foreach ($FromFile in $Files)
{
    $ToDirectory = [System.Io.Path]::Combine($FromFile.DirectoryName.Replace($From.FullName, $To.FullName), $PathToAppend)
    $ToFileName = [System.Io.Path]::Combine($ToDirectory, $FromFile.Name)
    if ($RenameToAppConfig -eq "True" -and ($FromFile.Name.EndsWith(".exe.config") -or $FromFile.Name.EndsWith(".dll.config")))
    {
        $ToFileName = $ToFileName.Replace($FromFile.Name, "App.config")
    }    
    if (Test-Path -Path $ToDirectory) 
    {   
        Write-Host $ToFileName
		if ("$($FromFile.FullName)" -ne "$ToFileName") {
        	Copy-Item $FromFile.FullName $ToFileName;
		}
        if ([System.Io.File]::Exists("$Root/ConfigProperties.user.txt"))
        {
            & "$Root/Tools/ConfigFileUpdater.exe" "sourceFile=$ToFileName" "destinationFile=$ToFileName" "propertiesFile=$Root/ConfigProperties.user.txt" "suffixes=User,$Suffixes"
        }
        & "$Root/Tools/ConfigFileUpdater.exe" "sourceFile=$ToFileName" "destinationFile=$ToFileName" "propertiesFile=$Root/ConfigProperties.txt" "suffixes=$Suffixes"
        if ($LASTEXITCODE -ne 0)
        {
            return
        }
    }
}