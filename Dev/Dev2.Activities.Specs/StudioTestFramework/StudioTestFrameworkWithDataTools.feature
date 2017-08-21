@StudioTestFrameworkWithDataTools
Feature: StudioTestFrameworkWithDataTools
	In order to test workflows that contain data tools in warewolf 
	As a user
	I want to create, edit, delete and update tests in a test window


Background: Setup for workflows for tests
		Given test folder is cleaned	
		And I have "Workflow 1" with inputs as
			| Input Var Name |
			| [[a]]          |
		And "Workflow 1" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |		
		Given I have "Workflow 2" with inputs as
			| Input Var Name |
			| [[rec().a]]    |
			| [[rec().b]]    |
		And "Workflow 2" has outputs as
			| Ouput Var Name |
			| [[returnVal]]  |
		Given I have "Workflow 3" with inputs as
			| Input Var Name |
			| [[A]]          |
			| [[B]]          |
			| [[C]]          |
		And "Workflow 3" has outputs as
			| Ouput Var Name |
			| [[message]]    |
		Given I have "WorkflowWithTests" with inputs as 
			| Input Var Name |
			| [[input]]      |
		And "WorkflowWithTests" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |			
		And "WorkflowWithTests" Tests as 
			| TestName | AuthenticationType | Error | TestFailing | TestPending | TestInvalid | TestPassed |
			| Test1    | Windows            | false | false       | false       | false       | true       |
			| Test2    | Windows            | false | true        | false       | false       | false      |
			| Test3    | Windows            | false | false       | false       | true        | false      |
			| Test4    | Windows            | false | false       | true        | false       | false      |


Scenario: Test WF with Assign
	Given I have a workflow "AssignTestWF"
	And "AssignTestWF" contains an Assign "TestAssign" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |
	And I save workflow "AssignTestWF"
	Then the test builder is open with "AssignTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestAssign" as TestStep
	And I add StepOutputs as 
	| Variable Name | Condition | Value | 
	| [[rec(1).a]]  | =         | yes   | 
	| [[rec(2).a]]  | =         | no    | 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "AssignTestWF" is deleted as cleanup

Scenario: Test WF with Assign Object
		Given I have a workflow "AssignObjectTestWF"
		And "AssignObjectTestWF" contains an Assign Object "TestAssignObject" as 
		 | variable            | value |
		 | [[@Person.Name]]    | yes   |
		 | [[@Person.Surname]] | no    |
		 And I save workflow "AssignObjectTestWF"
		 Then the test builder is open with "AssignObjectTestWF"
		 And I click New Test
		 And a new test is added	
	   	 And test name starts with "Test 1"
		 And I Add "TestAssignObject" as TestStep
		And I add StepOutputs as 
		 | Variable Name       | Condition | Value | 
		 | [[@Person.Name]]    | =         | yes   | 
		 | [[@Person.Surname]] | =         | no    | 
		 When I save
		 And I run the test
		 Then test result is Passed
		 When I delete "Test 1"
		 Then The "DeleteConfirmation" popup is shown I click Ok
		 Then workflow "AssignObjectTestWF" is deleted as cleanup
		 

Scenario: Test WF with BaseConvert 
		Given I have a workflow "BaseConvertTestWF"
		And "BaseConvertTestWF" contains an Assign "TestAssign" as
		  | variable | value                                                                                                    |
		  | [[a]]    | 01001001001000000111011101100001011100110010000001101101011000010110111001100111011011000110010101100100 |
		And "BaseConvertTestWF" contains Base convert "TestBaseConvert" as
		  | Variable  | From   | To   |
		  | [[a]] | Binary | Text |
		 And I save workflow "BaseConvertTestWF"
		 Then the test builder is open with "BaseConvertTestWF"
		 And I click New Test
		 And a new test is added	
		 And test name starts with "Test 1"
		 And I Add "TestBaseConvert" as TestStep
		And I add StepOutputs as 
		  | Variable Name | Condition | Value         |
		  | [[a]]         | =         | I was mangled |
		 When I save
		 And I run the test
		 Then test result is Passed
		 When I delete "Test 1"
		 Then The "DeleteConfirmation" popup is shown I click Ok
		 Then workflow "BaseConvertTestWF" is deleted as cleanup
		
