@FileAndFolderCreate
Feature: Create
	In order to be able to create files
	as a Warewolf user
	I want a tool that creates a file at a given location


Scenario Outline: Create file at location
	Given I have a destination path "<destination>" with value "<destinationLocation>"
	And overwrite is "<selected>"
	And destination credentials as "<username>" and "<password>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
	When the create file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | File or Folder                        | Overwrite  | Username   | Password | Destination Private Key File |
         | <destination> = <destinationLocation> | <selected> | <username> | String   | <destinationPrivateKeyFile>  |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| No | Name       | destination | destinationLocation                                                                             | selected | username        | password     | resultVar  | result  | errorOccured | destinationPrivateKeyFile |
		| 1  | Local      | [[path]]    | c:\myfile.txt                                                                                   | True     | ""              | ""           | [[result]] | Success | NO           |                           |
		| 2  | UNC        | [[path]]    | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileCreateSharedTestingSite\test.txt        | True     | ""              | ""           | [[result]] | Success | NO           |                           |
		| 3  | UNC Secure | [[path]]    | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileCreateSharedTestingSite\Secure\test.txt | True     | ""              | ""           | [[result]] | Success | NO           |                           |
		| 4  | FTP        | [[path]]    | ftp://DEVOPSPDC.premier.local:1001/FORCREATEFILETESTING/test.txt                                   | True     | ""              | ""           | [[result]] | Success | NO           |                           |
		| 5  | FTPS       | [[path]]    | ftp://DEVOPSPDC.premier.local:1002/FORCREATEFILETESTING/test.txt                                   | True     | Administrator   | Dev2@dmin123 | [[result]] | Success | NO           |                           |
		| 6  | SFTP       | [[path]]    | sftp://SVRDEV.premier.local/test.txt                                                            | True     | dev2            | Q/ulw&]      | [[result]] | Success | NO           |                           |
		| 7  | SFTP       | [[path]]    | sftp://SVRDEV.premier.local/test1.txt                                                           | True     | dev2            | Q/ulw&]      | [[result]] | Success | NO           | C:\\Temp\\key.opk         |

Scenario Outline: Create file at location with overwrite disabled
	Given I have a destination path "<destination>" with value "<destinationLocation>"
	And overwrite is "<selected>"
	And destination credentials as "<username>" and "<password>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
	When the create file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | File or Folder                        | Overwrite  | Username   | Password | Destination Private Key File |
         | <destination> = <destinationLocation> | <selected> | <username> | String   | <destinationPrivateKeyFile>  |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| No | Name       | destination | destinationLocation                                                                             | selected | username        | password     | resultVar  | result  | errorOccured | destinationPrivateKeyFile |
		| 1  | Local      | [[path]]    | c:\myfile.txt                                                                                   | False    | ""              | ""           | [[result]] | Success | NO           |                           |
		| 2  | UNC        | [[path]]    | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileCreateSharedTestingSite\test.txt        | False    | ""              | ""           | [[result]] | Success | NO           |                           |
		| 3  | UNC Secure | [[path]]    | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileCreateSharedTestingSite\Secure\test.txt | False    | ""              | ""           | [[result]] | Success | NO           |                           |
		| 4  | FTP        | [[path]]    | ftp://DEVOPSPDC.premier.local:1001/FORCREATEFILETESTING/test.txt                                   | False    | ""              | ""           | [[result]] | Success | NO           |                           |
		| 5  | FTPS       | [[path]]    | ftp://DEVOPSPDC.premier.local:1002/FORCREATEFILETESTING/test.txt                                   | False    | Administrator   | Dev2@dmin123 | [[result]] | Success | NO           |                           |
		| 6  | SFTP       | [[path]]    | sftp://SVRDEV.premier.local/test.txt                                                            | False    | dev2            | Q/ulw&]      | [[result]] | Success | NO           |                           |
		| 7  | SFTP       | [[path]]    | sftp://SVRDEV.premier.local/test1.txt                                                           | False    | dev2            | Q/ulw&]      | [[result]] | Success | NO           | C:\\Temp\\key.opk         |


Scenario Outline: Create file at location Nulls
	Given I have a destination path "<destination>" with value "<destinationLocation>"
	And overwrite is "<selected>"
	And destination credentials as "<username>" and "<password>"
	And use private public key for destination is "<destinationPrivateKeyFile>"
	And result as "<resultVar>"
	When the create file tool is executed
	Then the execution has "<errorOccured>" error
	Examples: 
		| No | Name       | destination | destinationLocation                                                                            | selected | username       | password     | resultVar  | result  | errorOccured | destinationPrivateKeyFile |
		| 1  | Local      | [[path]]    | NULL                                                                                           | True     |                |              | [[result]] | Failure | AN           |                           |
		| 2  | Local      | [[path]]    | v:\myfile.txt                                                                                  | True     |                |              | [[result]] | Failure | AN           |                           |
		| 3  | SFTP       | [[path]]    | sftp://SVRDEV.premier.local/test1.txt                                                          | True     | ""             | Q/ulw&]      | [[result]] | Failure | AN           | C:\\Temp\                 |
#TODO: re-introduce this test in the premier.local domain		| 5  | UNC Secure | [[path]]    | \\\\SVRDEV.premier.local\FileSystemShareTestingSite\FileCreateSharedTestingSite\Secure\test.tx | True     | Administrator  | Dev2@dmin123 | [[result]] | Failure | AN           |                           |


Scenario Outline: Create file at location with invalid directories
	Given I have a destination path "<destination>" with value "<destinationLocation>"
	And overwrite is "<selected>"
	And destination credentials as "<username>" and "<password>"
	And result as "<resultVar>"
	When the create file tool is executed
	Then the result variable "<resultVar>" will be "<result>"
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | File or Folder                        | Overwrite  | Username   | Password |
         | <destination> = <destinationLocation> | <selected> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| No | Name  | destination  | destinationLocation | selected | username | password | resultVar  | result | errorOccured |
		| 1  | Local | [[variable]] | ""                  | False    | dev2     | Q/ulw&]  | [[result]] |        | AN           |
		| 2  | Local | [[var]]      |                     | False    | dev2     | Q/ulw&]  | [[result]] |        | AN           |
		| 3  | Local | 8751         | 8751                | False    | dev2     | Q/ulw&]  | [[result]] |        | AN           |

