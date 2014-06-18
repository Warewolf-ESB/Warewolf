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
		|                        |
		| <resultVar> = <result> |
	Examples: 
	| Name           | source         | sourceLocation                                                 | username          | password | destination  | destinationLocation                                         | destUsername      | destPassword | selected | resultVar       | result  | errorOccured |
	| Local to Local | [[sourcePath]] | c:\renamefile0.txt                                             | ""                | ""       | [[destPath]] | C:\renamed0.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| Local to FTP   | [[sourcePath]] | c:\renamefile1.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed0.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| Local to FTPS  | [[sourcePath]] | c:\renamefile2.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed0.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| Local to SFTP  | [[sourcePath]] | c:\renamefile3.txt                                             | ""                | ""       | [[destPath]] | sftp://localhost/renamed0.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| Local to UNC   | [[sourcePath]] | c:\renamefile4.txt                                             | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed0.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| UNC to Local   | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile0.txt | ""                | ""       | [[destPath]] | C:\renamed1.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| UNC to FTP     | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile1.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed1.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| UNC to FTPS    | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile2.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed1.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| UNC to SFTP    | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile3.txt | ""                | ""       | [[destPath]] | sftp://localhost/renamed1.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| UNC to UNC     | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamefile4.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed1.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTP to Local   | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile0.txt          | ""                | ""       | [[destPath]] | C:\renamed2.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTP to UNC     | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile1.txt          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed2.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTP to FTPS    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile2.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed2.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| FTP to SFTP    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile3.txt          | ""                | ""       | [[destPath]] | sftp://localhost/renamed2.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| FTP to FTP     | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamefile4.txt          | ""                |          | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed2.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTPS to Local  | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile0.txt          | integrationtester | I73573r0 | [[destPath]] | C:\renamed3.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTPS to UNC    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile1.txt          | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed3.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTPS to FTPS   | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile2.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed3.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| FTPS to SFTP   | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile3.txt          | integrationtester | I73573r0 | [[destPath]] | sftp://localhost/renamed3.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| FTPS to FTP    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamefile4.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed3.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to Local  | [[sourcePath]] | sftp://localhost/renamefile0.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\renamed4.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to UNC    | [[sourcePath]] | sftp://localhost/renamefile1.txt                               | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\renamed4.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to FTP    | [[sourcePath]] | sftp://localhost/renamefile2.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/renamed4.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to FTPS   | [[sourcePath]] | sftp://localhost/renamefile3.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/renamed4.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| SFTP to SFTP   | [[sourcePath]] | sftp://localhost/renamefile4.txt                               | dev2              | Q/ulw&]  | [[destPath]] | sftp://localhost/renamed4.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	#| Local to Local | [[sourcePath]] | c:\renamefile0.txt                                             | ""                | ""       | [[destPath]] | C:\renamed0.txt                                             | ""                | ""           | True     | [[result]][[a]] | Success | AN           |