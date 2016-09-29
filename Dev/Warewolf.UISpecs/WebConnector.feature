Feature: WebConnector
	In order to connecto to web services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Web Connector
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Click New Web Source Ribbon Button
	And I Type TestSite into Web Source Wizard Address Textbox
	And I Click New Web Source Test Connection Button
	And I Save With Ribbon Button And Dialog As "UITestingWebSource"
	And I Click Close Web Source Wizard Tab Button
	And I Drag GET Web Connector Onto DesignSurface
	And I Try Clear Toolbox Filter
	And I Open GET Web Connector Tool Large View
	And I Select Last Source From GET Web Large View Source Combobox
	And I Click GET Web Large View Generate Outputs
	And I Click GET Web Large View Test Inputs Button
	And I Click GET Web Large View Test Inputs Done Button
	And I Click GET Web Large View Done Button
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
