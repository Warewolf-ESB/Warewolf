@StudioTestFrameworkWithScriptingTools
Feature: StudioTestFrameworkWithScriptingTools
	In order to test workflows that contain scripting tools in warewolf 
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
			| [[A]]              |
			| [[B]]              |
			| [[C]]              |
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
			

Scenario: Test WF with Cmd Script
	Given I have a workflow "CmdScriptTestWF"	
	And "CmdScriptTestWF" contains a Cmd Script "testCmdScript" ScriptToRun "echo Kingdom of KwaZulu Natal" and result as "[[result]]"	
	And I save workflow "CmdScriptTestWF"
	Then the test builder is open with "CmdScriptTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testCmdScript" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value                    |
	  	 | [[result]]    | =         | Kingdom of KwaZulu Natal |  
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "CmdScriptTestWF" is deleted as cleanup
	
Scenario: Test WF with JavaScript
	Given I have a workflow "JavaScriptTestWF"	
	And "JavaScriptTestWF" contains a Java Script "testJavaScript" ScriptToRun "return Math.sqrt(49);" and result as "[[result]]"
	And I save workflow "JavaScriptTestWF"
	Then the test builder is open with "JavaScriptTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testJavaScript" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 7     |  
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "JavaScriptTestWF" is deleted as cleanup

Scenario: Test WF with Python
	Given I have a workflow "PythonTestWF"	
	And "PythonTestWF" contains a Python "testPython" ScriptToRun "return { '1': "one", '2': "two",}.get('7', "not one or two")" and result as "[[result]]"		
	And I save workflow "PythonTestWF"
	Then the test builder is open with "PythonTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testPython" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value          |
	  	 | [[result]]    | =         | not one or two |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "PythonTestWF" is deleted as cleanup

Scenario: Test WF with Ruby
	Given I have a workflow "RubyTestWF"
	And "RubyTestWF" contains a Ruby "testRuby" ScriptToRun "sleep(5)" and result as "[[result]]"		
	And I save workflow "RubyTestWF"
	Then the test builder is open with "RubyTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testRuby" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 5     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RubyTestWF" is deleted as cleanup

Scenario: Test WF with Sharepoint Copy File
	Given I have a workflow "ShapointCopyFileTestWF"	
	  And "ShapointCopyFileTestWF" contains SharepointUploadFile "TestSharePUploadFile" as 
	| Server                 | LocalPathFrom                                     | ServerPathTo | Result     |
	| SharePoint Test Server | C:\ProgramData\Warewolf\Resources\Hello World.xml | e.xml        | [[Result]] |	  
	And "ShapointCopyFileTestWF" contains SharepointCopyFile "TestSharePCopyFile" as 
	| Server                 | ServerPathFrom | ServerPathTo | Overwrite | Result         |
	| SharePoint Test Server | e.xml          | f.xml        | true      | [[copyResult]] |
	And I save workflow "ShapointCopyFileTestWF"
	Then the test builder is open with "ShapointCopyFileTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePCopyFile" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name  | Condition | Value   |
	  	 | [[copyResult]] | =         | Success |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointCopyFileTestWF" is deleted as cleanup
	
