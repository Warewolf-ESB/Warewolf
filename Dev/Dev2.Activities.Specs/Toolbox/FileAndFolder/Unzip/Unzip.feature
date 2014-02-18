@fileFeature
Feature: Unzip
	In order to be able to Unzip File or Folder 
	as a Warewolf user
	I want a tool that will Unzip File(s) or Folder(s) at a given location
	
@ignore
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
		| source   | sourceLocation                                          | username          | password | destination | destinationLocation                             | destUsername      | destPassword | selected | archivepassword | resultVar  | result  | errorOccured |
		| [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | c:\                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | c:\                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | c:\                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | c:\                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | c:\                                             | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\ | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/          | ""                | ""           | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/          | integrationtester | I73573r0     | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | c:\test.zip                                             | ""                | ""       | [[path1]]   | sftp://localhost/                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.zip | ""                | ""       | [[path1]]   | sftp://localhost/                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.zip          | ""                | ""       | [[path1]]   | sftp://localhost/                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.zip          | integrationtester | I73573r0 | [[path1]]   | sftp://localhost/                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
		| [[path]] | sftp://localhost/test.zip                               | dev2              | Q/ulw&]  | [[path1]]   | sftp://localhost/                               | dev2              | Q/ulw&]      | True     | ""              | [[result]] | Success | NO           |
