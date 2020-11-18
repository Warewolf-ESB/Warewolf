Feature: Move
	In order to be able to Move a File or Folder 
	as a Warewolf user
	I want a tool that will Move File(s) or Folder(s) from a given location to another location
	
@FileMoveFromLocalWithOverwrite
Scenario Outline: Move file at local location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name             | source         | sourceLocation   | username | password | destination  | destinationLocation                                                                       | destUsername  | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | Local to Local   | [[sourcePath]] | c:\movefile0.txt | ""       | ""       | [[destPath]] | C:\moved0.txt                                                                             | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 2  | Local to FTP     | [[sourcePath]] | c:\movefile1.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved0.txt                          | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | Local to FTPS    | [[sourcePath]] | c:\movefile2.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved0.txt                          | Administrator | Dev2@dmin123 | True     | [[result]] | Success | NO           |                      |                           |
	   | 4  | Local to SFTP    | [[sourcePath]] | c:\movefile3.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/moved0.txt                                                    | dev2          | Q/ulw&]      | True     | [[result]] | Success | NO           |                      |                           |
	   | 5  | Local to UNC     | [[sourcePath]] | c:\movefile4.txt | ""       | ""       | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved0.txt  | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
	
@FileMoveFromUNCWithOverwrite
Scenario Outline: Move file at UNC location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name             | source         | sourceLocation                                                                              | username | password | destination  | destinationLocation                                                                       | destUsername  | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | UNC to Local     | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile0.txt | ""       | ""       | [[destPath]] | C:\moved1.txt                                                                             | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 2  | UNC to FTP       | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile1.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved1.txt                          | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | UNC to FTPS      | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile2.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved1.txt                          | Administrator | Dev2@dmin123 | True     | [[result]] | Success | NO           |                      |                           |
	   | 4  | UNC to SFTP      | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile3.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/moved1.txt                                                    | dev2          | Q/ulw&]      | True     | [[result]] | Success | NO           |                      |                           |
	   | 5  | UNC TO UNC       | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile4.txt | ""       | ""       | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved1.txt  | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
	
@FileMoveFromFTPWithOverwrite
Scenario Outline: Move file at FTP location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name             | source         | sourceLocation                                                      | username | password | destination  | destinationLocation                                                                       | destUsername  | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | FTP to Local     | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile0.txt | ""       | ""       | [[destPath]] | C:\moved2.txt                                                                             | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
	   | 2  | FTP to UNC       | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile1.txt | ""       | ""       | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved2.txt  | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | FTP to FTPS      | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile2.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved2.txt                          | Administrator | Dev2@dmin123 | True     | [[result]] | Success | NO           |                      |                           |
	   | 4  | FTP to SFTP      | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile3.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/moved2.txt                                                    | dev2          | Q/ulw&]      | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 5  | FTP to FTP       | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile4.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved2.txt                          | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
	
@FileMoveFromFTPSWithOverwrite
Scenario Outline: Move file at FTPS location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name          | source         | sourceLocation                                                      | username      | password     | destination  | destinationLocation                                                                       | destUsername  | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | FTPS to Local | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile0.txt | Administrator | Dev2@dmin123 | [[destPath]] | C:\moved3.txt                                                                             | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
	   | 2  | FTPS to UNC   | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile1.txt | Administrator | Dev2@dmin123 | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved3.txt  | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | FTPS to FTPS  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile2.txt | Administrator | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved3.txt                          | Administrator | Dev2@dmin123 | True     | [[result]] | Success | NO           |                      |                           |
	   | 4  | FTPS to SFTP  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile3.txt | Administrator | Dev2@dmin123 | [[destPath]] | sftp://SVRDEV.premier.local/moved3.txt                                                    | dev2          | Q/ulw&]      | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 5  | FTPS to FTP   | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile4.txt | Administrator | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved3.txt                          | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
	
