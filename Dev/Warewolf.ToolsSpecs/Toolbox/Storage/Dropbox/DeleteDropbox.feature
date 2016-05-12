Feature: DeleteDropbox
	In order to delete from an dropbox Server
	As a Warewolf User
	I want to be to delete files on a dropbox account

Scenario: Open new Delete Dropbox Tool
	And I drag Delete Dropbox Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	And Delete Dropbox File is Enabled
	When I Click New
	Then the New Dropbox Source window is opened
	
Scenario: Editing Delete Dropbox Tool
	And I drag Delete Dropbox Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	And Delete Dropbox File is Enabled
	When I Select "Drop" as the Delete source
	Then Delete Edit is Enabled
	When I click "Edit"
	Then the "Drop" Dropbox Source window is opened

Scenario: Change Delete Dropbox Source
	And I drag Delete Dropbox Tool onto the design surface
    And Delete New is Enabled
	And Delete Edit is Disabled
	When I Select "Drop" as the Delete source
	Then Delete Edit is Enabled
	And I set Dropbox File equals "Home.txt"
	When I change source from "Drop" to "BackupSource"
	And Dropbox File equals ""
