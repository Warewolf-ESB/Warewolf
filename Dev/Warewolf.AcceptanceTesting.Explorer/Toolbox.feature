@Toolbox
Feature: Toolbox
	In order to use tools
	As a warewolf user
	I want a toolbox


Scenario: Searching a Tool in toolbox
     Given "Localhost" Toolbox is loaded
	 When I search for "Decision" in the toolbox
	 Then "Controlflow\Decision" is visible
	 And "Controlflow\Data Merge" is not visible
	 And "Controlflow\Data Split" is not visible
	 And "Controlflow\Delete" is not visible


Scenario: Searching for Tools
     Given "Localhost" Toolbox is loaded
     When I search for "B" in toolbox 
	 Then "Data\Base Conversion" is visible
	 And "Dropbox\Drop box" is visible
	 And "Recordset\SQL Bulk Insert" is visible
	 And "Recordset\Web Request" is visible
	 And "Utility\Format Number" is visible

Scenario: Searching for Tool with wrong name
     Given "Localhost" Toolbox is loaded
     When I search for "gang" in toolbox 
	 Then all tools are "Invisible"

Scenario: Clear filter button is clearing the filter textbox
     Given "Localhost" Toolbox is loaded
	 When I search for "Decision" in toolbox
	 Then "Controlflow\Decision" is visible
	 And "Controlflow\Data Merge" is not visible
	 And "Controlflow\Data Split" is not visible
	 And "Controlflow\Delete" is not visible
	 Then I clear the toolbox filter
	 Then toolbox filter textbox is ""
	 And all tools are "Visible"






	  













