@StudioTestFrameworkWithFileAndFolderTools
Feature: StudioTestFrameworkWithFileAndFolderTools
	In order to test workflows that contain file ops tools in warewolf 
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


Scenario: Test WF with Create
		Given I have a workflow "CreateTestWF"
		And "CreateTestWF" contains an Assign "Assign to create" as
		  | variable    | value           |
		  | [[rec().a]] | C:\copied00.txt |
		And "CreateTestWF" contains an Create "Create1" as
			 | File or Folder | If it exits | Username | Password | Result   |
			 | [[rec().a]]    | True        |          |          | [[res1]] |
		And I save workflow "CreateTestWF"
		Then the test builder is open with "CreateTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "Create1" as TestStep
		And I add StepOutputs as 
		| Variable Name | Condition | Value   |
		| [[res1]]      | =         | Success |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "CreateTestWF" is deleted as cleanup

Scenario: Test WF with Create and Delete folder
		Given I have a workflow "DeleteFolderTestWF"
		And "DeleteFolderTestWF" contains an Assign "Assign to create" as
		  | variable    | value           |
		  | [[rec().a]] | C:\copied00.txt |
		And "DeleteFolderTestWF" contains an Create "Create1" as
		  | File or Folder | If it exits | Username | Password | Result   |
		  | [[rec().a]]    | True        |          |          | [[res1]] |
	    And "DeleteFolderTestWF" contains an Delete Folder "DeleteFolder" as
	      | Recordset   | Result   |
	      | [[rec().a]] | [[res2]] |
		And I save workflow "DeleteFolderTestWF"
		Then the test builder is open with "DeleteFolderTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "DeleteFolder" as TestStep
		And I add StepOutputs as 
		| Variable Name | Condition | Value   |
		| [[res2]]      | =         | Success |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "DeleteFolderTestWF" is deleted as cleanup

Scenario: Test WF with Move
		Given I have a workflow "MoveTestWF"
		And I create temp file as "C:\copied00.txt" 
		And "MoveTestWF" contains an Assign "Assign to Move" as
		  | variable    | value           |
		  | [[rec().a]] | C:\copied00.txt |
		  | [[rec().b]] | C:\copied01.txt |
		And "MoveTestWF" contains an Move "Move1" as
		  | File or Folder | If it exits | Destination | Username | Password | Result     |
		  | [[rec().a]]    | True        | [[rec().b]] |          |          | [[result]] |	  
		And I save workflow "MoveTestWF"
		Then the test builder is open with "MoveTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "Move1" as TestStep
		And I add StepOutputs as 
		| Variable Name | Condition | Value   |
		| [[result]]    | =         | Success |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "MoveTestWF" is deleted as cleanup
		
Scenario: Test WF with Read File
		Given I have a workflow "ReadFileTestWF"
		And I create temp file to read from as "C:\ProgramData\Warewolf\Resources\Log.txt" 
		And "ReadFileTestWF" contains an Read File "ReadFile" as
		  | File or Folder                            |Username | Password | Result     |
		  | C:\ProgramData\Warewolf\Resources\Log.txt |         |          | [[Result]] |
		And I save workflow "ReadFileTestWF"
		Then the test builder is open with "ReadFileTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "ReadFile" as TestStep
	And I add StepOutputs as 
		| Variable Name | Condition | Value |
		| [[Result]]    | Contains  | Hello |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "ReadFileTestWF" is deleted as cleanup

Scenario: Test WF with Rename File
		Given I have a workflow "RenameFileTestWF"		
		And "RenameFileTestWF" contains an Create "Create1" as
		  | File or Folder                                     | If it exits | Username | Password | Result   |
		  | C:\ProgramData\Warewolf\Resources\FileToRename.txt | True        |          |          | [[res1]] |
		And "RenameFileTestWF" contains an Rename "RenameFile" as
		  | File or Folder                                     | Destination                                   | If it exits | Username | Password | Result     | Folders |
		  | C:\ProgramData\Warewolf\Resources\FileToRename.txt | C:\ProgramData\Warewolf\Resources\Renamed.txt | True        |          |          | [[result]] | True    |
		And I save workflow "RenameFileTestWF"
		Then the test builder is open with "RenameFileTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "RenameFile" as TestStep
	And I add StepOutputs as 
		| Variable Name | Condition | Value   |
		| [[result]]    | =         | Success |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "RenameFileTestWF" is deleted as cleanup

		
Scenario: Test WF with Unzip File
		Given I have a workflow "UnzipFileTestWF"		
		And "UnzipFileTestWF" contains an Create "Create1" as
		  | File or Folder                                          | If it exits | Username | Password | Result   |
		  | C:\ProgramData\Warewolf\Resources\FileToZipAndUnzip.txt | True        |          |          | [[res1]] |
		And "UnzipFileTestWF" contains an Zip "ZipFile" as
		  | File or Folder                                          | Destination                                                   | If it exits | Username | Password | Result        | Folders |
		  | C:\ProgramData\Warewolf\Resources\FileToZipAndUnzip.txt | C:\ProgramData\Warewolf\Resources\FileToZipAndUnzipZipped.zip | True        |          |          | [[ZipResult]] | True    |
		And "UnzipFileTestWF" contains an UnZip "UnZipFile" as
		  | File or Folder                                                | Destination                                                | If it exits | Username | Password | Result          | Folders |
		  | C:\ProgramData\Warewolf\Resources\FileToZipAndUnzipZipped.zip | C:\ProgramData\Warewolf\Resources\FileToZipAndUnzipZipped1 | True        |          |          | [[UnZipResult]] | True    |
		And I save workflow "UnzipFileTestWF"
		Then the test builder is open with "UnzipFileTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "UnZipFile" as TestStep
		And I add StepOutputs as 
		| Variable Name   | Condition | Value   |
		| [[UnZipResult]] | =         | Success |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "UnzipFileTestWF" is deleted as cleanup
		
Scenario: Test WF with Zip File
		Given I have a workflow "ZipFileTestWF"		
		And "ZipFileTestWF" contains an Create "Create1" as
		  | File or Folder                                     | If it exits | Username | Password | Result   |
		  | C:\ProgramData\Warewolf\Resources\FileToZip.txt | True        |          |          | [[res1]] |
		And "ZipFileTestWF" contains an Zip "ZipFile" as
		  | File or Folder                                  | Destination                                  | If it exits | Username | Password | Result     | Folders |
		  | C:\ProgramData\Warewolf\Resources\FileToZip.txt | C:\ProgramData\Warewolf\Resources\Zipped.txt | True        |          |          | [[result]] | True    |
		And I save workflow "ZipFileTestWF"
		Then the test builder is open with "ZipFileTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "ZipFile" as TestStep
		And I add StepOutputs as 
		| Variable Name | Condition | Value   |
		| [[result]]    | =         | Success |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "ZipFileTestWF" is deleted as cleanup
		