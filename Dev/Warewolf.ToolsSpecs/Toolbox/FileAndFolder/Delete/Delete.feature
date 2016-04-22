﻿@fileFeature
Feature: Delete
	In order to be able to Delete file
	as a Warewolf user
	I want a tool that Delete a file at a given location


Scenario Outline: Delete file at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	And use private public key for source is '<sourcePrivateKeyFile>'
	When the delete file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                  | Username   | Password | Private Key File |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	| Name       | source   | sourceLocation                                                         | username                     | password | resultVar  | result  | errorOccured | sourcePrivateKeyFile |
	| Local      | [[path]] | c:\filetodelete.txt                                                    | ""                           | ""       | [[result]] | Success | NO           |                      |
	| UNC        | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetodelete.txt        | ""                           | ""       | [[result]] | Success | NO           |                      |
	| UNC Secure | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetodelete.txt | dev2.local\IntegrationTester | I73573r0 | [[result]] | Success | NO           |                      |
	| FTP        | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetodelete.txt                 | ""                           | ""       | [[result]] | Success | NO           |                      |
	| FTPS       | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetodele.txt                   | IntegrationTester            | I73573r0 | [[result]] | Success | NO           |                      |
	| SFTP       | [[path]] | sftp://localhost/filetodelete.txt                                      | dev2                         | Q/ulw&]  | [[result]] | Success | NO           |                      |
	| SFTP PK    | [[path]] | sftp://localhost/filetodelete1.txt                                     | dev2                         | Q/ulw&]  | [[result]] | Success | NO           | C:\\Temp\\key.opk    |

Scenario Outline: Delete file at location Null
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	And use private public key for source is '<sourcePrivateKeyFile>'
	When the delete file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	Then the execution has "<errorOccured>" error
	Examples: 
	| Name  | source   | sourceLocation | username | password | resultVar  | result | errorOccured | sourcePrivateKeyFile |
	| Local | [[path]] | NULL           | ""       | ""       | [[result]] |        | AN           |                      |
#	| Local      | [[path]] | G:\filetodelete                                                        | ""                           | ""       | [[result]] | Failure | AN           |                       |
#	| UNC        | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Memo.txt                | ""                           | ""       | [[result]] | Failure | AN           |                       |
#	| UNC Secure | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetodelete.txt | dev2.local\IntegrationTester | password | [[result]] | Failure | AN           |                       |
#	| FTP        | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetodelete.xtx                 | ""                           | ""       | [[result]] | Failure | AN           |                       |
#	| FTPS       | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetodele.txt/                  | IntegrationTester            | I73573r0 | [[result]] | Failure | AN           |                       |
#	| SFTP       | [[path]] | sftp://localhost/Memo.txt                                              | dev2.local                   | Q/ulw&]  | [[result]] | Failure | AN           |                       |
#	| SFTP PK    | [[path]] | sftp://localhost                                                       | dev2                         | Q/ulw&]  | [[result]] | Failure | AN           | C:\\Temp\Temp\key.opk |


