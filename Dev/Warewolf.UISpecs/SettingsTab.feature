Feature: SettingsTab
	In order to configure settings
	As a Warewolf Studio user
	I want to perform a composition of recorded actions against the settings tab

Scenario: Checking and Unchecking Permissions Checkboxes
	Given The Warewolf Studio is running
	When I Try Remove "Dice1" From Explorer 
	And I Select NewWorkFlowService From ContextMenu 
	And I Drag Toolbox Random Onto DesignSurface 
	And I Enter Dice Roll Values 
	And I Save With Ribbon Button And Dialog As "Dice1"
	And I Click Close Workflow Tab Button 
	And I Click Explorer Refresh Button 
	And I Click ConfigureSetting From Menu 
	And I Check Public Contribute 
	And I Check Public Administrator 
	And I UnCheck Public View 
	And I Check Public Administrator 
	And I UnCheck Public Administrator 
	And I Click Save Ribbon Button With No Save Dialog 
	And I Click Select Resource Button From Resource Permissions 
	And I Select "Dice1" From Service Picker
	And I Assert Dice Is Selected InSettings Tab Permissions Row 1 
	And I Enter Public As Windows Group 
	And I Check Resource Contribute 
	And I Click Save Ribbon Button With No Save Dialog 
	And I Click Close Settings Tab Button 
