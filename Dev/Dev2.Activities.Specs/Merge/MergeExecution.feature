@WorkflowMerging
Feature: MergeExecution
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Merge AssignOnlyWithNoOutput Workflow with Same Version
	 Given I Load workflow "AssignOnlyWithNoOutput" from "localhost"
	 And I Load workflow "AssignOnlyWithNoOutput" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "AssignOnlyWithNoOutput"
	 Then Current workflow contains "2" tools
	 Then Current workflow contains "1" connectors
	 And Different workflow contains "2" tools
	 And Different workflow contains "1" connectors
	 And Merge conflicts count is "3"
	 And Merge variable conflicts is false
	 And Merge window has no Conflicting tools


Scenario: Merge VersionHelloWorld Workflow 
	 Given I Load workflow "MergeHelloWorld" from "localhost"
	 And I Load workflow "VersionHelloWorld" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "VersionHelloWorld"
	 Then Current workflow contains "6" tools
	 Then Current workflow contains "5" connectors
	 And Different workflow contains "6" tools
	 And Different workflow contains "5" connectors
	 And Merge conflicts count is "11"
	 And Merge variable conflicts is false
	 And Merge window has "1" Conflicting tools

Scenario: Merge WorkFlowWithOneScalar different input mapping
	 Given I Load workflow "WorkFlowWithOneScalar" from "localhost"
	 And I Load workflow version of WorkFlowWithOneScalar
	 When Merge Window is opened with local "WorkFlowWithOneScalar"
	 Then Current workflow contains "2" tools
	 Then Current workflow contains "1" connectors
	 And Different workflow contains "2" tools
	 And Different workflow contains "1" connectors
	 And Merge conflicts count is "3"
	 And Merge variable conflicts is false
	 And Merge window has "2" Conflicting tools

Scenario: Merge WorkFlowWithOneRecordSet different input mapping
	 Given I Load workflow "WorkFlowWithOneRecordSet" from "localhost"
	 And I Load workflow version of WorkFlowWithOneRecordSet
	 When Merge Window is opened with local "WorkFlowWithOneRecordSet"
	 Then Current workflow contains "2" tools
	 Then Current workflow contains "1" connectors
	 And Different workflow contains "2" tools
	 And Different workflow contains "1" connectors
	 And Merge conflicts count is "3"
	 And Merge variable conflicts is false
	 And Merge window has "2" Conflicting tools

Scenario: Merge WorkFlowWithOneObject different input mapping
	 Given I Load workflow "WorkFlowWithOneObject" from "localhost"
	 And I Load workflow version of WorkFlowWithOneObject
	 When Merge Window is opened with local "WorkFlowWithOneObject"
	 Then Current workflow contains "2" tools
	 Then Current workflow contains "1" connectors
	 And Different workflow contains "2" tools
	 And Different workflow contains "1" connectors
	 And Merge conflicts count is "3"
	 And Merge variable conflicts is false
	 And Merge window has "2" Conflicting tools

Scenario: Merge Workflow with Assign tool As First Tool And Split tool as Second tool count
	 Given I Load workflow "WorkflowWithDifferentToolSequence" from "localhost"
	 And I Load workflow "WorkflowWithDifferentToolSequence" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "WorkflowWithDifferentToolSequence"
	 Then Current workflow contains "3" tools
	 Then Current workflow contains "2" connectors
	 And Different workflow contains "3" tools
	 And Different workflow contains "2" connectors
	 And Merge conflicts count is "5"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing SequenceTool With Different Children Counts Equals One
	 Given I Load workflow "WorkflowWithSequenceToolWithDifferentChildren" from "localhost"
	 And I Load workflow version "1" of "WorkflowWithSequenceToolWithDifferentChildren" from "localhost"
	 When Merge Window is opened with local "WorkflowWithSequenceToolWithDifferentChildren"
	 Then Current workflow contains "2" tools
	 Then Current workflow contains "1" connectors
	 And Different workflow contains "2" tools
	 And Different workflow contains "1" connectors
	 And Merge conflicts count is "3"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing SequenceTool With Different Children Sequence
	 Given I Load workflow "WorkflowWithSequenceToolWithChildrenInDifferentOrder" from "localhost"
	 And I Load workflow version "1" of "WorkflowWithSequenceToolWithChildrenInDifferentOrder" from "localhost"
	 When Merge Window is opened with local "WorkflowWithSequenceToolWithChildrenInDifferentOrder"
	 Then Current workflow contains "2" tools
	 Then Current workflow contains "1" connectors
	 And Different workflow contains "2" tools
	 And Different workflow contains "1" connectors
	 And Merge conflicts count is "3"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing Same tools But disconnected Arms
	 Given I Load workflow "WorkflowWithAssignToolsWithDisconnectedArms" from "localhost"
	 And I Load workflow "WorkflowWithAssignToolsWithDisconnectedArms" from "Remote Connection Integration"	 
	 When Merge Window is opened with remote "WorkflowWithAssignToolsWithDisconnectedArms"
	 Then Current workflow contains "3" tools
	 Then Current workflow contains "1" connectors
	 And Different workflow contains "3" tools
	 And Different workflow contains "1" connectors
	 And Merge conflicts count is "4"
	 And Merge variable conflicts is false