Scenario Outline: Delete file Validation
    Given I have a variable "[[a]]" with a value '<Val1>'
	Given I have a variable "[[b]]" with a value '<Val2>'
	Given I have a variable "[[rec(1).a]]" with a value '<Val1>'
	Given I have a variable "[[rec(2).a]]" with a value '<Val2>'
	Given I have a variable "[[index]]" with a value "1"
	Given I have a source path '<File or Folder>' with value '<sourceLocation>' 
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	When validating the tool
	Then validation is '<ValidationResult>'
	And validation message is '<DesignValidation>'
    When the delete file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And execution error message will be '<DesignValidation>'
	And the debug inputs as
         | Input Path                          | Username   | Password |
         | <File or Folder> = <sourceLocation> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| No | File or Folder                  | Val1                                           | Val2               | sourceLocation                                                          | username              | resultVar              | result  | errorOccured | ValidationResult | DesignValidation                                                                                                              | OutputError                                                                                                                      |
		| 1  | [[sourcePath]]                  |                                                |                    | c:\filetodelete.txt                                                     | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 2  | [[sourcePath]]                  |                                                |                    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetodelete1.txt        | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 3  | [[sourcePath]]                  |                                                |                    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetodelete2.txt | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 4  | [[sourcePath]]                  |                                                |                    | c:\filetodelete2.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 5  | [[a][[b]].txt                   | c:\file                                        | todelete3          | c:\filetodelete3.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 6  | [[rec([[index]]).a]]            | c:\filetodelete13.txt                          |                    | c:\filetodelete13.txt                                                   | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 7  | [[a]][[b]]                      | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | \filetodelete2.txt | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetodelete2.txt        | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 8  | [[a]]\[[b]]                     | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | \filetodelete3.txt | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetodelete3.txt        | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 9  | [[a]]:[[b]]                     | c                                              | filetodelete4.txt  | c:\filetodelete4.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 10 | C:[[a]][[b]].txt                | file                                           | todelete5          | c:\filetodelete5.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 11 | [[rec(1).a]][[rec(2).a]]        | c:\fileto                                      | delete6.txt        | c:\filetodelete6.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 12 | [[rec(1).a]]\[[rec(2).a]]       | c:                                             | filetodelete7.txt  | c:\filetodelete7.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 13 | [[rec(1).a]][[rec(2).a]] .txt   | c:\fileto                                      | delete8            | c:\filetodelete8.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 14 | [[rec(1).a]]:[[rec(2).a]]       | c                                              | \filetodelete9.txt | c:\filetodelete9.txt                                                    | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 15 | C:[[rec(1).a]][[rec(2).a]] .txt | \fileto                                        | delete10           | c:\filetodelete10.txt                                                   | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 16 | c:\copyfile0.txt                |                                                |                    |                                                                         | ""                    | [[result]]             | ""      | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 17 | [[rec([[index]]).a]]            | c:\filetodelete16.txt                          |                    | ""                                                                      | ""                    | [[result]]             | Success | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 18 | [[a&]]                          |                                                |                    |                                                                         | ""                    | [[result]]             | ""      | AN           | True             | File or Folder - Variable name [[a&]] contains invalid character(s)                                                           | 1.File or Folder - Variable name [[a&]] contains invalid character(s)                                                            |
		| 19 | [[rec(**).a]]                   |                                                |                    |                                                                         | ""                    | [[result]]             | ""      | AN           | True             | File or Folder - Recordset index (**) contains invalid character(s)                                                           | 1.File or Folder - Recordset index (**) contains invalid character(s)                                                            |
		| 20 | c(*()                           |                                                |                    |                                                                         | ""                    | [[result]]             | ""      | AN           | True             | Please supply a valid File or Folder                                                                                          | 1.Please supply a valid File or Folder                                                                                           |
		| 21 | C:\\\\\gvh                      |                                                |                    |                                                                         | ""                    | [[result]]             | ""      | NO           | False            | ""                                                                                                                            | ""                                                                                                                               |
		| 22 | [[rec([[inde$x]]).a]]           |                                                |                    |                                                                         | ""                    | [[result]]             | ""      | AN           | True             | File or Folder - Variable name [[index$x]] contains invalid character(s)                                                      | 1.File or Folder - Variable name [[index$x]] contains invalid character(s)                                                       |
		| 23 | [[sourcePath]]                  |                                                |                    |                                                                         | ""                    | [[result]]             | ""      | AN           | False            | ""                                                                                                                            | 1.No Value assigned for [[a]]                                                                                                    |
		| 24 | [[sourcePath]]                  |                                                |                    | c:\filetodelete1.txt                                                    | [[$#]]                | [[result]]             | ""      | AN           | True             | Username - Variable name [[$#]] contains invalid character(s)                                                                 | 1.Username - Variable name [[$#]] contains invalid character(s)                                                                  |
		| 25 | [[sourcePath]]                  |                                                |                    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetodelete1.txt        | [[a]]\[[b]]           | [[result]]             | ""      | AN           | False            | ""                                                                                                                            | 1.No Value assigned for [[a]] 2.1.No Value assigned for [[b]]                                                                    |
		| 26 | [[sourcePath]]                  |                                                |                    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetodelete2.txt | [[rec([[index]]).a]]  | [[result]]             | ""      | AN           | False            | ""                                                                                                                            | 1.No Value assigned for [[index]]                                                                                                |
		| 27 | [[sourcePath]].txt              |                                                |                    | c:\filetodelete2.txt                                                    | [[rec([[index&]]).a]] | [[result]]             | ""      | AN           | True             | Username - Recordset name [[indexx&]] contains invalid character(s)                                                           | Username - Recordset name [[indexx&]] contains invalid character(s)                                                              |
		| 28 | [[sourcePath]].txt              |                                                |                    | c:\filetodelete2.txt                                                    | [[a]]*]]              | [[result]]             | ""      | AN           | True             | Username - Invalid expression: opening and closing brackets don't match                                                       | 1.Username - Invalid expression: opening and closing brackets don't match                                                        |
		| 29 | [[sourcePath]]                  |                                                |                    | c:\filetodelete1.txt                                                    | ""                    | [[result]][[a]]        | ""      | AN           | True             | The result field only allows a single result                                                                                  | 1.The result field only allows a single result                                                                                   |
		| 30 | [[sourcePath]]                  |                                                |                    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetodelete1.txt        | ""                    | [[a]]*]]               | ""      | AN           | True             | Result - The Result field only allows single result                                                                           | 1.Result - The Result field only allows single result                                                                            |
		| 31 | [[sourcePath]]                  |                                                |                    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetodelete2.txt | ""                    | [[var@]]               | ""      | AN           | True             | Result - Variable name [[var@]] contains invalid character(s)                                                                 | 1.Result - Variable name [[var@]] contains invalid character(s)                                                                  |
		| 32 | [[sourcePath]]                  |                                                |                    | c:\filetodelete2.txt                                                    | ""                    | [[var]]00]]            | ""      | AN           | True             | Result - Invalid expression: opening and closing brackets don't match                                                         | 1.Result - Invalid expression: opening and closing brackets don't match                                                          |
		| 33 | [[sourcePath]]                  |                                                |                    | c:\filetodelete3.txt                                                    | ""                    | [[(1var)]]             | ""      | AN           | True             | Result - Variable name [[var@]] contains invalid character(s)                                                                 | 1.Result - Variable name [[var@]] contains invalid character(s)                                                                  |
		| 34 | [[sourcePath]]                  |                                                |                    | c:\filetodelete13.txt                                                   | ""                    | [[var[[a]]]]           | ""      | AN           | True             | Result - Invalid Region [[var[[a]]]]                                                                                          | 1.Result - Invalid Region [[var[[a]]]]                                                                                           |
		| 35 | [[sourcePath]]                  |                                                |                    | c:\filetodelete14.txt                                                   | ""                    | [[var.a]]              | ""      | AN           | True             | Result - Variable name [[var.a]]contains invalid character(s)                                                                 | 1.Result - Variable name [[var.a]] contains invalid character(s)                                                                 |
		| 36 | [[sourcePath]]                  |                                                |                    | c:\filetodelete15.txt                                                   | ""                    | [[@var]]               | ""      | AN           | True             | Result - Variable name [[@var]] contains invalid character(s)                                                                 | 1.Result - Variable name [[@var]] contains invalid character(s)                                                                  |
		| 37 | [[sourcePath]]                  |                                                |                    | c:\filetodelete16.txt                                                   | ""                    | [[var 1]]              | ""      | AN           | True             | Result - Variable name [[var 1]] contains invalid character(s)                                                                | 1.Result - Variable name [[var 1]] contains invalid character(s)                                                                 |
		| 38 | [[sourcePath]]                  |                                                |                    | c:\filetodelete17.txt                                                   | ""                    | [[rec(1).[[rec().1]]]] | ""      | AN           | True             | Result - Invalid Region [[var[[a]]]]                                                                                          | 1.Result - Invalid Region [[var[[a]]]]                                                                                           |
		| 39 | [[sourcePath]]                  |                                                |                    | c:\filetodelete18.txt                                                   | ""                    | [[rec(@).a]]           | ""      | AN           | True             | Result - Recordset index [[@]] contains invalid character(s)                                                                  | 1.Result - Recordset index [[@]] contains invalid character(s)                                                                   |
		| 40 | [[sourcePath]]                  |                                                |                    | c:\filetodelete19.txt                                                   | ""                    | [[rec"()".a]]          | ""      | AN           | True             | Result - Recordset name [[rec"()"]] contains invalid character(s)                                                             | 1.Result - Recordset name [[rec"()"]] contains invalid character(s)                                                              |
		| 41 | [[sourcePath]]                  |                                                |                    | c:\filetodelete20.txt                                                   | ""                    | [[rec([[[[b]]]]).a]]   | ""      | AN           | True             | Result - Invalid Region [[rec([[[[b]]]]).a]]                                                                                  | 1.Result - Invalid Region [[rec([[[[b]]]]).a]]                                                                                   |
		| 42 | [[var@]]                        |                                                |                    |                                                                         | [[var@]]              | [[var@]]               | ""      | AN           | True             | Username - Variable name [[$#]] contains invalid character(s)   Result - Variable name [[var@]] contains invalid character(s) | 1.Username - Variable name [[$#]] contains invalid character(s)  2.Result - Variable name [[var@]] contains invalid character(s) |                            

Scenario Outline: Delete file at location with incorrect directories
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	When the delete file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                  | Username   | Password |
         | <source> = <sourceLocation> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	| Name       | source       | sourceLocation      | username                     | password | resultVar  | result | errorOccured | 
	| Local      | 1234         | c:\filetodelete.txt | ""                           | ""       | [[result]] |        | AN           | 
	| UNC        | [[var]]      |                     | ""                           | ""       | [[result]] |        | AN           | 
	| UNC Secure | [[variable]] | ""                  | dev2.local\IntegrationTester | I73573r0 | [[result]] |        | AN           | 

@ignore
#Complex Types WOLF-1042
Scenario Outline: Delete file at location using complex types
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	When the delete file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                  | Username   | Password |
         | <source> = <sourceLocation> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	| Name  | source                              | sourceLocation      | username | password | resultVar  | result  | errorOccured |
	| Local | [[file().resources().path]]         | c:\filetodelete.txt | ""       | ""       | [[result]] | Success | NO           |
	| Local | [[file(*).resources(3).path]]       | c:\filetodelete.txt | ""       | ""       | [[result]] | Success | NO           |
	| Local | [[file(1).resources([[int]]).path]] | c:\delete.txt       | ""       | ""       | [[result]] | Success | NO           |


