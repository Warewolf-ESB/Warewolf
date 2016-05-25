Feature: SharePointDownLoad
In order to be able to download a file
	as a Warewolf user to Sharepoint
	I want a tool that reads the contents of a Folder at a given location

Scenario: Download Sharepoint file to location	
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And use private public key for source is '<sourcePrivateKeyFile>'
	And Read is '<read>'   
	And result as '<resultVar>'
	When the sharepoint read folder file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'

	Scenario: Download Sharepoint File to local Machine
	Given I have a Sharepoint Server download path with value '/Shared Documents/text.txt'
	And I have a local file url with value 'C:\New folder\'
	And source Sharepoint server credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source Sharepoint sharepoint Server url is 'https://dvtdev.sharepoint.com' 
	When the Sharepoint download file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'

Scenario: Download Sharepoint File to local Machine path empty
	Given I have a Sharepoint Server download path with value ''
	And I have a local file url with value 'C:\New folder\'
	And source Sharepoint server credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source Sharepoint sharepoint Server url is 'https://dvtdev.sharepoint.com' 
	When the Sharepoint download file tool is executed
	Then the result will be 'Error'

	Scenario: Download Sharepoint File to local Machine server empty
	Given I have a Sharepoint Server download path with value '/Shared Documents/text.txt'
	And I have a local file url with value 'C:\New folder\'
	And source Sharepoint server credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source Sharepoint sharepoint Server url is '' 
	When the Sharepoint download file tool is executed
	Then the result will be 'Error'
