@fileFeature
Feature: Write File
	In order to be able to Write File
	as a Warewolf user
	I want a tool that writes a file at a given location


Scenario Outline: Write file at location
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And overwrite is '<selected>'
	And Method is '<method>'
	And input contents as '<content>'     
	And result as '<resultVar>'
    When the write file tool is executed
	Then the result variable '<resultVar>' will be '<result>'
	And the execution has "<errorOccured>" error
	And the debug inputs as
         | Output Path                 | Method   | Username     | Password     | Overwrite  | File Contents |
         | <source> = <sourceLocation> | <method> | '<username>' | '<password>' | <selected> | <content>     |
	And the debug output as
		| Result                     |
		| '<resultVar>' = '<result>' |
		Examples: 
		| source   | sourceLocation                              | method        | content        | selected | username          | password | resultVar  | result  | errorOccured |
		| [[path]] | c:\myfile.txt                               | Overwrite     | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Overwrite     | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Overwrite     | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Overwrite     | warewolf rules | True     | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Overwrite     | warewolf rules | True     | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               | Append Top    | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Append Top    | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Append Top    | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Append Top    | warewolf rules | True     | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Append Top    | warewolf rules | True     | dev2              | Q/ulw&]  | [[result]] | Success | NO           |
		| [[path]] | c:\myfile.txt                               | Append Bottom | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | \\rsaklfsvrtfsbld\testing\test.txt          | Append Bottom | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftp:\\dev2.co.za\testing\test.txt           | Append Bottom | warewolf rules | True     |                   |          | [[result]] | Success | NO           |
		| [[path]] | ftps:\\dev2ftps.dev2.local\testing\test.txt | Append Bottom | warewolf rules | True     | integrationtester | I73573r0 | [[result]] | Success | NO           |
		| [[path]] | sftp:\\dev2.co.za\testing\test.txt          | Append Bottom | warewolf rules | True     | dev2              | Q/ulw&]  | [[result]] | Success | NO           |