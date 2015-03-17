@Toolbox
Feature: Toolbox
	In order to use tools
	As a warewolf user
	I want a toolbox


Scenario: Searching a Tool in toolbox
     Given Toolbox is loaded
	 When I search for "Decision" in the toolbox
	 Then "Controlflow\Decision" is visible
	 And "Controlflow\Data Merge" is not visible
	 And "Controlflow\Data Split" is not visible
	 And "Controlflow\Delete" is not visible


Scenario: Searching for Tools
     Given Toolbox is loaded
     When I search for "B" in the toolbox 
	 Then "Data\Base Conversion" is visible
	 And "Dropbox\Drop box" is visible
	 And "Recordset\SQL Bulk Insert" is visible
	 And "Recordset\Web Request" is visible
	 And "Utility\Format Number" is visible

Scenario: Searching for Tool with wrong name
     Given Toolbox is loaded
     When I search for "gang" in the toolbox 
	 Then all tools are not visible

Scenario: Clear filter button is clearing the filter textbox
     Given Toolbox is loaded
	 When I search for "Decision" in the toolbox
	 Then "Controlflow\Decision" is visible
	 And "Controlflow\Data Merge" is not visible
	 And "Controlflow\Data Split" is not visible
	 And "Controlflow\Delete" is not visible
	 When I clear the toolbox filter
	 Then all tools are visible






	  













