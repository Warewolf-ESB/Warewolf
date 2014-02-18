@fileFeature
Feature: Rename
	In order to be able to Rename File or Folder 
	as a Warewolf user
	I want a tool that will rename a File or Floder at a given location


Scenario Outline: Rename file at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And I have a destination path '<destination>' with value '<destinationLocation>'
    And destination credentials as '<destUsername>' and '<destPassword>'
	And overwrite is '<selected>'
	And result as '<resultVar>'
    When the rename file tool is executed
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
		| [[sourcePath]] | c:\renamefile.txt                                             | ""                | ""       | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\renamefile.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\renamefile.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\renamefile.txt                                             | ""                | ""       | [[destPath]] | sftp://localhost/renamed.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\renamefile.txt                                             | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | c:\renamefile.txt                                             | ""                | ""       | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile.txt | ""                | ""       | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile.txt | ""                | ""       | [[destPath]] | sftp://localhost/renamed.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile.txt | ""                | ""       | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed.txt                         | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile.txt          | ""                | ""       | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile.txt          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile.txt          | ""                | ""       | [[destPath]] | sftp://localhost/renamed.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile.txt          | ""                | ""       | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile.txt          | ""                |          | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile.txt          | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile.txt          | integrationtester | I73573r0 | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/renamefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/renamefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed.txt | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/renamefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed.txt          | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/renamefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed.txt          | integrationtester | I73573r0     | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/renamefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\renamed.txt                                             | ""                | ""           | True     | [[result]] | Success | NO           |
		| [[sourcePath]] | sftp://localhost/renamefile.txt                               | dev2              | Q/ulw&]  | [[destPath]] | sftp://localhost/renamed.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | Success | NO           |
