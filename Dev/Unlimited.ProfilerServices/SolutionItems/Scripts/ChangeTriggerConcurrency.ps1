$Workspace = $env:WORKSPACE 


#####################################################################################################


$find_MandateCollectionSuccess_Concurrency = '#{WarewolfProfilerMandateCollectionSuccessTriggerConcurrency}'
$replace_MandateCollectionSuccess_Concurrency =  $env:PROFILER_MANDATECOLLECTIONSUCCESS_TRIGGER_CONCURRENCY

$find_MandateCollectionFailure_Concurrency = '#{WarewolfProfilerMandateCollectionFailureTriggerConcurrency}'
$replace_MandateCollectionFailure_Concurrency =  $env:PROFILER_MANDATECOLLECTIONFAILURE_TRIGGER_CONCURRENCY

#####################################################################################################

#####################################################################################################
# MandateCollectionSuccess Start
$fileNamePath = '\Warewolf.ProfilerServices\Triggers\MandateCollectionSuccessTrigger.txt'


$filePath = -join($Workspace,$fileNamePath)

Write-Host "$filePath"
$tempFilePath = "$env:TEMP\$($filePath | Split-Path -Leaf)"

(Get-Content -Path $filePath) -replace $find_MandateCollectionSuccess_Concurrency, $replace_MandateCollectionSuccess_Concurrency | Add-Content -Path $tempFilePath

Remove-Item -Path $filePath
Move-Item -Path $tempFilePath -Destination $filePath


Write-Host "MandateCollectionSuccess trigger concurrency Changed"

# MandateCollectionSuccess End
#####################################################################################################

#####################################################################################################
# MandateCollectionFailure Start
$fileNamePath = '\Warewolf.ProfilerServices\Triggers\MandateCollectionFailureTrigger.txt'


$filePath = -join($Workspace,$fileNamePath)

Write-Host "$filePath"
$tempFilePath = "$env:TEMP\$($filePath | Split-Path -Leaf)"

(Get-Content -Path $filePath) -replace $find_MandateCollectionFailure_Concurrency, $replace_MandateCollectionFailure_Concurrency | Add-Content -Path $tempFilePath

Remove-Item -Path $filePath
Move-Item -Path $tempFilePath -Destination $filePath


Write-Host "MandateCollectionFailure trigger concurrency Changed"

# MandateCollectionFailure End
#####################################################################################################
