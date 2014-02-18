Feature: Zip
	In order to be able to Zip File or Folder 
	as a Warewolf user
	I want a tool that Zip File or Folder at a given location

Scenario Outline: Zip file at location
	Given I have a source path '<source>' with value '<sourceLocation>'	
	And source credentials as '<username>' and '<password>'
	And I have a destination path '<destination>' with value '<destinationLocation>'
	And destination credentials as '<destUsername>' and '<destPassword>'
	And overwrite is '<selected>'
	And result as '<resultVar>'	
	And Archive Password as '<archivepassword>'
	And the Compression as '<compression>'
    When the Zip file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  | Archive Password  | Compression Ratio |
         | <source> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> | <archivepassword> | <compression>     |
	And the debug output as
		| Result                 |
		| <resultVar> = <result> |
	Examples: 
		| source   | sourceLocation                              | username          | password | destination | destinationLocation                          | destUsername      | destPassword | selected | archivepassword | compression      | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]]   | c:\My New.txt                                |                   |              | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]]   | c:\My New.txt                                |                   |              | True     |                 | Best Speed       | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]]   | c:\My New.txt                                |                   |              | True     |                 | Default          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]]   | c:\My New.txt                                |                   |              | True     |                 | Best Compression | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]]   | c:\My New.txt                                |                   |              | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]]   | \\rsaklfsvrtfsbld\testing\test.txt           |                   |              | True     |                 | Best Speed       | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]]   | \\rsaklfsvrtfsbld\testing\test.txt           |                   |              | True     |                 | Default          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]]   | \\rsaklfsvrtfsbld\testing\test.txt           |                   |              | True     |                 | Best Compression | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]]   | \\rsaklfsvrtfsbld\testing\test.txt           |                   |              | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]]   | \\rsaklfsvrtfsbld\testing\test.txt           |                   |              | True     |                 | Best Speed       | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]]   | ftp:\\dev2.co.za\testing\test.txt            |                   |              | True     |                 | Default          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]]   | ftp:\\dev2.co.za\testing\test.txt            |                   |              | True     |                 | Best Compression | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]]   | ftp:\\dev2.co.za\testing\test.txt            |                   |              | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]]   | ftp:\\dev2.co.za\testing\test.txt            |                   |              | True     |                 | Best Speed       | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]]   | ftp:\\dev2.co.za\testing\test.txt            |                   |              | True     |                 | Default          | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]]   | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0     | True     |                 | Best Compression | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]]   | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0     | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]]   | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0     | True     |                 | Best Speed       | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]]   | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0     | True     |                 | Default          | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]]   | ftps:\\dev2ftps.dev2.local\testing\test.txt  | integrationtester | I73573r0     | True     |                 | Best Compression | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]]   | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]      | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]]   | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]      | True     |                 | Best Speed       | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]]   | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]      | True     |                 | Default          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]]   | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]      | True     |                 | Best Compression | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]]   | sftp:\\dev2.co.za\testing\test.txt           | dev2              | Q/ulw&]      | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               |                   |          | [[path1]]   | c:\My New1.txt                               |                   |              | True     |                 | No Compression   | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[path1]]   | \\rsaklfsvrtfsbld\testing1\test.txt          |                   |              | True     |                 | Best Speed       | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[path1]]   | ftp:\\dev2.co.za\testing1\test.txt           |                   |              | True     |                 | Default          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[path1]]   | ftps:\\dev2ftps.dev2.local\testin1g\test.txt | integrationtester | I73573r      | True     |                 | Best Compression | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[path1]]   | sftp:\\dev2.co.za\testing1\test.txt          | dev2              | Q/ulw&]      | True     |                 | No Compression   | [[result]] | Success | NO           |  
