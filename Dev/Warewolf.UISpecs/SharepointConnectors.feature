@SharepointConnector
Feature: SharepointConnector
	In order to connect to sharepoint servers
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Create Sharepoint Source From Tool
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I drag a "Sharepoint" tool
	And I Select New Sharepoint Server Source
	When I Enter Sharepoint ServerSource ServerName
	And I Click UserButton OnSharepointSource
	And I Enter Sharepoint ServerSource User Credentials
	And I Click Sharepoint Server Source TestConnection
	And I Click Close Sharepoint Server Source Tab
	And I Click Close Workflow Tab
