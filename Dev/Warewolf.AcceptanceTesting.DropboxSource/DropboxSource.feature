Feature: DropboxSource
	In order to save content to an Online Server
	As a Warewolf User
	I want to be to backup/move files to a dropbox account

@ignore
Scenario: New Dropbox Source
	Given I have open New Dropbox Source
	Then "New Dropbox Source" tab is opened
	And title is "New Dropbox Source"
	Then "Email" has the focus
	And "Email" is ""
	And "Password" is ""
	And "Forgot your Password" is visible
	And "New to Dropbox?Create an account" is visible
	And "Log in" is enabled

Scenario: Create a new Dropbox Source
	Given I have open New Dropbox Source
	Then "New Dropbox Source" tab is opened
	And title is "New Dropbox Source"
	Then "Email" has the focus
	And "Email" is "Someone@dev2.co.za"
	And "Password" is "•••••••"
	And I click "Log In"
	Then "Warewolf ESB would like access to the files and folders in your Dropbox" is visible
	And "Cancel" is "Enabled"
	And "Allow" is "Enabled"
	When "Allow" is clicked
	Then the save dialog is opened
	And I save as "MyDropboxSource"
	Then "MyDropboxSource is visible in the Explorer

Scenario: Denying Warewolf access to Dropbox
	Given I have open New Dropbox Source
	Then "New Dropbox Source" tab is opened
	And title is "New Dropbox Source"
	Then "Email" has the focus
	And "Email" is "Someone@dev2.co.za"
	And "Password" is "•••••••"
	And I click "Log In"
	Then "Warewolf ESB would like access to the files and folders in your Dropbox" is visible
	And "Cancel" is "Enabled"
	And "Allow" is "Enabled"
	When "Cancel" is clicked
	Then "New Dropbox Source" tab is closed

Scenario: Edit saved Dropbox Source
	Given I click "Edit - Dropbox Source"
	Then "Edit - Dropbox Source" tab is opened
	And title is "Edit - Dropbox Source"
	And "Email" has the focus
	And "Email" is ""
	And "Password" is ""

