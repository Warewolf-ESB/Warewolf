@fileFeature
Feature: Delete
	In order to be able to Delete file
	as a Warewolf user
	I want a tool that Delete a file at a given location


Scenario Outline: Delete file at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	When the delete file tool is executed
	Then the result variable '<resultVar>' will be <result>
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                  | Username   | Password |
         | <source> = <sourceLocation> | <username> | String   |
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| source   | sourceLocation                                                         | username               | password | resultVar  | result  | errorOccured |
		| [[path]] | c:\filetodelete.txt                                                    | ""                     | ""       | [[result]] | Success | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetodelete.txt        | ""                     | ""       | [[result]] | Success | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetodelete.txt | Dev2\IntegrationTester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetodelete.txt                 | ""                     | ""       | [[result]] | Success | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetodele.txt                   | IntegrationTester      | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp://localhost/filetodelete.txt                                      | dev2                   | Q/ulw&]  | [[result]] | Success | NO           |