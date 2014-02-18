Feature: Rename
	In order to be able to Rename File or Folder 
	as a Warewolf user
	I want a tool that will rename a File or Floder at a given location


Scenario Outline: Rename file at location
	Given I have a variable '<variable>' with value '<location>'
	And input username as '<username>' and password '<password>'
	And a New Name '<New Name>'
    And folder username as '<username1>' and password '<password1>'
	And overwrite is '<selected>'
    When the Rename file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | File or Folder              | Username     | Password     | Destination                   | Username      | Password      | Overwrite    |
         | '<variable>' = '<location>' | '<username>' | '<password>' | '<variable1>' = '<location1>' | '<username1>' | '<password1>' | '<selected>' |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| variable | location                                    | username          | password | New Name      | username1 | password1 | selected | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | c:\My New.txt |           |           | True     | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | c:\My New.txt |           |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | c:\My New.txt |           |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | c:\My New.txt |           |           | True     | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | c:\My New.txt |           |           | True     | [[result]] | Success | NO           |
