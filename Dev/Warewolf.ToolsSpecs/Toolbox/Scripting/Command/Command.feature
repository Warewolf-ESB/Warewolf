Feature: Command
	In order to execute command line scripts
	As a Warewolf user
	I want a tool that allows me to execute commands 

Scenario: Execute commands 
	Given I have a command variable "[[drive]]" equal to "C:\"
	Given I have these command scripts to execute in a single execution run
	| script                        |
	| @echo off                     |
	| REM Testing multiple commands |
	| dir [[drive]]                   |
	When the command tool is executed
	Then the result of the command tool will be "Volume in drive C has no label"
	And the execution has "NO" error
	And the debug inputs as  
	| Command         |
	| String = String |  
	And the debug output as 
	|                      |
	| [[result]] = String |

Scenario: Execute a command that requires user interaction like pause
	Given I have this command script to execute "pause"
	When the command tool is executed
	Then the result of the command tool will be "Press any key to continue . . ."
	And the execution has "NO" error
	And the debug inputs as  
	| Command |
	| pause   |  
	And the debug output as 
	|                                               |
	| [[result]] = Press any key to continue . . . |

Scenario: Execute a blank cmd
	Given I have this command script to execute ""
	When the command tool is executed
	Then the result of the command tool will be ""
	And the execution has "AN" error
	And the debug inputs as
	| Command |
	| ""      |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Execute invalid cmd
	Given I have this command script to execute "asdf"
	When the command tool is executed
	Then the result of the command tool will be ""
	And the execution has "AN" error
	And the debug inputs as
	| Command |
	| asdf    |  
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Execute cmd with negative recordset index
	Given I have this command script to execute "dir [[my(-1).dir]]"
	When the command tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| Command              |
	| dir [[my(-1).dir]] = |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Execute a NULL cmd
	Given I have a command variable "[[command]]" equal to "NULL"
	And I have this command script to execute "dir c:\[[command]]"
	When the command tool is executed
	Then the execution has "No" error


Scenario: Execute a non existent variable cmd 
	Given I have this command script to execute "dir c:\[[command]]"
	When the command tool is executed
	Then the result of the command tool will be ""
	And the execution has "AN" error

Scenario: Execute commands with star notation
	Given I have a command variable "[[coms().command]]" equal to "bob"
	And I have a command variable "[[coms().command]]" equal to "dora"
	And I have a command variable "[[coms().command]]" equal to "bill"
	And I have a command variable "[[results().res]]" equal to "res1"	
	And I have a command result equal to "[[results(*).res]]"
	And I have these command scripts to execute in a single execution run
	| script                    |
	| echo [[coms(*).command]] |
	When the command tool is executed
	Then the execution has "NO" error
	And the debug inputs as  
	| Command         |
	| String = String |  
	| String = String |  
	| String = String |  
	And the debug output as 
	|                             |
	| [[results(1).res]] = bob |
	| [[results(2).res]] = dora |
	| [[results(3).res]] = bill |

Scenario: Execute commands with star notation to append
	Given I have a command variable "[[coms().command]]" equal to "bob"
	And I have a command variable "[[coms().command]]" equal to "dora"
	And I have a command variable "[[coms().command]]" equal to "bill"
	And I have a command variable "[[results().res]]" equal to "res1"	
	And I have a command result equal to "[[results().res]]"
	And I have these command scripts to execute in a single execution run
	| script                    |
	| echo [[coms(*).command]] |
	When the command tool is executed
	Then the execution has "NO" error
	And the debug inputs as  
	| Command         |
	| String = String |  
	| String = String |  
	| String = String |  
	And the debug output as 
	|                           |	
	| [[results(4).res]] = bill |