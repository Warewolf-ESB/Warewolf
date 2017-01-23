@SharepointConnector
Feature: SharepointConnector
	In order to connect to sharepoint servers
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Create Sharepoint Source From Tool
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Drag Toolbox Sharepoint CopyFile Onto DesignSurface
	Then I Open Sharepoint Copy Tool Large View
	And I Click New Source Button From SharepointCopyFile Tool
	When I Enter Sharepoint ServerSource ServerName
	And I Click User Button On Sharepoint Source
	And I Enter Sharepoint ServerSource User Credentials
	And I Click Sharepoint Server Source TestConnection
