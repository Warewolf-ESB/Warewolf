Feature: GateLinearBackoff
	In order allow no backoff from a gate
	As a Warewolf user
	I want the tool to have a Gate selection to proceed

Scenario: Gate tool has no conditions
	Given I have the following conditions
		| match | matchtype | match |
	And GateFailure has "StopOnError" selected
	And Gates has "" selected
	And GateRetryStrategy has "LinearBackoff" selected
	And Resume is set to "No"
	And the Gate tool is executed
	Then the execution has errors
		| error                                         |
		| error: gate not executed, no conditions found |