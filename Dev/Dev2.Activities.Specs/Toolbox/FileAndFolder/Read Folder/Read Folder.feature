Feature: Read Folder
	In order to be able to Read Folder File or Folder 
	as a Warewolf user
	I want a tool that reads the contents of a Folder at a given location


	Scenario Outline: Read Folder file at location	
	Given I have a source path '<variable>' with value '<location>'
	And source credentials as '<username>' and '<password>'
	And Read is '<read>'   
	And result as '<resultVar>'
    When the read folder file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Directory               | Read   | Username   | Password |
         | <variable> = <location> | <read> | <username> | String   |
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
    Examples: 
		| variable | location                                    | read          | username          | password | resultVar  | result | errorOccured |
		| [[path]] | c:\myfile.txt                               | Files         |                   |          | [[result]] | String | NO           |
		| [[path]] | \\\\rsaklfsvrtfsbld\testing\test.txt        | Files         |                   |          | [[result]] | String | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Files         |                   |          | [[result]] | String | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Files         | integrationtester | I73573r0 | [[result]] | String | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Files         | dev2              | Q/ulw&]  | [[result]] | String | NO           |
		| [[path]] | c:\myfile.txt                               | Folders       |                   |          | [[result]] | String | NO           |
		| [[path]] | \\\\rsaklfsvrtfsbld\testing\test.txt        | Folders       |                   |          | [[result]] | String | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Folders       |                   |          | [[result]] | String | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Folders       | integrationtester | I73573r0 | [[result]] | String | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Folders       | dev2              | Q/ulw&]  | [[result]] | String | NO           |
		| [[path]] | c:\myfile.txt                               | Files&Folders |                   |          | [[result]] | String | NO           |
		| [[path]] | \\\\\rsaklfsvrtfsbld\testing\test.txt       | Files&Folders |                   |          | [[result]] | String | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Files&Folders |                   |          | [[result]] | String | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Files&Folders | integrationtester | I73573r0 | [[result]] | String | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Files&Folders | dev2              | Q/ulw&]  | [[result]] | String | NO           |