@StudioTestFrameworkWithDropboxTools
Feature: StudioTestFrameworkkWithDropboxTools
	In order to test workflows that contain dropbox tools in warewolf 
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


Scenario: Test Wf With Dropbox Upload Tool
	Given I have a workflow "TestWFWithDropBoxUpload"	
	And "TestWFWithDropBoxUpload" contains a DropboxUpload "UploadTool" Setup as
	| Local File      | OverwriteOrAdd | DropboxFile | Result  |
	| C:\Home.Dropbox | Overwrite      | source.xml  | [[res]] |
	And I save workflow "TestWFWithDropBoxUpload"
	Then the test builder is open with "TestWFWithDropBoxUpload"
	And I click New Test
	And I Add "UploadTool" as TestStep 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"

Scenario: Test Wf With Dropbox Delete Tool
	Given I have a workflow "TestWFWithDropBoxDelete"	
	And "TestWFWithDropBoxDelete" contains a DropboxUpload "UploadTool" Setup as
	| Local File     | OverwriteOrAdd | DropboxFile  | Result  |
	| C:\Home.Delete | Overwrite      | ToDelete.xml | [[res]] |
	And "TestWFWithDropBoxDelete" contains a DropboxDelete "DeleteTool" Setup as
	| DropboxFile  |  Result  |
	| ToDelete.xml |  [[res]] |
	And I save workflow "TestWFWithDropBoxDelete"
	Then the test builder is open with "TestWFWithDropBoxDelete"
	And I click New Test
	And I Add "UploadTool" as TestStep 
	And I Add "DeleteTool" as TestStep 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"

Scenario: Test Wf With Dropbox Download Tool
	Given I have a workflow "TestWFWithDropBoxDowload"	
	And "TestWFWithDropBoxDowload" contains a DropboxUpload "UploadTool" Setup as
	| Local File      | OverwriteOrAdd | DropboxFile  | Result  |
	| C:\Home.Delete | Overwrite       | Download.xml | [[res]] |
	And "TestWFWithDropBoxDowload" contains a DropboxDownLoad "DownloadTool" Setup as
	| Local File     | OverwriteOrAdd | DropboxFile  | Result  |
	| C:\Home.Delete | Overwrite      | Download.xml | [[res]] |
	And I save workflow "TestWFWithDropBoxDowload"
	Then the test builder is open with "TestWFWithDropBoxDowload"
	And I click New Test
	And I Add "UploadTool" as TestStep 
	And I Add "DownloadTool" as TestStep 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"

Scenario: Test Wf With Dropbox List Tool
	Given I have a workflow "TestWFWithDropBoxList"	
	And "TestWFWithDropBoxList" contains a DropboxUpload "UploadTool" Setup as
	| Local File     | OverwriteOrAdd | DropboxFile       | Result  |
	| C:\Home.Delete | Overwrite      | Home/Download.xml | [[res]] |
	And "TestWFWithDropBoxList" contains a DropboxList "ListTool" Setup as
	| Read  | LoadSubFolders | DropboxFile | Result          |
	| Files | true           | Home        | [[res().Files]] |
	And I save workflow "TestWFWithDropBoxList"
	Then the test builder is open with "TestWFWithDropBoxList"
	And I click New Test
	And I Add "UploadTool" as TestStep 
	And I Add "ListTool" as TestStep 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"

Scenario:Test Workflow which contains COM DLL
	 Given I have a workflow "TestWFCOMDLL"
	 And "TestWFCOMDLL" contains an COM DLL "COMService" as
	 | Source       | Namespace     | Action |
	 | RandomSource | System.Random | Next   |
	   And I save workflow "TestWFCOMDLL"
	  Then the test builder is open with "TestWFCOMDLL"
	  And I click New Test
	  And I Add "COMService" as TestStep
	  And I add StepOutputs as  
	  	 | Variable Name            | Condition | Value |
	  	 | [[PrimitiveReturnValue]] | Not Date  |       |
	  When I save
	  And I run the test
	  Then test result is Passed
	  When I delete "Test 1"

