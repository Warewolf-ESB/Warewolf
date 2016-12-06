Feature: RemoteServerSpec
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: Create A Workflow On The Remote Server Refreshes Only The Remote Server
	Given I Try Remove "TestServerRefresh" From Remote Server Explorer
	Then I Click New Workflow Ribbon Button
	And I Drag Toolbox Date And Time Onto DesignSurface
	And I Save With Ribbon Button And Dialog As "TestServerRefresh"
	Then Remote Server Refreshes
	And I Try Close Workflow
	And I Try Remove "TestServerRefresh" From Explorer
	And I Click Explorer Connect Remote Server Button
	And I Try Remove "TestServerRefresh" From Remote Server Explorer

Scenario: Disconnect Remote Integragion On Deploy Destination Does Not Disconect On The Explorer
	Given I Try Connect To Remote Server
	And I Click Deploy Ribbon Button
	And I Click Deploy Tab Destination Server Combobox
	And I Click Deploy Tab Destination Server Remote Connection Intergration Item
	And I Click Deploy Tab Destination Server Connect Button
	And I Click Explorer Connect Remote Server Button
	Then Destination Remote Server Is Connected