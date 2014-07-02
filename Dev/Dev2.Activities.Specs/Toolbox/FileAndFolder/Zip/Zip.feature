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
		| No | Name           | source   | sourceLocation                                                | username          | password | destination | destinationLocation                                           | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result  | errorOccured |
		| 1  | Local to Local | [[path]] | c:\filetozip0.txt                                             | ""                | ""       | [[path1]]   | c:\My New0.zip                                                | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| 2  | UNC to Local   | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip0.txt | ""                | ""       | [[path1]]   | c:\My New1.zip                                                | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| 3  | FTP to Local   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip0.txt          | ""                | ""       | [[path1]]   | c:\My New2.zip                                                | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| 4  | FTPS to Local  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip0.txt          | integrationtester | I73573r0 | [[path1]]   | c:\My New3.zip                                                | ""                | ""           | True     |                 | BestCompression | [[result]] | Success | NO           |
		| 5  | SFTP to Local  | [[path]] | sftp://localhost/filetozip0.txt                               | dev2              | Q/ulw&]  | [[path1]]   | c:\My New4.zip                                                | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| 6  | Local to UNC   | [[path]] | c:\filetozip1.txt                                             | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip0.txt | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| 7  | UNC to UNC     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip1.txt | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip1.txt | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| 8  | FTP to UNC     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip1.txt          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip2.txt | ""                | ""           | True     |                 | BestCompression | [[result]] | Success | NO           |
		| 9  | FTPS to UNC    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip1.txt          | integrationtester | I73573r0 | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip3.txt | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| 10 | SFTP to UNC    | [[path]] | sftp://localhost/filetozip1.txt                               | dev2              | Q/ulw&]  | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip4.txt | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| 11 | Local to FTP   | [[path]] | c:\filetozip2.txt                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip0.txt          | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| 12 | UNC to FTP     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip2.txt | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip1.txt          | ""                | ""           | True     |                 | BestCompression | [[result]] | Success | NO           |
		| 13 | FTP to FTP     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip2.txt          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip2.txt          | ""                | ""           | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| 14 | FTPS to FTP    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip2.txt          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip3.txt          | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| 15 | SFTP to FTP    | [[path]] | sftp://localhost/filetozip2.txt                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip4.txt          | ""                | ""           | True     |                 | Default         | [[result]] | Success | NO           |
		| 16 | Local to FTPS  | [[path]] | c:\filetozip3.txt                                             | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip0.txt          | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Success | NO           |
		| 17 | UNC to FTPS    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip3.txt | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip1.txt          | integrationtester | I73573r0     | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| 18 | FTP to FTPS    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip3.txt          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip2.txt          | integrationtester | I73573r0     | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| 19 | FTPS to FTPS   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip3.txt          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip3.txt          | integrationtester | I73573r0     | True     |                 | Default         | [[result]] | Success | NO           |
		| 20 | SFTP to FTPS   | [[path]] | sftp://localhost/filetozip3.txt                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip4.txt          | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Success | NO           |
		| 21 | Local to SFTP  | [[path]] | c:\filetozip4.txt                                             | ""                | ""       | [[path1]]   | sftp://localhost/filetozip0.txt                               | dev2              | Q/ulw&]      | True     |                 | NoCompression   | [[result]] | Success | NO           |
		| 22 | UNC to SFTP    | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip4.txt | ""                | ""       | [[path1]]   | sftp://localhost/filetozip1.txt                               | dev2              | Q/ulw&]      | True     |                 | BestSpeed       | [[result]] | Success | NO           |
		| 23 | FTP to SFTP    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip4.txt          | ""                | ""       | [[path1]]   | sftp://localhost/filetozip2.txt                               | dev2              | Q/ulw&]      | True     |                 | Default         | [[result]] | Success | NO           |
		| 24 | FTPS to SFTP   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip4.txt          | integrationtester | I73573r0 | [[path1]]   | sftp://localhost/filetozip3.txt                               | dev2              | Q/ulw&]      | True     |                 | BestCompression | [[result]] | Success | NO           |
		| 25 | SFTP to SFTP   | [[path]] | sftp://localhost/filetozip4.txt                               | dev2              | Q/ulw&]  | [[path1]]   | sftp://localhost/filetozip4.txt                               | dev2              | Q/ulw&]      | True     |                 | NoCompression   | [[result]] | Success | NO           |
		
		
		
