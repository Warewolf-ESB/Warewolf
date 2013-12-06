Feature: Script
	In order to execute scripts
	As a Warewolf user
	I want a tool that allows me to execute javascripts, ruby or python 

Scenario: Execute Javascript Variable is 1
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case  1 : val = "one"; break; case 2 : val = "two"; break; default : val = "not one or two"; break; } return val;}; return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "one"
	And script execution has "NO" error

Scenario: Execute Javascript Variable is 2
	Given I have a script variable "[[val]]" with this value "2"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case  1 : val = "one"; break; case 2 : val = "two"; break; default : val = "not one or two"; break; } return val;}; return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "two"
	And script execution has "NO" error

Scenario: Execute Javascript Variable is 3
	Given I have a script variable "[[val]]" with this value "3"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case  1 : val = "one"; break; case 2 : val = "two"; break; default : val = "not one or two"; break; } return val;}; return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "not one or two"
	And script execution has "NO" error

Scenario: Execute Javascript Variable is 100
	Given I have a script variable "[[val]]" with this value "100"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case  1 : val = "one"; break; case 2 : val = "two"; break; default : val = "not one or two"; break; } return val;}; return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "not one or two"
	And script execution has "NO" error

Scenario: Execute badly formed Javascript
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "function getDescription(){var val = "";case; ([[val]]){case  1 : val = "one"; break; case 2 : val = "two"; break; default : val = "not one or two"; break; } return val;}; return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then script execution has "AN" error
	
Scenario: Execute Javascript with 2 variables
	Given I have a script variable "[[val1]]" with this value "1"
	Given I have a script variable "[[val2]]" with this value "1"
	And I have this script to execute "function getDescription(){var val = ""; var toSwitch = [[val1]]+[[val2]]; switch (toSwitch) { case  1 : val = "one"; break; case 2 : val = "two"; break; default : val = "not one or two"; break; } return val;}; return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "two"
	And script execution has "NO" error

#Scenario: Execute badly formed Ruby
#Scenario: Execute badly formed Python
#Scenario: Execute blank Javascript
#Scenario: Execute blank formed Ruby
#Scenario: Execute blank formed Python
#Scenario: Execute Javascript with 2 variables
#Scenario: Execute Ruby with 2 variables
#Scenario: Execute Python with 2 variables