Scenario: Merge Workflow Containing Removed tool with same Variable List
	 Given I Load workflow "MergeRemovedTool" from "localhost"
	 And I Load workflow version of MergeRemovedTool
	 When Merge Window is opened with local "MergeRemovedTool"
	 Then Current workflow contains "4" tools
	 Then Current workflow contains "3" connectors
	 And Different workflow contains "4" tools
	 And Different workflow contains "3" connectors
	 And Merge conflicts count is "7"
	 And Merge variable conflicts is false
	 And I select Current Tool
	 And I select Current Arm
	 And I select Current Arm
	 Then Save is enabled

Scenario: Merge Workflow Containing Switch tool
	 Given I Load workflow "MergeSwitchTool" from "localhost"
	 And I Load workflow version of MergeSwitchTool
	 When Merge Window is opened with local "MergeSwitchTool"
	 Then Current workflow contains "4" tools
	 Then Current workflow contains "3" connectors
	 And Different workflow contains "4" tools
	 And Different workflow contains "3" connectors
	 And Merge conflicts count is "7"
	 And Merge variable conflicts is false
	 And conflict "2" Current matches tool "[[a]]"
	 And conflict "2" Different matches tool "Switch"
	 And conflict "3" Current Connector matches tool "Switch : 1 -> Assign (0)"
	 And conflict "3" Different Connector matches tool is null
	 And conflict "4" Current Connector matches tool "Switch : Default -> Assign (0)"
	 And conflict "4" Different Connector matches tool is null
	 And conflict "5" Current matches tool "Assign (0)"
	 And conflict "6" Current matches tool "Assign (0)"
	 And I select Current Tool
	 And I select Current Arm
	 And I select Current Tool
	 And I select Current Arm
	 Then Save is enabled

Scenario: Merge Workflow Containing Position Change tools
	 Given I Load workflow "MergeToolPositionChange" from "localhost"
	 And I Load workflow version conflict MergeToolPositionChange
	 When Merge Window is opened with local "MergeToolPositionChange"
	 Then Current workflow contains "3" tools
	 Then Current workflow contains "3" connectors
	 And Different workflow contains "3" tools
	 And Different workflow contains "3" connectors
	 And Current workflow header is "MergeToolPositionChange"
	 And Current workflow header version is "[Current]"
	 And Different workflow header is "MergeToolPositionChange"
	 And Different workflow header version is "[v.2]"
	 And Merge conflicts count is "6"
	 And Merge variable conflicts is false
	 And conflict "0" Current matches tool "Start"
	 And conflict "0" Different matches tool "Start"
	 And conflict "1" Current Connector matches tool "Start -> Data Merge (0)"
	 And conflict "1" Different Connector matches tool "Start -> Assign (0)"
	 And conflict "2" Current matches tool "Data Merge (0)"
	 And conflict "2" Different matches tool "Data Merge (0)"
	 And conflict "3" Current Connector matches tool "Data Merge (0) -> Assign (0)"
	 And conflict "3" Different Connector matches tool is null
	 And conflict "4" Current matches tool "Assign (0)"
	 And conflict "4" Different matches tool "Assign (0)"
	 And conflict "5" Current Connector matches tool is null
	 And conflict "5" Different Connector matches tool "Assign (0) -> Data Merge (0)"
	 And I select Current Tool
	 And I select Current Arm
	 And I select Different Arm
	 Then Save is enabled

Scenario: Merge Workflow Version Containing Position Change tools
	 Given I Load workflow "MergeToolPositionChange" from "localhost"
	 And I Load workflow version conflict MergeToolPositionChange
	 When Merge Window is opened with local version "MergeToolPositionChange"
	 Then Current workflow contains "3" tools
	 Then Current workflow contains "3" connectors
	 And Different workflow contains "3" tools
	 And Different workflow contains "3" connectors
	 And Current workflow header is "MergeToolPositionChange"
	 And Current workflow header version is "[v.2]"
	 And Different workflow header is "MergeToolPositionChange"
	 And Different workflow header version is "[Current]"
	 And Merge conflicts count is "6"
	 And Merge variable conflicts is false
	 And conflict "0" Current matches tool "Start"
	 And conflict "0" Different matches tool "Start"
	 And conflict "1" Current Connector matches tool "Start -> Assign (0)"
	 And conflict "1" Different Connector matches tool "Start -> Data Merge (0)"
	 And conflict "2" Current matches tool "Assign (0)"
	 And conflict "2" Different matches tool "Assign (0)"
	 And conflict "3" Current Connector matches tool "Assign (0) -> Data Merge (0)"
	 And conflict "3" Different Connector matches tool is null
	 And conflict "4" Current matches tool "Data Merge (0)"
	 And conflict "4" Different matches tool "Data Merge (0)"
	 And conflict "5" Current Connector matches tool is null
	 And conflict "5" Different Connector matches tool "Data Merge (0) -> Assign (0)"
	 And I select Different Tool
	 And I select Different Arm
	 And I select Current Arm
	 Then Save is enabled


Scenario: Merge Validate All tools are mapped
	 Given I Load All tools and expect all tools to be mapped