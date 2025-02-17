$categoryName = "Warewolf"
$categoryHelp = "Warewolf Performance Counters"
$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::SingleInstance
$instanceName = "*"

function Convert-CounterType {
    param (
        [string]$counterType
    )
    
    switch ($counterType) {
        "AverageBase" { return [System.Diagnostics.PerformanceCounterType]::AverageBase }
        "AverageTimer32" { return [System.Diagnostics.PerformanceCounterType]::AverageTimer32 }
        "NumberOfItems32" { return [System.Diagnostics.PerformanceCounterType]::NumberOfItems32 }
        "RateOfCountsPerSecond32" { return [System.Diagnostics.PerformanceCounterType]::RateOfCountsPerSecond32 }
        default { throw "Unhandled PerformanceCounterType: $counterType" }
    }
}

if (-Not [System.Diagnostics.PerformanceCounterCategory]::Exists($categoryName)) {
	$counters = @(@{
        CreationData = @(
		@{ CounterName = "Concurrent requests currently executing"; CounterHelp = "Concurrent requests currently executing"; CounterType = "NumberOfItems32" },
		@{ CounterName = "Total Errors"; CounterHelp = "Total Errors"; CounterType = "NumberOfItems32" },
        @{ CounterName = "Request Per Second"; CounterHelp = "Request Per Second"; CounterType = "RateOfCountsPerSecond32" },
        @{ CounterName = "Average workflow execution time"; CounterHelp = "Average workflow execution time"; CounterType = "AverageTimer32" },
        @{ CounterName = "Average workflow execution time base"; CounterHelp = "Average workflow execution time base"; CounterType = "AverageBase" },
        @{ CounterName = "Count of Not Authorised errors"; CounterHelp = "Count of Not Authorised errors"; CounterType = "NumberOfItems32" },
        @{ CounterName = "Count of requests for workflows which don't exist"; CounterHelp = "Count of requests for workflows which don't exist"; CounterType = "NumberOfItems32" }
		)
	})
    $counterCreationDataCollection = New-Object System.Diagnostics.CounterCreationDataCollection
    
    foreach ($counter in $counters) {
        foreach ($counterData in $counter.CreationData) {
            $systemCounterData = New-Object System.Diagnostics.CounterCreationData(
                $counterData.CounterName,
                $counterData.CounterHelp,
                (Convert-CounterType $counterData.CounterType)
            )
            $counterCreationDataCollection.Add($systemCounterData)
        }
    }
    [System.Diagnostics.PerformanceCounterCategory]::Create("Warewolf", "Warewolf Performance Counters", [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance, $counterCreationDataCollection)
}

$ErrorActionPreference = "Stop"
try {
    Get-Counter -Counter "\Warewolf(*)\Concurrent requests currently executing"
    Write-Output "Concurrent requests currently executing counter already exists."
} catch {
    New-Object System.Diagnostics.PerformanceCounter($categoryName, "Concurrent requests currently executing", $instanceName, $false);Get-Counter -ListSet "Warewolf"
}
try {
    Get-Counter -Counter "\Warewolf(*)\Total Errors"
    Write-Output "Total Errors counter already exists."
} catch {
    New-Object System.Diagnostics.PerformanceCounter($categoryName, "Total Errors", $instanceName, $false);Get-Counter -ListSet "Warewolf"
}
try {
    Get-Counter -Counter "\Warewolf(*)\Request Per Second"
    Write-Output "Request Per Second counter already exists."
} catch {
    New-Object System.Diagnostics.PerformanceCounter($categoryName, "Request Per Second", $instanceName, $false);Get-Counter -ListSet "Warewolf"
}
try {
    Get-Counter -Counter "\Warewolf(*)\Average workflow execution time"
    Write-Output "Average workflow execution time counter already exists."
} catch {
    New-Object System.Diagnostics.PerformanceCounter($categoryName, "Average workflow execution time", $instanceName, $false);Get-Counter -ListSet "Warewolf"
}
try {
    Get-Counter -Counter "\Warewolf(*)\Count of Not Authorised errors"
    Write-Output "Count of Not Authorised errors counter already exists."
} catch {
    New-Object System.Diagnostics.PerformanceCounter($categoryName, "Count of Not Authorised errors", $instanceName, $false);Get-Counter -ListSet "Warewolf"
}
try {
    Get-Counter -Counter "\Warewolf(*)\Count of requests for workflows which don't exist"
    Write-Output "Count of requests for workflows which don't exist counter already exists."
} catch {
    New-Object System.Diagnostics.PerformanceCounter($categoryName, "Count of requests for workflows which don't exist", $instanceName, $false);Get-Counter -ListSet "Warewolf"
}
$ErrorActionPreference = "Continue"
Write-Output "$categoryHelp created and initialized for instance: $instanceName"