Scenario: Test WF with Sharepoint Create List Items
	Given I have a workflow "ShapointCreateListItemsTestWF"	
	  And "ShapointCreateListItemsTestWF" contains an Assign "MyAssign" as
	    | variable                                          | value                                                                |
	    | [[AcceptanceTesting_Create().Title]]              | Mr                                                                   |
	    | [[AcceptanceTesting_Create().Name]]               | Micky                                                                |
	    | [[AcceptanceTesting_Create().IntField]]           | 1.1                                                                  |
	    | [[AcceptanceTesting_Create().CurrencyField]]      | 2211                                                                 |
	    | [[AcceptanceTesting_Create().DateField]]          | 2016/11/10                                                           |
	    | [[AcceptanceTesting_Create().DateTimeField]]      | 2016/11/10                                                           |
	    | [[AcceptanceTesting_Create().BoolField]]          | True                                                                 |
	    | [[AcceptanceTesting_Create().MultilineTextField]] | <div class="ExternalClassD0D0AB75CC30454599C3D12D077D6D8D">123</div> |
	    | [[AcceptanceTesting_Create().RequiredField]]      | Text                                                                 |
	    | [[AcceptanceTesting_Create().Loc]]                | True                                                             |
	    
	And "ShapointCreateListItemsTestWF" contains CreateListItems "TestSharePCreateItemList" as 
	| Server                 | List                     | Result     |
	| SharePoint Test Server | AcceptanceTesting_Create | [[Result]] |
	And I save workflow "ShapointCreateListItemsTestWF"
	Then the test builder is open with "ShapointCreateListItemsTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePCreateItemList" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value   |
	  	 | [[Result]]    | =         | Success |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointCreateListItemsTestWF" is deleted as cleanup
	
Scenario: Test WF with Sharepoint Delete File List
	Given I have a workflow "ShapointDeleteFileListTestWF"	 
	And "ShapointDeleteFileListTestWF" contains SharepointDeleteFile "TestSharePDeleteFile" as 
	| Server                 | SharepointList | Result        |
	| SharePoint Test Server | AccTesting     | [[delResult]] |
	And I save workflow "ShapointDeleteFileListTestWF"
	Then the test builder is open with "ShapointDeleteFileListTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePDeleteFile" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value |
	  	 | [[delResult]] | =         | 0     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointDeleteFileListTestWF" is deleted as cleanup
	
Scenario: Test WF with Sharepoint Delete File
	Given I have a workflow "ShapointDelSingleItemTestWF"		
	And "ShapointDelSingleItemTestWF" contains SharepointDeleteSingle "TestSharePdeleteListItem" as 
	| Server                 | ServerPath | Result     |
	| SharePoint Test Server | 125698.xml | [[Result]] |
	And I save workflow "ShapointDelSingleItemTestWF"
	Then the test builder is open with "ShapointDelSingleItemTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePdeleteListItem" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value |
	  	 | [[Result]]    | =         |       |
	And I expect Error "File Not Found"  
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointDelSingleItemTestWF" is deleted as cleanup

Scenario: Test WF with Sharepoint Download File
	Given I have a workflow "ShapointDownloadFileTestWF"
	And "ShapointUploadFileTestWF" contains SharepointUploadFile "TestSharePUploadFile" as 
	| Server                 | LocalPathFrom                                     | ServerPathTo    | Result       |
	| SharePoint Test Server | C:\ProgramData\Warewolf\Resources\Hello World.xml | Hello World.xml | [[Uploaded]] |
	And "ShapointDownloadFileTestWF" contains SharepointDownloadFile "TestSharePDownloadFile" as 
		| Server                 | ServerPathFrom  | LocalPathTo                                                                | Overwrite | Result         |
		| SharePoint Test Server | Hello World.xml | C:\ProgramData\Warewolf\Resources\DownloadedFromSharepoint\Hello World.xml | True      | [[Downloaded]] |
	And I save workflow "ShapointDownloadFileTestWF"
	Then the test builder is open with "ShapointDownloadFileTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePDownloadFile" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name  | Condition | Value   |
	  	 | [[Downloaded]] | =         | Success |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointDownloadFileTestWF" is deleted as cleanup
	
Scenario: Test WF with Sharepoint Upload File
	Given I have a workflow "ShapointUploadFileTestWF"		 
	And "ShapointUploadFileTestWF" contains SharepointUploadFile "TestSharePUploadFile" as 
	| Server                 | LocalPathFrom                                     | ServerPathTo | Result     |
	| SharePoint Test Server | C:\ProgramData\Warewolf\Resources\Hello World.xml | a.xml        | [[Result]] |
	And I save workflow "ShapointUploadFileTestWF"
	Then the test builder is open with "ShapointUploadFileTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePUploadFile" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value   |
	  	 | [[Result]]    | =         | Success |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointUploadFileTestWF" is deleted as cleanup