@FileMoveFromSFTPWithOverwrite
Scenario Outline: Move file at SFTP location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name             | source         | sourceLocation                             | username | password | destination  | destinationLocation                                                                       | destUsername  | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | SFTP to Local    | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile0.txt  | dev2     | Q/ulw&]  | [[destPath]] | C:\moved4.txt                                                                             | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
	   | 2  | SFTP to UNC      | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile1.txt  | dev2     | Q/ulw&]  | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved4.txt  | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | SFTP to FTP      | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile2.txt  | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved4.txt                          | ""            | ""           | True     | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 4  | SFTP to FTPS     | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile3.txt  | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved4.txt                          | Administrator | Dev2@dmin123 | True     | [[result]] | Success | NO           |                      |                           |
	   | 5  | SFTP to SFTP     | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile4.txt  | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/moved4.txt                                                    | dev2          | Q/ulw&]      | True     | [[result]] | Success | NO           |                      |                           |
	   | 6  | SFTP to Local PK | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile01.txt | dev2     | Q/ulw&]  | [[destPath]] | C:\moved41.txt                                                                            | ""            | ""           | True     | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
	   | 7  | SFTP to UNC  PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile11.txt | dev2     | Q/ulw&]  | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved41.txt | ""            | ""           | True     | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 8  | SFTP to FTP  PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile21.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved41.txt                         | ""            | ""           | True     | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 9  | SFTP to FTPS PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile31.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved41.txt                         | Administrator | Dev2@dmin123 | True     | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
	   | 10 | SFTP to SFTP PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile41.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/moved41.txt                                                   | dev2          | Q/ulw&]      | True     | [[result]] | Success | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

@FileMoveFromLocalWithoutOverwrite
Scenario Outline: Move file at local location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name             | source         | sourceLocation   | username | password | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | Local to Local   | [[sourcePath]] | c:\movefile0.txt | ""       | ""       | [[destPath]] | C:\moved0.txt                                                                             | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 2  | Local to FTP     | [[sourcePath]] | c:\movefile1.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved0.txt                          | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | Local to FTPS    | [[sourcePath]] | c:\movefile2.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved0.txt                          | Administrator     | Dev2@dmin123 | False    | [[result]] | Success | NO           |                      |                           |
	   | 4  | Local to SFTP    | [[sourcePath]] | c:\movefile3.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/moved0.txt                                                    | dev2              | Q/ulw&]      | False    | [[result]] | Success | NO           |                      |                           |
	   | 5  | Local to UNC     | [[sourcePath]] | c:\movefile4.txt | ""       | ""       | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved0.txt  | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |

@FileMoveFromUNCWithoutOverwrite
Scenario Outline: Move file at UNC location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name         | source         | sourceLocation                                                                              | username | password | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | UNC to Local | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile0.txt | ""       | ""       | [[destPath]] | C:\moved1.txt                                                                             | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 2  | UNC to FTP   | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile1.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved1.txt                          | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | UNC to FTPS  | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile2.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved1.txt                          | Administrator     | Dev2@dmin123 | False    | [[result]] | Success | NO           |                      |                           |
	   | 4  | UNC to SFTP  | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile3.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/moved1.txt                                                    | dev2              | Q/ulw&]      | False    | [[result]] | Success | NO           |                      |                           |
	   | 6  | UNC TO UNC   | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile4.txt | ""       | ""       | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved1.txt  | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |

@FileMoveFromFTPWithoutOverwrite
Scenario Outline: Move file at FTP location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name         | source         | sourceLocation                                                      | username      | password     | destination  | destinationLocation                                                                      | destUsername      | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | FTP to Local | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile0.txt | ""            | ""           | [[destPath]] | C:\moved2.txt                                                                            | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
	   | 2  | FTP to UNC   | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile1.txt | ""            | ""           | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved2.txt | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | FTP to FTPS  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile2.txt | ""            | ""           | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved2.txt                         | Administrator     | Dev2@dmin123 | False    | [[result]] | Success | NO           |                      |                           |
	   | 4  | FTP to SFTP  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile3.txt | ""            | ""           | [[destPath]] | sftp://SVRDEV.premier.local/moved2.txt                                                   | dev2              | Q/ulw&]      | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 5  | FTP to FTP   | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/movefile4.txt | ""            | ""           | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved2.txt                         | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |

@FileMoveFromFTPSWithoutOverwrite
Scenario Outline: Move file at FTPS location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name          | source         | sourceLocation                                                      | username      | password     | destination  | destinationLocation                                                                      | destUsername      | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | FTPS to Local | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile0.txt | Administrator | Dev2@dmin123 | [[destPath]] | C:\moved3.txt                                                                            | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
	   | 2  | FTPS to UNC   | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile1.txt | Administrator | Dev2@dmin123 | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved3.txt | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | FTPS to FTPS  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile2.txt | Administrator | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved3.txt                         | Administrator     | Dev2@dmin123 | False    | [[result]] | Success | NO           |                      |                           |
	   | 4  | FTPS to SFTP  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile3.txt | Administrator | Dev2@dmin123 | [[destPath]] | sftp://SVRDEV.premier.local/moved3.txt                                                   | dev2              | Q/ulw&]      | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 5  | FTPS to FTP   | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/movefile4.txt | Administrator | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved3.txt                         | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |

