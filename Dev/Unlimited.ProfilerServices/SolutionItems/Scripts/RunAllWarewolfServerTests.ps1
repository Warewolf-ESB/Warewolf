####### Precheck Unit Check #########
$TestcasePrecheck = $env:TEST_CASE_PRECHECK_FLAG
if($TestcasePrecheck -eq 0){
	Write-Host "Test case step is disabled"
    # Creating HTML page of test case result
	Set-Content -Path .\UnitTestResult.html -Value "Test case step is disabled/skipped"
    # Creating HTML page of Coverag
	Set-Content -Path .\UnitTestCoverage.html -Value "Test case step is disabled/skipped"
    exit 0
} else {
	Write-Host "Waiting for container to start"
    Start-Sleep 100
}

########  Format-Json function start ########
function Format-Json {
    <#
    .SYNOPSIS
        Prettifies JSON output.
    .DESCRIPTION
        Reformats a JSON string so the output looks better than what ConvertTo-Json outputs.
    .PARAMETER Json
        Required: [string] The JSON text to prettify.
    .PARAMETER Minify
        Optional: Returns the json string compressed.
    .PARAMETER Indentation
        Optional: The number of spaces (1..1024) to use for indentation. Defaults to 4.
    .PARAMETER AsArray
        Optional: If set, the output will be in the form of a string array, otherwise a single string is output.
    .EXAMPLE
        $json | ConvertTo-Json  | Format-Json -Indentation 2
    #>
    [CmdletBinding(DefaultParameterSetName = 'Prettify')]
    Param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true)]
        [string]$Json,
        [Parameter(ParameterSetName = 'Minify')]
        [switch]$Minify,
        [Parameter(ParameterSetName = 'Prettify')]
        [ValidateRange(1, 1024)]
        [int]$Indentation = 4,
        [Parameter(ParameterSetName = 'Prettify')]
        [switch]$AsArray
    )
    if ($PSCmdlet.ParameterSetName -eq 'Minify') {
        return ($Json | ConvertFrom-Json) | ConvertTo-Json -Depth 100 -Compress
    }
    # If the input JSON text has been created with ConvertTo-Json -Compress
    # then we first need to reconvert it without compression
    if ($Json -notmatch '\r?\n') {
        $Json = ($Json | ConvertFrom-Json) | ConvertTo-Json -Depth 100
    }
    $indent = 0
    $regexUnlessQuoted = '(?=([^"]*"[^"]*")*[^"]*$)'
    $result = $Json -split '\r?\n' |
        ForEach-Object {
            # If the line contains a ] or } character, 
            # we need to decrement the indentation level unless it is inside quotes.
            if ($_ -match "[}\]]$regexUnlessQuoted") {
                $indent = [Math]::Max($indent - $Indentation, 0)
            }
            # Replace all colon-space combinations by ": " unless it is inside quotes.
            $line = (' ' * $indent) + ($_.TrimStart() -replace ":\s+$regexUnlessQuoted", ': ')
            # If the line contains a [ or { character, 
            # we need to increment the indentation level unless it is inside quotes.
            if ($_ -match "[\{\[]$regexUnlessQuoted") {
                $indent += $Indentation
            }
            $line
        }
    if ($AsArray) { return $result }
    return $result -Join [Environment]::NewLine
}
########  Format-Json function end ########

$ServerUsername = "WarewolfAdmin"
$ServerPassword = "W@rEw0lf@dm1n"
$ServerPath= "http://localhost:7994"
if (!$ServerPath) {
	$ServerPath = "http://localhost:7994"
    Write-Host $ServerPath
}

if (!$ServerUsername -or $ServerUsername -eq "Unknown User") {
	$ServerUsername = "Unknown User"
} else {
	$pair = "$($ServerUsername):$($ServerPassword)"
	$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
	$basicAuthValue = "Basic $encodedCreds"
	$Headers = @{
		Authorization = $basicAuthValue
	}
}

$WarewolfServerURL = "$ServerPath/secure/.tests"
Write-Host $WarewolfServerURL
Write-Warning "Connecting to $WarewolfServerURL"
try {
	    $ConnectToWarewolfServer = Invoke-WebRequest $WarewolfServerURL -TimeoutSec 180 -UseBasicParsing -Headers $Headers
		$textBody = $ConnectToWarewolfServer.RawContent
		#ConvertTo-Json @($textBody) -Depth 3 | Format-Json
		$htmlexport = "<pre style='word-wrap: break-word; white-space: pre-wrap;'>$($textBody)</pre>"
		# Creating HTML page of test case result
		Set-Content -Path .\UnitTestResult.html -Value $htmlexport
	} catch {
	# Creating HTML page of test case result
	Set-Content -Path .\UnitTestResult.html -Value $_
	throw $_.Exception
}

$WarewolfServerURL = "$ServerPath/secure/.coverage"
Write-Host $WarewolfServerURL
Write-Warning "Connecting to $WarewolfServerURL"

$Threshold = $env:TEST_CASE_THRESOLD

try {
	$request = Invoke-WebRequest -Uri $WarewolfServerURL -TimeoutSec 180 -UseBasicParsing -Headers $Headers
	# Create HTML file Object
	$HTML = New-Object -Com "HTMLFile"
	$htmlBody = $request.Content
	#$HTML.IHTMLDocument2_write($htmlBody)

	try {
		# This works in PowerShell with Office installed
		$HTML.IHTMLDocument2_write($htmlBody)
	}
	catch {
		# This works when Office is not installed    
		$src = [System.Text.Encoding]::Unicode.GetBytes($htmlBody)
		$HTML.write($src)
	}

	# Creating HTML page of test case result
	Set-Content -Path .\UnitTestCoverage.html -Value $htmlBody

} catch {
	# Creating HTML page of test case result
	Set-Content -Path .\UnitTestCoverage.html -Value $_
	throw $_.Exception
}

Write-Host Test case thresold value is $Threshold%

$WarewolfServerURL = "$ServerPath/secure/.coverage.json"
Write-Host $WarewolfServerURL
Write-Warning "Connecting to $WarewolfServerURL"
try {
	    $ConnectToWarewolfServer = Invoke-WebRequest $WarewolfServerURL -TimeoutSec 180 -UseBasicParsing -Headers $Headers
	} 
catch {
	    throw $_.Exception
}

$CoverageSummaryData = (ConvertFrom-Json $ConnectToWarewolfServer).CoverageSummary
$TestSummaryData = (ConvertFrom-Json $ConnectToWarewolfServer).TestSummary
$Total_Tests_TestsFailed = $TestSummaryData.TestsFailed
$Total_Tests_Coverage = $CoverageSummaryData.TotalCoverage
#$Passed_Tests_Count = $WarewolfServiceData.TestsPassed

# Total test case count
Write-Host "Failed test case count : $Total_Tests_TestsFailed"
Write-Host "Test coverage : $Total_Tests_Coverage"

if($Total_Tests_TestsFailed -ne 0){
    Write-Host "Few test cases are failed"
	throw 'Few test cases are failed'
	exit 1
}

# Passed test case count
#Write-Host $Passed_Tests_Count 

# Test case thresold calculation
#$Minimum_Rate = ($Total_Tests_Count * $Threshold)/100
#Write-Host minimum $Minimum_Rate test case need to pass.


if($Total_Tests_Coverage -ge $Threshold){
Write-Host "Test case thresold criteria is satisfied"
    exit 0

}else{
    throw 'Test case thresold criteria is not satisfied'
    exit 1
}