Scenario: Test WF with CaseConvert
		Given I have a workflow "CaseConvertTestWF"
			And "CaseConvertTestWF" contains an Assign "TestAssign" as
		 | variable    | value |
		 | [[rec().a]] | 50    |
		 | [[rec().a]] | test  |
		 | [[rec().a]] | 100   |
		And "CaseConvertTestWF" contains case convert "TestCaseConvert" as
		  | Variable     | Type  |
		  | [[rec(2).a]] | UPPER |
		 And I save workflow "CaseConvertTestWF"
		 Then the test builder is open with "CaseConvertTestWF"
		 And I click New Test
		 And a new test is added	
		 And test name starts with "Test 1"
		 And I Add "TestCaseConvert" as TestStep
	 And I add StepOutputs as 
		 | Variable Name | Condition | Value |
		 | [[rec(2).a]]  | =         | TEST  |
		 When I save
		 And I run the test
		 Then test result is Passed
		 When I delete "Test 1"
		 Then The "DeleteConfirmation" popup is shown I click Ok
		 Then workflow "CaseConvertTestWF" is deleted as cleanup
	
Scenario: Test WF with Data split
		Given I have a workflow "DataSplitTestWF"
		And "DataSplitTestWF" contains an Assign "TestAssign" as
		 | variable        | value                                                                              |
		 | [[FileContent]] | Brad,5546854,brad@mail.com Bob,65548912,bob@mail.com Bill,3215464987,bill@mail.com |
		And "DataSplitTestWF" contains Data Split "TestDataSplit" as	
		| String          | Variable       | Type  | At | Include | Escape |
		| [[FileContent]] | [[rec().Name]] | Chars | ,  |         |        |
		 And I save workflow "DataSplitTestWF"
		 Then the test builder is open with "DataSplitTestWF"
		 And I click New Test
		 And a new test is added	
		 And test name starts with "Test 1"
		 And I Add "TestDataSplit" as TestStep
		And I add StepOutputs as 
	  	 | Variable Name   | Condition | Value             |
	  	 | [[rec(1).Name]] | =         | Brad              |
	  	 | [[rec(2).Name]] | =         | 5546854           |
	  	 | [[rec(3).Name]] | =         | brad@mail.com Bob |
	  	 | [[rec(4).Name]] | =         | 65548912          |
	  	 | [[rec(5).Name]] | =         | bob@mail.com Bill |
	  	 | [[rec(6).Name]] | =         | 3215464987        |
	  	 | [[rec(7).Name]] | =         | bill@mail.com     |
		 When  I save
		 And I run the test
		 Then test result is Passed
		 When I delete "Test 1"
		 Then The "DeleteConfirmation" popup is shown I click Ok
		 Then workflow "DataSplitTestWF" is deleted as cleanup
		
Scenario: Test WF with Find Index
		Given I have a workflow "FindIndexTestWF"
		And "FindIndexTestWF" contains an Assign "TestAssign" as
		   | variable    | value    |
			  | [[rec().a]] | test     |
			  | [[rec().b]] | nothing  |
			  | [[rec().a]] | warewolf |
			  | [[rec().b]] | nothing  |
		And "FindIndexTestWF" contains Find Index "TestIndex" into "[[indexResult]]" as
		  | In Fields   | Index           | Character | Direction     |
		  | [[rec().a]] | First Occurence | e         | Left to Right |
		 And I save workflow "FindIndexTestWF"
		 Then the test builder is open with "FindIndexTestWF"
		 And I click New Test
		 And a new test is added	
		 And test name starts with "Test 1"
		 And I Add "TestIndex" as TestStep
		And I add StepOutputs as 
		 | Variable Name   | Condition | Value |
		 | [[indexResult]] | =         | 4     |
		 When I save
		 And I run the test
		 Then test result is Passed
		 When I delete "Test 1"
	 	 Then The "DeleteConfirmation" popup is shown I click Ok
		 Then workflow "FindIndexTestWF" is deleted as cleanup


