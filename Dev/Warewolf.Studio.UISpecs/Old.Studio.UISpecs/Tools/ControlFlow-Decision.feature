﻿Feature: ControlFlow-Decision
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Decision
Scenario: SaveDecisionWithBlankFieldsExpectedDecisionSaved12
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	#Dragging Decision Tool From ToolBox
	When I send "Decision" to "TOOLBOX,PART_SearchBox"
	Then I drag "TOOLBOX,PART_Tools,Control Flow,System.Activities.Statements.FlowDecision" onto "WORKSURFACE,StartSymbol"
	#Saving Decision With Blank Fields
	And "WebBrowserWindow" is visible within "10" seconds
	Given I click point "634,482" on "WebBrowserWindow"
	Given "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),FlowDecisionDesigner" is visible within "10" seconds
	