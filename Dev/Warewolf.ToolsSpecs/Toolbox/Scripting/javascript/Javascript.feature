@JavascriptFeature
Feature: Javascript
	In order to execute javascripts
	As a Warewolf user
	I want a tool that allows me to execute javascripts

Scenario: Execute Javascript Variable is 1
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "javascript_one_variable.txt"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "one"
	And the execution has "NO" error
	And the debug inputs as  
	| Language   | Script          |
	| JavaScript | String = String |
	And the debug output as 
	|                   |
	| [[result]] = one |

Scenario: Execute Javascript blank script	
	Given I have this script to execute ""
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be ""
	And the execution has "AN" error
	And the debug inputs as  
	| Language   | Script |
	| JavaScript | ""     |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Execute Javascript Variable is 2
	Given I have a script variable "[[val]]" with this value "2"
	And I have this script to execute "javascript_one_variable.txt"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language   | Script          |
	| JavaScript | String = String |
	And the debug output as 
	|                   |
	| [[result]] = two |

Scenario: Execute Javascript Variable is 3
	Given I have a script variable "[[val]]" with this value "3"
	And I have this script to execute "javascript_one_variable.txt"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "not one or two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language   | Script          |
	| JavaScript | String = String |
	And the debug output as 
	|                              |
	| [[result]] = not one or two |

Scenario: Execute Javascript Variable is 100
	Given I have a script variable "[[val]]" with this value "100"
	And I have this script to execute "javascript_one_variable.txt"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "not one or two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language   | Script          |
	| JavaScript | String = String |
	And the debug output as 
	|                              |
	| [[result]] = not one or two |

Scenario: Execute badly formed Javascript
	Given I have a script variable "[[val]]" with this value "1"
	And I have this script to execute "javascript_badly_formatted.txt"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "AN" error
	And the debug inputs as  
	| Language   | Script          |
	| JavaScript | String = String |
	And the debug output as 
	|              |
	| [[result]] = |
	
Scenario: Execute Javascript with 2 variables
	Given I have a script variable "[[val1]]" with this value "1"
	Given I have a script variable "[[val2]]" with this value "1"
	And I have this script to execute "javascript_two_variables.txt"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the script result should be "two"
	And the execution has "NO" error
	And the debug inputs as  
	| Language   | Script          |
	| JavaScript | String = String |
	And the debug output as 
	|                   |
	| [[result]] = two |

	Scenario Outline:Excute Javascript with incorrect values
	Given I have the script to execute "<script>"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "AN" error
	And the execution has "<ErrorOccured>" error
	And the debug inputs as  
	| Language | Script           |
	| JavaScript   | <script> |
	And the debug output as 
	|               |
	| [[result]] = |
	Examples: 
	| Script  | ErrorOccured                                                                                                               |
	| 88      | There was an error when returning a value from your script, remember to use the "Return" keyword when returning the result |
	| [[var]] | Scalar value {var} is NULL                                                                                                 |

Scenario: Execute JavaScript with a null variable 
	Given I have a script variable "[[val1]]" with this value "null"
	Given I have a script variable "[[val2]]" with this value "null"
	And I have this script to execute "[[val2]]"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "An" error


	Scenario: Execute JavaScript with a non existent variable 
	Given I have this script to execute "[[val2]]"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "AN" error