@fileFeature
Feature: Unzip
	In order to be able to Unzip File or Folder 
	as a Warewolf user
	I want a tool that will Unzip File(s) or Folder(s) at a given location
	
Scenario Outline: Unzip file at location
	Given I have a source path '<source>' with value '<sourceLocation>'	
	And zip credentials as '<username>' and '<password>'
	And I have a destination path '<destination>' with value '<destinationLocation>'
	And destination credentials as '<destUsername>' and '<destPassword>'
	And overwrite is '<selected>'
	And result as '<resultVar>'	
	And Archive Password as '<archivepassword>'
    When the Unzip file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  | Archive Password |
         | <source> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> | String           |         
	And the debug output as
		| Result                 |
		| <resultVar> = <result> |
	Examples: 
	| Name           | source   | sourceLocation                                          | username          | password | destination | destinationLocation                                | destUsername      | destPassword | selected | archivepassword | resultVar  | result  | errorOccured |
	| Local to Local | [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| UNC to Local   | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | c:\ZIP                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| FTP to Local   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | c:\ZIP                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| FTPS to Local  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | c:\ZIP                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| SFTP to Local  | [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | c:\ZIP                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| Local to UNC   | [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| UNC to UNC     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| FTP to UNC     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| FTPS to UNC    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| SFTP to UNC    | [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| Local to FTP   | [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| UNC to FTP     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| FTP to FTP     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| FTPS to FTP    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| SFTP to FTP    | [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| Local to FTPS  | [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| UNC to FTPS    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| FTP tp FTPS    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| FTPS to FTPS   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| SFTP to FTPS   | [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| Local to SFTP  | [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | sftp://localhost/ZIP                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| UNC to SFTP    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | sftp://localhost/ZIP                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| FTP to SFTP    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | sftp://localhost/ZIP                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| FTPS to SFTP   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | sftp://localhost/ZIP                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| SFTP to SFTP   | [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | sftp://localhost/ZIP                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
