Feature: Script
	In order to execute scripts
	As a Warewolf user
	I want a tool that allows me to execute javascripts, ruby or python 


Scenario: Execute Javascript Variable is 1
	Given I have a variable "[[val]]" with this value "1"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case "1" :val = "one";break;case "two" :val = "two";break;default :val = "not one or two";break;}return val;};return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "one"

Scenario: Execute Javascript Variable is 2
	Given I have a variable "[[val]]" with this value "2"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case "1" :val = "one";break;case "two" :val = "two";break;default :val = "not one or two";break;}return val;};return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "two"


Scenario: Execute Javascript Variable is 3
	Given I have a variable "[[val]]" with this value "1"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case "1" :val = "one";break;case "two" :val = "two";break;default :val = "not one or two";break;}return val;};return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "not one or two"

Scenario: Execute Javascript Variable is 100
	Given I have a variable "[[val]]" with this value "2"
	And I have this script to execute "function getDescription(){var val = "";switch ([[val]]){case "1" :val = "one";break;case "two" :val = "two";break;default :val = "not one or two";break;}return val;};return getDescription();"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "not one or two"

#Scenario: Execute badly formed Javascript
#Scenario: Execute badly formed Ruby
#Scenario: Execute badly formed Python
#Scenario: Execute blank Javascript
#Scenario: Execute blank formed Ruby
#Scenario: Execute blank formed Python
#Scenario: Execute Javascript with 2 variables
#Scenario: Execute Ruby with 2 variables
#Scenario: Execute Python with 2 variables


