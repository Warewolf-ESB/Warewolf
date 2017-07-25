@Storage
Feature: DeleteDropbox
	In order to delete from an dropbox Server
	As a Warewolf User
	I want to be to delete files on a dropbox account

Scenario: Open new Delete Dropbox Tool
	And I drag Delete Dropbox Tool onto the design surface
    And Dropbox Delete New is Enabled
	And Dropbox Delete Edit is Disabled
	And Delete Dropbox File is Enabled
	When I Click Delete New
	
	
Scenario: Editing Delete Dropbox Tool
	And I drag Delete Dropbox Tool onto the design surface
    And Dropbox Delete New is Enabled
	And Dropbox Delete Edit is Disabled
	And Delete Dropbox File is Enabled
	When I Select "Drop" as the Delete source
	Then Dropbox Delete Edit is Enabled
	When I click Dropbox Delete Edit
	

Scenario: Change Delete Dropbox Source
	And I drag Delete Dropbox Tool onto the design surface
    And Dropbox Delete New is Enabled
	And Dropbox Delete Edit is Disabled
	When I Select "Drop" as the Delete source
	Then Dropbox Delete Edit is Enabled
	And I set Delete Dropbox File equals "Home.txt"
	When I change Delete source from "Drop" to "BackupSource"
	And Delete Dropbox File equals ""
