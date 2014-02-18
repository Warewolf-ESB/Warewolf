@fileFeature
Feature: Move
	In order to be able to Move a File or Folder 
	as a Warewolf user
	I want a tool that will Move File(s) or Folder(s) from a given location to another location
	
Scenario Outline: Move file at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And I have a destination path '<destination>' with value '<destinationLocation>'
    And destination credentials as '<destUsername>' and '<destPassword>'
	And overwrite is '<selected>'
	And result as '<resultVar>'
    When the Move file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> |       
	And the debug output as
		| Result                 |
		| <resultVar> = <result> |
	Examples: 
		| source         | sourceLocation                                              | username          | password | destination  | destinationLocation                                      | destUsername      | destPassword | selected | resultVar  | result  | errorOccured |
		| [[sourcePath]] | c:\movefile.txt                                             | ""                | ""       | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\movefile.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\movefile.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\movefile.txt                                             | ""                | ""       | [[destPath]] | sftp://localhost/moved.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\movefile.txt                                             | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\movefile.txt                                             | ""                | ""       | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile.txt | ""                | ""       | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile.txt | ""                | ""       | [[destPath]] | sftp://localhost/moved.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile.txt | ""                | ""       | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile.txt          | ""                | ""       | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile.txt          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile.txt          | ""                | ""       | [[destPath]] | sftp://localhost/moved.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile.txt          | ""                | ""       | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile.txt          | ""                |          | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile.txt          | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/movefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/movefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/movefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/movefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/movefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\moved.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/movefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | sftp://localhost/moved.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |






