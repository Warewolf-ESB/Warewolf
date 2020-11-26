Feature: Zip
	In order to be able to Zip File or Folder 
	as a Warewolf user
	I want a tool that Zip File or Folder at a given location

@ZipFromLocal
@ZipFromLocalWithOverwrite
Scenario Outline: Zip file at local location
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
		| No | Name            | source   | sourceLocation   | username | password | destination | destinationLocation                                                                          | destUsername  | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1 | Local to Local  | [[path]] | c:\filetozip0.txt | ""       | ""       | [[path1]]   | c:\My New0.zip                                                                               | ""            | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 2 | Local to FTP    | [[path]] | c:\filetozip2.txt | ""       | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip10.txt                             | ""            | ""           | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 3 | Local to FTPS   | [[path]] | c:\filetozip3.txt | ""       | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip20.txt                             | Administrator | Dev2@dmin123 | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 4 | Local to SFTP   | [[path]] | c:\filetozip4.txt | ""       | ""       | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |

@ZipFromFTP
@ZipFromFTPWithOverwrite
Scenario Outline: Zip file at FTP location
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
		| No | Name            | source   | sourceLocation                                                  | username | password | destination | destinationLocation                                                                          | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | FTP to Local    | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip0.txt | ""       | ""       | [[path1]]   | c:\My New2.zip                                                                               | ""                | ""           | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 2  | FTP to UNC      | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip1.txt | ""       | ""       | [[path1]]   | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileZipSharedTestingSite\filetozip12.txt | ""                | ""           | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 3  | FTP to SFTP     | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip4.txt | ""       | ""       | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2              | Q/ulw&]      | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 4  | FTP to FTP      | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip2.txt | ""       | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip12.txt                             | ""                | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 5  | FTP to FTPS     | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip3.txt | ""       | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip22.txt                             | Administrator     | Dev2@dmin123 | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |

@ZipFromFTPS
@ZipFromFTPSWithOverwrite
Scenario Outline: Zip file at FTPS location
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
		| No | Name            | source   | sourceLocation                                                                      | username          | password | destination | destinationLocation                                                                  | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | FTPS to Local   | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip0.txt | Administrator | Dev2@dmin123 | [[path1]]   | c:\My New3.zip                                                                               | ""            | ""           | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 2  | FTPS to UNC     | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip1.txt | Administrator | Dev2@dmin123 | [[path1]]   | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileZipSharedTestingSite\filetozip13.txt | ""            | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 3  | FTPS to FTPS    | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip3.txt | Administrator | Dev2@dmin123 | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip23.txt                             | Administrator | Dev2@dmin123 | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 4  | FTPS to SFTP    | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip4.txt | Administrator | Dev2@dmin123 | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 5  | FTPS to FTP     | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip2.txt | Administrator | Dev2@dmin123 | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip13.txt                             | ""            | ""           | True     |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |

@ZipFromSFTP
@ZipFromSFTPWithOverwrite
Scenario Outline: Zip file at SFTP location
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
		| No | Name            | source   | sourceLocation                                                                      | username          | password | destination | destinationLocation                                                                  | destUsername      | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | SFTP to Local   | [[path]] | sftp://SVRDEV.premier.local/filetozip0.txt  | dev2 | Q/ulw&]  | [[path1]]   | c:\My New4.zip                                                                               | ""            | ""           | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 2  | SFTP to SFTP PK | [[path]] | sftp://SVRDEV.premier.local/filetozip41.txt | dev2 | Q/ulw&]  | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | True     |                 | None            | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |
		| 3  | SFTP to FTP     | [[path]] | sftp://SVRDEV.premier.local/filetozip2.txt  | dev2 | Q/ulw&]  | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip14.txt                             | ""            | ""           | True     |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 4  | SFTP to FTPS    | [[path]] | sftp://SVRDEV.premier.local/filetozip3.txt  | dev2 | Q/ulw&]  | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip24.txt                             | Administrator | Dev2@dmin123 | True     |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 5  | SFTP to SFTP    | [[path]] | sftp://SVRDEV.premier.local/filetozip4.txt  | dev2 | Q/ulw&]  | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | True     |                 | None            | [[result]] | "Success" | NO           |                      |                           |

@ZipFromLocal
@ZipFromLocalWithoutOverwrite
Scenario Outline: Zip file at local location with overwrite disabled
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
		| No | Name            | source   | sourceLocation    | username | password | destination | destinationLocation                                                                          | destUsername  | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | Local to Local  | [[path]] | c:\filetozip0.txt | ""       | ""       | [[path1]]   | c:\My New0.zip                                                                               | ""            | ""           | False    |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 2  | Local to FTP    | [[path]] | c:\filetozip2.txt | ""       | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip10.txt                             | ""            | ""           | False    |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 3  | Local to FTPS   | [[path]] | c:\filetozip3.txt | ""       | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip20.txt                             | Administrator | Dev2@dmin123 | False    |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 4  | Local to SFTP   | [[path]] | c:\filetozip4.txt | ""       | ""       | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | False    |                 | None            | [[result]] | "Success" | NO           |                      |                           |

