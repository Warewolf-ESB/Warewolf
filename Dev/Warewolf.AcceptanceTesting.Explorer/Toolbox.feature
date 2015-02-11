@Toolbox
Feature: Toolbox
	In order to use tools
	As a warewolf user
	I want a toolbox


Scenario: Searching a Tool in toolbox
     Given warewolf "Localhost" Toolbox is loaded
	 When I search for "Decision" in toolbox
	 Then "Controlflow\Decision" is visible
	 And "Controlflow\Data Merge" is not visible
	 And "Controlflow\Data Split" is not visible
	 And "Controlflow\Delete" is not visible


Scenario: Searching for Tools
     Given warewolf "Localhost" Toolbox is loaded
     When I search for "B" in toolbox 
	 Then "Data\Base Conversion" is visible
	 And "Dropbox\Drop box" is visible
	 And "Recordset\SQL Bulk Insert" is visible
	 And "Recordset\Web Request" is visible
	 And "Utility\Format Number" is visible

Scenario: Searching for Tool with wrong name
     Given warewolf "Localhost" Toolbox is loaded
     When I search for "gang" in toolbox 
	 Then "Data\Base Conversion" is not visible
	 And "Dropbox\Drop box" is not visible
	 And "Recordset\SQL Bulk Insert" is not visible
	 And "Recordset\Web Request" is not visible
	 And "Utility\Format Number" is not visible
	 And all tools are not visible

	  

Scenario: Resizing Toolbox
     Given warewolf "Localhost" Toolbox is loaded
	 When the toolbox is resized "Horizontaly"
     Then the tools are refreshed
     And they appear from a "left to right" 
