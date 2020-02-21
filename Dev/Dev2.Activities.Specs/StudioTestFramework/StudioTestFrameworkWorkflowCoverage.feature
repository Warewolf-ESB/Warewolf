Feature: StudioTestFrameworkWorkflowCoverage
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@StudioTestFrameworkWorkflowCoverage
Scenario: Run an individual test to show partial coverage of nodes
		Given a saved test is run
		And generate test coverage is selected
		And workflow only tests
		| node          |
		| assign(input) |
		| Decision		|
		| False			|
		| Assign(error) |
		When I run the test 
		And test coverage is generated 
		Then the covered nodes are 
		| node          |
		| assign(input) |
		| Decision		|
		| False			|
		| Assign(error) |
		And the test coverage is "35%"

