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
	And datetimediff execution has "NO" error

Scenario: Calculate the number of years with mulitpart text and variable inputs to both input fields
	Given I have a DateAndTimeDifference variable "[[years]]" equal to 13
	And I have a first date "20[[years]]-11-29" 
	And I have a DateAndTimeDifference variable "[[years2]]" equal to 14
	And I have a second date "20[[years]]-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Years" 	
	When the datetime difference tool is executed
	Then the difference should be "0"
	And datetimediff execution has "NO" error

Scenario: Calculate the number of Months between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Months" 	
	When the datetime difference tool is executed
	Then the difference should be "12"
	And datetimediff execution has "NO" error

Scenario: Calculate the number of hours between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be "8088"
	And datetimediff execution has "NO" error

Scenario: Calculate the number of minutes between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the difference should be "485280"
	And datetimediff execution has "NO" error

Scenario: Calculate the number of seconds between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Seconds" 	
	When the datetime difference tool is executed
	Then the difference should be "29116800"
	And datetimediff execution has "NO" error

Scenario: Calculate the number of split seconds between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "SplitSeconds" 	
	When the datetime difference tool is executed
	Then the difference should be "29116800000"
	And datetimediff execution has "NO" error

Scenario: Calculate the number of weeks between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be "8088"
	And datetimediff execution has "NO" error

Scenario: Calculate the number of minutes between two blank inputs
	Given I have a first date "" 
	And I have a second date "" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And datetimediff execution has "AN" error

Scenario: Calculate the number of seconds with badly formed input format
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyy-mm-dd"
	And I selected output in "Seconds" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And datetimediff execution has "AN" error

Scenario: Leave input dates blank
	Given I selected output in "Years"
	When the datetime difference tool is executed
	Then the difference should be "0"
	And datetimediff execution has "AN" error