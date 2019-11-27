Feature: GateConstantBackoff
	In order allow constant backoff from a gate
	As a Warewolf user
	I want the tool to have a Gate selection to proceed

Scenario: Gate tool has no conditions
	Given I have the following conditions
		| match | matchtype | match |
	And GateFailure has "StopOnError" selected
	And Gates has "" selected
	And GateRetryStrategy has "ConstantBackoff" selected
	And Increment is set to "50"
	And Resume is set to "No"
	And the Gate tool is executed
	Then the execution has errors
		| error                                         |
		| error: gate not executed, no conditions found |

Scenario: Gate tool has stop on error with no resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "StopOnError" selected
	And Gates has "" selected
	And GateRetryStrategy has "ConstantBackoff" selected
	And Increment is set to "50"
	And Resume is set to "No"
	And the Gate tool is executed
	Then the execution has errors
		| error                               |
		| error: stop on error with no resume |

Scenario: Gate tool has stop on error with resume
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | =         | 10    |
		| [[b]] | =         | 20    |
	And GateFailure has "StopOnError" selected
	And Gates has "" selected
	And GateRetryStrategy has "ConstantBackoff" selected
	And Increment is set to "50"
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
	And Gates has "Gate" selected
	And GateRetryStrategy has "ConstantBackoff" selected
	And Increment is set to "50"
	And Resume is set to "No"
	And the Gate tool is executed
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
	And GateRetryStrategy has "ConstantBackoff" selected
	And Increment is set to "50"
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
	And GateRetryStrategy has "ConstantBackoff" selected
	And Increment is set to "-1"
	And Resume is set to "Yes"
	And the Gate tool is executed
	Then the execution has errors
		| error                                         |
		| error: increment index out of range exception |
