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
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| source         | sourceLocation                                               | username          | password | destination  | destinationLocation                                        | destUsername      | destPassword | selected | resultVar       | result  | errorOccured |
		| [[sourcePath]] | c:\copyfile0.txt                                             | ""                | ""       | [[destPath]] | C:\copied00.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | c:\copyfile1.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied0.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | c:\copyfile2.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied0.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | c:\copyfile3.txt                                             | ""                | ""       | [[destPath]] | sftp://localhost/copied0.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | c:\copyfile4.txt                                             | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied0.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | c:\copyfile5.txt                                             | ""                | ""       | [[destPath]] | C:\copied01.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile0.txt | ""                | ""       | [[destPath]] | C:\copied10.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile1.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied1.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile2.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied1.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile3.txt | ""                | ""       | [[destPath]] | sftp://localhost/copied1.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile4.txt | ""                | ""       | [[destPath]] | C:\copied11.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copyfile5.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied1.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile0.txt          | ""                | ""       | [[destPath]] | C:\copied20.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile1.txt          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied2.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile2.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied2.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile3.txt          | ""                | ""       | [[destPath]] | sftp://localhost/copied2.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile4.txt          | ""                | ""       | [[destPath]] | C:\copied21.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile5.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied2.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile0.txt          | integrationtester | I73573r0 | [[destPath]] | C:\copied30.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile1.txt          | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied3.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile2.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied3.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile3.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied3.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile4.txt          | integrationtester | I73573r0 | [[destPath]] | C:\copied31.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile5.txt          | integrationtester | I73573r0 | [[destPath]] | C:\copied32.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile0.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile1.txt                               | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\copied4.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile2.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied4.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile3.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied4.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile4.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\copied41.txt                                            | ""                | ""           | True     | [[result]]      | Success | NO           |
		| [[sourcePath]] | sftp://localhost/copyfile5.txt                               | dev2              | Q/ulw&]  | [[destPath]] | sftp://localhost/copied4.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
		#| [[sourcePath]] | c:\copyfile0.txt                                             | ""                | ""       | [[destPath]] | C:\copied00.txt                                            | ""                | ""           | True     | [[result]][[a]] | Success | AN           |
