@WebConnector
Feature: WebConnector
	In order to connecto to web services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Web GET Connector
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Click New Web Source Ribbon Button
	And I Type The Testing Site into Web GET Source Wizard Address Textbox
	And I Click New Web Source Test Connection Button
	And I Save With Ribbon Button And Dialog As "WebGETUITestingSource"
	And I Click Close Web Source Wizard Tab Button
	And I Drag GET Web Connector Onto DesignSurface
	And I Select Test Source From GET Web Large View Source Combobox
	And I Click GET Web Large View Generate Outputs
	And I Click GET Web Large View Test Inputs Button
	And I Click GET Web Large View Test Inputs Done Button
	And I Click GET Web Large View Done Button
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
	
Scenario: Web POST Connector
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	When I Drag PostWeb RequestTool Onto DesignSurface
	And I Select Test Source From POST Web Large View Source Combobox
	And I Click POST Web Large View Generate Outputs
	And I Click POST Web Large View Test Inputs Button
	And I Click POST Web Large View Test Inputs Done Button
	And I Click POST Web Large View Done Button
	
Scenario: Web PUT Connector
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	When I Drag PutWeb Tool Onto DesignSurface
	And I Select Test Source From PUT Web Large View Source Combobox
	And I Click PUT Web Large View Generate Outputs
	And I Click PUT Web Large View Test Inputs Button
	And I Click PUT Web Large View Test Inputs Done Button
	And I Click PUT Web Large View Done Button
	
Scenario: Web DELETE Connector
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	When I Drag DeleteWeb Toolbox Onto Workflow Surface
	And I Select Test Source From DELETE Web Large View Source Combobox
	And I Click DELETE Web Large View Generate Outputs
	And I Click DELETE Web Large View Test Inputs Button
	And I Click DELETE Web Large View Test Inputs Done Button
	And I Click DELETE Web Large View Done Button

Scenario: Right click adorner control with error
	Given The Warewolf Studio is running
	When I Filter the Explorer with "BrokenDeleteWeb" 
	And I DoubleClick Explorer Localhost First Item
	And I Open DeleteWeb Tool Large View
	And I Click DELETE Web Large View Generate Outputs
	And I Click DELETE Web Large View Test Inputs Button
	Then There is an error
	And I RightClick Ardoner Hyperlink
	And There is an error



	