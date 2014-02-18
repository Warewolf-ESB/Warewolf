Feature: Unzip
	In order to be able to Unzip File or Folder 
	as a Warewolf user
	I want a tool that will Unzip File(s) or Folder(s) at a given location


Scenario Outline: Unzip file at location
	Given I have a variable '<variable>' with value '<location>'
	And input username as '<username>' and password '<password>'
	And I have a  destination variable '<variable1>' with value '<location1>'
    And folder username as '<username1>' and password '<password1>'
	And overwrite is '<selected>'
	And Archive Password as '<archivepassword>'
    When the Unzip file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | File or Folder              | Username     | Password     | Destination                   | Username      | Password      | Overwrite    |Archive Password  |
         | '<variable>' = '<location>' | '<username>' | '<password>' | '<variable1>' = '<location1>' | '<username1>' | '<password1>' | '<selected>' |<archivepassword> |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| variable | location                                    | username          | password | variable1 | location1                                    | username1         | password1 | selected | archivepassword | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | c:\My New.txt                                |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | c:\My New.txt                                |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing1\test.txt          |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New1.txt                               |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing1\test.txt          |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing1\test.txt           |                   |           | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testin1g\test.txt | integrationtester | I73573r   | True     | archipassword   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing1\test.txt          | dev2              | Q/ulw&]   | True     | archipassword   | [[result]] | Success | NO           |