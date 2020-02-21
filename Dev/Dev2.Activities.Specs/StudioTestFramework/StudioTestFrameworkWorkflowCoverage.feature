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

Scenario: Run all tests to generate total nodes covered in workflow
		Given two saved tests "Test 1" and "Test 2"
		And generate test coverage is selected
		When I run all the tests
		And the test coverage is 
		| name						 | coverage |
		| Test Decision false branch | 35%      |
		| Test Decision true branch  | 50%      |
		Then the total workflow test coverage is "85%"
		And the covered nodes are
		| node          |
		| assign(input) |
		| Decision		|
		| False branch	|
		| Assign(error) |
		| assign(input) |
		| Decision		|
		| True branch   |
		| SQL			|