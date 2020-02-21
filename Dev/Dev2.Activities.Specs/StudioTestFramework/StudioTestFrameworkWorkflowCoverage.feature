Feature: StudioTestFrameworkWorkflowCoverage
	In order to able to tell which nodes of the workflow has coverage
	As a warewolf user
	I want to be able to generate test coverage results

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
		Given saved test(s) below is run
		| name						 |
		| Test Decision false branch |
		| Test Decision true branch  |
		And generate test coverage is selected
		When I run all the tests
		And the test coverage is 
		| name						 | coverage |
		| Test Decision false branch | 35%      |
		| Test Decision true branch  | 50%      |
		Then the total workflow test coverage is "85%"
		And the nodes covered are
		| node          |
		| assign(input) |
		| Decision		|
		| False branch	|
		| Assign(error) |
		| assign(input) |
		| Decision		|
		| True branch   |
		| SQL			|

Scenario: Run all tests should show which nodes have no coverage reports
		Given saved test(s) below is run
		| name						 |
		| Test Decision false branch |
		| Test Decision true branch  | 
		And I run all the tests with generate coverage selected
		Then the nodes not covered are
		| node				|
		| assign(person)	|
		| SMTP Send			|


