if ([string]::IsNullOrEmpty($PSScriptRoot)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$SolutionDir = (Get-Item $PSScriptRoot).parent.parent.parent.parent.FullName

# Write Test Runner
& "$SolutionDir\TestScripts\Tests\VSTest\CI\Run-All-Other-Unit-Tests.ps1"

# Write DotCover Runner XML
Out-File -LiteralPath "$PSScriptRoot\DotCoverRunner.xml" -Encoding default -InputObject @"
<AnalyseParams>
	<TargetExecutable>$SolutionDir\TestScripts\Tests\VSTest\CI\RunTests.bat</TargetExecutable>
	<TargetArguments></TargetArguments>
	<Output>$PSScriptRoot\ActivitiesDesignersUnitTestsDotCoverOutput.dcvr</Output>
	<Scope>
		<ScopeEntry>$SolutionDir\Warewolf.Tests\**\*.dll</ScopeEntry>
		<ScopeEntry>$SolutionDir\Warewolf.Tests\**\*.exe</ScopeEntry>
	</Scope>
</AnalyseParams>
"@

#Write DotCover Runner Batch File
Out-File -LiteralPath $PSScriptRoot\RunDotCover.bat -Encoding default -InputObject "`"$env:LocalAppData\JetBrains\Installations\dotCover07\dotCover.exe`" cover `"$PSScriptRoot\DotCoverRunner.xml`" /LogFile=`"$PSScriptRoot\DotCoverRunner.xml.log`""