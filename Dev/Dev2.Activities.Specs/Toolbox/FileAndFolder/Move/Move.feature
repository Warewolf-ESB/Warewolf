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
		|                        |
		| <resultVar> = <result> |
	Examples: 
	| Name           | source         | sourceLocation                                               | username          | password | destination  | destinationLocation                                       | destUsername      | destPassword | selected | resultVar       | result  | errorOccured |
	| Local to Local | [[sourcePath]] | c:\movefile0.txt                                             | ""                | ""       | [[destPath]] | C:\moved0.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| Local to FTP   | [[sourcePath]] | c:\movefile1.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved0.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| Local to FTPS  | [[sourcePath]] | c:\movefile2.txt                                             | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved0.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| Local to SFTP  | [[sourcePath]] | c:\movefile3.txt                                             | ""                | ""       | [[destPath]] | sftp://localhost/moved0.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| Local to UNC   | [[sourcePath]] | c:\movefile4.txt                                             | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved0.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| UNC to Local   | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile0.txt | ""                | ""       | [[destPath]] | C:\moved1.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| UNC to FTP     | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile1.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved1.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| UNC to FTPS    | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile2.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved1.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| UNC to SFTP    | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile3.txt | ""                | ""       | [[destPath]] | sftp://localhost/moved1.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| UNC TO UNC     | [[sourcePath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\movefile4.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved1.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTP to Local   | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile0.txt          | ""                | ""       | [[destPath]] | C:\moved2.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTP to UNC     | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile1.txt          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved2.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTP to FTPS    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile2.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved2.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| FTP to SFTP    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile3.txt          | ""                | ""       | [[destPath]] | sftp://localhost/moved2.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| FTP to FTP     | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/movefile4.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved2.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTPS to Local  | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile0.txt          | integrationtester | I73573r0 | [[destPath]] | C:\moved3.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTPS to UNC    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile1.txt          | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved3.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| FTPS to FTPS   | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile2.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved3.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| FTPS to SFTP   | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile3.txt          | integrationtester | I73573r0 | [[destPath]] | sftp://localhost/moved3.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	| FTPS to FTP    | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/movefile4.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved3.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to Local  | [[sourcePath]] | sftp://localhost/movefile0.txt                               | dev2              | Q/ulw&]  | [[destPath]] | C:\moved4.txt                                             | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to UNC    | [[sourcePath]] | sftp://localhost/movefile1.txt                               | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\moved4.txt | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to FTP    | [[sourcePath]] | sftp://localhost/movefile2.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/moved4.txt          | ""                | ""           | True     | [[result]]      | Success | NO           |
	| SFTP to FTPS   | [[sourcePath]] | sftp://localhost/movefile3.txt                               | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/moved4.txt          | integrationtester | I73573r0     | True     | [[result]]      | Success | NO           |
	| SFTP to SFTP   | [[sourcePath]] | sftp://localhost/movefile4.txt                               | dev2              | Q/ulw&]  | [[destPath]] | sftp://localhost/moved4.txt                               | dev2              | Q/ulw&]      | True     | [[result]]      | Success | NO           |
	#| Local to Local | [[sourcePath]] | c:\movefile0.txt                                             | ""                | ""       | [[destPath]] | C:\moved0.txt                                             | ""                | ""           | True     | [[result]][[a]] | Success | AN           |





