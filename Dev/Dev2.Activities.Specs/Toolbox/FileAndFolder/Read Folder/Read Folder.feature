@fileFeature
Feature: Read Folder
	In order to be able to Read Folder File or Folder 
	as a Warewolf user
	I want a tool that reads the contents of a Folder at a given location


Scenario Outline: Read Folder file at location	
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And Read is '<read>'   
	And result as '<resultVar>'
    When the read folder file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                   | Read   | Username   | Password |
         | <source> = <sourceLocation> | <read> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
    Examples: 
	| Name        | source   | sourceLocation                                 | read            | username          | password | resultVar  | result | errorOccured |
	| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[result]] | String | NO           |
	| UNC         | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | Files           | ""                | ""       | [[result]] | String | NO           |
	| FTP         | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/         | Files           | ""                | ""       | [[result]] | String | NO           |
	| FTPS        | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/         | Files           | integrationtester | I73573r0 | [[result]] | String | NO           |
	| SFTP        | [[path]] | sftp://localhost                               | Files           | dev2              | Q/ulw&]  | [[result]] | String | NO           |
	| Local       | [[path]] | c:\                                            | Folders         | ""                | ""       | [[result]] | String | NO           |
	| UNC         | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | Folders         | ""                | ""       | [[result]] | String | NO           |
	| FTP         | [[path]] | ftp://rsaklfsvrsbspdc:1001/                    | Folders         | ""                | ""       | [[result]] | String | NO           |
	| FTPS        | [[path]] | ftp://rsaklfsvrsbspdc:1002/                    | Folders         | integrationtester | I73573r0 | [[result]] | String | NO           |
	| SFTP        | [[path]] | sftp://localhost                               | Folders         | dev2              | Q/ulw&]  | [[result]] | String | NO           |
	| Local       | [[path]] | c:\                                            | Files & Folders | ""                | ""       | [[result]] | String | NO           |
	| UNC         | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | Files & Folders | ""                | ""       | [[result]] | String | NO           |
	| FTP         | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/         | Files & Folders | ""                | ""       | [[result]] | String | NO           |
	| FTPS        | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/         | Files & Folders | integrationtester | I73573r0 | [[result]] | String | NO           |
	| SFTP        | [[path]] | sftp://localhost                               | Files & Folders | dev2              | Q/ulw&]  | [[result]] | String | NO           |