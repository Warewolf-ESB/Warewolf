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

Scenario Outline: Execute a command that requires recordsets
	Given I have this command script to execute "<variable>" with "<val>"
	When the command tool is executed
	Then the result of the command tool will be "<Result>"
	And the execution has "<Error>" error
	And the debug inputs as  
	| variable   | Command |
	| <variable> | <val>   |  
	And the debug output as 
	|                             |
	| <resultVariable> = <result> |
	Examples: 
	| Variable                          | Val                | resultVariable               | Result                                                                                    | Error |
	| [[rec().set]]                     | Echo a message     | [[rj().a]]                   | a message                                                                                 | No    |
	| [[rec(*).set]]                    | Echo Press any key | [[rj(1).a]]                  | Press any key                                                                             | No    |
	| [[rec([[int]]).set]], [[int]] = 1 | Echo a message     | [[rj(*).a]]                  | a message                                                                                 | No    |
	| [[rec(1).set]]                    | Echo a message     | [[rj([[int]]).a]],[[int]] =3 | a message                                                                                 | No    |
	| [[var]]                           | 444                | [[rj([[int]]).a]],[[int]] =3 | "444" is not recognized as an internal or external command,operable program or batch file | An    |
	| [[v]]                             |                    | [[int]]                      | Empty script to execute                                                                   | An    |


#Complex Types WOLF-1042
Scenario Outline: Execute a command that requires complex types
	Given I have this command script to execute "<object>" with "<val>"
	When the command tool is executed
	Then the result of the command tool will be "<Result>"
	And the execution has "<Error>" error
	And the debug inputs as  
	| object   | Command |
	| <object> | <val>   |  
	And the debug output as 
	|                             |
	| <resultVariable> = <result> |
	Examples: 
	| object                 | Val            | resultVariable              | Result    | Error |
	| [[rec().set(1).value]] | Echo a message | [[rj().set([[int]]).value]] | a message | No    |
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