
$Workspace = $env:WORKSPACE 
$fileNamePath = '\Warewolf.ProfilerServices\Sources\ProfilerDBSource.bite'

$filePath = -join($Workspace,$fileNamePath)

Write-Host "$filePath"
$tempFilePath = "$env:TEMP\$($filePath | Split-Path -Leaf)"

$find_db_pid = '#{Warewolf.ProfilerServices.ProfilerDBSource.DB.Password}'
$replace_db_pid = $env:DBPASSWORD

$find_db_uid = '#{Warewolf.ProfilerServices.ProfilerDBSource.DB.Username}'
$replace_db_uid = $env:DBUSERNAME

$find_db_server = '#{Warewolf.ProfilerServices.ProfilerDBSource.DB.Server}'
$replace_db_server = $env:DBSERVER

$find_db_name = '#{Warewolf.ProfilerServices.ProfilerDBSource.DB.DatabaseName}'
$replace_db_name = 'ProfilerDB'

#####################################################################################################

(Get-Content -Path $filePath) -replace $find_db_pid, $replace_db_pid -replace $find_db_uid, $replace_db_uid -replace $find_db_server, $replace_db_server -replace $find_db_name, $replace_db_name| Add-Content -Path $tempFilePath

Remove-Item -Path $filePath
Move-Item -Path $tempFilePath -Destination $filePath


Write-Host "ConnectionString Changed"


#####################################################################################################