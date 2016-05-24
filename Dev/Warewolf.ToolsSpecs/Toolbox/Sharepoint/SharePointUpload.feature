Feature: SharePointUpload
	In order to be able to upload a file
	as a Warewolf user to Sharepoint
	I want a tool that reads the contents of a Folder at a given location

Scenario: Upload Sharepoint file to location	
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And use private public key for source is '<sourcePrivateKeyFile>'
	And Read is '<read>'   
	And result as '<resultVar>'
	When the sharepoint read folder file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'

Scenario: Upload Sharepoint File to location from local Machine
	Given I have a Sharepoint Server path with value 'Shared Documents'
	And I have a local file url with value 'C:\New folder\test.txt'
	And source Sharepoint server credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source Sharepoint sharepoint Server url is 'https://dvtdev.sharepoint.com' 
	When the Sharepoint upload file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'
	
Scenario: Upload Sharepoint File to location from local Machine Empty path
	Given I have a Sharepoint Server path with value 'Shared Documents'
	And I have a local file url with value ''
	And source Sharepoint server credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source Sharepoint sharepoint Server url is 'https://dvtdev.sharepoint.com' 
	When the Sharepoint upload file tool is executed
	Then the result will be 'Error'

Scenario: Upload Sharepoint File to location from local Machine Empty Server
	Given I have a Sharepoint Server path with value 'Shared Documents'
	And I have a local file url with value 'C:\New folder\test.txt'
	And source Sharepoint server credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source Sharepoint sharepoint Server url is 'https://dvtdev.sharepoint.com' 
	When the Sharepoint upload file tool is executed
	Then the result will be 'Error'