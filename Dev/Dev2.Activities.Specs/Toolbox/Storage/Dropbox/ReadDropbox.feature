Feature: ReadDropbox
In order to save content to an Online Server
	As a Warewolf User
	I want to be to backup/move files to a dropbox account


Scenario: Open new Dropbox Tool
	Given I open New Workflow
	And I drag Read Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	And Local File is Enabled
	And Dropbox File is Enabled
	When I Click New
	Then the New Dropbox Source window is opened
	
Scenario: Editing Dropbox Tool
	Given I open New Workflow
	And I drag Read Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	And Local File is Enabled
	And Dropbox File is Enabled
	When I Select "Drop" as the source
	Then Edit is Enabled
	When I click "Edit"
	Then the "Drop" Dropbox Source window is opened

Scenario: Change Dropbox Source
	Given I open New Workflow
	And I drag Read Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	When I Select "Drop" as the source
	Then Edit is Enabled
	And Local File equals "E:\test.txt"
	And Dropbox File equals "Home.txt"
	When I change source from "Drop" to "BackupSource"
	Then Local File equals ""
	And Dropbox File equals ""