﻿@FileAndFolderCopy
Feature: Copy
	In order to be able to Copy File or Folder 
	as a Warewolf user
	I want a tool that Copy File or Folder from a given location to another location

Scenario Outline: Copy file at location
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
	And I authenticate for share at "\\RSAKLFSVRPDC\FileSystemShareTestingSite" as user "dev2\IntegrationTester" with saved password
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
		 | No | source         | sourceLocation                                                                      | username          | password | destination  | destinationLocation                                                               | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | c:\copyfile0.txt                                                                    | ""                | ""       | [[destPath]] | C:\copied00.txt                                                                   | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | c:\copyfile1.txt                                                                    | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied0.txt                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | c:\copyfile2.txt                                                                    | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied0.txt                            | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | c:\copyfile3.txt                                                                    | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied0.txt                                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | c:\copyfile4.txt                                                                    | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied0.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 6  | [[sourcePath]] | c:\copyfile5.txt                                                                    | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied61.txt                                             | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 7  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile0.txt | ""                | ""       | [[destPath]] | C:\copied10.txt                                                                   | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 8  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile1.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied1.txt                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 9  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile2.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied1.txt                            | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           | "C:\\Temp\\key.opk"  |                           |
		 | 10 | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile3.txt | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied1.txt                                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 11 | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile5.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied1.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 12 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile0.txt                            | ""                | ""       | [[destPath]] | C:\copied20.txt                                                                   | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 13 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile1.txt                            | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied2.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 14 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile2.txt                            | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied2.txt                            | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 15 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile3.txt                            | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied2.txt                                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 16 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile4.txt                            | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied2.txt                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 17 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile0.txt                            | integrationtester | I73573r0 | [[destPath]] | C:\copied30.txt                                                                   | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 18 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile1.txt                            | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied3.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 19 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile2.txt                            | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied3.txt                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 20 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile3.txt                            | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied3.txt                            | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 21 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile4.txt                            | integrationtester | I73573r0 | [[destPath]] | sftp://SVRDEV.dev2.local/copied3.txt                                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 22 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile0.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                                                   | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 23 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile1.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied4.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 24 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile2.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied4.txt                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 25 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile3.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied4.txt                            | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 26 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile5.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.dev2.local/copied4.txt                                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 27 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile6.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.dev2.local/copied51.txt                                             | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 28 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile7.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.dev2.local/copied71.txt                                             | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |
           
Scenario Outline: Copy file at location with overwrite disabled
	Given I have a source path "<source>" with value "<sourceLocation>" 
	And source credentials as "<username>" and "<password>"
	And I have a destination path "<destination>" with value "<destinationLocation>"
    And destination credentials as "<destUsername>" and "<destPassword>"
	And overwrite is "<selected>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
	And I authenticate for share at "\\RSAKLFSVRPDC\FileSystemShareTestingSite" as user "dev2\IntegrationTester" with saved password
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
		 | No | source         | sourceLocation                                                                      | username          | password | destination  | destinationLocation                                                               | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | c:\copyfile0.txt                                                                    | ""                | ""       | [[destPath]] | C:\copied00.txt                                                                   | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | c:\copyfile1.txt                                                                    | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied0.txt                            | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | c:\copyfile2.txt                                                                    | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied0.txt                            | integrationtester | I73573r0     | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | c:\copyfile3.txt                                                                    | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied0.txt                                              | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | c:\copyfile4.txt                                                                    | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied0.txt | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 6  | [[sourcePath]] | c:\copyfile5.txt                                                                    | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied61.txt                                             | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 7  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile0.txt | ""                | ""       | [[destPath]] | C:\copied10.txt                                                                   | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 8  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile1.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied1.txt                            | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 9  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile2.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied1.txt                            | integrationtester | I73573r0     | False    | [[result]] | "Success" | NO           | "C:\\Temp\\key.opk"  |                           |
		 | 10 | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile3.txt | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied1.txt                                              | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 11 | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copyfile5.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied1.txt | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 12 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile0.txt                            | ""                | ""       | [[destPath]] | C:\copied20.txt                                                                   | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 13 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile1.txt                            | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied2.txt | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 14 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile2.txt                            | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied2.txt                            | integrationtester | I73573r0     | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 15 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile3.txt                            | ""                | ""       | [[destPath]] | sftp://SVRDEV.dev2.local/copied2.txt                                              | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 16 | [[sourcePath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copyfile4.txt                            | ""                | ""       | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied2.txt                            | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 17 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile0.txt                            | integrationtester | I73573r0 | [[destPath]] | C:\copied30.txt                                                                   | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 18 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile1.txt                            | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied3.txt | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 19 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile2.txt                            | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied3.txt                            | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 20 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile3.txt                            | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied3.txt                            | integrationtester | I73573r0     | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 21 | [[sourcePath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copyfile4.txt                            | integrationtester | I73573r0 | [[destPath]] | sftp://SVRDEV.dev2.local/copied3.txt                                              | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 22 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile0.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                                                   | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 23 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile1.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\FileCopySharedTestingSite\copied4.txt | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 24 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile2.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrpdc:1001/FORCOPYFILETESTING/copied4.txt                            | ""                | ""           | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 25 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile3.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrpdc:1002/FORCOPYFILETESTING/copied4.txt                            | integrationtester | I73573r0     | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 26 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile5.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.dev2.local/copied4.txt                                              | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           |                      |                           |
		 | 27 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile6.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.dev2.local/copied51.txt                                             | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 28 | [[sourcePath]] | sftp://SVRDEV.dev2.local/copyfile7.txt                                              | dev2              | Q/ulw&]  | [[destPath]] | sftp://SVRDEV.dev2.local/copied71.txt                                             | dev2              | Q/ulw&]      | False    | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |

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
                   																										 
