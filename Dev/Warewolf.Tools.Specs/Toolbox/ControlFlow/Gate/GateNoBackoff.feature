Feature: GateNoBackoff
	In order allow no backoff from a gate
	As a Warewolf user
	I want the tool to have a Gate selection to proceed

Scenario: Gate tool has no conditions
	Given I have the following conditions
		| match | matchtype | match |
	And Gates has "" selected
	And GateRetryStrategy has "NoBackoff" selected
	And Resume is set to "No"
	And the Gate tool is executed
	Then the execution has no errors

Scenario: Gate tool has stop on error with no resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And Gates has "" selected
	And GateRetryStrategy has "NoBackoff" selected
	And Resume is set to "No"
	And the Gate tool is executed
	Then the execution has errors
		| error                        |
		| stop on error with no resume |

Scenario: Gate tool has stop on error with resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And Gates has "" selected
	And GateRetryStrategy has "NoBackoff" selected
	And Resume is set to "Yes"
	And the Gate tool is executed
	Then the execution has errors
		| error                        |
		| stop on error with no resume |

Scenario: Gate tool has retry with no resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And Gates has "Gate" selected
	And GateRetryStrategy has "NoBackoff" selected
	And Resume is set to "No"
	And the Gate tool is executed with next gate
	Then the execution has errors
		| error                        |
		| stop on error with no resume |

Scenario: Gate tool has retry with resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And Gates has "Gate" selected
	And GateRetryStrategy has "NoBackoff" selected
	And Resume is set to "Yes"
	And the Gate tool is executed with next gate
	Then the execution has errors
		| error                        |
		| stop on error with no resume |