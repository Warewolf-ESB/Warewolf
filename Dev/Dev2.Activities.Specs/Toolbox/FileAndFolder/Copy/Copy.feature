@fileFeature
Feature: Copy
	In order to be able to Copy File or Folder 
	as a Warewolf user
	I want a tool that Copy File or Folder from a given location to another location

Scenario Outline: Copy file at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And I have a destination path '<destination>' with value '<destinationLocation>'
    And destination credentials as '<destUsername>' and '<destPassword>'
	And overwrite is '<selected>'
	And result as '<resultVar>'
    When the copy file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> |
	And the debug output as
		| Result                 |
		| <resultVar> = <result> |
	Examples: 
		| source         | sourceLocation                                              | username          | password | destination  | destinationLocation                                       | destUsername      | destPassword | selected | resultVar  | result  | errorOccured |
		| [[sourcePath]] | c:\copyfile.txt                                             | ""                | ""       | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\copyfile.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\copyfile.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\copyfile.txt                                             | ""                | ""       | [[destPath]] | sftp://localhost/copied.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\copyfile.txt                                             | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\copyfile.txt                                             | ""                | ""       | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile.txt | ""                | ""       | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile.txt | ""                | ""       | [[destPath]] | sftp://localhost/copied.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile.txt | ""                | ""       | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied.txt                         | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile.txt          | ""                | ""       | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile.txt          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile.txt          | ""                | ""       | [[destPath]] | sftp://localhost/copied.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile.txt          | ""                | ""       | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile.txt          | ""                |          | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile.txt          | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\copied.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | sftp://localhost/copied.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
