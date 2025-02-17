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

if (-Not [System.Diagnostics.PerformanceCounterCategory]::Exists("Warewolf")) {
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
	Write-Host Warewolf Performance Counters created.
	exit 0
} else {
	Write-Host Warewolf Performance Counters already exist.
	exit 1
}