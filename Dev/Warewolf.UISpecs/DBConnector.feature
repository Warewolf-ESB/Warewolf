@DBConnector
Feature: DBConnector
	In order to connect to databases
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Create New DB Connector With Variables
	Given The Warewolf Studio is running
	And I Click New SQLServerSource Explorer Context Menu
	And I Type rsaklfsvrgen into DB Source Wizard Server Textbox
	Given RSAKLFSVRGENDEV appears as an option in the DB source wizard server combobox
	When I Select RSAKLFSVRGENDEV From Server Source Wizard Dropdownlist
	And I Click UserButton On Database Source
	And I Enter TestUser Username And Password on Database source
	And I Click DB Source Wizard Test Connection Button
	Given The DB Source Wizard Test Succeeded Image Is Visible
	When I Select Dev2TestingDB From DB Source Wizard Database Combobox
	And I Save With Ribbon Button And Dialog As "CodedUITestingDBSource"

Scenario: Execute New DB Connector With Variables
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button	
	And I Drag Toolbox SQL Server Tool Onto DesignSurface
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
	And I Expand Debug Output Recordset
	Then The GetCountries Recordset Is Visible in Debug Output

Scenario: Create DB Source From Tool
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Drag Toolbox SQL Server Tool Onto DesignSurface
	And I Select NewSQLServerDatabaseSource FromSqlServerTool
	And I Type RSAKLFSVRGENDEV into DB Source Wizard Server Textbox
	And I Click DB Source Wizard Test Connection Button
	Then The DB Source Wizard Test Succeeded Image Is Visible
	