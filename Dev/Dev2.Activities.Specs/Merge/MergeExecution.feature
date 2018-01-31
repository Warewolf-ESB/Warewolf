@WorkflowMerging
Feature: MergeExecution
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Merge AssignOnlyWithNoOutput Workflow with Same Version
	 Given I Load workflow "AssignOnlyWithNoOutput" from "localhost"
	 And I Load workflow "AssignOnlyWithNoOutput" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "AssignOnlyWithNoOutput"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false
	 And Merge window has no Conflicting tools


Scenario: Merge VersionHelloWorld Workflow 
	 Given I Load workflow "MergeHelloWorld" from "localhost"
	 And I Load workflow "VersionHelloWorld" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "VersionHelloWorld"
	 Then Current workflow contains "8" tools
	 And Different workflow contains "8" tools
	 And Merge conflicts count is "8"
	 And Merge variable conflicts is false
	 And Merge window has "2" Conflicting tools

Scenario: Merge WorkFlowWithOneScalar different input mapping
	 Given I Load workflow "WorkFlowWithOneScalar" from "localhost"
	 And I Load workflow version "1" of "WorkFlowWithOneScalar" from "localhost"
	 When Merge Window is opened with local "WorkFlowWithOneScalar"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is true
	 And Merge window has "1" Conflicting tools

Scenario: Merge WorkFlowWithOneRecordSet different input mapping
	 Given I Load workflow "WorkFlowWithOneRecordSet" from "localhost"
	 And I Load workflow version "1" of "WorkFlowWithOneRecordSet" from "localhost"
	 When Merge Window is opened with local "WorkFlowWithOneRecordSet"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is true
	 And Merge window has "1" Conflicting tools

Scenario: Merge WorkFlowWithOneObject different input mapping
	 Given I Load workflow "WorkFlowWithOneObject" from "localhost"
	 And I Load workflow version "1" of "WorkFlowWithOneObject" from "localhost"
	 When Merge Window is opened with local "WorkFlowWithOneObject"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is true
	 And Merge window has "1" Conflicting tools

Scenario: Merge Workflow with Assign tool As First Tool And Split tool as Second tool count
	 Given I Load workflow "WorkflowWithDifferentToolSequence" from "localhost"
	 And I Load workflow "WorkflowWithDifferentToolSequence" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "WorkflowWithDifferentToolSequence"
	 Then Current workflow contains "3" tools
	 And Different workflow contains "3" tools
	 And Merge conflicts count is "3"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing SequenceTool With Different Children Counts Equals One
	 Given I Load workflow "WorkflowWithSequenceToolWithDifferentChildren" from "localhost"
	 And I Load workflow version "1" of "WorkflowWithSequenceToolWithDifferentChildren" from "localhost"
	 When Merge Window is opened with local "WorkflowWithSequenceToolWithDifferentChildren"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing SequenceTool With Different Children Sequence
	 Given I Load workflow "WorkflowWithSequenceToolWithChildrenInDifferentOrder" from "localhost"
	 And I Load workflow version "1" of "WorkflowWithSequenceToolWithChildrenInDifferentOrder" from "localhost"
	 When Merge Window is opened with local "WorkflowWithSequenceToolWithChildrenInDifferentOrder"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing Same tools But disconnected Arms
	 Given I Load workflow "WorkflowWithAssignToolsWithDisconnectedArms" from "localhost"
	 And I Load workflow "WorkflowWithAssignToolsWithDisconnectedArms" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "WorkflowWithAssignToolsWithDisconnectedArms"
	 Then Current workflow contains "1" tools
	 And Different workflow contains "1" tools
	 And Merge conflicts count is "1"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing Removed tool with same Variable List
	 Given I Load workflow "MergeRemovedTool" from "localhost"
	 And I Load workflow version "1" of "MergeRemovedTool" from "localhost"	 
	 When Merge Window is opened with local "MergeRemovedTool"
	 Then Current workflow contains "5" tools
	 And Different workflow contains "5" tools
	 And Merge conflicts count is "5"
	 And Merge variable conflicts is false
	 And I select Current Tool
	 And I select Current Arm
	 And I select Current Arm
	 Then Save is enabled

Scenario: Merge Workflow Containing Switch tool
	 Given I Load workflow "MergeSwitchTool" from "localhost"
	 And I Load workflow version "1" of "MergeSwitchTool" from "localhost"	 
	 When Merge Window is opened with local "MergeSwitchTool"
	 Then Current workflow contains "5" tools
	 And Different workflow contains "5" tools
	 And Merge conflicts count is "5"
	 And Merge variable conflicts is false
	 And I select Current Tool
	 And I select Current Arm
	 And I select Current Tool
	 And I select Current Arm
	 Then Save is enabled

Scenario: Merge Validate All tools are mapped
	 Given I Load All tools and expect all tools to be mapped