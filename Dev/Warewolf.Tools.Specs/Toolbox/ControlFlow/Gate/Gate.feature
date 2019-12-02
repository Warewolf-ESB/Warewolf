Feature: Gate
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Gate tool resume with no gate selected
	Given I have the following conditions
		| match | matchtype | match |
		| [[a]] | IsEqual   | 10    |
		| [[b]] | IsEqual   | 20    |
	And GateFailure has "Retry" selected
	And Gates has "" selected
	And the Gate tool is executed
	Then the execution has errors
		| error                                  |
		| invalid retry config: no gate selected |