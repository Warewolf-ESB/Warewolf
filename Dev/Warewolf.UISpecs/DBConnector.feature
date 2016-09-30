Feature: DBConnector
	In order to connect to databases
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: DB Connector
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Click New Database Source Ribbon Button
	And I Select MSSQLSERVER From DB Source Wizard Address Protocol Dropdown
	And I Type rsaklfsvrgen into DB Source Wizard Server Textbox
	And I Select RSAKLFSVRGENDEV From Server Source Wizard Dropdownlist
	And I Click DB Source Wizard Test Connection Button
	And I Select Dev2TestingDB From DB Source Wizard Database Combobox
	And I Save With Ribbon Button And Dialog As "UITestingDBSource"
	And I Click Close DB Source Wizard Tab Button
	And I Drag Toolbox SQL Server Tool Onto DesignSurface
	And I Try Clear Toolbox Filter
	And I Open Sql Server Tool Large View
	And I Select UITestingDBSource From SQL Server Large View Source Combobox
	And I Select GetCountries From SQL Server Large View Action Combobox
	And I Type 0 Into SQL Server Large View Inputs Row1 Data Textbox
	And I Click SQL Server Large View Generate Outputs
	And I Type 0 Into SQL Server Large View Test Inputs Row1 Test Data Textbox
	And I Click SQL Server Large View Test Inputs Button
	And I Click SQL Server Large View Test Inputs Done Button
	And I Click SQL Server Large View Done Button
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
