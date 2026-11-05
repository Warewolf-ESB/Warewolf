$Workspace = $env:WORKSPACE 


#####################################################################################################

$find_ESLogging_Source = '#{Warewolf.ProfilerServices.ESLoggingSource.Host}'
$replace_ESLogging_Source = $env:ESLOGGING_SOURCE

$find_ESLogging_Index = '#{Warewolf.ProfilerServices.ESLoggingSource.Index}'
$replace_ESLogging_Index = $env:ESLOGGING_INDEX_PROFILER

$find_ESLogging_PortNo = '#{Warewolf.ProfilerServices.ESLoggingSource.PortNo}'
$replace_ESLogging_PortNo = $env:ESLOGGING_PORT

#####################################################################################################

$fileNamePath = '\Warewolf.ProfilerServices\Sources\auditingSettings.json'

$filePath = -join($Workspace,$fileNamePath)

Write-Host "$filePath"
$tempFilePath = "$env:TEMP\$($filePath | Split-Path -Leaf)"

(Get-Content -Path $filePath) -replace $find_ESLogging_Source, $replace_ESLogging_Source -replace $find_ESLogging_Index, $replace_ESLogging_Index -replace $find_ESLogging_PortNo, $replace_ESLogging_PortNo | Add-Content -Path $tempFilePath

Remove-Item -Path $filePath
Move-Item -Path $tempFilePath -Destination $filePath


Write-Host "Eastic search logging txt Changed"

#####################################################################################################

$fileNamePath = '\Warewolf.ProfilerServices\Sources\ProfilerLoggingESSource.bite'

$filePath = -join($Workspace,$fileNamePath)

Write-Host "$filePath"
$tempFilePath = "$env:TEMP\$($filePath | Split-Path -Leaf)"

(Get-Content -Path $filePath) -replace $find_ESLogging_Source, $replace_ESLogging_Source -replace $find_ESLogging_Index, $replace_ESLogging_Index -replace $find_ESLogging_PortNo, $replace_ESLogging_PortNo | Add-Content -Path $tempFilePath

Remove-Item -Path $filePath
Move-Item -Path $tempFilePath -Destination $filePath


Write-Host "Eastic search logging source Changed"