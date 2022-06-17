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
		 | No | source         | sourceLocation   | username | password | destination  | destinationLocation                                                            | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | c:\copyfile0.txt | ""       | ""       | [[destPath]] | C:\copied00.txt                                                                | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | c:\copyfile1.txt | ""       | ""       | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied0.txt              | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | c:\copyfile2.txt | ""       | ""       | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied0.txt              | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | c:\copyfile3.txt | ""       | ""       | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied0.txt           | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | c:\copyfile4.txt | ""       | ""       | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied0.txt | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 6  | [[sourcePath]] | c:\copyfile5.txt | ""       | ""       | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied61.txt          | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |

@CopyFileFromUNC
@CopyFileFromUNCWithOverwrite
Scenario Outline: Copy file at UNC location
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
		| No | source         | sourceLocation                                                                   | username          | password     | destination  | destinationLocation                                                                       | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile0.txt | ""                | ""           | [[destPath]] | C:\copied10.txt                                                                           | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		| 2  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile1.txt | ""                | ""           | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied1.txt                         | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		| 3  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile2.txt | ""                | ""           | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied1.txt                         | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           | "C:\\Temp\\key.opk"  |                           |
		| 4  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile3.txt | ""                | ""           | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied1.txt                                                   | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		| 5  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile5.txt | ""                | ""           | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied1.txt | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |


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
		 | No | source         | sourceLocation                                                      | username | password | destination  | destinationLocation                                                            | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile0.txt | dev2     | Q/ulw&]  | [[destPath]] | C:\copied20.txt                                                                | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile1.txt | dev2     | Q/ulw&]  | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied2.txt | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile2.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied2.txt              | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile3.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied2.txt           | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile4.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied2.txt              | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |

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
		 | No | source         | sourceLocation                                                      | username          | password     | destination  | destinationLocation                                                                 | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile0.txt | dev2              | Q/ulw&]      | [[destPath]] | C:\copied30.txt                                                                              | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile1.txt | dev2              | Q/ulw&]      | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied3.txt | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile2.txt | dev2              | Q/ulw&]      | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied3.txt                            | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile3.txt | dev2              | Q/ulw&]      | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied3.txt                            | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile4.txt | dev2              | Q/ulw&]      | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied3.txt                                                      | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |

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
		 | No | source         | sourceLocation                            | username | password | destination  | destinationLocation                                                                          | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile0.txt | dev2              | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                                                              | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile1.txt | dev2              | Q/ulw&]  | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied4.txt | ""           | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile2.txt | dev2              | Q/ulw&]  | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied4.txt                            | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile3.txt | dev2              | Q/ulw&]  | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied4.txt                            | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile5.txt | dev2              | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied4.txt                                                      | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile6.txt | dev2              | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied51.txt                                                     | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 6  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile7.txt | dev2              | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied71.txt                                                     | dev2         | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

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
		 | No | source         | sourceLocation   | username          | password     | destination  | destinationLocation                                                                          | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | c:\copyfile0.txt | ""                | ""           | [[destPath]] | C:\copied00.txt                                                                              | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | c:\copyfile1.txt | ""                | ""           | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied0.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | c:\copyfile2.txt | ""                | ""           | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied0.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | c:\copyfile3.txt | ""                | ""           | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied0.txt                                                      | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | c:\copyfile4.txt | ""                | ""           | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied0.txt | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 6  | [[sourcePath]] | c:\copyfile5.txt | ""                | ""           | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied61.txt                                                     | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |

@CopyFileFromUNC
@CopyFileFromUNCWithoutOverwrite
Scenario Outline: Copy file at UNC location with overwrite disabled
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
		| No | source         | sourceLocation                                                                                 | username | password | destination  | destinationLocation                                                                       | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		| 1  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile0.txt | ""       | ""       | [[destPath]] | C:\copied10.txt                                                                           | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		| 2  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile1.txt | ""       | ""       | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied1.txt                         | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		| 3  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile2.txt | ""       | ""       | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied1.txt                         | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           | "C:\\Temp\\key.opk"  |                           |
		| 4  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile3.txt | ""       | ""       | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied1.txt                                                   | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		| 5  | [[sourcePath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile5.txt | ""       | ""       | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied1.txt | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |

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
		 | No | source         | sourceLocation                                                      | username | password | destination  | destinationLocation                                                                          | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile0.txt | dev2              | Q/ulw&]  | [[destPath]] | C:\copied20.txt                                                                              | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile1.txt | dev2              | Q/ulw&]  | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied2.txt | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile2.txt | dev2              | Q/ulw&]  | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied2.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile3.txt | dev2              | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied2.txt                                                      | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | ftp://localhost/FORCOPYFILETESTING/copyfile4.txt | dev2              | Q/ulw&]  | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied2.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |

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
		 | No | source         | sourceLocation                                                      | username | password | destination  | destinationLocation                                                                 | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile0.txt | dev2     | Q/ulw&]  | [[destPath]] | C:\copied30.txt                                                                              | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile1.txt | dev2     | Q/ulw&]  | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied3.txt | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile2.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied3.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile3.txt | dev2     | Q/ulw&]  | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied3.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | ftp://localhost:1010/FORCOPYFILETESTING/copyfile4.txt | dev2     | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied3.txt                                                      | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |

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
		 | No | source         | sourceLocation                            | username | password | destination  | destinationLocation                                                                          | destUsername | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile0.txt | dev2              | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                                                              | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile1.txt | dev2              | Q/ulw&]  | [[destPath]] | \\\\localhost\FileSystemShareTestingSite\FileCopySharedTestingSite\copied4.txt | ""           | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile2.txt | dev2              | Q/ulw&]  | [[destPath]] | ftp://localhost/FORCOPYFILETESTING/copied4.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile3.txt | dev2              | Q/ulw&]  | [[destPath]] | ftp://localhost:1010/FORCOPYFILETESTING/copied4.txt                            | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile5.txt | dev2              | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied4.txt                                                      | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 6  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile6.txt | dev2              | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied51.txt                                                     | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 7  | [[sourcePath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copyfile7.txt | dev2              | Q/ulw&]  | [[destPath]] | sftp://3xhcmicj2djiu.southafricanorth.azurecontainer.io/upload/copied71.txt                                                     | dev2         | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

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
                   																										 
