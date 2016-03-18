Feature: UploadDropbox
	In order to save content to an Online Server
	As a Warewolf User
	I want to be to backup/move files to a dropbox account


Scenario: Open new Dropbox Tool
	Given I open New Workflow
	And I drag Upload Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	And Local File is Enabled
	And Dropbox File is Enabled
	When I Click New
	Then the New Dropbox Source window is opened
	
Scenario: Editing Dropbox Tool
	Given I open New Workflow
	And I drag Upload Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	And Local File is Enabled
	And Dropbox File is Enabled
	When I Select "Drop" as the source
	Then Edit is Enabled
	And I Click Edit
	Then the "Drop" Dropbox Source window is opened

Scenario: Change Dropbox Source
	Given I open New Workflow
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

@ignore
Scenario Outline: Uploading to Dropbox Source
	Given I open New Workflow
	And I drag Upload Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	When I Select "Drop" as the source
	Then Edit is Enabled
	And I set Local File equals "<local>" with values "<localValue>" 
	And I set Dropbox File equals "<Dropbox>" with values "<DropboxValue>"
	When the Dropbox tool is executed
	Then the workflow execution has "NO" error
Examples: 
| Local                  | LocalValue      | Dropbox                | DropboxValue     |
| [[var]]                | E:\automate.jpg | [[rec([[index]]).set]] | home/home.jpg    |
| [[rec().set]]          | E:\test.exe     | [[rec().set]]          | test.exe         |
| [[rec(1).set]]         | E:\trumphet.png | [[var]]                | t.png            |
| [[rec(*).set]]         | E:\test.zip     | [[rec(1).set]]         | x.zip            |
| [[rec([[index]]).set]] | E:\tr.txt       | [[rec(*).set]]         | test.txt         |
|                        | E:\xx.zip       |                        | warewolfproj.zip |

@ignore
Scenario Outline: Incorrect parameters to Dropbox Source
	Given I open New Workflow
	And I drag Upload Dropbox Tool onto the design surface
    And New is Enabled
	And Edit is Disabled
	When I Select "Drop" as the source
	Then Edit is Enabled
	And I set Local File equals "<local>" with values "<localValue>" 
	And I set Dropbox File equals "<Dropbox>" with values "<DropboxValue>"
	When the Dropbox tool is executed
	Then the execution has "An" error
	And the result is "<error>"
Examples: 
| Local                  | LocalValue      | Dropbox                | DropboxValue  | error                                                                                                    |
| [[var]]                |                 | [[rec([[index]]).set]] | home.jpg      | Cannot locate local file/s to be uploaded.Please confirm that the correct file location has been entered |
| [[rec().set]]          | E:\test.exe     | [[rec().set]]          |               | Incorrect destination provided                                                                                  |
| [[rec().set]]          | E:\test.exe     | [[rec().set]]          | home\test.exe | Incorrect destination provided                                                                                  |
| [[rec(1).set]]         | 1               | [[var]]                | t.png         | Cannot locate local file/s to be uploaded.Please confirm that the correct file location has been entered |
| [[rec(*).set]]         | Z:\test.zip     | [[rec(1).set]]         | x.zip         | Cannot locate local file/s to be uploaded.Please confirm that the correct file location has been entered |
| [[rec([[index]]).set]] | E:\treasure.txt | [[rec(*).set]]         | test.txt      | Cannot locate local file/s to be uploaded.Please confirm that the correct file location has been entered |
| [[sca().]]             |                 | [[va/]]                |               | Incorrect path provided                                                                                  |

