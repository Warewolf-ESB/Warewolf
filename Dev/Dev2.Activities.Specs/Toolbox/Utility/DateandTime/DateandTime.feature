Feature: DateandTime
	In order to work with date and time
	As a Warewolf user
	I want a tool that will allow me to do date time calcuations

Scenario: Add weeks to a given date
	Given I have a date "2013-11-29" 
	And the input format as "yyyy-mm-dd"
	And I selected Add time as "Weeks" with a value of "52"
	And the output format as "yyyy-mm-dd"
	When the datetime tool is executed
	Then the date should be "2014-11-28"

