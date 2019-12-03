Feature: GateResumption
	In order allow gate resumption via workflow url
	As a Warewolf user
	I want the tool to have a Gate selection to proceed

Scenario: Gate tool has retry with resume set to ResumeEndpoint
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "Retry" selected
	And Gates has "Gate" selected
	And GateRetryStrategy has "NoBackoff" selected
	And Resume is set to "Yes"
	And ResumeEndpoint is set to "acb75027-ddeb-47d7-814e-a54c37247ec1"
	And the Gate tool is executed with resumption endpoint
	Then the execution has errors
		| error                        |
		| stop on error with no resume |