Feature: Copy
	In order to be able to Copy File or Folder 
	as a Warewolf user
	I want a tool that Copy File or Folder from a given location to another location

@CopyFileFromLocal
@CopyFileFromLocalWithOverwrite
Scenario Outline: Copy file at local location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation   | username | password | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | c:\copyfile0.txt | ""       | ""       | [[destPath]] | C:\copied00.txt                                                                           | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online		 | 2  | [[sourcePath]] | c:\copyfile1.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied0.txt                         | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online		 | 3  | [[sourcePath]] | c:\copyfile2.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied0.txt                         | Administrator     | Dev2@dmin123 | True     | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until SVRDEV.premier.local is back online		 | 4  | [[sourcePath]] | c:\copyfile3.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/copied0.txt                                                   | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until SVRDEV.premier.local is back online		 | 5  | [[sourcePath]] | c:\copyfile5.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/copied61.txt                                                  | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |

@Ignore #DevOps: Ignoring until DEVOPSPDC.premier.local is back online
@CopyFileFromFTP
@CopyFileFromFTPWithOverwrite
Scenario Outline: Copy file at FTP location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation                                                      | username          | password     | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile0.txt | ""                | ""           | [[destPath]] | C:\copied20.txt                                                                           | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile2.txt | ""                | ""           | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied2.txt                         | Administrator     | Dev2@dmin123 | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile3.txt | ""                | ""           | [[destPath]] | sftp://SVRDEV.premier.local/copied2.txt                                                   | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile4.txt | ""                | ""           | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied2.txt                         | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |

@Ignore #DevOps: Ignoring until DEVOPSPDC.premier.local is back online
@CopyFileFromFTPS
@CopyFileFromFTPSWithOverwrite
Scenario Outline: Copy file at FTPS location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation                                                      | username          | password     | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile0.txt | Administrator     | Dev2@dmin123 | [[destPath]] | C:\copied30.txt                                                                           | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile2.txt | Administrator     | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied3.txt                         | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile3.txt | Administrator     | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied3.txt                         | Administrator     | Dev2@dmin123 | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile4.txt | Administrator     | Dev2@dmin123 | [[destPath]] | sftp://SVRDEV.premier.local/copied3.txt                                                   | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |

@Ignore #DevOps: Ignoring until SVRDEV.premier.local is back online
@CopyFileFromSFTP
@CopyFileFromSFTPWithOverwrite
Scenario Outline: Copy file at SFTP location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation                            | username | password | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile0.txt | dev2     | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                                                           | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile2.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied4.txt                         | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile3.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied4.txt                         | Administrator     | Dev2@dmin123 | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile5.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/copied4.txt                                                   | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile6.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/copied51.txt                                                  | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 6  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile7.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/copied71.txt                                                  | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

@CopyFileFromLocal
@CopyFileFromLocalWithoutOverwrite
Scenario Outline: Copy file at local location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation   | username          | password     | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | c:\copyfile0.txt | ""                | ""           | [[destPath]] | C:\copied00.txt                                                                           | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online		 | 2  | [[sourcePath]] | c:\copyfile1.txt | ""                | ""           | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied0.txt                         | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until DEVOPSPDC.premier.local is back online		 | 3  | [[sourcePath]] | c:\copyfile2.txt | ""                | ""           | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied0.txt                         | Administrator     | Dev2@dmin123 | False    | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until SVRDEV.premier.local is back online		 | 4  | [[sourcePath]] | c:\copyfile3.txt | ""                | ""           | [[destPath]] | sftp://SVRDEV.premier.local/copied0.txt                                                   | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
#DevOps: Ignoring until SVRDEV.premier.local is back online		 | 5  | [[sourcePath]] | c:\copyfile5.txt | ""                | ""           | [[destPath]] | sftp://SVRDEV.premier.local/copied61.txt                                                  | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |

