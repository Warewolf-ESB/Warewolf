@DotNetPluginErrorHandling
Feature: DotNetPluginErrorHandling
	In order to test error handling in DotNet Plugin activities
	As a Warewolf user
	I want to ensure that errors are properly handled by the "On Error" framework

Scenario: DotNet Plugin Activity Error Handling With Error Thrower
	Given I edit the ErrorThrowingPlugin.bite file to use the current test assembly location
	And I refresh the server resource catalogue
	And I delete the error log file if it exists
	When I execute the "ErrorThrower" workflow
	Then the error log file should exist at "c:\error.log"
	And the execution has "NO" error
