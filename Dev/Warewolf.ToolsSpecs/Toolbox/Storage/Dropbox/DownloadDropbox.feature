Feature: DownloadDropbox
	In order to save content to an Online Server
	As a Warewolf User
	I want to be to backup/move files to a dropbox account


Scenario: Open new Dropbox Download Tool
	Given I drag DropboxDownload Tool onto the design surface
    And DropboxDownload New is Enabled
	And DropboxDownload Edit is Disabled
	And DropboxDownload Local File is Enabled
	And DropboxDownload File is Enabled
	When DropboxDownload I Click New
	
Scenario: Editing Dropbox Download Tool
	Given I drag DropboxDownload Tool onto the design surface
    And DropboxDownload New is Enabled
	And DropboxDownload Edit is Disabled
	And DropboxDownload Local File is Enabled
	And DropboxDownload File is Enabled
	When DropboxDownload I Select "Drop" as the source
	Then DropboxDownload Edit is Enabled
	And DropboxDownload I Click Edit

Scenario: Change Dropbox Download Source
	Given I drag DropboxDownload Tool onto the design surface
    And DropboxDownload New is Enabled
	And DropboxDownload Edit is Disabled
	When DropboxDownload I Select "Drop" as the source
	Then DropboxDownload Edit is Enabled
	And I set Download Dropbox Local File equals "E:\test.txt"
	And I set Download Dropbox File equals "Home.txt"
	When Download I change source from "Drop" to "BackupSource"
	Then Download Local File equals ""
	And Download Dropbox File equals ""
