Feature: ReadDropbox
In order to save content to an Online Server
	As a Warewolf User
	I want to be to backup/move files to a dropbox account


Scenario: Open new Dropbox Tool
	Given I open New Workflow
	And I drag Read Dropbox Tool onto the design surface
    And Read New is Enabled
	And Read Edit is Disabled
	And Read Local File is Enabled
	And Read Dropbox File is Enabled
	When I Click Read New
	Then the Read New Dropbox Source window is opened
	
Scenario: Editing Dropbox Tool
	Given I open New Workflow
	And I drag Read Dropbox Tool onto the design surface
    And Read New is Enabled
	And Read Edit is Disabled
	And Read Local File is Enabled
	And Read Dropbox File is Enabled
	When I Select "Drop" as the Read source
	Then Read Edit is Enabled
	When I click "Edit"
	Then the "Drop" Read Dropbox Source window is opened

Scenario: Change Dropbox Source
	Given I open New Workflow
	And I drag Read Dropbox Tool onto the design surface
    And Read New is Enabled
	And Read Edit is Disabled
	When I Select "Drop" as the Read source
	Then Read Edit is Enabled
	And I set Read Dropbox Local File equals "E:\test.txt"
	And I set Read Dropbox File equals "Home.txt"
	When I change Read source from "Drop" to "BackupSource"
	Then Read Local File equals ""
	And Read Dropbox File equals ""