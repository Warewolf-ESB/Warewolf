@fileFeature
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
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| Name           | source   | sourceLocation                                                | username          | password | destination | destinationLocation                                           | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result  | errorOccured |
		| Local to Local | [[path]] | c:\filetozip0.txt                                             | ""                | ""       | [[path1]]   | c:\My New0.zip                                                | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| UNC to Local   | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip0.txt | ""                | ""       | [[path1]]   | c:\My New1.zip                                                | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| FTP to Local   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip0.txt          | ""                | ""       | [[path1]]   | c:\My New2.zip                                                | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| FTPS to Local  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip0.txt          | integrationtester | I73573r0 | [[path1]]   | c:\My New3.zip                                                | ""                | ""           | True     |                 | BestCompression | [[result]] | Success | NO           |
		| SFTP to Local  | [[path]] | sftp://localhost/filetozip0.txt                               | dev2              | Q/ulw&]  | [[path1]]   | c:\My New4.zip                                                | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| Local to UNC   | [[path]] | c:\filetozip1.txt                                             | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip0.txt | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| UNC to UNC     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip1.txt | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip1.txt | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| FTP to UNC     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip1.txt          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip2.txt | ""                | ""           | True     |                 | BestCompression | [[result]] | Success | NO           |
		| FTPS to UNC    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip1.txt          | integrationtester | I73573r0 | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip3.txt | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| SFTP to UNC    | [[path]] | sftp://localhost/filetozip1.txt                               | dev2              | Q/ulw&]  | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip4.txt | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| Local to FTP   | [[path]] | c:\filetozip2.txt                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip0.txt          | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| UNC to FTP     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip2.txt | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip1.txt          | ""                | ""           | True     |                 | BestCompression | [[result]] | Success | NO           |
		| FTP to FTP     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip2.txt          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip2.txt          | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| FTPS to FTP    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip2.txt          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip3.txt          | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| SFTP to FTP    | [[path]] | sftp://localhost/filetozip2.txt                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip4.txt          | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| Local to FTPS  | [[path]] | c:\filetozip3.txt                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip0.txt          | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Success | NO           |
		| UNC to FTPS    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip3.txt | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip1.txt          | integrationtester | I73573r0     | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| FTP to FTPS    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip3.txt          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip2.txt          | integrationtester | I73573r0     | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| FTPS to FTPS   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip3.txt          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip3.txt          | integrationtester | I73573r0     | True     |                 | Default         | [[result]] | Success | NO           |
		| SFTP to FTPS   | [[path]] | sftp://localhost/filetozip3.txt                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip4.txt          | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Success | NO           |
		| Local to SFTP  | [[path]] | c:\filetozip4.txt                                             | ""                | ""       | [[path1]]   | sftp://localhost/filetozip0.txt                               | dev2              | Q/ulw&]      | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| UNC to SFTP    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip4.txt | ""                | ""       | [[path1]]   | sftp://localhost/filetozip1.txt                               | dev2              | Q/ulw&]      | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| FTP to SFTP    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip4.txt          | ""                | ""       | [[path1]]   | sftp://localhost/filetozip2.txt                               | dev2              | Q/ulw&]      | True     |                 | Default         | [[result]] | Success | NO           |
		| FTPS to SFTP   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip4.txt          | integrationtester | I73573r0 | [[path1]]   | sftp://localhost/filetozip3.txt                               | dev2              | Q/ulw&]      | True     |                 | BestCompression | [[result]] | Success | NO           |
		| SFTP to SFTP   | [[path]] | sftp://localhost/filetozip4.txt                               | dev2              | Q/ulw&]  | [[path1]]   | sftp://localhost/filetozip4.txt                               | dev2              | Q/ulw&]      | True     |                 | NoCompression   | [[result]] | Success | NO           |
		