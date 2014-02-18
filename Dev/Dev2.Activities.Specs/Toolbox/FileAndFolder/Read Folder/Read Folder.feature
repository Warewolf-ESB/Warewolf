Feature: Read Folder
	In order to be able to Read Folder File or Folder 
	as a Warewolf user
	I want a tool that reads the contents of a Folder at a given location


	Scenario Outline: Read Folder file at location
	Given I have a variable '<variable>' with value '<location>'
	And Read is '<read>'
    And input username as '<username>' and password '<password>'
    When the Read Folder file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | Directory                   | Read | Username     | Password     |
         | '<variable>' = '<location>' | <read> | '<username>' | '<password>' |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
    Examples: 
		| variable | location                                    | Read          | username          | password | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               | Files         |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Files         |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Files         |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Files         | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Files         | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               | Folders       |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Folders       |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Folders       |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Folders       | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Folders       | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               | Files&Folders |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Files&Folders |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Files&Folders |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Files&Folders | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Files&Folders | dev2              | Q/ulw&]  | [[result]] | Success | NO           |