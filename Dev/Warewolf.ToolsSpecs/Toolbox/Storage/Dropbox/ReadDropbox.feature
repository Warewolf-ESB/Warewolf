Feature: ReadDropbox
	In order to read a list of directories from an dropbox Server
	As a Warewolf User
	I want to be to view files available on a dropbox account


Scenario: Open new Read Dropbox Tool
	Given I drag Readlist Dropbox Tool onto the design surface
    And Readlist New is Enabled
	And Readlist Edit is Disabled
	And Readlist Dropbox File is Enabled
	When I Click Readlist New
	
Scenario: Editing Read Dropbox Tool
	Given I drag Readlist Dropbox Tool onto the design surface
    And Readlist New is Enabled
	And Readlist Edit is Disabled
	And Readlist Dropbox File is Enabled
	When I Select "Drop" as the Readlist source
	Then Readlist Edit is Enabled
	When I Readlist click Edit

Scenario: Change Read Dropbox Source
	Given I drag Readlist Dropbox Tool onto the design surface
    And Readlist New is Enabled
	And Readlist Edit is Disabled
	When I Select "Drop" as the Readlist source
	Then Readlist Edit is Enabled
	And I set Readlist Dropbox File equals "Home.txt"
	When I change Readlist source from "Drop" to "BackupSource"
	And Readlist Dropbox File equals ""