Feature: Command
	In order to execute command line scripts
	As a Warewolf user
	I want a tool that allows me to execute commands 

Scenario: Execute commands 
	Given I have a command variable "[[drive]]" equal to "C:\"
	And I have this command script to execute "@echo off
		REM Next command returns a list of programs on the given drive
		dir "[[drive]]Program Files""	
	When the command tool is executed
	Then the result of the command tool will be System.string

Scenario: Execute a command that requires user interaction like pause
	Given I have this command scrip to execute "pause"
	When the command tool is executed
	Then the result of the command tool will be "Press any key to continue..."

Scenario: Execute a blank cmd
	Given I have this command scrip to execute ""
	When the command tool is executed
	Then the result of the command tool will be ""

Scenario: Execute invalid cmd
	Given I have this command scrip to execute "asdf"
	When the command tool is executed
	Then the result of the command tool will be ""


