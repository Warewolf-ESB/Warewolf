@DBConnector
Feature: DBConnector
	In order to connect to databases
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Create New DB Connector With Variables
	Given The Warewolf Studio is running
	And I Wait For Explorer Localhost Spinner
	And I Click New SQLServerSource Explorer Context Menu
	And I depend on "MSSQL"
	When I Type SVRDEV into DB Source Wizard Server Textbox
	And I Click UserButton On Database Source
	And I Enter TestUser Username "testuser" And Password "test123" on Database source
	And I Click DB Source Wizard Test Connection Button
	Given The DB Source Wizard Test Succeeded Image Is Visible
	When I Select Dev2TestingDB From DB Source Wizard Database Combobox
	And I Save With Ribbon Button And Dialog As "CodedUITestingDBSource"

Scenario: Create DB Source From Tool
	Given The Warewolf Studio is running
	And I depend on "MSSQL"
	When I Click New Workflow Ribbon Button
	And I Drag Toolbox SQL Server Tool Onto DesignSurface
	And I Select NewSQLServerDatabaseSource FromSqlServerTool
	And I Can Select Hostname From Server Source Wizard Dropdownlist
	And I Type SVRDEV into DB Source Wizard Server Textbox
	And I Click DB Source Wizard Test Connection Button
	Then The DB Source Wizard Test Succeeded Image Is Visible