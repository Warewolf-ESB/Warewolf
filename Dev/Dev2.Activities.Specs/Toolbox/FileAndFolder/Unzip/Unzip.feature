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
		|                        |
		| <resultVar> = <result> |
	Examples: 
	| No | Name           | source   | sourceLocation                                           | username          | password | destination | destinationLocation                                 | destUsername      | destPassword | selected | archivepassword | resultVar  | result  | errorOccured |
	| 1  | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 2  | UNC to Local   | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test0.zip | ""                | ""       | [[path1]]   | c:\ZIP1                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 3  | FTP to Local   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test0.zip          | ""                | ""       | [[path1]]   | c:\ZIP2                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 4  | FTPS to Local  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test0.zip          | integrationtester | I73573r0 | [[path1]]   | c:\ZIP3                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 5  | SFTP to Local  | [[path]] | sftp://localhost/test0.zip                               | dev2              | Q/ulw&]  | [[path1]]   | c:\ZIP4                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 6  | Local to UNC   | [[path]] | c:\test1.zip                                             | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP0 | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 7  | UNC to UNC     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test1.zip | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP1 | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 8  | FTP to UNC     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test1.zip          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP2 | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 9  | FTPS to UNC    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test1.zip          | integrationtester | I73573r0 | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP3 | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 10 | SFTP to UNC    | [[path]] | sftp://localhost/test1.zip                               | dev2              | Q/ulw&]  | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP4 | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 11 | Local to FTP   | [[path]] | c:\test2.zip                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP0          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 12 | UNC to FTP     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test2.zip | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP1          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 13 | FTP to FTP     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test2.zip          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP2          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 14 | FTPS to FTP    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test2.zip          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP3          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 15 | SFTP to FTP    | [[path]] | sftp://localhost/test2.zip                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/ZIP4          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
	| 16 | Local to FTPS  | [[path]] | c:\test3.zip                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP0          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| 17 | UNC to FTPS    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test3.zip | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP1          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| 18 | FTP tp FTPS    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test3.zip          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP2          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| 19 | FTPS to FTPS   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test3.zip          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP3          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| 20 | SFTP to FTPS   | [[path]] | sftp://localhost/test3.zip                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP4          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
	| 21 | Local to SFTP  | [[path]] | c:\test4.zip                                             | ""                | ""       | [[path1]]   | sftp://localhost/ZIP0                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| 22 | UNC to SFTP    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test4.zip | ""                | ""       | [[path1]]   | sftp://localhost/ZIP1                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| 23 | FTP to SFTP    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test4.zip          | ""                | ""       | [[path1]]   | sftp://localhost/ZIP2                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| 24 | FTPS to SFTP   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test4.zip          | integrationtester | I73573r0 | [[path1]]   | sftp://localhost/ZIP3                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
	| 25 | SFTP to SFTP   | [[path]] | sftp://localhost/test5.zip                               | dev2              | Q/ulw&]  | [[path1]]   | sftp://localhost/ZIP6                                | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
#Bug 12180	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[result]][[a]]        | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[a]]*]]               | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[var@]]               | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[var]]00]]            | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[(1var)]]             | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[var[[a]]]]           | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[var.a]]              | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[@var]]               | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[var 1]]              | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[rec(1).[[rec().1]]]] | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[rec(@).a]]           | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[rec"()".a]]          | Failure | AN           |
	#| 26 | Local to Local | [[path]] | c:\test0.zip                                             | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[rec([[[[b]]]]).a]]   | Failure | AN           |
	






























