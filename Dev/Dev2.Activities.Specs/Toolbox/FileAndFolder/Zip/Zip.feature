Feature: Zip
	In order to be able to Zip File or Folder 
	as a Warewolf user
	I want a tool that Zip File or Folder at a given location


Scenario Outline: Zip file at location
	Given I have a variable '<variable>' with value '<location>'
	And input username as '<username>' and password '<password>'
	And I have a  destination variable '<variable1>' with value '<location1>'
    And folder username as '<username1>' and password '<password1>'
	And overwrite is '<selected>'
	And Archive Password as '<archivepassword>'
	And the Compression as '<compression>'
    When the Zip file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | File or Folder              | Username     | Password     | Destination                   | Username      | Password      | Overwrite    | Archive Password  |
         | '<variable>' = '<location>' | '<username>' | '<password>' | '<variable1>' = '<location1>' | '<username1>' | '<password1>' | '<selected>' | <archivepassword> |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| variable | location                                    | username          | password | variable1 | location1                                    | username1         | password1 | selected | archivepassword | compression           | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New1.txt                               |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing1\test.txt          |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing1\test.txt           |                   |           | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testin1g\test.txt | integrationtester | I73573r   | True     |                 | None (No Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing1\test.txt          | dev2              | Q/ulw&]   | True     |                 | None (No Compression) | [[result]] | Success | NO           |

		
Scenario Outline: Zip file at location with compression type as "Partial (Best Speed)"
	Given I have a variable '<variable>' with value '<location>'
	And input username as '<username>' and password '<password>'
	And I have a  destination variable '<variable1>' with value '<location1>'
    And folder username as '<username1>' and password '<password1>'
	And overwrite is '<selected>'
	And Archive Password as '<archivepassword>'
	And the Compression as '<compression>'
    When the Zip file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | File or Folder              | Username     | Password     | Destination                   | Username      | Password      | Overwrite    | Archive Password  |
         | '<variable>' = '<location>' | '<username>' | '<password>' | '<variable1>' = '<location1>' | '<username1>' | '<password1>' | '<selected>' | <archivepassword> |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| variable | location                                    | username          | password | variable1 | location1                                    | username1         | password1 | selected | archivepassword | compression          | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New1.txt                               |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing1\test.txt          |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing1\test.txt           |                   |           | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testin1g\test.txt | integrationtester | I73573r   | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing1\test.txt          | dev2              | Q/ulw&]   | True     |                 | Partial (Best Speed) | [[result]] | Success | NO           |

				
Scenario Outline: Zip file at location with compression type as "Normal (Default)"
	Given I have a variable '<variable>' with value '<location>'
	And input username as '<username>' and password '<password>'
	And I have a  destination variable '<variable1>' with value '<location1>'
    And folder username as '<username1>' and password '<password1>'
	And overwrite is '<selected>'
	And Archive Password as '<archivepassword>'
	And the Compression as '<compression>'
    When the Zip file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | File or Folder              | Username     | Password     | Destination                   | Username      | Password      | Overwrite    | Archive Password  |
         | '<variable>' = '<location>' | '<username>' | '<password>' | '<variable1>' = '<location1>' | '<username1>' | '<password1>' | '<selected>' | <archivepassword> |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| variable | location                                    | username          | password | variable1 | location1                                    | username1         | password1 | selected | archivepassword | compression      | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New1.txt                               |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing1\test.txt          |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing1\test.txt           |                   |           | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testin1g\test.txt | integrationtester | I73573r   | True     |                 | Normal (Default) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing1\test.txt          | dev2              | Q/ulw&]   | True     |                 | Normal (Default) | [[result]] | Success | NO           |

	Scenario Outline: Zip file at location with compression type as "Max (Best Compression)"
	Given I have a variable '<variable>' with value '<location>'
	And input username as '<username>' and password '<password>'
	And I have a  destination variable '<variable1>' with value '<location1>'
    And folder username as '<username1>' and password '<password1>'
	And overwrite is '<selected>'
	And Archive Password as '<archivepassword>'
	And the Compression as '<compression>'
    When the Zip file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | File or Folder              | Username     | Password     | Destination                   | Username      | Password      | Overwrite    | Archive Password  |
         | '<variable>' = '<location>' | '<username>' | '<password>' | '<variable1>' = '<location1>' | '<username1>' | '<password1>' | '<selected>' | <archivepassword> |
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| variable | location                                    | username          | password | variable1 | location1                                    | username1         | password1 | selected | archivepassword | compression            | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | c:\My New.txt                                |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | \\rsaklfsvrtfsbld\testing\test.txt           |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftp:\\dev2.co.za\testing\test.txt            |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0  | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]   | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]] | c:\My New1.txt                               |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]] | \\rsaklfsvrtfsbld\testing1\test.txt          |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]] | ftp:\\dev2.co.za\testing1\test.txt           |                   |           | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]] | ftps:\\dev2ftps.dev2.local\testin1g\test.txt | integrationtester | I73573r   | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]] | sftp:\\dev2.co.za\testing1\test.txt          | dev2              | Q/ulw&]   | True     |                 | Max (Best Compression) | [[result]] | Success | NO           |