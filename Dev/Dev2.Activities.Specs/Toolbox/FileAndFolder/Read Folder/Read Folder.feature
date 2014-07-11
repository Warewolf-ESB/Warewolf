@fileFeature
Feature: Read Folder
	In order to be able to Read Folder File or Folder 
	as a Warewolf user
	I want a tool that reads the contents of a Folder at a given location


Scenario Outline: Read Folder file at location	
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And Read is '<read>'   
	And result as '<resultVar>'
    When the read folder file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                   | Read   | Username   | Password |
         | <source> = <sourceLocation> | <read> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
    Examples: 
	| No | Name        | source   | sourceLocation                                 | read            | username          | password | resultVar  | result | errorOccured |
	| 1  | Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[result]] | String | NO           |
	| 2  | UNC         | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | Files           | ""                | ""       | [[result]] | String | NO           |
	| 3  | FTP         | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/         | Files           | ""                | ""       | [[result]] | String | NO           |
	| 4  | FTPS        | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/         | Files           | integrationtester | I73573r0 | [[result]] | String | NO           |
	| 5  | SFTP        | [[path]] | sftp://localhost                               | Files           | dev2              | Q/ulw&]  | [[result]] | String | NO           |
	| 6  | Local       | [[path]] | c:\                                            | Folders         | ""                | ""       | [[result]] | String | NO           |
	| 7  | UNC         | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | Folders         | ""                | ""       | [[result]] | String | NO           |
	| 8  | FTP         | [[path]] | ftp://rsaklfsvrsbspdc:1001/                    | Folders         | ""                | ""       | [[result]] | String | NO           |
	| 9  | FTPS        | [[path]] | ftp://rsaklfsvrsbspdc:1002/                    | Folders         | integrationtester | I73573r0 | [[result]] | String | NO           |
	| 10 | SFTP        | [[path]] | sftp://localhost                               | Folders         | dev2              | Q/ulw&]  | [[result]] | String | NO           |
	| 11 | Local       | [[path]] | c:\                                            | Files & Folders | ""                | ""       | [[result]] | String | NO           |
	| 12 | UNC         | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite | Files & Folders | ""                | ""       | [[result]] | String | NO           |
	| 13 | FTP         | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/         | Files & Folders | ""                | ""       | [[result]] | String | NO           |
	| 14 | FTPS        | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/         | Files & Folders | integrationtester | I73573r0 | [[result]] | String | NO           |
	| 15 | SFTP        | [[path]] | sftp://localhost                               | Files & Folders | dev2              | Q/ulw&]  | [[result]] | String | NO           |
#Bug 12180	#|16| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[result]][[a]]        | String | AN           |
	        #|17| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[a]]*]]               | String | AN           |
	        #|18| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[var@]]               | String | AN           |
	        #|19| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[var]]00]]            | String | AN           |
	        #|20| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[(1var)]]             | String | AN           |
	        #|21| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[var[[a]]]]           | String | AN           |
	        #|22| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[var.a]]              | String | AN           |
	        #|23| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[@var]]               | String | AN           |
	        #|24| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[var 1]]              | String | AN           |
	        #|25| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[rec(1).[[rec().1]]]] | String | AN           |
	        #|26| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[rec(@).a]]           | String | AN           |
	        #|27| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[rec"()".a]]          | String | AN           |
	        #|28| Local Files | [[path]] | c:\                                            | Files           | ""                | ""       | [[rec([[[[b]]]]).a]]   | String | AN           |
	        																														