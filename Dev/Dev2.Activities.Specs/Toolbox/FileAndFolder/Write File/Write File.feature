Feature: Write File
	In order to be able to Write File
	as a Warewolf user
	I want a tool that writes a file at a given location


Scenario Outline: Write file at location
	Given I have a variable '<variable>' with value '<location>'
	And Method is '<method>'
	And input contents as '<content>' 
    And input username as '<username>' and password '<password>'
    When the Write file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | Directory                   | Read   | Username     | Password     | File Contents |
         | '<variable>' = '<location>' | <read> | '<username>' | '<password>' | <content>     |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
		Examples: 
		| variable | location                                    | Read          | content        | username          | password | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               | Overwrite     | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Overwrite     | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Overwrite     | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Overwrite     | warewolf rules | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Overwrite     | warewolf rules | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               | Append Top    | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Append Top    | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Append Top    | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Append Top    | warewolf rules | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Append Top    | warewolf rules | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               | Append Bottom | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Append Bottom | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Append Bottom | warewolf rules |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Append Bottom | warewolf rules | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Append Bottom | warewolf rules | dev2              | Q/ulw&]  | [[result]] | Success | NO           |