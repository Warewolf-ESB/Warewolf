@RemoteServerSpecFeature 
Feature: RemoteServerSpec
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: Create A Workflow On The Remote Server Refreshes Only The Remote Server	
	Given I Connect To Remote Server
	And I Refresh Explorer
	Then Remote Server Refreshes
	And I Click Explorer Connect Remote Server Button
