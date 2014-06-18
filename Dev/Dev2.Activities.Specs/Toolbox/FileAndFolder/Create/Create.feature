@fileFeature
Feature: Create
	In order to be able to create files
	as a Warewolf user
	I want a tool that creates a file at a given location


Scenario Outline: Create file at location
	Given I have a destination path '<destination>' with value '<destinationLocation>'
	And overwrite is '<selected>'
	And destination credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	When the create file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | File or Folder                        | Overwrite  | Username   | Password |
         | <destination> = <destinationLocation> | <selected> | <username> | String   |
	And the debug output as
		|                        |
		| <resultVar> = <result> |
	Examples: 
		| Name       | destination | destinationLocation                                            | selected | username               | password | resultVar       | result  | errorOccured |
		| Local      | [[path]]    | c:\myfile.txt                                                  | True     | ""                     | ""       | [[result]]      | Success | NO           |
		| UNC        | [[path]]    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\test.txt        | True     | ""                     | ""       | [[result]]      | Success | NO           |
		| UNC Secure | [[path]]    | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\test.txt | True     | Dev2\IntegrationTester | I73573r0 | [[result]]      | Success | NO           |
		| FTP        | [[path]]    | ftp://rsaklfsvrsbspdc:1001/FORTESTING/test.txt                 | True     | ""                     | ""       | [[result]]      | Success | NO           |
		| FTPS       | [[path]]    | ftp://rsaklfsvrsbspdc:1002/FORTESTING/test.txt                 | True     | IntegrationTester      | I73573r0 | [[result]]      | Success | NO           |
		| SFTP       | [[path]]    | sftp://localhost/test.txt                                      | True     | dev2                   | Q/ulw&]  | [[result]]      | Success | NO           |
		#| SFTP       | [[path]]    | sftp://localhost/test.txt                                      | True     | dev2                   | Q/ulw&]  | [[result]][[a]] | Success | AN           |