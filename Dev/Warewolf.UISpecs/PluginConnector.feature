@PluginConnector
Feature: PluginConnector
	In order to connect to plugin services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Plugin Connector
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Click NewPluginSource Ribbon Button
	And I Type "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Numerics.dll" into Plugin Source Wizard Assembly Textbox
	And I Save With Ribbon Button And Dialog As "UITestingPluginSource"
	And I Click Close Plugin Source Wizard Tab Button
	And I Drag DotNet DLL Connector Onto DesignSurface
	And I Select First Item From DotNet DLL Large View Source Combobox
	And I Select Namespace
	And I Select Action
	And I Click DotNet DLL Large View Generate Outputs
	And I Click DotNet DLL Large View Test Inputs Button
	And I Click DotNet DLL Large View Test Cancel Done Button
	And I Click DotNet DLL Large View Done Button
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
	And I Click Close Workflow Tab Button
	And I Click MessageBox No
