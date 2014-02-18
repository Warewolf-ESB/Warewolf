@fileFeature
Feature: Read File
	In order to be able to Read File
	as a Warewolf user
	I want a tool that reads the contents of a file at a given location


Scenario Outline: Read File at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And result as '<resultVar>'
	When the read file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Input Path                  | Username   | Password |
         | <source> = <sourceLocation> | <username> | String   |
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
	Examples: 
		| source   | sourceLocation                                                       | username               | password | resultVar  | result | errorOccured |
		| [[path]] | c:\filetoread.txt                                                    | ""                     | ""       | [[result]] | String | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\filetoread.txt        | ""                     | ""       | [[result]] | String | NO           |
		| [[path]] | \\\\RSAKLFSVRSBSPDC\FileSystemShareTestingSite\Secure\filetoread.txt | Dev2\IntegrationTester | I73573r0 | [[result]] | String | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1001/FORTESTING/filetoread.txt                 | ""                     | ""       | [[result]] | String | NO           |
		| [[path]] | ftp://rsaklfsvrsbspdc:1002/FORTESTING/filetodele.txt                 | IntegrationTester      | I73573r0 | [[result]] | String | NO           |
		| [[path]] | sftp://localhost/filetoread.txt                                      | dev2                   | Q/ulw&]  | [[result]] | String | NO           |