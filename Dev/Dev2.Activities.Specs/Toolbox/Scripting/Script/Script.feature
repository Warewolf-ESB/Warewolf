﻿Feature: Script
	In order to execute scripts
	As a Warewolf user
	I want a tool that allows me to execute javascripts, ruby or python 

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

Scenario: Execute JavaScript with a negative recordset index
	Given I have this script to execute "[[my(-1).val]]"
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "AN" error
	And the debug inputs as  
	| Language   | Script           |
	| JavaScript | [[my(-1).val]] = |
	And the debug output as 
	|               |
	| [[result]] = |
	
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

Scenario Outline:Excute Javascript with incorrect values
	Given I have the script to execute '<script>'
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "AN" error
	And the execution has "<errorOccured>" error
	And the debug inputs as  
	| Language | Script           |
	| JavaScript   | <script> |
	And the debug output as 
	|               |
	| [[result]] = |
	Examples: 
	| Script  | ErrorOccured                                                                                                               |
	| 88      | There was an error when returning a value from your script, remember to use the 'Return' keyword when returning the result |
	| [[var]] | Scalar value {var} is NULL                                                                                                 |

Scenario Outline:Execute Javascript using recordsets
	Given I have the script to execute '<script>' equals to '<val>'
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "No" error
	And the debug inputs as  
	| Language   | Script   |
	| JavaScript | <script> |
	And the debug output as 
	|                        |
	| <result> = <ResultVal> |
	Examples: 
	| Script                        | val                 | result                      | ResultVal |
	| [[rec().a]]                   | return "a message"; | [[rs().a]]                  | a message |
	| [[rec().a]]                   | return "a message"; | [[rs(1).a]]                 | a message |
	| [[a]]                         | return "a message"; | [[rs().a]]                  | a message |
	| [[rec(*).a]]                  | return "a message"; | [[rs(*).a]]                 | a message |
	| [[rec([[int]]).a]],[[int]] =1 | return "a message"; | [[rs([[int]]).a]],[[int]]=1 | a message |

#Complex Types WOLF-1042
@ignore
Scenario Outline:Execute Javascript using complex types
	Given I have the script to execute '<script>' equals to '<val>'
	And I have selected the language as "JavaScript"
	When I execute the script tool
	Then the execution has "No" error
	And the debug inputs as  
	| Language   | Script   |
	| JavaScript | <script> |
	And the debug output as 
	|                        |
	| <result> = <ResultVal> |
	Examples: 
	| Script                 | val                 | result                       | ResultVal |
	| [[rec().row(*).value]] | return "a message"; | [[rs(1).row([[int]]).value]] | a message |


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