@Ignore #DevOps: Ignoring until DEVOPSPDC.premier.local is back online
@CopyFileFromFTP
@CopyFileFromFTPWithoutOverwrite
Scenario Outline: Copy file at FTP location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation                                                      | username | password | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile0.txt | ""       | ""       | [[destPath]] | C:\copied20.txt                                                                           | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile2.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied2.txt                         | Administrator     | Dev2@dmin123 | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile3.txt | ""       | ""       | [[destPath]] | sftp://SVRDEV.premier.local/copied2.txt                                                   | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copyfile4.txt | ""       | ""       | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied2.txt                         | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |

@Ignore #DevOps: Ignoring until DEVOPSPDC.premier.local is back online
@CopyFileFromFTPS
@CopyFileFromFTPSWithoutOverwrite
Scenario Outline: Copy file at FTPS location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation                                                      | username          | password     | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile0.txt | Administrator     | Dev2@dmin123 | [[destPath]] | C:\copied30.txt                                                                           | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile2.txt | Administrator     | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied3.txt                         | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile3.txt | Administrator     | Dev2@dmin123 | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied3.txt                         | Administrator     | Dev2@dmin123 | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copyfile4.txt | Administrator     | Dev2@dmin123 | [[destPath]] | sftp://SVRDEV.premier.local/copied3.txt                                                   | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |

@Ignore #DevOps: Ignoring until SVRDEV.premier.local is back online
@CopyFileFromSFTP
@CopyFileFromSFTPWithoutOverwrite
Scenario Outline: Copy file at SFTP location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	And the result variable "<resultVar>" will be "<result>"
	And the debug inputs as
         | Source Path                 | Username   | Password | Source Private Key File | Destination Path                      | Destination Username | Destination Password | Destination Private Key File | Overwrite  |
         | <source> = <sourceLocation> | <username> | String   | <sourcePrivateKeyFile>  | <destination> = <destinationLocation> | <destUsername>       | String               | <destinationPrivateKeyFile>  | <selected> |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		 | No | source         | sourceLocation                            | username | password | destination  | destinationLocation                                                                       | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile0.txt | dev2     | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                                                           | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile2.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1001/FORCOPYFILETESTING/copied4.txt                         | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile3.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://DEVOPSPDC.premier.local:1002/FORCOPYFILETESTING/copied4.txt                         | Administrator     | Dev2@dmin123 | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile5.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/copied4.txt                                                   | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile6.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/copied51.txt                                                  | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 6  | [[sourcePath]] | sftp://SVRDEV.premier.local/copyfile7.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.premier.local/copied71.txt                                                  | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

@FileAndFolderCopy
Scenario Outline: Copy file at location Null			
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	Examples: 
		 | No | source       | sourceLocation    | username | password | destination  | destinationLocation | destUsername | destPassword | selected | resultVar  | result | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[variable]] | NULL              | ""       | ""       | [[destPath]] | c:\test.txt         | ""           | ""           | True     | [[result]] | Error  | An           |                      |                           |
		 | 2  | [[variable]] | c:\copyfile0.txt  | ""       | ""       | [[destPath]] | NULL                | ""           | ""           | True     | [[result]] | Error  | An           |                      |                           |
		 | 3  | [[variable]] | c:\copyfile0.txt  | ""       | ""       | [[destPath]] | v:\                 | ""           | ""           | True     | [[result]] | Error  | An           |                      |                           |

@FileAndFolderCopy
Scenario Outline: Copy file at location Null with overwrite disabled	
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
    When the copy file tool is executed
	Then the execution has "<errorOccured>" error
	Examples: 
		 | No | source       | sourceLocation   | username | password | destination  | destinationLocation | destUsername | destPassword | selected | resultVar  | result | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[variable]] | NULL             | ""       | ""       | [[destPath]] | c:\test.txt         | ""           | ""           | False    | [[result]] | Error  | An           |                      |                           |
		 | 2  | [[variable]] | c:\copyfile0.txt | ""       | ""       | [[destPath]] | NULL                | ""           | ""           | False    | [[result]] | Error  | An           |                      |                           |
		 | 3  | [[variable]] | c:\copyfile0.txt | ""       | ""       | [[destPath]] | v:\                 | ""           | ""           | False    | [[result]] | Error  | An           |                      |                           |
                   																										 
