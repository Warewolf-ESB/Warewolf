@Help
Feature: Help
	In order to be able to properly use Warewolf
	As a Warewolf user
	I want to be able to see help

Scenario: Default help is display startup
	Given help is visible
	When main window is selected
	Then the default help of "This is the default help" is displayed
	