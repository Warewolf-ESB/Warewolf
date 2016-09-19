@Explorer
Feature: Explorer
	In order to manage my service
	As a Warewolf User
	I want explorer view of my resources with management options

@Explorer
Scenario: Connected to localhost server
	Given the explorer is visible
	When I open "localhost" server
	Then I should see "5" folders

@Explorer
Scenario: Expand a folder
	Given the explorer is visible
	And I open "localhost" server
	When I open Resource "Folder 2"
	Then I should see "18" children for "Folder 2"

@Explorer
Scenario: Rename folder
	Given the explorer is visible
	And I open "localhost" server
	When I rename "localhost\Folder 2" to "Folder New"
	Then I should see "18" children for "Folder New"
	Then I should see the path "localhost\Folder New" 
	Then I should not see the path "localhost\Folder 2" 

@Explorer
Scenario: Search explorer
	Given the explorer is visible
	And I open "localhost" server
	When I search for "Folder 3"
	Then I should see "localhost\Folder 3" only
	And I should not see "Folder 1"
	And I should not see "Folder 2"
	And I should not see "Folder 4"
	And I should not see "Folder 5"

@Explorer
Scenario: Creating Folder in localhost
   Given the explorer is visible
   When I open "localhost" server
   Then I should see "5" folders
   When I add "MyNewFolder" in "localhost"
   Then I should see the path "localhost\MyNewFolder" 

@Explorer
Scenario: Deleting Resource in folders
   Given the explorer is visible
   When I open "localhost" server
   Then I should see "5" folders
   When I open Resource "Folder 5"
   And I create the "localhost\Folder 5\deleteresource" of type "WorkflowService" 
   Then I should see the path "localhost\Folder 5\deleteresource"
   When I delete "localhost\Folder 5\deleteresource"
   Then I should not see "deleteresouce" in "Folder 5"

@Explorer
Scenario: Deleting Resource in localhost Server
   Given the explorer is visible
   When I open "localhost" server
   And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
   Then I should see "5" folders
   Then I should see the path "localhost\Folder 1\Resource 1"
    And I choose to "OK" Any Popup Messages
   When I delete "localhost\Folder 1\Resource 1"
   Then I should not see the path "localhost\Folder 1\Resource 1"

@Explorer
Scenario: Deleting Resource in localhost Server with Tests
   Given the explorer is visible
   When I open "localhost" server
   And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
   And I create 2 Tests for "localhost\Folder 1\Resource 1"
   Then "localhost\Folder 1\Resource 1" has 2 tests  
   Then I should see "5" folders
   Then I should see the path "localhost\Folder 1\Resource 1"
    And I choose to "OK" Any Popup Messages
   When I delete "localhost\Folder 1\Resource 1"
   Then I should not see the path "localhost\Folder 1\Resource 1"
   And "localhost\Folder 1\Resource 1" has 0 tests

@Explorer
Scenario: Renaming Folder And Workflow Service
	Given the explorer is visible
	And I open "localhost" server
	When I rename "localhost\Folder 2" to "Folder New"
	Then I should see "18" children for "Folder New"
	When I open Resource "Folder New"
	And I create the "localhost\Folder New\Resource 1" of type "WorkflowService" 
	And I create the "localhost\Folder New\Resource 2" of type "WorkflowService" 
	Then I should see the path "localhost\Folder New"
	Then I should see the path "localhost\Folder New\Resource 1"
	And I should not see "Folder 2"
	And I should not see the path "localhost\Folder 2"
	When I rename "localhost\Folder New\Resource 1" to "WorkFlow1"	
	Then I should see the path "localhost\Folder New\WorkFlow1"
	When I rename "localhost\Folder New\Resource 2" to "WorkFlow1"	
	Then Conflict error message occurs

@Explorer
Scenario: Searching resources by using filter
  Given the explorer is visible
  And I open "localhost" server
  When I open Resource "Folder 1"
  And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
  Then I should see the path "localhost\Folder 1\Resource 1"
  When I search for "Folder 1" in explorer
  Then I should see the path "localhost\Folder 1"
  Then I should not see the path "localhost\Folder 1\Resource 1"
  Then I should not see the path "localhost\Folder 2"
  When I search for "Resource 1" in explorer
  When I open Resource "Folder 1"
  Then I should see the path "localhost\Folder 1\Resource 1"

