Feature: WebRequest
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document


Scenario: Enter a URL to download html
	Given I have the url "www.warewolf.io"	
	When the web request tool is executed 
	Then the result should have the string "What if you could build business applications 400% faster?"
