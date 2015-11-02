﻿@fileFeature
Feature: Read Folder
	In order to be able to Read Folder File or Folder 
	as a Warewolf user
	I want a tool that reads the contents of a Folder at a given location


Scenario Outline: Read Folder file at location	
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And use private public key for source is '<sourcePrivateKeyFile>'
	And Read is '<read>'   
	And result as '<resultVar>'
    When the read folder file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                   | Read   | Username   | Password |Private Key File       |
         | <source> = <sourceLocation> | <read> | <username> | String   | <sourcePrivateKeyFile> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
    Examples: 
	| No | Name                      | source   | sourceLocation                                          | read            | username          | password | resultVar  | result | errorOccured | sourcePrivateKeyFile |
	| 1  | Local Files               | [[path]] | c:\                                                     | Files           | ""                | ""       | [[result]] | String | NO           |                      |
	| 2  | UNC                       | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite          | Files           | ""                | ""       | [[result]] | String | NO           |                      |
	| 3  | FTP                       | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/                  | Files           | ""                | ""       | [[result]] | String | NO           |                      |
	| 4  | FTPS                      | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/                  | Files           | integrationtester | I73573r0 | [[result]] | String | NO           |                      |
	| 5  | SFTP                      | [[path]] | sftp://localhost                                        | Files           | dev2              | Q/ulw&]  | [[result]] | String | NO           |                      |
	| 6  | Local                     | [[path]] | c:\                                                     | Folders         | ""                | ""       | [[result]] | String | NO           |                      |
	| 7  | UNC                       | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite          | Folders         | ""                | ""       | [[result]] | String | NO           |                      |
	| 8  | FTP                       | [[path]] | ftp://rsaklfsvrsbspdc:1001/                             | Folders         | ""                | ""       | [[result]] | String | NO           |                      |
	| 9  | FTPS                      | [[path]] | ftp://rsaklfsvrsbspdc:1002/                             | Folders         | integrationtester | I73573r0 | [[result]] | String | NO           |                      |
	| 10 | SFTP                      | [[path]] | sftp://localhost                                        | Folders         | dev2              | Q/ulw&]  | [[result]] | String | NO           |                      |
	| 11 | Local                     | [[path]] | c:\                                                     | Files & Folders | ""                | ""       | [[result]] | String | NO           |                      |
	| 12 | UNC                       | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite          | Files & Folders | ""                | ""       | [[result]] | String | NO           |                      |
	| 13 | FTP                       | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/                  | Files & Folders | ""                | ""       | [[result]] | String | NO           |                      |
	| 14 | FTPS                      | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/                  | Files & Folders | integrationtester | I73573r0 | [[result]] | String | NO           |                      |
	| 15 | SFTP                      | [[path]] | sftp://localhost                                        | Files & Folders | dev2              | Q/ulw&]  | [[result]] | String | NO           |                      |
	| 16 | Empty_Local_Files         | [[path]] | c:\emptydir                                             | Files           | ""                | ""       | [[result]] | String | NO           |                      |
	| 17 | Empty_UNC_Files           | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\emptydir | Files           | ""                | ""       | [[result]] | String | NO           |                      |
	| 18 | Empty_FTP_Files           | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/emptydir          | Files           | ""                | ""       | [[result]] | String | NO           |                      |
	| 19 | Empty_FTPS_Files          | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/emptydir          | Files           | integrationtester | I73573r0 | [[result]] | String | NO           |                      |
	| 20 | Empty_SFTP_Files          | [[path]] | sftp://localhost/emptydir                               | Files           | dev2              | Q/ulw&]  | [[result]] | String | NO           |                      |
	| 21 | Empty_Local_Folders       | [[path]] | c:\emptydir                                             | Folders         | ""                | ""       | [[result]] | String | NO           |                      |
	| 22 | Empty_UNC_Folders         | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\emptydir | Folders         | ""                | ""       | [[result]] | String | NO           |                      |
	| 23 | Empty_FTP_Folders         | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/emptydir          | Folders         | ""                | ""       | [[result]] | String | NO           |                      |
	| 24 | Empty_FTPS_Folders        | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/emptydir          | Folders         | integrationtester | I73573r0 | [[result]] | String | NO           |                      |
	| 25 | Empty_SFTP_Folders        | [[path]] | sftp://localhost/emptydir                               | Folders         | dev2              | Q/ulw&]  | [[result]] | String | NO           |                      |
	| 26 | Empty_Local_Files_Folders | [[path]] | c:\emptydir                                             | Files & Folders | ""                | ""       | [[result]] | String | NO           |                      |
	| 27 | Empty_UNC_Files_Folders   | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\emptydir | Files & Folders | ""                | ""       | [[result]] | String | NO           |                      |
	| 28 | Empty_FTP_Files_Folders   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/emptydir          | Files & Folders | ""                | ""       | [[result]] | String | NO           |                      |
	| 29 | Empty_FTPS_Files_Folders  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/emptydir          | Files & Folders | integrationtester | I73573r0 | [[result]] | String | NO           |                      |
	| 30 | Empty_SFTP_Files_Folders  | [[path]] | sftp://localhost/emptydir                               | Files & Folders | dev2              | Q/ulw&]  | [[result]] | String | NO           |                      |
	| 31 | SFTP PK                   | [[path]] | sftp://localhost                                        | Files & Folders | dev2              | Q/ulw&]  | [[result]] | String | NO           | C:\\Temp\\key.opk    |
	| 1  | Local Files               | [[path]] | NULL                                                    | Files           | ""                | ""       | [[result]] | Error  | AN           |                      |
	
	
