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
	| Result              |
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
	| Result                                       |
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
	| Result       |
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
	| Result       |
	| [[result]] = |

Scenario: Execute cmd with negative recordset index
	Given I have this command script to execute "dir [[my(-1).dir]]"
	When the command tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| Command              |
	| dir [[my(-1).dir]] = |
	And the debug output as 
	| Result       |
	| [[result]] = |