##Bug 12091	
#Scenario Outline: Zip file at Invalid location
#	Given I have a source path '<source>' with value '<sourceLocation>'
#	And source credentials as '<username>' and '<password>'
#	And I have a destination path '<destination>' with value '<destinationLocation>'
#	And destination credentials as '<destUsername>' and '<destPassword>'
#	And overwrite is '<selected>'
#	And result as '<resultVar>'	
#	And Archive Password as '<archivepassword>'
#	And the Compression as '<compression>'
#    When the Zip file tool is executed
#	Then the result variable '<resultVar>' will be '<result>'
#	And the execution has "<errorOccured>" error
#	And the debug inputs as
#         | Source Path                 | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  | Archive Password  | Compression Ratio |
#         | <source> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> | <archivepassword> | <compression>     |
#	And the debug output as
#		|                        |
#		| <resultVar> = <result> |
#	Examples: 
#		| No | Name           | source   | sourceLocation                                                | username          | password | destination | destinationLocation                                           | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result  | errorOccured |	
#		| 26 | Local to Local | [[path]] |                                                               | ""                | ""       | [[path1]]   | c:\My New0.zip                                                | ""                | ""           | True     |                 | NoCompression   | [[result]] | Failure | AN           |
#		| 27 | UNC to Local   | [[path]] |                                                               | ""                | ""       | [[path1]]   | c:\My New1.zip                                                | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Failure | AN           |
#		| 28 | FTP to Local   | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip0.txt          | ""                | ""       | [[path1]]   |                                                               | ""                | ""           | True     |                 | Default         | [[result]] | Failure | AN           |
#		| 29 | FTPS to Local  | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip0.txt          |                   | I73573r0 | [[path1]]   | c:\My New3.zip                                                | ""                | ""           | True     |                 | BestCompression | [[result]] | Failure | AN           |
#		| 30 | SFTP to Local  | [[path]] | sftp://localhost/filetozip0.txt                               |                   | Q/ulw&]  | [[path1]]   | c:\My New4.zip                                                | ""                | ""           | True     |                 | NoCompression   | [[result]] | Failure | AN           |
#		| 31 | Local to UNC   | [[path]] | c:\filetozip1.txt                                             | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip0.txt | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Failure | AN           |
#		| 32 | UNC to UNC     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip1.txt | ""                | ""       | [[path1]]   |                                                               | ""                | ""           | True     |                 | Default         | [[result]] | Failure | AN           |
#		| 33 | FTP to UNC     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip1.txt          | ""                | ""       | [[path1]]   |                                                               | ""                | ""           | True     |                 | BestCompression | [[result]] | Failure | AN           |
#		| 34 | FTPS to UNC    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip1.txt          | integrationtester | I73573r0 | [[path1]]   |                                                               | ""                | ""           | True     |                 | NoCompression   | [[result]] | Failure | AN           |
#		| 35 | SFTP to UNC    | [[path]] | sftp://localhost/filetozip1.txt                               | dev2              | Q/ulw&]  | [[path1]]   |                                                               | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Failure | AN           |
#		| 36 | Local to FTP   | [[path]] | c:\filetozip2.txt                                             | ""                | ""       | [[path1]]   |                                                               | ""                | ""           | True     |                 | Default         | [[result]] | Failure | AN           |
#		| 37 | UNC to FTP     | [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetozip2.txt | ""                | ""       | [[path1]]   |                                                               | ""                | ""           | True     |                 | BestCompression | [[result]] | Failure | AN           |
#		| 38 | FTP to FTP     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip2.txt          | ""                | ""       | [[path1]]   |                                                               | ""                | ""           | True     |                 | NoCompression   | [[result]] | Failure | AN           |
#		| 39 | FTPS to FTP    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip2.txt          | integrationtester | I73573r0 | [[path1]]   |                                                               | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Failure | AN           |
#		| 40 | SFTP to FTP    | [[path]] |                                                               | dev2              | Q/ulw&]  | [[path1]]   |                                                               | ""                | ""           | True     |                 | Default         | [[result]] | Failure | AN           |
#		| 41 | Local to FTPS  | [[path]] |                                                               | ""                | ""       | [[path1]]   |                                                               | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Failure | AN           |
#		| 42 | UNC to FTPS    | [[path]] |                                                               | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip1.txt          | integrationtester | I73573r0     | True     |                 | NoCompression   | [[result]] | Failure | AN           |
#		| 43 | FTP to FTPS    | [[path]] |                                                               | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip2.txt          | integrationtester | I73573r0     | True     |                 | BestSpeed       | [[result]] | Failure | AN           |
#		| 44 | FTPS to FTPS   | [[path]] |                                                               | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip3.txt          | integrationtester | I73573r0     | True     |                 | Default         | [[result]] | Failure | AN           |
#		| 45 | SFTP to FTPS   | [[path]] |                                                               | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip4.txt          | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Failure | AN           |
#		| 46 | Local to SFTP  | [[path]] |                                                               | ""                | ""       | [[path1]]   | sftp://localhost/filetozip0.txt                               | dev2              | Q/ulw&]      | True     |                 | NoCompression   | [[result]] | Failure | AN           |
#		| 47 | UNC to SFTP    | [[path]] |                                                               | ""                | ""       | [[path1]]   | sftp://localhost/filetozip1.txt                               | dev2              | Q/ulw&]      | True     |                 | BestSpeed       | [[result]] | Failure | AN           |
#		| 48 | FTP to SFTP    | [[path]] |                                                               | ""                | ""       | [[path1]]   | sftp://localhost/filetozip2.txt                               | dev2              | Q/ulw&]      | True     |                 | Default         | [[result]] | Failure | AN           |
#		| 49 | FTPS to SFTP   | [[path]] |                                                               | integrationtester | I73573r0 | [[path1]]   | sftp://localhost/filetozip3.txt                               | dev2              | Q/ulw&]      | True     |                 | BestCompression | [[result]] | Failure | AN           |
#		| 50 | SFTP to SFTP   | [[path]] |                                                               | dev2              | Q/ulw&]  | [[path1]]   | sftp://localhost/filetozip4.txt                               | dev2              | Q/ulw&]      | True     |                 | NoCompression   | [[result]] | Failure | AN           |