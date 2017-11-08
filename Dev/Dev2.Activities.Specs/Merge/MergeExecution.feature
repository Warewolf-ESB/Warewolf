Feature: MergeExecution
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Merge AssignOnlyWithNoOutput Workflow with Same Version
	 Given I Load workflow "AssignOnlyWithNoOutput" from "localhost"
	 And I Load workflow "AssignOnlyWithNoOutput" from "Remote Connection Integration"	 
	 When Merge Window is opened with "AssignOnlyWithNoOutput"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false
	 And Merge window has no Conflicting tools


Scenario: Merge VersionHelloWorld Workflow 
	 Given I Load workflow "Hello World" from "localhost"
	 And I Load workflow "VersionHelloWorld" from "Remote Connection Integration"	 
	 When Merge Window is opened with "VersionHelloWorld"
	 Then Current workflow contains "6" tools
	 And Different workflow contains "7" tools
	 And Merge conflicts count is "7"
	 And Merge variable conflicts is false
	 And Merge window has "0" Conflicting tools

Scenario: Merge WorkFlowWithOneScalar Same VariableList
	 Given I Load workflow "WorkFlowWithOneScalar" from "localhost"
	 And I Load workflow "WorkFlowWithOneScalar" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkFlowWithOneScalar"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false
	 And Merge window has "1" Conflicting tools

Scenario: Merge WorkFlowWithOneScalar different input mapping
	 Given I Load workflow "WorkFlowWithOneScalar" from "localhost"
	 And I Load workflow version "1" of "WorkFlowWithOneScalar" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkFlowWithOneScalar"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false
	 And Merge window has "1" Conflicting tools

Scenario: Merge WorkFlowWithOneRecordSet different input mapping
	 Given I Load workflow "WorkFlowWithOneRecordSet" from "localhost"
	 And I Load workflow version "1" of "WorkFlowWithOneRecordSet" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkFlowWithOneRecordSet"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false
	 And Merge window has "1" Conflicting tools

Scenario: Merge WorkFlowWithOneObject different input mapping
	 Given I Load workflow "WorkFlowWithOneObject" from "localhost"
	 And I Load workflow version "1" of "WorkFlowWithOneObject" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkFlowWithOneObject"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false
	 And Merge window has "1" Conflicting tools

Scenario: Merge Workflow with Assign tool As First Tool And Split tool as Second tool count
	 Given I Load workflow "WorkflowWithDifferentToolSequence" from "localhost"
	 And I Load workflow "WorkflowWithDifferentToolSequence" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkflowWithDifferentToolSequence"
	 Then Current workflow contains "2" tools
	 And Different workflow contains "2" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is "0" 

Scenario: Merge Workflow Containing SequenceTool With Different Children Count
	 Given I Load workflow "WorkflowWithSequenceToolWithDifferentChildren" from "localhost"
	 And I Load workflow "WorkflowWithSequenceToolWithDifferentChildren" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkflowWithSequenceToolWithDifferentChildren"
	 Then Current workflow contains "2" tools
	 And Different workflow contains "2" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is "0" 

Scenario: Merge Workflow Containing SequenceTool With Different Children Sequence
	 Given I Load workflow "WorkflowWithSequenceToolWithChildrenInDifferentOrder" from "localhost"
	 And I Load workflow "WorkflowWithSequenceToolWithChildrenInDifferentOrder" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkflowWithSequenceToolWithChildrenInDifferentOrder"
	 Then Current workflow contains "2" tools
	 And Different workflow contains "2" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is "0" 

Scenario: Merge Workflow Containing Same tools But disconnected Arms
	 Given I Load workflow "WorkflowWithAssignToolsWithDisconnectedArms" from "localhost"
	 And I Load workflow "WorkflowWithAssignToolsWithDisconnectedArms" from "Remote Connection Integration"	 
	 When Merge Window is opened with "WorkflowWithAssignToolsWithDisconnectedArms"
	 Then Current workflow contains "2" tools
	 And Different workflow contains "2" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is "0" 