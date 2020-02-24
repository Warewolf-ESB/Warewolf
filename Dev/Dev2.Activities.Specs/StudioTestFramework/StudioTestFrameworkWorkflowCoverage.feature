Feature: StudioTestFrameworkWorkflowCoverage
	In order to able to tell which nodes of the workflow has coverage
	As a warewolf user
	I want to be able to generate test coverage results

@StudioTestFrameworkWorkflowCoverage
Scenario: Run an individual test to show partial coverage of nodes
		Given a workflow with below nodes
		| node           |
		| Assign(input)  |
		| Decision       |
		| Assign(error)  |
		| SQL            |
		| Assign(person) |
		| SMTP Send      |
		And generate test coverage is selected
		When I run test "Test Decision false branch"
		And test coverage is generated 
		Then the covered nodes are 
		| node          |
		| Assign(input) |
		| Decision		|
		| False			|
		| Assign(error) |
		And the test coverage is "50%"

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

Scenario: Test coverage summary view folders should have coverage of all workflows it contains
		Given a test coverage summary view is opened
		When a folder containing test coverage reports is loaded
		Then information bar will have these values
		| total | passed | failed |
		| 1324	| 1300	 | 24	  |
		And the per folder coverage summary is
		| name		 | coverage							 |
		| Folder-one | 70 %								 |
		| Folder-two | warning: no coverage report found |

Scenario: Test coverage summary view workflows should have per workflow coverage
		Given a test coverage summary view is opened 
		And a folder containing test coverage reports is loaded
		And information bar will have these values
		| total | passed | failed |
		| 1324	| 1300	 | 24	  |
		And the per workflow coverage summary is
		| name   | coverage								| branch_coverage |
		| wf-one | 85%									| 30%             |
		| wf-two | warning: no coverage report found	| 15%             |
		When I select "wf-one" within test coverage summary view
		Then the workflow nodes will be as 
		| passed | node           |
		| true   | assign(input)  |
		| false  | Decision       |
		| true   | False branch   |
		| true   | Assign(error)  |
		| true   | assign(input)  |
		| true   | Decision       |
		| true   | True branch    |
		| true   | SQL            |
		| true   | assign(person) |
		| true	 | SMTP Send	  |
