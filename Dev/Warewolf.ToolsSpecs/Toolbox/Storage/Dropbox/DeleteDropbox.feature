﻿Feature: DeleteDropbox
	In order to delete from an dropbox Server
	As a Warewolf User
	I want to be to delete files on a dropbox account

Scenario: Open new Dropbox Tool
	Given I open New Workflow
	And I drag Delete Dropbox Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	And Delete Dropbox File is Enabled
	When I Click Delete New
	Then the New Dropbox Source window is opened
	
Scenario: Editing Dropbox Tool
	Given I open New Workflow
	And I drag Delete Dropbox Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	And Delete Dropbox File is Enabled
	When I Select "Drop" as the Delete source
	Then Delete Edit is Enabled
	When I click "Edit"
	Then the Dropbox Source window is opened

Scenario: Change Dropbox Source
	Given I open New Workflow
	And I drag Delete Dropbox Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	When I Select "Drop" as the Delete source
	Then Delete Edit is Enabled
	And I set Delete Dropbox File equals "Home.txt"
	When I change Delete source from "Drop" to "BackupSource"
	And Delete Dropbox File equals ""
