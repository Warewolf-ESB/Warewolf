Feature: ReadDropbox
	In order to read a list of directories from an dropbox Server
	As a Warewolf User
	I want to be to view files available on a dropbox account


Scenario: Open new Read Dropbox Tool
	And I drag Read Dropbox Tool onto the design surface
    And Read New is Enabled
	And Read Edit is Disabled
	And Read Dropbox File is Enabled
	When I Click Read New
	Then the New Dropbox Source window is opened
	
Scenario: Editing Read Dropbox Tool
	And I drag Read Dropbox Tool onto the design surface
    And Read New is Enabled
	And Read Edit is Disabled
	And Read Dropbox File is Enabled
	When I Select "Drop" as the Read source
	Then Read Edit is Enabled
	When I click "Edit"
	Then the "Drop" Dropbox Source window is opened

Scenario: Change Read Dropbox Source
	And I drag Read Dropbox Tool onto the design surface
    And Read New is Enabled
	And Read Edit is Disabled
	When I Select "Drop" as the Read source
	Then Read Edit is Enabled
	And I set Read Dropbox File equals "Home.txt"
	When I change Read source from "Drop" to "BackupSource"
	And Dropbox File equals ""