Scenario: Test WF with Data Merge
		Given I have a workflow "DataMergeTestWF"
		And "DataMergeTestWF" contains an Assign "TestAssign" as
		  | variable      | value    |
		  | [[a]]         | Test     |
		  | [[b]]         | Warewolf |
		  | [[split().a]] | Workflow |
		And "DataMergeTestWF" contains Data Merge "TestDataMerge" into "[[result]]" as			
		  | Variable | Type  | Using | Padding | Alignment |
		  | [[a]]    | Index | 4     |         | Left      |
		  | [[b]]    | Index | 8     |         | Left      |
		And I save workflow "DataMergeTestWF"
		Then the test builder is open with "DataMergeTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "TestDataMerge" as TestStep
		And I add StepOutputs as 
		| Variable Name | Condition | Value        |
		| [[result]]    | =         | TestWarewolf |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "DataMergeTestWF" is deleted as cleanup
		
Scenario: Test WF with Replace
		Given I have a workflow "ReplaceTestWF"
		And "ReplaceTestWF" contains an Assign "TestAssign" as
		 | variable    | value    |
		  | [[rec().a]] | test     |
		  | [[rec().b]] | nothing  |
		  | [[rec().a]] | warewolf |
		  | [[rec().b]] | nothing  |
	  And "ReplaceTestWF" contains Replace "TestReplace" into "[[replaceResult]]" as	
		  | In Fields  | Find | Replace With |
		  | [[rec(*)]] | e    | REPLACED     |
		And I save workflow "ReplaceTestWF"
		Then the test builder is open with "ReplaceTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "TestReplace" as TestStep
		And I add StepOutputs as 
		| Variable Name     | Condition | Value           |
		| [[rec(1).a]]      | =         | tREPLACEDst     |
		| [[rec(2).a]]      | =         | warREPLACEDwolf |		
		| [[replaceResult]] | =         | 2               |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "ReplaceTestWF" is deleted as cleanup

