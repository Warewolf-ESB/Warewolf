Feature: Command
	In order to execute command line scripts
	As a Warewolf user
	I want a tool that allows me to execute commands 

Scenario: Execute command 
	Given I have a drive "[[drive]]" with this value "C:\"
	And I have this command script to execute "cd [[drive]] \r\n dir"	
	When the command tool is executed
	Then the result of the command tool will be ""
