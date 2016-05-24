Feature: SharePointReadFolder
In order to be able to Read Folder File or Folder 
	as a Warewolf user From Sharepoint
	I want a tool that reads the contents of a Folder at a given location

@SharePoint
Scenario: Read Sharepoint Folder file at location	
	Given I have a source path '<source>' with value '<sourceLocation>'
	And source credentials as '<username>' and '<password>'
	And use private public key for source is '<sourcePrivateKeyFile>'
	And Read is '<read>'   
	And result as '<resultVar>'
	When the sharepoint read folder file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'


	Scenario: Read Sharepoint File at location	
	Given I have a sharepoint path with value 'Shared Documents'
	And source sharepoint credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source sharepoint sharepoint Server is 'https://dvtdev.sharepoint.com'
	And Read Type is 'Files'   
	When the sharepoint read file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'

	Scenario: Read Sharepoint Folder at location	
	Given I have a sharepoint path with value 'Shared Documents'
	And source sharepoint credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source sharepoint sharepoint Server is 'https://dvtdev.sharepoint.com'
	And Read Type is 'Folders'   
	When the sharepoint read file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'

	Scenario: Read Sharepoint Files and Folder at location	
	Given I have a sharepoint path with value 'Shared Documents'
	And source sharepoint credentials as 'Bernartdt@dvtdev.onmicrosoft.com' and 'Kailey@40'
	And source sharepoint sharepoint Server is 'https://dvtdev.sharepoint.com'  
	And Read Type is ''   
	When the sharepoint read file tool is executed
	Then the result variable '<resultVar>' will be '[[<resultVar>]]'