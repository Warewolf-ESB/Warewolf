# Define constants and counter definitions
$category     = "Warewolf"
$instanceName = "All"  # Change this instance name if desired

# List of counters to be defined in the Warewolf category.
$countersDefinition = @(
    @{ Name = "Concurrent requests currently executing"; Help = "Concurrent requests currently executing"; Type = "NumberOfItems32" },
    @{ Name = "Total Errors"; Help = "Total Errors"; Type = "NumberOfItems32" },
    @{ Name = "Request Per Second"; Help = "Request Per Second"; Type = "RateOfCountsPerSecond32" },
    @{ Name = "Average workflow execution time"; Help = "Average workflow execution time"; Type = "AverageTimer32" },
    @{ Name = "Average workflow execution time base"; Help = "Average workflow execution time base"; Type = "AverageBase" },
    @{ Name = "Count of Not Authorised errors"; Help = "Count of Not Authorised errors"; Type = "NumberOfItems32" },
    @{ Name = "Count of requests for workflows which don't exist"; Help = "Count of requests for workflows which don't exist"; Type = "NumberOfItems32" }
)

# Helper function to convert our string representation to the actual PerformanceCounterType
function Convert-CounterType {
    param (
        [string]$counterType
    )
    switch ($counterType) {
        "AverageBase"               { return [System.Diagnostics.PerformanceCounterType]::AverageBase }
        "AverageTimer32"            { return [System.Diagnostics.PerformanceCounterType]::AverageTimer32 }
        "NumberOfItems32"           { return [System.Diagnostics.PerformanceCounterType]::NumberOfItems32 }
        "RateOfCountsPerSecond32"   { return [System.Diagnostics.PerformanceCounterType]::RateOfCountsPerSecond32 }
        default                     { throw "Unhandled PerformanceCounterType: $counterType" }
    }
}

# Function to check for, and create if needed, the performance counter category.
function Ensure-CounterCategory {
    param (
        [string]$category,
        $counterDefs
    )
    if (-not [System.Diagnostics.PerformanceCounterCategory]::Exists($category)) {
        Write-Host "Creating performance counter category '$category'..."
        $counterCreationDataCollection = New-Object System.Diagnostics.CounterCreationDataCollection
        
        foreach ($counter in $counterDefs) {
            $data = New-Object System.Diagnostics.CounterCreationData(
                $counter.Name,
                $counter.Help,
                (Convert-CounterType $counter.Type)
            )
            $counterCreationDataCollection.Add($data)
        }
        
        [System.Diagnostics.PerformanceCounterCategory]::Create(
            $category,
            "Warewolf Performance Counters",
            [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance,
            $counterCreationDataCollection
        )
        Write-Host "Category '$category' created successfully."
    }
    else {
        Write-Host "Category '$category' already exists."
    }
}

# Function to initialize an instance for each counter so that they remain active
function Initialize-CounterInstances {
    param (
        [string]$category,
        [string]$instanceName,
        $counterDefs
    )
    $counterInstances = @{}
    foreach ($counter in $counterDefs) {
        try {
            $pc = New-Object System.Diagnostics.PerformanceCounter($category, $counter.Name, $instanceName, $false)
            $pc.RawValue = 0
            $counterInstances[$counter.Name] = $pc
            Write-Host "Initialized counter '$($counter.Name)' for instance '$instanceName' with value 0."
        }
        catch {
            Write-Error "Failed to initialize counter '$($counter.Name)': $_"
        }
    }
    return $counterInstances
}

# Main execution flow:
Ensure-CounterCategory -category $category -counterDefs $countersDefinition

# Create (or open) the counter instances. They will be available system-wide.
$global:performanceCounters = Initialize-CounterInstances -category $category -instanceName $instanceName -counterDefs $countersDefinition

Write-Host "All counters have been initialized. They are now available with Get-Counter -Counter ""\${category}(*)\*""."

# Keep the instance(s) alive. This loop helps if you want the process to continue updating or to simply maintain an open handle.
while ($true) {
    Start-Sleep -Seconds 30
    # Optionally, update counters here if needed.
}