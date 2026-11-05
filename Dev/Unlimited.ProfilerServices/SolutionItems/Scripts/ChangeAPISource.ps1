$Workspace = $env:WORKSPACE 

########################################### CollectionEngineAPISource #####################################################

$find_APIAddress_Source = '#{Warewolf.CollectionServiceSource.Host}'
$replace_APIAddress_Source = $env:API_BASEURL + 'collection/public/'

$APISource = '\Warewolf.ProfilerServices\Sources\ProfilerService_CollectionApiSource.bite'
$APISourcePath = -join($Workspace,$APISource)
Write-Host "$APISourcePath"

$APISourcetempFilePath = "$env:TEMP\$($APISourcePath | Split-Path -Leaf)"


(Get-Content -Path $APISourcePath) -replace $find_APIAddress_Source, $replace_APIAddress_Source | Add-Content -Path $APISourcetempFilePath

Remove-Item -Path $APISourcePath
Move-Item -Path $APISourcetempFilePath -Destination $APISourcePath

Write-Host "Collection Engine API source changed"

########################################### CompuscanAPISource #####################################################

$find_APIAddress_Source = '#{Warewolf.ProfilerServices.CompuscanAPISource.Host}'
$replace_APIAddress_Source = $env:API_BASEURL + 'helper/public/'

$APISource = '\Warewolf.ProfilerServices\Sources\CampuscanAPISource.bite'
$APISourcePath = -join($Workspace,$APISource)
Write-Host "$APISourcePath"

$APISourcetempFilePath = "$env:TEMP\$($APISourcePath | Split-Path -Leaf)"


(Get-Content -Path $APISourcePath) -replace $find_APIAddress_Source, $replace_APIAddress_Source | Add-Content -Path $APISourcetempFilePath

Remove-Item -Path $APISourcePath
Move-Item -Path $APISourcetempFilePath -Destination $APISourcePath

Write-Host "Compuscan API source changed"

########################################### ALGNAPISource #####################################################

$find_APIAddress_Source = '#{Warewolf.ProfilerServices.ALGNAPISource.Host}'
$replace_APIAddress_Source = $env:API_BASEURL + 'collection/public/'

$APISource = '\Warewolf.ProfilerServices\Sources\ALGNAPISource.bite'
$APISourcePath = -join($Workspace,$APISource)
Write-Host "$APISourcePath"

$APISourcetempFilePath = "$env:TEMP\$($APISourcePath | Split-Path -Leaf)"


(Get-Content -Path $APISourcePath) -replace $find_APIAddress_Source, $replace_APIAddress_Source | Add-Content -Path $APISourcetempFilePath

Remove-Item -Path $APISourcePath
Move-Item -Path $APISourcetempFilePath -Destination $APISourcePath

Write-Host "ALGN API source changed"

########################################### Global Variables #####################################################

$find_CollectionAPIKEY_Source = '#{Warewolf.ProfilerServices.GlobalVariables.CollectionAPIKEY}'
$replace_CollectionAPIKEY_Source = 'eyJuYW1lIjoiY29sbGVjdGlvbiJ9'

$find_HelperAPIKEY_Source = '#{Warewolf.ProfilerServices.GlobalVariables.HelperAPIKEY}'
$replace_HelperAPIKEY_Source = 'ASLSIENWSXDIENTUUC5582A54S82D82AS285D'

$find_rabbitmq_authtoken = '#{Warewolf.ProfilerServices.GlobalVariables.RabbitMQAuthToken}'
$replace_rabbitmq_authtoken = $env:RMQ_AUTHORIZATIONTOKEN

$APISource = '\Warewolf.ProfilerServices\Sources\GlobalVariables.bite'
$APISourcePath = -join($Workspace,$APISource)
Write-Host "$APISourcePath"

$APISourcetempFilePath = "$env:TEMP\$($APISourcePath | Split-Path -Leaf)"


(Get-Content -Path $APISourcePath) -replace $find_CollectionAPIKEY_Source, $replace_CollectionAPIKEY_Source -replace $find_HelperAPIKEY_Source, $replace_HelperAPIKEY_Source  -replace $find_rabbitmq_authtoken,$replace_rabbitmq_authtoken | Add-Content -Path $APISourcetempFilePath

Remove-Item -Path $APISourcePath
Move-Item -Path $APISourcetempFilePath -Destination $APISourcePath

Write-Host "ALGN API source changed"