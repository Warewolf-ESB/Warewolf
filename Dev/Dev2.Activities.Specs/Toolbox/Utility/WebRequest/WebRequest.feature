Feature: WebRequest
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document


Scenario: Enter a URL to download html
	Given I have the url "http://companyweb/Shared%20Documents/Integration%20Test%20Files/IntegrationTestFileDoNotTouch.txt"	
	When the web request tool is executed 
	Then the result should contain the string "Got it"
	And the web request execution has "NO" error

Scenario: Enter a badly formed URL
	Given I have the url "www.google.com"	
	When the web request tool is executed 
	Then the result should contain the string ""
	And the web request execution has "AN" error

Scenario: Enter a URL made up of text and variables with no header
    Given I have the url "http://[[site]][[file]]"	
	And I have a web request variable "[[site]]" equal to "companyweb/Shared%20Documents/Integration%20Test%20Files/"	
	And I have a web request variable "[[file]]" equal to "IntegrationTestFileDoNotTouch.html"
	When the web request tool is executed 
	Then the result should contain the string "Here"
	And the web request execution has "NO" error

Scenario: Enter a URL and 2 variables each with a header parameter
	Given I have the url "http://companyweb/Shared%20Documents/Integration%20Test%20Files/IntegrationTestFileDoNotTouch.txt"	
	And I have a web request variable "[[ContentType]]" equal to "Content-Type"	
	And I have a web request variable "[[Type]]" equal to "json"	
	And I have the Header "[[ContentType]]: [[Type]]"
	When the web request tool is executed 
	Then the result should contain the string "json data ???"
	And the web request execution has "NO" error

Scenario: Enter a URL that returns json
	Given I have the url "http://rsaklfsvrwrwbld:1234/services/Get Computer Name.json"
	When the web request tool is executed	
	Then the result should contain the string "{"ComputerName":"dev2.local"}"
	And the web request execution has "NO" error

Scenario: Enter a URL that returns xml
	Given I have the url "http://rsaklfsvrwrwbld:1234/services/Get Computer Name.xml"
	When the web request tool is executed	
	Then the result should contain the string "<ComputerName>dev2.local</ComputerName>"
	And the web request execution has "NO" error

Scenario: Enter a blank URL
	Given I have the url ""
	When the web request tool is executed	
	Then the result should contain the string ""
	And the web request execution has "AN" error
