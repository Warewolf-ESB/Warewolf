@Scripting
@PythonFeature
Feature: Python
	In order to execute Python
	As a Warewolf user
	I want a tool that allows me to execute Python

Scenario: Execute Python Variable is 1
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "python_one_variable.txt"
	And I have selected the language as "Python"
	When I execute the script tool
	Then the script result should be "one"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Python   | String = String |
	And the debug output as 
	|                   |
	| [[result]] = one |


Scenario: Execute Python blank script	
	Given I have this script to execute ""
	And I have selected the language as "Python"
	When I execute the script tool
	Then the script result should be ""
	And the execution has "AN" error
	And the debug inputs as  
	| Language | Script |
	| Python   | ""     |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Execute Python Variable is 2
	Given I have a script variable "[[val]]" with this value "2"
	And I have this script to execute "python_one_variable.txt"
	And I have selected the language as "Python"
	When I execute the script tool
	Then the script result should be "two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Python   | String = String |
	And the debug output as 
	|                   |
	| [[result]] = two |

Scenario: Execute Python Variable is 3
	Given I have a script variable "[[val]]" with this value "3"
	And I have this script to execute "python_one_variable.txt"
	And I have selected the language as "Python"
	When I execute the script tool
	Then the script result should be "not one or two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Python   | String = String |
	And the debug output as 
	|                              |
	| [[result]] = not one or two |

Scenario: Execute Python Variable is 100
	Given I have a script variable "[[val]]" with this value "100"
	And I have this script to execute "python_one_variable.txt"
	And I have selected the language as "Python"
	When I execute the script tool
	Then the script result should be "not one or two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Python   | String = String |
	And the debug output as 
	|                              |
	| [[result]] = not one or two |

Scenario: Execute badly formed Python
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "python_badly_formatted.txt"
	And I have selected the language as "Python"
	When I execute the script tool
	Then the execution has "AN" error
	And the debug inputs as  
	| Language | Script          |
	| Python   | String = String |
	And the debug output as 
	|               |
	| [[result]] = |
	
Scenario: Execute Python with 2 variables
	Given I have a script variable "[[val1]]" with this value "1"
	Given I have a script variable "[[val2]]" with this value "1"
	And I have this script to execute "python_two_variables.txt"
	And I have selected the language as "Python"
	When I execute the script tool
	Then the script result should be "two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Python   | String = String |
	And the debug output as 
	|                   |
	| [[result]] = two |

Scenario: Execute Python with a negative recordset index
	Given I have this script to execute "[[my(-1).val]]"
	And I have selected the language as "Python"
	When I execute the script tool
	Then the execution has "AN" error
	And the debug inputs as  
	| Language | Script           |
	| Python   | [[my(-1).val]] = |
	And the debug output as 
	|               |
	| [[result]] = |
