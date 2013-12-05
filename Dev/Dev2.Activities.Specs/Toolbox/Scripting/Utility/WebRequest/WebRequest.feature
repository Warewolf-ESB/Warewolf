Feature: WebRequest
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document


Scenario: Enter a URL to download html
	Given I have the url "www.warewolf.io"	
	When the web request tool is executed 
	Then the result should have the string "What if you could build business applications 400% faster?"

#Scenario: Enter a badly formed URL
#Scenario: Enter a URL made up of text and variables with no header
#Scenario: Enter a URL and 2 variables each with a header parameter
#Scenario: Enter a URL that returns json
#Scenario: Enter a blank URL