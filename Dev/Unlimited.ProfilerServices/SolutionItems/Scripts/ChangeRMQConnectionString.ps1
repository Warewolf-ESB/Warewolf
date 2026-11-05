$Workspace = $env:WORKSPACE 



#####################################################################################################

$find_Address_Source = '#{Warewolf.ProfilerServices.RabbitMQSource.Host}'
$replace_Address_Source = $env:RMQ_CONNECTIONSTRING

$find_UserName_Source = '#{Warewolf.ProfilerServices.RabbitMQSource.UserName}'
$replace_UserName_Source = $env:RMQ_USERNAME

$find_Password_Source = '#{Warewolf.ProfilerServices.RabbitMQSource.Password}'
$replace_Password_Source = $env:RMQ_PASSWORD

$find_virutalhost_Source = '#{Warewolf.ProfilerServices.RabbitMQSource.VirtualHost}'
$replace_virtualhost_Source = $env:RMQ_VIRTUALHOST


#####################################################################################################

$RMQSource = '\Warewolf.ProfilerServices\Sources\ProfilerRabbitMQSource.bite'
$RMQSourcePath = -join($Workspace,$RMQSource)
Write-Host "$RMQSourcePath"

$RMQSourcetempFilePath = "$env:TEMP\$($RMQSourcePath | Split-Path -Leaf)"


(Get-Content -Path $RMQSourcePath) -replace $find_Address_Source, $replace_Address_Source -replace $find_UserName_Source, $replace_UserName_Source -replace $find_Password_Source, $replace_Password_Source -replace $find_virutalhost_Source, $replace_virtualhost_Source | Add-Content -Path $RMQSourcetempFilePath

Remove-Item -Path $RMQSourcePath
Move-Item -Path $RMQSourcetempFilePath -Destination $RMQSourcePath

Write-Host "RMQ Connection String change for RabbitMQSource"

#####################################################################################################

$find_AddressAPI_Source = '#{Warewolf.ProfilerServices.RabbitMQAPISource.Host}'
$replace_AddressAPI_Source = 'http://' + $env:RMQ_CONNECTIONSTRING + ':15672/api/exchanges/' + $env:RMQ_VIRTUALHOST + '/'

$RMQAPISource = '\Warewolf.ProfilerServices\Sources\ProfilerRabbitMQAPISource.bite'
$RMQAPISourcePath = -join($Workspace,$RMQAPISource)
Write-Host "$RMQAPISourcePath"

$RMQSourcetempFilePath = "$env:TEMP\$($RMQAPISourcePath | Split-Path -Leaf)"


(Get-Content -Path $RMQAPISourcePath) -replace $find_AddressAPI_Source, $replace_AddressAPI_Source | Add-Content -Path $RMQSourcetempFilePath

Remove-Item -Path $RMQAPISourcePath
Move-Item -Path $RMQSourcetempFilePath -Destination $RMQAPISourcePath

Write-Host "RMQ API Connection String changed"

