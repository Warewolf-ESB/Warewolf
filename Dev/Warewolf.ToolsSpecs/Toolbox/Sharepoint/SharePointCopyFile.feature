Feature: SharePointCopyFile
	In order to be able to Copy a file 
	as a Warewolf user From Sharepoint
	I want a tool that reads the contents of a Folder at a given location



@SharePoint
Scenario: Copy Sharepoint file at location	
	Given I have a source path "<source>" with value "<sourceLocation>"
	And source credentials as "<username>" and "<password>"
	And use private public key for source is "<sourcePrivateKeyFile>"
	And Read is "<read>"   
	And result as "<resultVar>"
	When the sharepoint read folder file tool is executed
	Then the result variable "<resultVar>" will be "[[<resultVar>]]"

Scenario: Copy Sharepoint File to new Folder
	Given I have a sharepoint path with value "clocks.dat"
	And I have a Server file path "TestFolder/clocks.dat"
	And source sharepoint credentials as "Bernartdt@dvtdev.onmicrosoft.com" and "Kailey@40"
	And source sharepoint sharepoint Server is "https://dvtdev.sharepoint.com"
	When the sharepoint copy file tool is executed
	Then the result variable "<resultVar>" will be "[[<resultVar>]]"
	
Scenario: Copy Sharepoint Server empty
Given I have a sharepoint path with value "clocks.dat"
	And I have a Server file path "TestFolder/clocks.dat"
	And source sharepoint credentials as "Bernartdt@dvtdev.onmicrosoft.com" and "Kailey@40"
	And source sharepoint sharepoint Server is ""
	When the sharepoint copy file tool is executed
	Then the result will be "Error"

Scenario: Copy Sharepoint Server Path empty
Given I have a sharepoint path with value "clocks.dat"
	And I have a Server file path ""
	And source sharepoint credentials as "Bernartdt@dvtdev.onmicrosoft.com" and "Kailey@40"
	And source sharepoint sharepoint Server is "https://dvtdev.sharepoint.com"
	When the sharepoint copy file tool is executed
	Then the result will be "Error"