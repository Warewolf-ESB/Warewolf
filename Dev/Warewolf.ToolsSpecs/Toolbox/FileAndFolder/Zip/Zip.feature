@fileFeature
Feature: Zip
	In order to be able to Zip File or Folder 
	as a Warewolf user
	I want a tool that Zip File or Folder at a given location

Scenario Outline: Zip file at location
	Given I have a source path "<source>" with value "<sourceLocation>"
	And source credentials as "<username>" and "<password>" for zip tests
	And I have a destination path "<destination>" with value "<destinationLocation>"
	And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"	
	And Archive Password as "<archivepassword>"
	And the Compression as "<compression>"
    When the Zip file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File |Overwrite  | Archive Password  | Compression Ratio |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  |<selected> | <archivepassword> | <compression>     |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| No | Name            | source   | sourceLocation                                             | username          | password | destination | destinationLocation                                            | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | Local to Local  | [[path]] | c:\filetozip0.txt                                          | ""                | ""       | [[path1]]   | c:\My New0.zip                                                 | ""                | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 2  | UNC to Local    | [[path]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip0.txt | ""                | ""       | [[path1]]   | c:\My New1.zip                                                 | ""                | ""           | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 3  | FTP to Local    | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip0.txt          | ""                | ""       | [[path1]]   | c:\My New2.zip                                                 | ""                | ""           | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 4  | FTPS to Local   | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip0.txt          | integrationtester | I73573r0 | [[path1]]   | c:\My New3.zip                                                 | ""                | ""           | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 5  | SFTP to Local   | [[path]] | sftp://rsaklfsvrgendev/filetozip0.txt                      | dev2              | Q/ulw&]  | [[path1]]   | c:\My New4.zip                                                 | ""                | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 6  | Local to UNC    | [[path]] | c:\filetozip1.txt                                          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip10.txt | ""                | ""           | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 7  | UNC to UNC      | [[path]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip1.txt | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip11.txt | ""                | ""           | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 8  | FTP to UNC      | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip1.txt          | ""                | ""       | [[path1]]   | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip12.txt | ""                | ""           | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 9  | FTPS to UNC     | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip1.txt          | integrationtester | I73573r0 | [[path1]]   | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip13.txt | ""                | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 10 | SFTP to UNC     | [[path]] | sftp://rsaklfsvrgendev/filetozip1.txt                      | dev2              | Q/ulw&]  | [[path1]]   | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip14.txt | ""                | ""           | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 11 | Local to FTP    | [[path]] | c:\filetozip2.txt                                          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip10.txt          | ""                | ""           | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 12 | UNC to FTP      | [[path]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip2.txt | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip11.txt          | ""                | ""           | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 13 | FTP to FTP      | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip2.txt          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip12.txt          | ""                | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 14 | FTPS to FTP     | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip2.txt          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip13.txt          | ""                | ""           | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 15 | SFTP to FTP     | [[path]] | sftp://rsaklfsvrgendev/filetozip2.txt                      | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip14.txt          | ""                | ""           | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 16 | Local to FTPS   | [[path]] | c:\filetozip3.txt                                          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip20.txt          | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 17 | UNC to FTPS     | [[path]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip3.txt | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip21.txt          | integrationtester | I73573r0     | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 18 | FTP to FTPS     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip3.txt          | ""                | ""       | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip22.txt          | integrationtester | I73573r0     | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 19 | FTPS to FTPS    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip3.txt          | integrationtester | I73573r0 | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip23.txt          | integrationtester | I73573r0     | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 20 | SFTP to FTPS    | [[path]] | sftp://rsaklfsvrgendev/filetozip3.txt                      | dev2              | Q/ulw&]  | [[path1]]   | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip24.txt          | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 21 | Local to SFTP   | [[path]] | c:\filetozip4.txt                                          | ""                | ""       | [[path1]]   | sftp://rsaklfsvrgendev/filetozip10.txt                               | dev2              | Q/ulw&]      | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 22 | UNC to SFTP     | [[path]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\filetozip4.txt | ""                | ""       | [[path1]]   | sftp://rsaklfsvrgendev/filetozip11.txt                               | dev2              | Q/ulw&]      | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 23 | FTP to SFTP     | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip4.txt          | ""                | ""       | [[path1]]   | sftp://rsaklfsvrgendev/filetozip12.txt                               | dev2              | Q/ulw&]      | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 24 | FTPS to SFTP    | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip4.txt          | integrationtester | I73573r0 | [[path1]]   | sftp://rsaklfsvrgendev/filetozip13.txt                               | dev2              | Q/ulw&]      | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 25 | SFTP to SFTP    | [[path]] | sftp://rsaklfsvrgendev/filetozip4.txt                      | dev2              | Q/ulw&]  | [[path1]]   | sftp://rsaklfsvrgendev/filetozip14.txt                               | dev2              | Q/ulw&]      | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 26 | SFTP to SFTP PK | [[path]] | sftp://rsaklfsvrgendev/filetozip41.txt                     | dev2              | Q/ulw&]  | [[path1]]   | sftp://rsaklfsvrgendev/filetozip141.txt                              | dev2              | Q/ulw&]      | True     |                 | None            | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

Scenario Outline: Zip file at location Null
	Given I have a source path "<source>" with value "<sourceLocation>"
	And source credentials as "<username>" and "<password>" for zip tests
	And I have a destination path "<destination>" with value "<destinationLocation>"
	And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"	
	And Archive Password as "<archivepassword>"
	And the Compression as "<compression>"
    When the Zip file tool is executed
	Then the execution has "<errorOccured>" error
	Examples: 
		| No | Name          | source   | sourceLocation                                       | username | password | destination | destinationLocation | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | Local to FTPS | [[path]] | NULL                                                 | ""       | ""       | [[path1]]   | c:\filetozip0.txt   | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Error  | An           |                      |                           |
		| 2  | Local to FTPS | [[path]] | c:\filetozip0.txt                                    | ""       | ""       | [[path1]]   | Null                | integrationtester | I73573r0     | True     |                 | BestCompression | [[result]] | Error  | An           |                      |                           |
		| 3  | UNC to Local  | [[path]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\      | ""       | ""       | [[path1]]   | c:\My New1.zip      | ""                | ""           | True     |                 | BestSpeed       | [[result]] | Error  | NO           |                      |                           |
		| 4  | FTP to Local  | [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetozip0.txt | ""       | ""       | [[path1]]   | " "                 | ""                | ""           | True     |                 | Default         | [[result]] | Error  | An           |                      |                           |
		| 5  | FTPS to Local | [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetozip0.txt |          | I73573r0 | [[path1]]   | c:\My New3.zip      | ""                | ""           | True     |                 | BestCompression | [[result]] | Error  | An           |                      |                           |
		| 6  | SFTP to Local | [[path]] | sftp://rsaklfsvrgendev/filetozip0.txt                      | dev2     | Q/ulw&]  | [[path1]]   | c:\My New4.zip      | ""                | ""           | True     |                 | None            | [[result]] | Error  | NO           |                      |                           |
		


Scenario Outline: Zip file at location is compressed at ratio
	Given I have a source path "<source>" with value "<sourceLocation>"
	And source credentials as "<username>" and "<password>" for zip tests
	And I have a destination path "<destination>" with value "<destinationLocation>"
	And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is ""
	And use private public key for destination is ""
	And result as "<resultVar>"	
	And Archive Password as "<archivepassword>"
	And the Compression as "<compression>"
    When the Zip file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the output is approximately "<compressionTimes>" the size of the original input 
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  | Archive Password  | Compression Ratio |
         | <source> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> | <archivepassword> | <compression>     |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| No | Name           | source   | sourceLocation    | username | password | destination | destinationLocation | destUsername | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | compressionTimes |
		| 1  | Local to Local | [[path]] | c:\filetozip0.txt | ""       | ""       | [[path1]]   | c:\My New0.zip      | ""           | ""           | True     |                 | None            | [[result]] | "Success" | NO           | 0.99             |
		| 2  | Local to Local | [[path]] | c:\filetozip0.txt | ""       | ""       | [[path1]]   | c:\My New0.zip      | ""           | ""           | True     |                 | Default         | [[result]] | "Success" | NO           | 1.71             |
		| 3  | Local to Local | [[path]] | c:\filetozip0.txt | ""       | ""       | [[path1]]   | c:\My New0.zip      | ""           | ""           | True     |                 | BestSpeed       | [[result]] | "Success" | NO           | 1.66             |
		| 4  | Local to Local | [[path]] | c:\filetozip0.txt | ""       | ""       | [[path1]]   | c:\My New0.zip      | ""           | ""           | True     |                 | BestCompression | [[result]] | "Success" | NO           | 1.71             |
	