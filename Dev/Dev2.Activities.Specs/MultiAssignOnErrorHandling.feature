@MultiAssignOnErrorHandling
Feature: MultiAssignOnErrorHandling
	In order to test error handling in MultiAssign activities
	As a Warewolf user
	I want to ensure that errors are properly handled by the "On Error" framework

Scenario: MultiAssign Activity Error Handling With Invalid Variable Assignment
	Given I have a workflow "MultiAssignErrorWorkflow"
	And "MultiAssignErrorWorkflow" contains an Assign "ErrorAssign" as
		| variable      | value |
		| [[var1]]      | valid |
		| [[..invalid]] | test  |
	And "ErrorAssign" has OnErrorVariable "[[Error]]"
	And "ErrorAssign" has OnErrorWorkflow ""
	And "ErrorAssign" has IsEndedOnError "False"
	When I execute the workflow "MultiAssignErrorWorkflow"
	Then the execution has "NO" error
	And "[[Error]]" equals "The following variable [[..invalid]] is not evaluated : parse error: {..invalid }."

Scenario: MultiAssign Activity Error Handling With OnError Workflow
	Given I have a workflow "MultiAssignErrorWorkflow"
	And "MultiAssignErrorWorkflow" contains an Assign "ErrorAssign" as
		| variable      | value |
		| [[var1]]      | valid |
		| [[..invalid]] | test  |
	And "ErrorAssign" has OnErrorVariable "[[Error]]"
	And "ErrorAssign" has OnErrorWorkflow "ErrorHandler"
	And "ErrorAssign" has IsEndedOnError "False"
	When I execute the workflow "MultiAssignErrorWorkflow"
	Then the execution has "NO" error

Scenario: MultiAssign Activity Error Handling With IsEndedOnError True
	Given I have a workflow "MultiAssignErrorWorkflow"
	And "MultiAssignErrorWorkflow" contains an Assign "ErrorAssign" as
		| variable      | value |
		| [[var1]]      | valid |
		| [[..invalid]] | test  |
	And "ErrorAssign" has OnErrorVariable "[[Error]]"
	And "ErrorAssign" has OnErrorWorkflow ""
	And "ErrorAssign" has IsEndedOnError "True"
	When I execute the workflow "MultiAssignErrorWorkflow"
	Then the execution has "AN" error
	And "[[Error]]" equals "The following variable [[..invalid]] is not evaluated : parse error: {..invalid }."

Scenario: MultiAssign Activity Success Case No Errors
	Given I have a workflow "MultiAssignSuccessWorkflow"
	And "MultiAssignSuccessWorkflow" contains an Assign "SuccessAssign" as
		| variable | value  |
		| [[var1]] | value1 |
		| [[var2]] | value2 |
	And "SuccessAssign" has OnErrorVariable "[[Error]]"
	And "SuccessAssign" has OnErrorWorkflow ""
	And "SuccessAssign" has IsEndedOnError "False"
	When I execute the workflow "MultiAssignSuccessWorkflow"
	Then the execution has "NO" error
	And "[[var1]]" equals "value1"
	And "[[var2]]" equals "value2"
	And "[[Error]]" equals ""