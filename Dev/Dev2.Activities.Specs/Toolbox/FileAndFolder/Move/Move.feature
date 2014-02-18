Feature: Move
	In order to be able to Move a File or Folder 
	as a Warewolf user
	I want a tool that will Move File(s) or Folder(s) from a given location to another location


Scenario Outline: Move file at location
	Given I have a variable '<variable>' with value '<location>'
	And input username as '<username>' and password '<password>'
	And I have a  destination variable '<variable1>' with value '<location1>'
    And input  destination username as '<username1>' and password '<password1>'
	And overwrite is '<selected>'
    When the Move file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | File or Folder              | Username     | Password     | Destination                   | Username      | Password      | Overwrite    |
         | '<variable>' = '<location>' | '<username>' | '<password>' | '<variable1>' = '<location1>' | '<username1>' | '<password1>' | '<selected>' |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| variable | location                                    | username          | password | variable1 | Location1                                   | username1         | password1 | selected | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | D:\copy.txt                                 |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt           |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0  | True     | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]   | True     | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\myfile1.txt                              |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | D:\copy.txt                                 |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt           |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0  | True     | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]   | True     | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | c:\myfile.txt                               |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\test\test4.txt            |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | D:\copy.txt                                 |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0  | True     | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]   | True     | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | c:\myfile.txt                               |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\test\test3.txt             |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | D:\copy.txt                                 |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftp:\\dev2.co.za\testing\test.txt           |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\test\test2.txt   | integrationtester | I73573r0  | True     | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | D:\copy.txt                                 |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | c:\myfile.txt                               |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | D:\copy.txt                                 |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftp:\\dev2.co.za\testing\test.txt           |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0  | True     | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | c:\myfile.txt                               |                   |           | True     | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\test\test1.txt            | dev2              | Q/ulw&]   | True     | [[result]] | Success | NO           |






