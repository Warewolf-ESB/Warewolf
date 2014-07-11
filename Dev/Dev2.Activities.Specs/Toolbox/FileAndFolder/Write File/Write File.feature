@fileFeature
Feature: Write File
	In order to be able to Write File
	as a Warewolf user
	I want a tool that writes a file at a given location

Scenario Outline: Write file at location
	Given I have a source path '<source>' with value '<sourceLocation>' 
	And source credentials as '<username>' and '<password>'	
	And Method is '<method>'
	And input contents as '<content>'     
	And result as '<resultVar>'
    When the write file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Output Path                 | Method   | Username   | Password | File Contents |
         | <source> = <sourceLocation> | <method> | <username> | String   | <content>     |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
		Examples: 
		| Name                     | source   | sourceLocation                                                  | method        | content        | username          | password | resultVar              | result  | errorOccured |
		| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| UNC with Overwrite       | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetowrite0.txt | Overwrite     | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| FTP with Overwrite       | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetowrite0.txt          | Overwrite     | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| FTPS with Overwrite      | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetowrite0.txt          | Overwrite     | warewolf rules | integrationtester | I73573r0 | [[result]]             | Success | NO           |
		| SFTP with Overwrite      | [[path]] | sftp://localhost/filetowrite0.txt                               | Overwrite     | warewolf rules | dev2              | Q/ulw&]  | [[result]]             | Success | NO           |
		| Local with Append Top    | [[path]] | c:\filetowrite1.txt                                             | Append Top    | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| UNC with Append Top      | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetowrite1.txt | Append Top    | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| FTP with Append Top      | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetowrite1.txt          | Append Top    | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| FTPS with Append Top     | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetowrite1.txt          | Append Top    | warewolf rules | integrationtester | I73573r0 | [[result]]             | Success | NO           |
		| SFTP with Append Top     | [[path]] | sftp://localhost/filetowrite1.txt                               | Append Top    | warewolf rules | dev2              | Q/ulw&]  | [[result]]             | Success | NO           |
		| Local with Append Bottom | [[path]] | c:\filetowrite2.txt                                             | Append Bottom | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| UNC with Append Bottom   | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetowrite2.txt | Append Bottom | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| FTP with Append Bottom   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetowrite2.txt          | Append Bottom | warewolf rules | ""                | ""       | [[result]]             | Success | NO           |
		| FTPS with Append Bottom  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetowrite2.txt          | Append Bottom | warewolf rules | integrationtester | I73573r0 | [[result]]             | Success | NO           |
		| SFTP with Append Bottom  | [[path]] | sftp://localhost/filetowrite2.txt                               | Append Bottom | warewolf rules | dev2              | Q/ulw&]  | [[result]]             | Success | NO           |
#Bug12180| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[result]][[a]]        | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[a]]*]]               | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[var@]]               | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[var]]00]]            | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[(1var)]]             | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[var[[a]]]]           | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[var.a]]              | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[@var]]               | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[var 1]]              | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[rec(1).[[rec().1]]]] | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[rec(@).a]]           | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[rec"()".a]]          | Failure | NO           |
		#| Local with Overwrite     | [[path]] | c:\filetowrite0.txt                                             | Overwrite     | warewolf rules | ""                | ""       | [[result]][[a]]        | Failure | NO           |





















Scenario: Write file with carriage returns
	Given I have a source path '[[path]]' with value 'c:\filetowrite1.txt' 	
	And source credentials as '' and ''	
	And Method is 'Overwrite'
	And the input contents from a file 'infile1WithCarriageReturn.txt'     
	And result as 'Success'
    When the write file tool is executed	
	Then the output contents from a file 'outfile1WithCarriageReturn.txt'
	And the execution has "NO" error
	