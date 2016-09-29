Feature: PluginConnector
	In order to connect to plugin services
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Plugin Connector
	When I Click New Workflow Ribbon Button
	And I Click NewPluginSource Ribbon Button
	And I Type "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll" into Plugin Source Wizard Assembly Textbox
	And I Save With Ribbon Button And Dialog As "UITestingPluginSource"
	And I Click Close Plugin Source Wizard Tab Button
	And I Drag DotNet DLL Connector Onto DesignSurface
	And I Open DotNet DLL Connector Tool Large View
	And I Select First Item From DotNet DLL Large View Source Combobox
	And I Select SystemObject From DotNet DLL Large View Namespace Combobox
	And I Select FirstItem From DotNet DLL Large View Action Combobox
	And I Click DotNet DLL Large View Generate Outputs
	And I Click DotNet DLL Large View Test Inputs Button
	And I Click DotNet DLL Large View Test Inputs Done Button
	And I Click DotNet DLL Large View Done Button
	And I Click Debug Ribbon Button
	And I Click DebugInput Debug Button
