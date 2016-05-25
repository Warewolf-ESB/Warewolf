Feature: SharePointMoveFile
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@SharePoint
Scenario: Move Sharepoint file at location	
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And use private public key for source is '<sourcePrivateKeyFile>'
	And Read is '<read>'   
	And result as '<resultVar>'
	When the sharepoint read folder file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'

Scenario: Move Sharepoint File to new Folder
	Given I have a sharepoint path with value 'clocks.dat'
	And I have a Server file path 'TestFolder/clocks.dat'
	And source sharepoint credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source sharepoint sharepoint Server is 'https://dvtdev.sharepoint.com'
	When the sharepoint move file tool is executed
	
Scenario: Move Sharepoint File to new Folder empty path
	Given I have a sharepoint path with value 'clocks.dat'
	And I have a Server file path ''
	And source sharepoint credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source sharepoint sharepoint Server is 'https://dvtdev.sharepoint.com'
	When the sharepoint move file tool is executed
	Then the result will be 'Error'


Scenario: Move Sharepoint File to new Folder empty Server
	Given I have a sharepoint path with value 'clocks.dat'
	And I have a Server file path 'TestFolder/clocks.dat'
	And source sharepoint credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source sharepoint sharepoint Server is ''
	When the sharepoint move file tool is executed
	Then the result will be 'Error'