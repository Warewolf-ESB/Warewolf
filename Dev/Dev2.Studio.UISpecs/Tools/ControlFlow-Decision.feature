Feature: ControlFlow-Decision
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: SaveDecisionWithBlankFieldsExpectedDecisionSaved12
	Given I have Warewolf running
	And all tabs are closed	
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	#Dragging Decision Tool From ToolBox
	When I send "Decision" to "TOOLBOX,PART_SearchBox"
	Then I drag "TOOLBOX,PART_Tools,Control Flow,System.Activities.Statements.FlowDecision" onto "WORKSURFACE,StartSymbol"
	#Saving Decision With Blank Fields
	And "WebBrowserWindow" is visible within "5" seconds
	Given I click point "634,482" on "WebBrowserWindow"
	Given "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),FlowDecisionDesigner" is visible
	