@ZipFromFTP
@ZipFromFTPWithoutOverwrite
Scenario Outline: Zip file at FTP location with overwrite disabled
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
		| No | Name            | source   | sourceLocation                                                  | username          | password | destination | destinationLocation                                                                          | destUsername  | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | FTP to Local    | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip0.txt | ""                | ""       | [[path1]]   | c:\My New2.zip                                                                               | ""            | ""           | False    |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 2  | FTP to UNC      | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip1.txt | ""                | ""       | [[path1]]   | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileZipSharedTestingSite\filetozip12.txt | ""            | ""           | False    |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 3  | FTP to FTP      | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip2.txt | ""                | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip12.txt                             | ""            | ""           | False    |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 4  | FTP to FTPS     | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip3.txt | ""                | ""       | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip22.txt                             | Administrator | Dev2@dmin123 | False    |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 5  | FTP to SFTP     | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip4.txt | ""                | ""       | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | False    |                 | Default         | [[result]] | "Success" | NO           |                      |                           |

@ZipFromFTPS
@ZipFromFTPSWithoutOverwrite
Scenario Outline: Zip file at FTPS location with overwrite disabled
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
		| No | Name            | source   | sourceLocation                                                  | username      | password     | destination | destinationLocation                                                                          | destUsername  | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | FTPS to Local   | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip0.txt | Administrator | Dev2@dmin123 | [[path1]]   | c:\My New3.zip                                                                               | ""            | ""           | False    |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 2  | FTPS to UNC     | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip1.txt | Administrator | Dev2@dmin123 | [[path1]]   | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileZipSharedTestingSite\filetozip13.txt | ""            | ""           | False    |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 3  | FTPS to FTP     | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip2.txt | Administrator | Dev2@dmin123 | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip13.txt                             | ""            | ""           | False    |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 4  | FTPS to FTPS    | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip3.txt | Administrator | Dev2@dmin123 | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip23.txt                             | Administrator | Dev2@dmin123 | False    |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 5  | FTPS to SFTP    | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip4.txt | Administrator | Dev2@dmin123 | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | False    |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |

@ZipFromSFTP
@ZipFromSFTPWithoutOverwrite
Scenario Outline: Zip file at SFTP location with overwrite disabled
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
		| No | Name            | source   | sourceLocation                              | username | password | destination | destinationLocation                                                                          | destUsername  | destPassword | selected | archivepassword | compression     | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | SFTP to Local   | [[path]] | sftp://SVRDEV.premier.local/filetozip0.txt  | dev2     | Q/ulw&]  | [[path1]]   | c:\My New4.zip                                                                               | ""            | ""           | False    |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 2  | SFTP to UNC     | [[path]] | sftp://SVRDEV.premier.local/filetozip1.txt  | dev2     | Q/ulw&]  | [[path1]]   | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileZipSharedTestingSite\filetozip14.txt | ""            | ""           | False    |                 | BestSpeed       | [[result]] | "Success" | NO           |                      |                           |
		| 3  | SFTP to FTP     | [[path]] | sftp://SVRDEV.premier.local/filetozip2.txt  | dev2     | Q/ulw&]  | [[path1]]   | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip14.txt                             | ""            | ""           | False    |                 | Default         | [[result]] | "Success" | NO           |                      |                           |
		| 4  | SFTP to FTPS    | [[path]] | sftp://SVRDEV.premier.local/filetozip3.txt  | dev2     | Q/ulw&]  | [[path1]]   | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip24.txt                             | Administrator | Dev2@dmin123 | False    |                 | BestCompression | [[result]] | "Success" | NO           |                      |                           |
		| 5  | SFTP to SFTP    | [[path]] | sftp://SVRDEV.premier.local/filetozip4.txt  | dev2     | Q/ulw&]  | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | False    |                 | None            | [[result]] | "Success" | NO           |                      |                           |
		| 6  | SFTP to SFTP PK | [[path]] | sftp://SVRDEV.premier.local/filetozip41.txt | dev2     | Q/ulw&]  | [[path1]]   | sftp://SVRDEV.premier.local/filetozip.zip                                                    | dev2          | Q/ulw&]      | False    |                 | None            | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

@Zip
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
		| No | Name          | source   | sourceLocation                                                                | username | password     | destination | destinationLocation | destUsername  | destPassword | selected | archivepassword | compression     | resultVar  | result | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | Local to FTPS | [[path]] | NULL                                                                          | ""       | ""           | [[path1]]   | c:\filetozip0.txt   | Administrator | Dev2@dmin123 | True     |                 | BestCompression | [[result]] | Error  | An           |                      |                           |
		| 2  | Local to FTPS | [[path]] | c:\filetozip0.txt                                                             | ""       | ""           | [[path1]]   | Null                | Administrator | Dev2@dmin123 | True     |                 | BestCompression | [[result]] | Error  | An           |                      |                           |
		| 3  | FTP to Local  | [[path]] | ftp://DEVOPSPDC.premier.local:1001/FORZIPTESTING/filetozip0.txt               | ""       | ""           | [[path1]]   | " "                 | ""            | ""           | True     |                 | Default         | [[result]] | Error  | An           |                      |                           |
		| 4  | FTPS to Local | [[path]] | ftp://DEVOPSPDC.premier.local:1002/FORZIPTESTING/filetozip0.txt               |          | Dev2@dmin123 | [[path1]]   | c:\My New3.zip      | ""            | ""           | True     |                 | BestCompression | [[result]] | Error  | An           |                      |                           |
		| 5  | SFTP to Local | [[path]] | sftp://SVRDEV.premier.local/filetozip0.txt                                    | dev2     | Q/ulw&]      | [[path1]]   | c:\My New4.zip      | ""            | ""           | True     |                 | None            | [[result]] | Error  | NO           |                      |                           |
		
@Zip
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
	