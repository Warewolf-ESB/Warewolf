@Storage
Feature: UploadDropbox
	In order to save content to an Online Server
	As a Warewolf User
	I want to be to backup/move files to a dropbox account


Scenario: Open new Dropbox Upload Tool
	And I drag Upload Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	And Local File is Enabled
	And Dropbox File is Enabled
	When I Click New

Scenario: Editing Dropbox Upload Tool
	And I drag Upload Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	And Local File is Enabled
	And Dropbox File is Enabled
	When I Select "Drop" as the source
	Then Edit is Enabled
	And I Click Edit
	Then the "Drop" Dropbox Source window is opened

Scenario: Change Dropbox Upload Source
	And I drag Upload Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	When I Select "Drop" as the source
	Then Edit is Enabled
	And I set Local File equals "E:\test.txt"
	And I set Dropbox File equals "Home.txt"
	When I change source from "Drop" to "BackupSource"
	Then Local File equals ""
	And Dropbox File equals ""


