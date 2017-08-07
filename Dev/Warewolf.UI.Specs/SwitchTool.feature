Feature: SwitchTool
	In order to continue to control the flow of execution through workflows
	As a Warewolf Studio user
	I want to perform a composition of recorded actions against the switch tool

Scenario: Switch Case Arm Autoconnectors
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	When I Drag Toolbox Switch Onto DesignSurface
	And I Click Switch Dialog Done Button
	And I First Drag Toolbox Comment Onto Switch Left Arm On DesignSurface
	And I Open Switch Tool Large View
	And I Click Switch Dialog Cancel Button
	And I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface
	And I Click Switch Dialog Cancel Button
	Then two autoconnectors exist on the design surface

Scenario: Switch Case Arm Autoconnectors Press Escape
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	When I Drag Toolbox Switch Onto DesignSurface
	And I Hit Escape Key On The Keyboard on Activity Default Window
	And I First Drag Toolbox Comment Onto Switch Left Arm On DesignSurface
	And I Open Switch Tool Large View
	And I Click Switch Dialog Cancel Button
	And I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface
	And I Hit Escape Key On The Keyboard on Activity Default Window
	Then two autoconnectors exist on the design surface