@Explorer
Scenario: Checking versions 
  Given the explorer is visible
  When I open "localhost" server
  And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
  Then I should see "5" folders
  And I Setup  "3" Versions to be returned for "localhost\Folder 1\Resource 1"
  When I Show Version History for "localhost\Folder 1\Resource 1"
  Then I should see "3" versions with "View" Icons in "localhost\Folder 1\Resource 1"
  When I search for "Resource 1" in explorer
  Then I should see the path "localhost\Folder 1\Resource 1"
  Then I should see "3" versions with "View" Icons in "localhost\Folder 1\Resource 1"

@Explorer
Scenario: Clear filter
  Given the explorer is visible
  And I open "localhost" server
  When I open Resource "Folder 1"
  And I create the "localhost\Folder 1\Resource 1" of type "WorkflowService" 
  Then I should see the path "localhost\Folder 1\Resource 1"
  When I search for "Folder 1" in explorer
  Then I should see the path "localhost\Folder 1"
  Then I should not see the path "localhost\Folder 1\Resource 1"
  Then I should not see the path "localhost\Folder 2"
  When I search for "Resource 1" in explorer
  When I open Resource "Folder 1"
  Then I should see the path "localhost\Folder 1\Resource 1"
  When I clear "Explorer" Filter
  Then I should see the path "localhost\Folder 2"
  Then I should see the path "localhost\Folder 2"
  Then I should see the path "localhost\Folder 2"
  Then I should see the path "localhost\Folder 2"

@Explorer
Scenario: Search explorer on remote server
	Given the explorer is visible
	And I connect to "Remote Connection Integration" server
	And I open "Remote Connection Integration" server
	And I create the "Remote Connection Integration\Hello World" of type "WorkflowService" 
	Then I should see "10" folders
	When I search for "Hello World"
	Then I should see "Remote Connection Integration\Hello World" only

@Explorer
Scenario: Connected to remote server
	Given the explorer is visible
	When I connect to "Remote Connection Integration" server
	And I open "Remote Connection Integration" server
	Then I should see "10" folders
	Then I should see the path "Remote Connection Integration\Folder 2"

@Explorer
Scenario: Creating Folder in remote host
   Given the explorer is visible
   And I connect to "Remote Connection Integration" server
   And I open "Remote Connection Integration" server
   And I should see "10" folders
   When I add "MyNewFolder" in "Remote Connection Integration"
   Then I should see the path "Remote Connection Integration\MyNewFolder" 

@Explorer
Scenario: Opening and Editing workflow from Explorer localhost
	Given the explorer is visible
	And I open "localhost" server
	And I create the "localhost\Hello World" of type "WorkflowService" 
	When I open "Hello World" in "localhost"
	And "Hello World" tab is opened

@Explorer
Scenario: Opening and Editing workflow from Explorer Remote
	Given the explorer is visible
	And I connect to "Remote Connection Integration" server
	And I open "Remote Connection Integration" server
	And I create the "Remote Connection Integration\Hello World" of type "WorkflowService" 
	When I open "Hello World" in "Remote Connection Integration"
	And "Hello World" tab is opened 

@Explorer
Scenario: Renaming Folder And Workflow Service on a remote server
	Given the explorer is visible
	And I connect to "Remote Connection Integration" server
    And I open "Remote Connection Integration" server
	When I rename "Remote Connection Integration\Folder 2" to "Folder New"
	Then I should see "18" children for "Folder New"
	When I open Resource "Folder New"
	And I create the "Remote Connection Integration\Folder New\Resource 1" of type "WorkflowService" 
	And I create the "Remote Connection Integration\Folder New\Resource 2" of type "WorkflowService" 
	Then I should see the path "Remote Connection Integration\Folder New"
	Then I should see the path "Remote Connection Integration\Folder New\Resource 1"
	And I should not see the path "Remote Connection Integration\Folder 2"
	When I rename "Remote Connection Integration\Folder New\Resource 1" to "WorkFlow1"	
	Then I should see the path "Remote Connection Integration\Folder New\WorkFlow1"
	When I rename "Remote Connection Integration\Folder New\Resource 2" to "WorkFlow1"	
	Then Conflict error message occurs