Scenario: Test WF with Sharepoint Move File
	Given I have a workflow "ShapointMoveFileTestWF"	
	And "ShapointMoveFileTestWF" contains SharepointUploadFile "TestSharePUploadFile" as 
	| Server                 | LocalPathFrom                                     | ServerPathTo | Result     |
	| SharePoint Test Server | C:\ProgramData\Warewolf\Resources\Hello World.xml | B.xml        | [[Result]] |	  
	And "ShapointMoveFileTestWF" contains SharepointMoveFile "TestSharePMoveFile" as 
	| Server                 | ServerPathFrom | ServerPathTo | Overwrite | Result         |
	| SharePoint Test Server | B.xml          | c.xml        | true      | [[MoveResult]] |
	And I save workflow "ShapointMoveFileTestWF"
	Then the test builder is open with "ShapointMoveFileTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePMoveFile" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name  | Condition | Value   |
	  	 | [[MoveResult]] | =         | Success |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointMoveFileTestWF" is deleted as cleanup

Scenario: Test WF with Sharepoint Read Folder
	Given I have a workflow "ShapointReadFolderTestWF"
	And "ShapointReadFolderTestWF" contains SharepointReadFolder "TestSharePReadFolder" as 
	| Server                 | ServerPath | Folders | Result     |
	| SharePoint Test Server |            | True    | [[Folders(*).Name]] |
	And I save workflow "ShapointReadFolderTestWF"
	Then the test builder is open with "ShapointReadFolderTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePReadFolder" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name       | Condition | Value                 |
	  	 | [[Folders(1).Name]] | =         | /Shared Documents/bob |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointReadFolderTestWF" is deleted as cleanup

Scenario: Test WF with Sharepoint Read List Item
	Given I have a workflow "ShapointReadListItemTestWF"	
	And "ShapointReadListItemTestWF" contains SharepointReadListItem "TestSharePReadListItem" as 
	| Server                 | List       |
	| SharePoint Test Server | AccTesting |
	And I save workflow "ShapointReadListItemTestWF"
	Then the test builder is open with "ShapointReadListItemTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePReadListItem" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name           | Condition | Value  |
	  	 | [[AccTesting(1).Title]] | =         | Mrs    |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointReadListItemTestWF" is deleted as cleanup

Scenario: Test WF with Sharepoint Update List Item
	Given I have a workflow "ShapointUpdateListItemTestWF"	
	   And "ShapointCreateListItemsTestWF" contains a recordset name randomizing Assign "MyAssign" as
	    | variable                            | value                                                                |
	    | [[AccTesting().Title]]              | Mrs                                                                  |
	    | [[AccTesting().Name]]               | Minnie                                                               |
	    | [[AccTesting().IntField]]           | 2.0                                                                  |
	    | [[AccTesting().CurrencyField]]      | 2211                                                                 |
	    | [[AccTesting().DateField]]          | 2016/11/5                                                            |
	    | [[AccTesting().DateTimeField]]      | 2016/10/10                                                           |
	    | [[AccTesting().BoolField]]          | True                                                                 |
	    | [[AccTesting().MultilineTextField]] | <div class="ExternalClassD0D0AB75CC30454599C3D12D077D6D8D">123</div> |
	    | [[AccTesting().RequiredField]]      | Text                                                                 |
	    | [[AccTesting().Loc]]                | True                                                                 |	    
	And "ShapointCreateListItemsTestWF" contains UpdateListItems "TestSharePUpdateListItem" as 
	| Server                 | List       | Result     |
	| SharePoint Test Server | AccTesting | [[Result]] |
	And I save workflow "ShapointUpdateListItemTestWF"
	Then the test builder is open with "ShapointUpdateListItemTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestSharePUpdateListItem" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value   |
	  	 | [[Result]]    | =         | Success |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "ShapointUpdateListItemTestWF" is deleted as cleanup