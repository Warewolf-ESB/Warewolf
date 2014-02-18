@fileFeature
Feature: Write File
	In order to be able to Write File
	as a Warewolf user
	I want a tool that writes a file at a given location

Scenario Outline: Write file at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'	
	And Method is '<method>'
	And input contents as '<content>'     
	And result as '<resultVar>'
    When the write file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Output Path                 | Method   | Username   | Password | File Contents |
         | <source> = <sourceLocation> | <method> | <username> | String   | <content>     |
	And the debug output as
		| Result                 |
		| <resultVar> = <result> |
		Examples: 
		| Name                     | source   | sourceLocation                                                 | method        | content        | username          | password | resultVar  | result  | errorOccured |
		| Local with Overwrite     | [[path]] | c:\filetowrite.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| UNC with Overwrite       | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetowrite.txt | Overwrite     | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| FTP with Overwrite       | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetowrite.txt          | Overwrite     | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| FTPS with Overwrite      | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetowrite.txt          | Overwrite     | warewolf rules | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| SFTP with Overwrite      | [[path]] | sftp://localhost/filetowrite.txt                               | Overwrite     | warewolf rules | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| Local with Append Top    | [[path]] | c:\filetowrite.txt                                             | Append Top    | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| UNC with Append Top      | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetowrite.txt | Append Top    | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| FTP with Append Top      | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetowrite.txt          | Append Top    | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| FTPS with Append Top     | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetowrite.txt          | Append Top    | warewolf rules | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| SFTP with Append Top     | [[path]] | sftp://localhost/filetowrite.txt                               | Append Top    | warewolf rules | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| Local with Append Bottom | [[path]] | c:\filetowrite.txt                                             | Append Bottom | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| UNC with Append Bottom   | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetowrite.txt | Append Bottom | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| FTP with Append Bottom   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetowrite.txt          | Append Bottom | warewolf rules | ""                | ""       | [[result]] | Success | NO           |
		| FTPS with Append Bottom  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetowrite.txt          | Append Bottom | warewolf rules | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| SFTP with Append Bottom  | [[path]] | sftp://localhost/filetowrite.txt                               | Append Bottom | warewolf rules | dev2              | Q/ulw&]  | [[result]] | Success | NO           |