#Bug 12091	
#Scenario Outline: Unzip file at Invalid file location
#	Given I have a source path '<source>' with value '<sourceLocation>'
#	And zip credentials as '<username>' and '<password>'
#	And I have a destination path '<destination>' with value '<destinationLocation>'
#	And destination credentials as '<destUsername>' and '<destPassword>'
#	And overwrite is '<selected>'
#	And result as '<resultVar>'	
#	And Archive Password as '<archivepassword>'
#    When the Unzip file tool is executed
#	Then the result variable '<resultVar>' will be '<result>'
#	And the execution has "<errorOccured>" error
#	And the debug inputs as
#         | Source Path                 | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  | Archive Password |
#         | <source> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> | String           |         
#	And the debug output as
#		|                        |
#		| <resultVar> = <result> |
#	Examples: 
#	| No | Name           | source   | sourceLocation                                           | username          | password | destination | destinationLocation                                 | destUsername      | destPassword | selected | archivepassword | resultVar  | result  | errorOccured |
#	| 1  | Local to Local | [[path]] |                                                          | ""                | ""       | [[path1]]   | c:\ZIP0                                             | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 2  | UNC to Local   | [[path]] |                                                          | ""                | ""       | [[path1]]   | c:\ZIP1                                             | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 3  | FTP to Local   | [[path]] |                                                          | ""                | ""       | [[path1]]   | c:\ZIP2                                             | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 4  | FTPS to Local  | [[path]] |                                                          | integrationtester | I73573r0 | [[path1]]   | c:\ZIP3                                             | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 5  | SFTP to Local  | [[path]] |                                                          | dev2              | Q/ulw&]  | [[path1]]   | c:\ZIP4                                             | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 6  | Local to UNC   | [[path]] |                                                          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP0 | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 7  | UNC to UNC     | [[path]] |                                                          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP1 | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 8  | FTP to UNC     | [[path]] |                                                          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP2 | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 9  | FTPS to UNC    | [[path]] |                                                          | integrationtester | I73573r0 | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ZIP3 | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 10 | SFTP to UNC    | [[path]] | sftp://localhost/test1.zip                               | dev2              | Q/ulw&]  | [[path1]]   |                                                     | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 11 | Local to FTP   | [[path]] | c:\test2.zip                                             | ""                | ""       | [[path1]]   |                                                     | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 12 | UNC to FTP     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test2.zip | ""                | ""       | [[path1]]   |                                                     | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 13 | FTP to FTP     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test2.zip          | ""                | ""       | [[path1]]   |                                                     | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 14 | FTPS to FTP    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test2.zip          | integrationtester | I73573r0 | [[path1]]   |                                                     | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 15 | SFTP to FTP    | [[path]] | sftp://localhost/test2.zip                               | dev2              | Q/ulw&]  | [[path1]]   |                                                     | ""                | ""           | True     | ""              | [[result]] | Failure | AN           |
#	| 16 | Local to FTPS  | [[path]] | c:\test3.zip                                             | ""                | ""       | [[path1]]   |                                                     | integrationtester | I73573r0     | True     | ""              | [[result]] | Failure | AN           |
#	| 17 | UNC to FTPS    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test3.zip | ""                | ""       | [[path1]]   |                                                     | integrationtester | I73573r0     | True     | ""              | [[result]] | Failure | AN           |
#	| 18 | FTP tp FTPS    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test3.zip          | ""                | ""       | [[path1]]   |                                                     | integrationtester | I73573r0     | True     | ""              | [[result]] | Failure | AN           |
#	| 19 | FTPS to FTPS   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test3.zip          | Invalid           | I73573r0 | [[path1]]   |                                                     | integrationtester | I73573r0     | True     | ""              | [[result]] | Failure | AN           |
#	| 20 | SFTP to FTPS   | [[path]] | sftp://localhost/test3.zip                               | Invalid           | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/ZIP4          | integrationtester | I73573r0     | True     | ""              | [[result]] | Failure | AN           |
#	| 21 | Local to SFTP  | [[path]] | c:\test4.zip                                             | Invalid           | ""       | [[path1]]   | sftp://localhost/ZIP0                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Failure | AN           |
#	| 22 | UNC to SFTP    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test4.zip | Invalid           | ""       | [[path1]]   | sftp://localhost/ZIP1                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Failure | AN           |
#	| 23 | FTP to SFTP    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test4.zip          | Invalid           | ""       | [[path1]]   | sftp://localhost/ZIP2                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Failure | AN           |
#	| 24 | FTPS to SFTP   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test4.zip          | Invalid           | I73573r0 | [[path1]]   | sftp://localhost/ZIP3                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Failure | AN           |
#	| 25 | SFTP to SFTP   | [[path]] | sftp://localhost/test5.zip                               | Invalid           | Q/ulw&]  | [[path1]]   | sftp://localhost/ZIP5                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Failure | AN           |