@FileMoveFromSFTPWithoutOverwrite
Scenario Outline: Move file at SFTP location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Source Path                 | Username   | Password |Source Private Key File | Destination Path                      | Destination Username | Destination Password |Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   |<sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               |<destinationPrivateKeyFile>  | <selected> |       
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
	   | No | Name             | source         | sourceLocation                             | username | password | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | SFTP to Local    | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile0.txt  | dev2     | Q/ulw&]  | [[destPath]] | C:\moved4.txt                                                                             | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
	   | 2  | SFTP to UNC      | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile1.txt  | dev2     | Q/ulw&]  | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved4.txt  | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 3  | SFTP to FTP      | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile2.txt  | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved4.txt                          | ""                | ""           | False    | [[result]] | Success | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 4  | SFTP to FTPS     | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile3.txt  | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved4.txt                          | Administrator     | Dev2@dmin123 | False    | [[result]] | Success | NO           |                      |                           |
	   | 5  | SFTP to SFTP     | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile4.txt  | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/moved4.txt                                                    | dev2              | Q/ulw&]      | False    | [[result]] | Success | NO           |                      |                           |
	   | 6  | SFTP to Local PK | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile01.txt | dev2     | Q/ulw&]  | [[destPath]] | C:\moved41.txt                                                                            | ""                | ""           | False    | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
	   | 7  | SFTP to UNC  PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile11.txt | dev2     | Q/ulw&]  | [[destPath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\moved41.txt | ""                | ""           | False    | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 8  | SFTP to FTP  PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile21.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved41.txt                         | ""                | ""           | False    | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 9  | SFTP to FTPS PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile31.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved41.txt                         | Administrator     | Dev2@dmin123 | False    | [[result]] | Success | NO           | C:\\Temp\\key.opk    |                           |
	   | 10 | SFTP to SFTP PK  | [[sourcePath]] | sftp://SVRDEV.premier.local/movefile41.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/moved41.txt                                                   | dev2              | Q/ulw&]      | False    | [[result]] | Success | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

@FileAndFolderMove
Scenario Outline: Move file at location Null
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the Move file tool is executed
	Then the execution has "<errorOccured>" error
	Examples: 
	   | No | Name           | source         | sourceLocation                                                                              | username | password | destination  | destinationLocation                                              | destUsername  | destPassword | selected | resultVar  | result  | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
	   | 1  | Local to Local | [[sourcePath]] | NULL                                                                                        | ""       | ""       | [[destPath]] | Null                                                             | ""            | ""           | True     | [[result]] | Failure | AN           |                      |                           |
	   | 2  | Local to Local | [[sourcePath]] | C:\moved0.txt                                                                               | ""       | ""       | [[destPath]] | C:\moved0.txt                                                    | ""            | ""           | True     | [[result]] | Failure | AN           |                      |                           |
	   | 3  | UNC to Local   | [[sourcePath]] | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileMoveSharedTestingSite\movefile0.txt | ""       | ""       | [[destPath]] | cv:\moved1.txt                                                   | ""            | ""           | True     | [[result]] | Failure | AN           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 4  | Local to FTP   | [[sourcePath]] | c:\temp\movefile1.txt                                                                       | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORMOVEFILETESTING/moved0.txt | ""            | ""           | True     | [[result]] | Failure | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online	   | 5  | Local to FTPS  | [[sourcePath]] | v:\movefile2.txt                                                                            | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORMOVEFILETESTING/moved0.txt | Administrator | Dev2@dmin123 | True     | [[result]] | Failure | AN           |                      |                           |
	   | 6  | Local to SFTP  | [[sourcePath]] | " "                                                                                         | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/moved0.txt                           | dev2          | Q/ulw&]      | True     | [[result]] | Failure | AN           |                      |                           |
	 
@FileAndFolderMove
Scenario Outline: Move file Validation
	Given I have a variable "[[a]]" with a value "<Val1>"
	And I have a variable "[[b]]" with a value "<Val2>"
	And I have a variable "[[rec(1).a]]" with a value "<Val1>"
	And I have a variable "[[rec(2).a]]" with a value "<Val2>"
	And I have a variable "[[index]]" with a value "1"
	And I have a source path "<File or Folder>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And overwrite is "<selected>"
	And result as "<resultVar>"
	Then validation is "<ValidationResult>"
	And validation message is "<DesignValidation>"
    When the Move file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
       | Source Path                         | Username   | Password | Destination Path                      | Destination Username | Destination Password | Overwrite  |
       | <File or Folder> = <sourceLocation> | <username> | String   | <destination> = <destinationLocation> | <destUsername>       | String               | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| No | File or Folder                 | Val1              | Val2            | sourceLocation         | username              | password | destination                    | destinationLocation  | destUsername | destPassword | selected | resultVar  | result  | errorOccured | ValidationResult | DesignValidation                                                              | OutputError                                                                     | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | [[sourcePath]]                 |                   |                 | c:\temp\movefile57.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved057.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 2  | [[sourcePath]]                 |                   |                 | c:\temp\movefile58.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved058.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 3  | [[sourcePath]]                 |                   |                 | c:\temp\movefile59.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved059.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 4  | [[sourcePath]]                 |                   |                 | c:\temp\movefile60.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved060.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 5  | [[sourcePath]]                 |                   |                 | c:\temp\movefile61.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved061.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 6  | [[sourcePath]]                 |                   |                 | c:\temp\movefile5.txt  | ""                    | ""       | [[destPath]]                   | C:\temp\moved05.txt  | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 7  | [[sourcePath]]                 |                   |                 | c:\temp\movefile6.txt  | ""                    | ""       | [[destPath]]                   | C:\temp\moved06.txt  | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 8  | [[sourcePath]]                 |                   |                 | c:\temp\movefile7.txt  | ""                    | ""       | [[destPath]]                   | C:\temp\moved07.txt  | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 9  | [[sourcePath]]                 |                   |                 | c:\temp\movefile8.txt  | ""                    | ""       | [[destPath]]                   | C:\temp\moved08.txt  | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 10 | [[sourcePath]]                 |                   |                 | c:\temp\movefile9.txt  | ""                    | ""       | [[destPath]]                   | C:\temp\moved09.txt  | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 11 | [[sourcePath]]                 |                   |                 | c:\temp\movefile10.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved010.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 12 | [[sourcePath]]                 |                   |                 | c:\temp\movefile11.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved011.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 13 | [[sourcePath]]                 |                   |                 | c:\temp\movefile12.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved012.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 14 | [[sourcePath]]                 |                   |                 | c:\temp\movefile13.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved013.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |
		| 15 | [[sourcePath]]                 |                   |                 | c:\temp\movefile14.txt | ""                    | ""       | [[destPath]]                   | C:\temp\moved014.txt | ""           | ""           | True     | [[result]] | Success | NO           | False            | ""                                                                            | ""                                                                              |                      |                           |				
		| 42 | [[a&]]                         |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | File or Folder - Variable name [[a&]] contains invalid character(s)           | 1.File or Folder - Variable name [[a&]] contains invalid character(s)           |                      |                           |
		| 43 | [[rec(**).a]]                  |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | File or Folder - Recordset index (**) contains invalid character(s)           | 1.File or Folder - Recordset index (**) contains invalid character(s)           |                      |                           |
		| 44 | [[a]                           |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | File or Folder - Invalid expression: opening and closing brackets don"t match | 1.File or Folder - Invalid expression: opening and closing brackets don"t match |                      |                           |
		| 45 | [[rec(a]]                      |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | File or Folder - Recordset name [[rec(a]] contains invalid character(s)       | 1.File or Folder - Recordset name [[rec(a]] contains invalid character(s)       |                      |                           |
		| 46 | c(*()                          |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Please supply a valid File or Folder                                          | 1.Please supply a valid File or Folder                                          |                      |                           |
		| 47 | [[rec([[inde$x]]).a]]          |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | File or Folder - Variable name [[inde$x]]  contains invalid character(s)      | 1.File or Folder - Variable name [[inde$x]]  contains invalid character(s)      |                      |                           |
		| 48 | ghjghj                         |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Please supply a valid File or Folder                                          | 1.Please supply a valid File or Folder                                          |                      |                           |
		| 49 | ""                             |                   |                 |                        | ""                    | ""       | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | File or Folder cannot be empty or only white space                            | 1.File or Folder cannot be empty or only white space                            |                      |                           |
		| 50 | [[sourcePath]]                 |                   |                 | c:\copyfile41.txt      | ""                    | ""       | [[a&]]                         | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Destination - Variable name [[a&]] contains invalid character(s)              | 1.Destination - Variable name [[a&]] contains invalid character(s)              |                      |                           |
		| 51 | [[sourcePath]]                 |                   |                 | c:\copyfile42.txt      | ""                    | ""       | [[rec(**).a]]                  | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Destination - Recordset index (**) contains invalid character(s)              | 1.Destination - Recordset index (**) contains invalid character(s)              |                      |                           |
		| 52 | [[sourcePath]]                 |                   |                 | c:\copyfile43.txt      | ""                    | ""       | [[a]                           | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Destination - Invalid expression: opening and closing brackets don"t match    | 1.Destination - Invalid expression: opening and closing brackets don"t match    |                      |                           |
		| 53 | [[sourcePath]]                 |                   |                 | c:\copyfile44.txt      | ""                    | ""       | [[rec(a]]                      | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Destination - Recordset name [[rec(a]] contains invalid character(s)          | 1.Destination - Recordset name [[rec(a]] contains invalid character(s)          |                      |                           |
		| 54 | [[sourcePath]]                 |                   |                 | c:\copyfile45.txt      | ""                    | ""       | c(*()                          | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Please supply a valid File or Folder                                          | 1.Please supply a valid File or Folder                                          |                      |                           |
		| 55 | [[sourcePath]]                 |                   |                 | c:\copyfile46.txt      | ""                    | ""       | [[rec([[inde$x]]).a]]          | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Destination - Variable name [[inde$x]]  contains invalid character(s)         | 1.Destination - Variable name [[inde$x]]  contains invalid character(s)         |                      |                           |
		| 56 | [[sourcePath]]                 |                   |                 | c:\copyfile47.txt      | ""                    | ""       | ghjghj                         | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Please supply a valid File or Folder                                          | 1.Please supply a valid File or Folder                                          |                      |                           |
		| 57 | [[sourcePath]]                 |                   |                 | c:\copyfile48.txt      | ""                    | ""       | ""                             | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Destination cannot be empty or only white space                               | 1.Destination cannot be empty or only white space                               |                      |                           |
		| 58 | [[sourcePath]]                 |                   |                 | c:\copyfile49.txt      | [[a&]]                | String   | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Username - Variable name [[a&]] contains invalid character(s)                 | 1.Username - Variable name [[a&]] contains invalid character(s)                 |                      |                           |
		| 59 | [[sourcePath]]                 |                   |                 | c:\copyfile50.txt      | [[rec(**).a]]         | String   | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Username - Recordset index (**) contains invalid character(s)                 | 1.Username - Recordset index (**) contains invalid character(s)                 |                      |                           |
		| 60 | [[sourcePath]]                 |                   |                 | c:\copyfile51.txt      | [[a]                  | String   | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Username - Invalid expression: opening and closing brackets don"t match       | 1.Username - Invalid expression: opening and closing brackets don"t match       |                      |                           |
		| 61 | [[sourcePath]]                 |                   |                 | c:\copyfile52.txt      | [[rec(a]]             | String   | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Username - Recordset name [[rec(a]] contains invalid character(s)             | 1.Username - Recordset name [[rec(a]] contains invalid character(s)             |                      |                           |
		| 62 | [[sourcePath]]                 |                   |                 | c:\copyfile53.txt      | [[rec([[inde$x]]).a]] | String   | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Username - Variable name [[inde$x]]  contains invalid character(s)            | 1.Username - Variable name [[inde$x]]  contains invalid character(s)            |                      |                           |
		| 64 | [[sourcePath]]                 |                   |                 | c:\copyfile55.txt      | [[a&]]                | String   | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Username - Variable name [[a&]] contains invalid character(s)                 | 1.Destination Username - Variable name [[a&]] contains invalid character(s)     |                      |                           |
		| 65 | [[sourcePath]]                 |                   |                 | c:\copyfile56.txt      | [[rec(**).a]]         | String   | [[destPath]]                   | C:\moved0.txt        | ""           | ""           | True     | [[result]] | ""      | AN           | True             | Username - Recordset index (**) contains invalid character(s)                 | 1.Destination Username - Recordset index (**) contains invalid character(s)     |                      |                           |
