Feature: SharepointConnector
	In order to connect to sharepoint servers
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

@mytag
Scenario: Create Source From Tool
	Given The Warewolf Studio is running
	When I Enter Sharepoint ServerSource ServerName
	And I Click UserButton OnSharepointSource
	And I Enter Sharepoint ServerSource User Credentials
	And I Click Sharepoint Server Source TestConnection
