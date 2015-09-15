Feature: WebRequest
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document
	
Scenario Outline: Enter a URL to download html with timeout specified 
	Given I have the url '<url>' with timeoutSeconds '<timeoutSeconds>'
	When the web request tool is executed 
	Then the result should contain the string "<result>"
	And the execution has "NO" error
	And the debug inputs as  
	| URL   | Header | Time Out Seconds |
	| <url> |        | <timeoutSeconds> |
	And the debug output as 
	|                     |
	| [[result]] = String |
	Examples:
	| url                                                   | timeoutSeconds | result          |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15  | 20             | Wait Successful |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=110 | 120            | Wait Successful |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=110 | 0              | Wait Successful |
	
Scenario Outline: Enter a URL to download html with timeout specified too short 
	Given I have the url '<url>' with timeoutSeconds '<timeoutSeconds>'
	When the web request tool is executed 
	Then the execution has "AN" error
	And the debug inputs as  
	| URL   | Header | Time Out Seconds |
	| <url> |        | <timeoutSeconds> |
	And the debug output as 
	|                     |
	| [[result]] = String |
	Examples:
	| url                                                   | timeoutSeconds |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=150 | 10             |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15  | 10             |