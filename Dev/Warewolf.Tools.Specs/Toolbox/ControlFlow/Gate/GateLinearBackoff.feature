Feature: GateLinearBackoff
	In order allow Linear backoff from a gate
	As a Warewolf user
	I want the tool to have a Gate selection to proceed

Scenario: Gate tool has no conditions
	Given I have the following conditions
		| match | matchtype | match |
	And GateFailure has "StopOnError" selected
	And Gates has "" selected
	And GateRetryStrategy has "LinearBackoff" selected
	And Linear Increment is set to "50"
	And Linear Timeout is set to "60000"
	And Linear Max Retries is set to "2"
	And Resume is set to "No"
	And the Gate tool is executed
	Then the execution has no errors

Scenario: Gate tool has stop on error with no resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "StopOnError" selected
	And Gates has "" selected
	And GateRetryStrategy has "LinearBackoff" selected
	And Linear Increment is set to "50"
	And Linear Timeout is set to "60000"
	And Linear Max Retries is set to "2"
	And Resume is set to "No"
	And the Gate tool is executed
	Then the execution has errors
		| error                               |
		| stop on error with no resume |

Scenario: Gate tool has stop on error with resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "StopOnError" selected
	And Gates has "" selected
	And GateRetryStrategy has "LinearBackoff" selected
	And Linear Increment is set to "50"
	And Linear Timeout is set to "60000"
	And Linear Max Retries is set to "2"
	And Resume is set to "Yes"
	And the Gate tool is executed
	Then the execution has errors
		| error                              |
		| expected stop on error with resume |

Scenario: Gate tool has retry with no resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "Retry" selected
	And next gate GateFailure has "Retry" selected
	And Gates has "Gate" selected
	And next gate Gates has "Gate" selected
	And GateRetryStrategy has "LinearBackoff" selected
	And next gate GateRetryStrategy has "LinearBackoff" selected
	And Linear Increment is set to "50"
	And next gate Linear Increment is set to "50"
	And Linear Timeout is set to "60000"
	And next gate Linear Timeout is set to "60000"
	And Linear Max Retries is set to "2"
	And next gate Linear Max Retries is set to "2"
	And Resume is set to "No"
	And next gate Resume is set to "No"
	And the Gate tool is executed with next gate
	Then the execution has errors
		| error                       |
		| error: retry with no resume |

Scenario: Gate tool has retry with resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "Retry" selected
	And Gates has "Gate" selected
	And GateRetryStrategy has "LinearBackoff" selected
	And Linear Increment is set to "50"
	And Linear Timeout is set to "60000"
	And Linear Max Retries is set to "2"
	And Resume is set to "Yes"
	And the Gate tool is executed
	Then the execution has errors
		| error                    |
		| error: retry with resume |

Scenario: Gate tool has retry with increment not allowed
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "Retry" selected
	And Gates has "Gate" selected
	And GateRetryStrategy has "LinearBackoff" selected
	And Linear Increment is set to "-1"
	And Linear Timeout is set to "60000"
	And Linear Max Retries is set to "2"
	And Resume is set to "Yes"
	And the Gate tool is executed
	Then the execution has errors
		| error                                         |
		| error: increment index out of range exception |