#Scenario Outline: Read Folder file at location1	
#    Given I have a variable "[[a]]" with a value '<Val1>'
#	Given I have a variable "[[b]]" with a value '<Val2>'
#	Given I have a variable "[[rec(1).a]]" with a value '<Val1>'
#	Given I have a variable "[[rec(2).a]]" with a value '<Val2>'
#	Given I have a variable "[[index]]" with a value "1"
#	Given I have a source path '<File or Folder>' with value '<sourceLocation>' 
#	And source credentials as '<username>' and '<password>'
#	And Read is '<read>'  
#	And result as '<resultVar>'
#	When validating the tool
#	Then validation is '<ValidationResult>'
#	And validation message is '<DesignValidation>'
#    When the read folder file tool is executed
#	Then the result variable '<resultVar>' will be '<result>'
#	And the execution has "<errorOccured>" error
#	And execution error message will be '<DesignValidation>'
#	And the debug inputs as
#         | Input Path                          | Username   | Password |
#         | <File or Folder> = <sourceLocation> | <username> | String   |
#	And the debug output as
#		|                        |
#		| <resultVar> = <result> |
#	Examples: 
#		| No | Name        | File or Folder                               | Val1                   | Val2                       | sourceLocation                                 | username              | password | resultVar              | result | errorOccured | ValidationResult | DesignValidation                                                                                                              | OutputError                                                                                                                      |
#		| 1  | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 2  | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 3  | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 4  | Local Files | [[a]]                                        | c:\                    | ""                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 5  | Local Files | [[a]][[b]]                                   | c                      | :\                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 6  | Local Files | [[rec([[index]]).a]]                         | c:\                    |                            | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 7  | Local Files | [[a]][[b]]                                   | ftp://rsaklfsvrsbspdc: | :1001/FORTESTING/          | ftp://rsaklfsvrsbspdc:1001/FORTESTING/         | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 8  | Local Files | [[a]]\[[b]]                                  | c:                     | ""                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 9  | Local Files | [[a]]:[[b]]                                  | c                      | \                          | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 10 | Local Files | \\\\RSAKLFSVRSBSPDC[[a]][[b]]                | \FileSystem            | ShareTestingSite           | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 11 | Local Files | [[rec(1).a]][[rec(2).a]]                     | c                      | :\                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 12 | Local Files | [[rec(1).a]]\[[rec(2).a]]                    | \\\\RSAKLFSVRSBSPDC    | FileSystemShareTestingSite | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 13 | Local Files | [[rec(1).a]][[rec(2).a]]                     | c                      | :\                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 14 | Local Files | [[rec(1).a]]:[[rec(2).a]]                    | c                      | \                          | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 15 | Local Files | \\\\RSAKLFSVRSBSPDC\[[rec(1).a]][[rec(2).a]] | FileSystem             | ShareTestingSite           | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 16 | Local Files | c:\copyfile0.txt                             | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 17 | Local Files | [[rec([[index]]).a]]                         | c:\                    | ""                         | c:\                                            | ""                    | ""       | [[result]]             | String | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
#		| 18 | Local Files | [[a&]]                                       | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | True             | Directory - Variable name [[a&]] contains invalid character(s)                                                                | 1.Directory - Variable name [[a&]] contains invalid character(s)                                                                 |
#		| 19 | Local Files | [[rec(**).a]]                                | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | True             | Directory - Recordset index (**) contains invalid character(s)                                                                | 1.Directory - Recordset index (**) contains invalid character(s)                                                                 |
#		| 20 | Local Files | c(*()                                        | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | True             | Please supply a valid File or Folder                                                                                          | 1.Please supply a valid File or Folder                                                                                           |
#		| 21 | Local Files | C:\\\\\gvh                                   | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | False            | ""                                                                                                                            | 1.Directory not found [ C:\\\\\gvh ]                                                                                             |
#		| 22 | Local Files | [[rec([[inde$x]]).a]]                        | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | True             | Directory - Variable name [[index$x]] contains invalid character(s)                                                           | 1.Directory - Variable name [[index$x]] contains invalid character(s)                                                            |
#		| 23 | Local Files | [[sourcePath]]                               | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | False            | ""                                                                                                                            | 1.No Value assigned for [[a]]                                                                                                    |
#		| 24 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | [[$#]]                | String   | [[result]]             | ""     | AN           | True             | Username - Variable name [[$#]] contains invalid character(s)                                                                 | 1.Username - Variable name [[$#]] contains invalid character(s)                                                                  |
#		| 25 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | [[a]]\[[b]]           | String   | [[result]]             | ""     | AN           | False            | ""                                                                                                                            | 1.No Value assigned for [[a]] 2.1.No Value assigned for [[b]]                                                                    |
#		| 26 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | [[rec([[index]]).a]]  | String   | [[result]]             | ""     | AN           | False            | ""                                                                                                                            | 1.No Value assigned for [[index]]                                                                                                |
#		| 27 | Local Files | [[sourcePath]].txt                           | ""                     | ""                         | c:\                                            | [[rec([[index&]]).a]] | String   | [[result]]             | ""     | AN           | True             | Username - Recordset name [[indexx&]] contains invalid character(s)                                                           | Username - Recordset name [[indexx&]] contains invalid character(s)                                                              |
#		| 28 | Local Files | [[sourcePath]].txt                           | ""                     | ""                         | c:\                                            | [[a]]*]]              | String   | [[result]]             | ""     | AN           | True             | Username - Invalid expression: opening and closing brackets don't match                                                       | 1.Username - Invalid expression: opening and closing brackets don't match                                                        |
#		| 29 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[result]][[a]]        | ""     | AN           | True             | The result field only allows a single result                                                                                  | 1.The result field only allows a single result                                                                                   |
#		| 30 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[a]]*]]               | ""     | AN           | True             | Result - Invalid expression: opening and closing brackets don't match                                                         | 1.Result - Invalid expression: opening and closing brackets don't match                                                          |
#		| 31 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[var@]]               | ""     | AN           | True             | Result - Variable name [[var@]] contains invalid character(s)                                                                 | 1.Result - Variable name [[var@]] contains invalid character(s)                                                                  |
#		| 32 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[var]]00]]            | ""     | AN           | True             | Result - Invalid expression: opening and closing brackets don't match                                                         | 1.Result - Invalid expression: opening and closing brackets don't match                                                          |
#		| 33 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[(1var)]]             | ""     | AN           | True             | Result - Variable name [[var@]] contains invalid character(s)                                                                 | 1.Result - Variable name [[var@]] contains invalid character(s)                                                                  |
#		| 34 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[var[[a]]]]           | ""     | AN           | True             | Result - Invalid Region [[var[[a]]]]                                                                                          | 1.Result - Invalid Region [[var[[a]]]]                                                                                           |
#		| 35 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[var.a]]              | ""     | AN           | True             | Result - Variable name [[var.a]]contains invalid character(s)                                                                 | 1.Result - Variable name [[var.a]] contains invalid character(s)                                                                 |
#		| 36 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[@var]]               | ""     | AN           | True             | Result - Variable name [[@var]] contains invalid character(s)                                                                 | 1.Result - Variable name [[@var]] contains invalid character(s)                                                                  |
#		| 37 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[var 1]]              | ""     | AN           | True             | Result - Variable name [[var 1]] contains invalid character(s)                                                                | 1.Result - Variable name [[var 1]] contains invalid character(s)                                                                 |
#		| 38 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[rec(1).[[rec().1]]]] | ""     | AN           | True             | Result - Invalid Region [[var[[a]]]]                                                                                          | 1.Result - Invalid Region [[var[[a]]]]                                                                                           |
#		| 39 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[rec(@).a]]           | ""     | AN           | True             | Result - Recordset index [[@]] contains invalid character(s)                                                                  | 1.Result - Recordset index [[@]] contains invalid character(s)                                                                   |
#		| 40 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[rec"()".a]]          | ""     | AN           | True             | Result - Recordset name [[rec"()"]] contains invalid character(s)                                                             | 1.Result - Recordset name [[rec"()"]] contains invalid character(s)                                                              |
#		| 41 | Local Files | [[sourcePath]]                               | ""                     | ""                         | c:\                                            | ""                    | ""       | [[rec([[[[b]]]]).a]]   | ""     | AN           | True             | Result - Invalid Region [[rec([[[[b]]]]).a]]                                                                                  | 1.Result - Invalid Region [[rec([[[[b]]]]).a]]                                                                                   |
#		| 42 | Local Files | [[a]                                         | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | True             | Directory - Invalid expression: opening and closing brackets don't match                                                      | 1.Directory - Invalid expression: opening and closing brackets don't match                                                       |
#		| 43 | Local Files | [[rec]                                       | ""                     | ""                         |                                                | ""                    | ""       | [[result]]             | ""     | AN           | True             | Directory - [[rec]] does not exist in your variable list                                                                      | 1.Directory - [[rec]] does not exist in your variable list                                                                       |
#		| 44 | Local Files | [[sourcePath]]                               | ""                     | """                        | c:\                                            | Test                  | ""       | [[result]]             | ""     | AN           | True             | Password cannot be empty or only white space                                                                                  | 1.Password cannot be empty or only white space                                                                                   |
#		| 45 | Local Files | [[var@]]                                     | ""                     | ""                         |                                                | [[var@]]              | String   | [[var@]]               | ""     | AN           | True             | Username - Variable name [[$#]] contains invalid character(s)   Result - Variable name [[var@]] contains invalid character(s) | 1.Username - Variable name [[$#]] contains invalid character(s)  2.Result - Variable name [[var@]] contains invalid character(s) |
#										 									