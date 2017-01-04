@RemoteServerSpecFeature 
Feature: RemoteServerSpec
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Refresh Remote Server Refreshes Only The Remote Server
	Given The Warewolf Studio is running	
	When I Connect To Remote Server
	And I Double Click Localhost Server
	And I Select RemoteConnectionIntegration From Explorer
	And I Refresh Explorer Withpout Waiting For Spinner
	Then Remote Server Refreshes