Scenario: Test WF with Replace with square brackets
		Given I have a workflow "ReplaceTestWF"
		And "ReplaceTestWF" contains an Assign "TestAssign" as
		 | variable    | value    |
		  | [[rec().a]] | test     |
		  | [[rec().b]] | nothing  |
		  | [[rec().a]] | warewolf |
		  | [[rec().b]] | nothing  |
	  And "ReplaceTestWF" contains Replace "TestReplace" into "[[replaceResult]]" as	
		  | In Fields  | Find | Replace With |
		  | [[rec(*)]] | e    | [[     |
		And I save workflow "ReplaceTestWF"
		Then the test builder is open with "ReplaceTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "TestReplace" as TestStep
		And I add StepOutputs as 
		| Variable Name     | Condition | Value           |
		| [[rec(1).a]]      | =         | t[[st     |
		| [[rec(2).a]]      | =         | war[[wolf |		
		| [[replaceResult]] | =         | 2               |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "ReplaceTestWF" is deleted as cleanup

		
Scenario: Test Wf With AssignObject And ObjectOutput
	   Given the test builder is open with existing service "TestWfWithAssignObjectAndObjectOutput"	
	And Tab Header is "TestWfWithAssignObjectAndObjectOutput - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "TestWfWithAssignObjectAndObjectOutput - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank	
	And I Add "Assign Object (3)" as TestStep		
	And I Clear existing StepOutputs
	And I add StepOutputs item as 
	| Variable Name | Condition | Value |
	| [[@aaa.a]]    | =         | ff    |
	And I add StepOutputs item as 
	| Variable Name | Condition | Value |
	| [[@aaa.a]]    | =         | ff    |
	And I add StepOutputs item as 
	| Variable Name | Condition | Value |
	| [[@aaba().a]]    | =         |     |
	And I Add outputs as
	| Variable Name  | Condition   | Value |
	| @aaba() | Is Not NULL |       |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned

Scenario: Test WF Workflow with Assign and Sequence(Assign, Datamerge, Data Split, Find Index and Replace) mock 
	Given I have a workflow "sequenceMockTestWF"		
	 And "sequenceMockTestWF" contains an Assign "Assign for sequence" as
      | variable    | value    |
      | [[rec().a]] | test     |
      | [[rec().b]] | nothing  |
      | [[rec().a]] | warewolf |
      | [[rec().b]] | nothing  |
	 And "sequenceMockTestWF" contains a Sequence "Sequence1" as
	 And "Sequence1" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 4     |         | Left      |
	  | [[rec(2).a]] | Index | 8     |         | Left      |
	 And "Sequence1" contains Data Split "Data Split" as
	  | String       | Variable     | Type  | At | Include    | Escape |
	  | testwarewolf | [[rec(1).b]] | Index | 4  | Unselected |        |
	  |              | [[rec(2).b]] | Index | 8  | Unselected |        |
	 And "Sequence1" contains Find Index "Index" into "[[indexResult]]" as
	  | In Fields    | Index           | Character | Direction     |
	  | [[rec().a]] | First Occurence | e         | Left to Right |
	 And "Sequence1" contains Replace "Replacing" into "[[replaceResult]]" as	
	  | In Fields  | Find | Replace With |
	  | [[rec(*)]] | e    | REPLACED     |
	 And I save workflow "sequenceMockTestWF"
	 Then the test builder is open with "sequenceMockTestWF"
	 And I click New Test
	 And I Add "Sequence1" as TestStep	
	 When I save
	 And I run the test
	 Then test result is Passed
	 When I delete "Test 1"
	 
Scenario: Test WF Workflow with Assign and Sequence(Assign, Datamerge, Data Split, Find Index and Replace) Assign
	Given I have a workflow "sequenceAssertTestWF"		
	 And "sequenceAssertTestWF" contains an Assign "Assign for sequence" as
      | variable    | value    |
      | [[rec().a]] | test     |
      | [[rec().b]] | nothing  |
      | [[rec().a]] | warewolf |
      | [[rec().b]] | nothing  |
	 And "sequenceAssertTestWF" contains a Sequence "Sequence1" as
	 And "Sequence1" contains Data Merge "Data Merge" into "[[result]]" as	
	  | Variable     | Type  | Using | Padding | Alignment |
	  | [[rec(1).a]] | Index | 4     |         | Left      |
	  | [[rec(2).a]] | Index | 8     |         | Left      |
	 And "Sequence1" contains Data Split "Data Split" as
	  | String       | Variable     | Type  | At | Include    | Escape |
	  | testwarewolf | [[rec(1).b]] | Index | 4  | Unselected |        |
	  |              | [[rec(2).b]] | Index | 8  | Unselected |        |
	 And "Sequence1" contains Find Index "Index" into "[[indexResult]]" as
	  | In Fields    | Index           | Character | Direction     |
	  | [[rec().a]] | First Occurence | e         | Left to Right |
	 And "Sequence1" contains Replace "Replacing" into "[[replaceResult]]" as	
	  | In Fields  | Find | Replace With |
	  | [[rec(*)]] | e    | REPLACED     |
	 And I save workflow "sequenceAssertTestWF"
	 Then the test builder is open with "sequenceAssertTestWF"
	 And I click New Test
	 And I Add "Sequence1" as TestStep All Assert
	 When I save
	 And I run the test
	 Then test result is Passed
	 When I delete "Test 1"
	 
Scenario: Test Workflow with ForEach which contains assign Mock
      Given I have a workflow "TestWFForEachMock"
	  And "TestWFForEachMock" contains an Assign "Rec To Convert" as
	    | variable    | value |
	    | [[Warewolf]] | bob   |
	  And "TestWFForEachMock" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2"
	  And "ForEachTest" contains an Assign "MyAssign" as
	    | variable    | value |
	    | [[rec().a]] | Test  |
      And I save workflow "TestWFForEachMock"
	  Then the test builder is open with "TestWFForEachMock"
	  And I click New Test
	  And I Add "ForEachTest" as TestStep	
	  When I save
	  And I run the test
	  Then test result is Passed
	  When I delete "Test 1"
	  
Scenario: Test Workflow with ForEach which contains assign Assert
	Given I have a workflow "TestWFForEachAssert"
	And "TestWFForEachAssert" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[Warewolf]] | bob   |
	And "TestWFForEachAssert" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2"
	And "ForEachTest" contains an Assign "MyAssign" as
	  | variable    | value |
	  | [[rec().a]] | Test  |
	   And I save workflow "TestWFForEachAssert"
	Then the test builder is open with "TestWFForEachAssert"
	And I click New Test
	And I Add "ForEachTest" as TestStep All Assert
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"

Scenario: Test Workflow with Loop Constructs - Select and Apply example workflow
	Given the test builder is open with "Select and Apply"
	And I click New Test
	And I Add all TestSteps
	When I save
	And I run the test
	Then test result is Failed
	And the service debug assert Json message contains "Message: Failed: Assert Equal. Expected Equal To '' for '@Pet' but got"
	When I delete "Test 1"