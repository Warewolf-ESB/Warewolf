@WebConnector
Feature: WebConnector
	In order to connecto to web services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Create and Execute New Web GET Connector
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	And I Drag Toolbox HTTPGETWebTool Onto DesignSurface
	And I Select Test Source From GET Web Large View Source Combobox
	And I Click GET Web Large View Generate Outputs
	And I Click GET Web Large View Test Inputs Button
	And I Click GET Web Tool Outputs Done Button
	And I Click GET Web Large View Done Button
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
	
Scenario: Create and Execute New Web POST Connector
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	When I Drag Toolbox HTTPPOSTWebTool Onto DesignSurface
	And I Select Test Source From POST Web Large View Source Combobox
	And I Click POST Web Large View Generate Outputs
	And I Click POST Web Large View Test Inputs Button
	And I Click POST Web Tool Outputs Done Button
	And I Click POST Web Large View Done Button
	
Scenario: Create and Execute New Web PUT Connector
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	When I Drag Toolbox HTTPPUTWebTool Onto DesignSurface
	And I Select Test Source From PUT Web Large View Source Combobox
	And I Click PUT Web Large View Generate Outputs
	And I Click PUT Web Large View Test Inputs Button
	And I Click PUT Web Tool Outputs Done Button
	And I Click PUT Web Large View Done Button
	
Scenario: Create and Execute New Web DELETE Connector
	Given The Warewolf Studio is running
	When I Create New Workflow using shortcut
	When I Drag Toolbox HTTPDELETEWebTool Onto DesignSurface
	And I Select Test Source From DELETE Web Large View Source Combobox
	And I Click DELETE Web Large View Generate Outputs
	And I Click DELETE Web Large View Test Inputs Button
	And I Click DELETE Web Tool Outputs Done Button
	And I Click DELETE Web Large View Done Button

Scenario: Right click adorner control with error
	Given The Warewolf Studio is running
	When I Filter the Explorer with "BrokenDeleteWeb" 
	And I DoubleClick Explorer Localhost First Item
	And I Open DeleteWeb Tool Large View
	And I Select Test Source From DELETE Web Large View Source Combobox
	And I Enter invalid data on the DELETE Web Large View
	And I Click DELETE Web Large View Generate Outputs
	And I Click DELETE Web Large View Test Inputs Button


	