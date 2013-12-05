Feature: DateandTimeDifference
	In order to work with date and time
	As a Warewolf user
	I want a tool that will allow me to compare two dates

Scenario: Calculate the number of days between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-28" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Weeks" 	
	When the datetime difference tool is executed
	Then the difference should be "52"

#Scenario: Calculate the number of Years between two given dates
#Scenario: Calculate the number of Months between two given dates
#Scenario: Calculate the number of hours between two given dates
#Scenario: Calculate the number of minutes between two given dates
#Scenario: Calculate the number of seconds between two given dates
#Scenario: Calculate the number of split seconds between two given dates
#Scenario: Calculate the number of weeks between two given dates
#Scenario: Calculate the number of minutes between two blank inputs
#Scenario: Calculate the number of seconds with badly formed input format
#Scenario: Calculate the number of seconds with no seconds in the input format
#Scenario: Calculate the number of years with mulitpart text and variable inputs to both input fields
#Scenario: Calculate the number of days with badly formed inputs