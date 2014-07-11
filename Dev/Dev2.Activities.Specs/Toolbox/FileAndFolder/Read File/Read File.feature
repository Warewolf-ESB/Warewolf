@fileFeature
Feature: Read File
	In order to be able to Read File
	as a Warewolf user
	I want a tool that reads the contents of a file at a given location


Scenario Outline: Read File at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	When the read file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                  | Username   | Password |
         | <source> = <sourceLocation> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	| NO | Name       | source   | sourceLocation                                                       | username                     | password | resultVar  | result | errorOccured |
	| 1 | Local      | [[path]] | c:\filetoread.txt                                                    | ""                           | ""       | [[result]] | Guid   | NO           |
	| 2 | UNC        | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetoread.txt        | ""                           | ""       | [[result]] | Guid   | NO           |
	| 3 | UNC Secure | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetoread.txt | dev2.local\IntegrationTester | I73573r0 | [[result]] | Guid   | NO           |
	| 4 | FTP        | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetoread.txt                 | ""                           | ""       | [[result]] | Guid   | NO           |
	| 5 | FTPS       | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetodele.txt                 | IntegrationTester            | I73573r0 | [[result]] | Guid   | NO           |
	|6  | SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[result]]             | Guid   | NO           |
#Bug 12180 #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[result]][[a]]        | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[a]]*]]               | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[var@]]               | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[var]]00]]            | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[(1var)]]             | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[var[[a]]]]           | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[var.a]]              | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[@var]]               | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[var 1]]              | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[rec(1).[[rec().1]]]] | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[rec(@).a]]           | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[rec"()".a]]          | Guid   | AN           |
	       #| SFTP       | [[path]] | sftp://localhost/filetoread.txt                                      | dev2                         | Q/ulw&]  | [[rec([[[[b]]]]).a]]   | Guid   | AN           |
	































