Feature: Read File
	In order to be able to Read File
	as a Warewolf user
	I want a tool that reads the contents of a file at a given location


Scenario Outline: Read File at location
	Given I have a variable '<variable>' with value '<location>'
    And input username as '<username>' and password '<password>'
    When the Read file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has '<errorOccured>' error
	And the debug inputs as
         | Directory                   | Username     | Password     |
         | '<variable>' = '<location>' | '<username>' | '<password>' | 
         
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
		Examples: 
		| variable | location                                    | username          | password | resultVar  | result | errorOccured |
		| [[path]] | c:\myfile.txt                               |                   |          | [[result]] | result | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          |                   |          | [[result]] | result | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           |                   |          | [[result]] | result | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | integrationtester | I73573r0 | [[result]] | result | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | dev2              | Q/ulw&]  | [[result]] | result | NO           |
		