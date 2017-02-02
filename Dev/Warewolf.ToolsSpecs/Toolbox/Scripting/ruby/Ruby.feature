@RubyFeature
Feature: Ruby
	In order to execute Ruby
	As a Warewolf user
	I want a tool that allows me to execute Ruby

Scenario: Execute Ruby Variable is 1
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "ruby_one_variable.txt"
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the script result should be "one"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Ruby     | String = String |
	And the debug output as 
	|                   |
	| [[result]] = one |

Scenario: Execute Ruby blank script	
	Given I have this script to execute ""
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the script result should be ""
	And the execution has "AN" error
	And the debug inputs as  
	| Language | Script |
	| Ruby     | ""     |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Execute Ruby Variable is 2
	Given I have a script variable "[[val]]" with this value "2"
	And I have this script to execute "ruby_one_variable.txt"
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the script result should be "two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Ruby     | String = String |
	And the debug output as 
	|                   |
	| [[result]] = two |

Scenario: Execute Ruby Variable is 3
	Given I have a script variable "[[val]]" with this value "3"
	And I have this script to execute "ruby_one_variable.txt"
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the script result should be "not one or two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Ruby     | String = String |
	And the debug output as 
	|                              |
	| [[result]] = not one or two |

Scenario: Execute Ruby Variable is 100
	Given I have a script variable "[[val]]" with this value "100"
	And I have this script to execute "ruby_one_variable.txt"
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the script result should be "not one or two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Ruby     | String = String |
	And the debug output as 
	|                              |
	| [[result]] = not one or two |

Scenario: Execute badly formed Ruby
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "ruby_badly_formatted.txt"
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the execution has "AN" error
	And the debug inputs as  
	| Language | Script          |
	| Ruby     | String = String |
	And the debug output as 
	|               |
	| [[result]] = |
	
Scenario: Execute Ruby with 2 variables
	Given I have a script variable "[[val1]]" with this value "1"
	Given I have a script variable "[[val2]]" with this value "1"
	And I have this script to execute "ruby_two_variables.txt"
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the script result should be "two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language | Script          |
	| Ruby     | String = String |
	And the debug output as 
	|                   |
	| [[result]] = two |

Scenario: Execute Ruby with a negative recordset index
	Given I have this script to execute "[[my(-1).val]]"
	And I have selected the language as "Ruby"
	When I execute the script tool
	Then the execution has "AN" error
	And the debug inputs as  
	| Language | Script           |
	| Ruby     | [[my(-1).val]] = |
	And the debug output as 
	|               |
	| [[result]] = |