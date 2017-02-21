@fileFeature
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
		 | No | source         | sourceLocation                                            | username          | password | destination  | destinationLocation                                        | destUsername      | destPassword | selected | resultVar  | result    | errorOccured | sourcePrivateKeyFile | destinationPrivateKeyFile |
		 | 1  | [[sourcePath]] | c:\copyfile0.txt                                          | ""                | ""       | [[destPath]] | C:\copied00.txt                                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 2  | [[sourcePath]] | c:\copyfile1.txt                                          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied0.txt          | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 3  | [[sourcePath]] | c:\copyfile2.txt                                          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied0.txt          | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 4  | [[sourcePath]] | c:\copyfile3.txt                                          | ""                | ""       | [[destPath]] | sftp://rsaklfsvrgendev/copied0.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 5  | [[sourcePath]] | c:\copyfile4.txt                                          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copied0.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 6  | [[sourcePath]] | c:\copyfile5.txt                                          | ""                | ""       | [[destPath]] | sftp://rsaklfsvrgendev/copied61.txt                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 7  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copyfile0.txt | ""                | ""       | [[destPath]] | C:\copied10.txt                                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 8  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copyfile1.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied1.txt          | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 9  | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copyfile2.txt | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied1.txt          | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           | "C:\\Temp\\key.opk"  |                           |
		 | 10 | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copyfile3.txt | ""                | ""       | [[destPath]] | sftp://rsaklfsvrgendev/copied1.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 11 | [[sourcePath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copyfile5.txt | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copied1.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 12 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile0.txt          | ""                | ""       | [[destPath]] | C:\copied20.txt                                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 13 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile1.txt          | ""                | ""       | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copied2.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 14 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile2.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied2.txt          | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 15 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile3.txt          | ""                | ""       | [[destPath]] | sftp://rsaklfsvrgendev/copied2.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 16 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copyfile4.txt          | ""                | ""       | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied2.txt          | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 17 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile0.txt          | integrationtester | I73573r0 | [[destPath]] | C:\copied30.txt                                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 18 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile1.txt          | integrationtester | I73573r0 | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copied3.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 19 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile2.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied3.txt          | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 20 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile3.txt          | integrationtester | I73573r0 | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied3.txt          | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 21 | [[sourcePath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copyfile4.txt          | integrationtester | I73573r0 | [[destPath]] | sftp://rsaklfsvrgendev/copied3.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 22 | [[sourcePath]] | sftp://rsaklfsvrgendev/copyfile0.txt                      | dev2              | Q/ulw&]  | [[destPath]] | C:\copied40.txt                                            | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 23 | [[sourcePath]] | sftp://rsaklfsvrgendev/copyfile1.txt                      | dev2              | Q/ulw&]  | [[destPath]] | \\\\RSAKLFSVRPDC\FileSystemShareTestingSite\copied4.txt | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 24 | [[sourcePath]] | sftp://rsaklfsvrgendev/copyfile2.txt                      | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/copied4.txt          | ""                | ""           | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 25 | [[sourcePath]] | sftp://rsaklfsvrgendev/copyfile3.txt                      | dev2              | Q/ulw&]  | [[destPath]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/copied4.txt          | integrationtester | I73573r0     | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 26 | [[sourcePath]] | sftp://rsaklfsvrgendev/copyfile5.txt                      | dev2              | Q/ulw&]  | [[destPath]] | sftp://rsaklfsvrgendev/copied4.txt                               | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           |                      |                           |
		 | 27 | [[sourcePath]] | sftp://rsaklfsvrgendev/copyfile6.txt                      | dev2              | Q/ulw&]  | [[destPath]] | sftp://rsaklfsvrgendev/copied51.txt                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    |                           |
		 | 28 | [[sourcePath]] | sftp://rsaklfsvrgendev/copyfile7.txt                      | dev2              | Q/ulw&]  | [[destPath]] | sftp://rsaklfsvrgendev/copied71.txt                              | dev2              | Q/ulw&]      | True     | [[result]] | "Success" | NO           | C:\\Temp\\key.opk    | C:\\Temp\\key.opk         |
                       																										 
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
		 | 4  | [[variable]] | c:\copyfile0.txt  | ""       | ""       | [[destPath]] | v:\                 | ""           | ""           | True     | [[result]] | Error  | An           |                      |                           |


                   																										 