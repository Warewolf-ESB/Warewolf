Feature: WebRequest
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document


Scenario: Enter a URL to download html
	Given I have the url "http://companyweb/Shared%20Documents/Integration%20Test%20Files/IntegrationTestFileDoNotTouch.txt"	
	When the web request tool is executed 
	Then the result should contain the string "Got it"

Scenario: Enter a badly formed URL
	Given I have the url "www.google.com"	
	When the web request tool is executed 
	Then the result should contain the string ""

Scenario: Enter a URL made up of text and variables with no header
	Given I have the variable "[[site]]" equal to "companyweb/Shared%20Documents/Integration%20Test%20Files/"
	And I have the variable [[file]] equal to "IntegrationTestFileDoNotTouch.html"
	And I have the URL "http://[[site]][[file]]"
	When the web request tool is executed 
	Then the result should contain the string "Here"

#Scenario: Enter a URL and 2 variables each with a header parameter
	Given I have the url "http://companyweb/Shared%20Documents/Integration%20Test%20Files/IntegrationTestFileDoNotTouch.txt"
	And I have the variable "[[ContentType]]" equal to "Content-Type: json"
	And I have the variable...
	And I have the Header "[[ContentType]] ..."
	When the web request tool is executed 
	Then the result should ... (I dont know how to set this up internaly and what it would return)

Scenario: Enter a URL that returns json
	Given I have the url "http://rsaklfsvrwrwbld:1234/services/Get Computer Name.json"
	When the web request tool is executed
	Then the result should containt "dev2.local"

Scenario: Enter a URL that returns xml
	Given I have the url "http://rsaklfsvrwrwbld:1234/services/Get Computer Name.xml"
	When the web request tool is executed
	Then the result should containt "<ComputerName>dev2.local</ComputerName>"

Scenario: Enter a blank URL
	Given I have the url ""
	When the web request tool is executed
	Then the result should containt ""
