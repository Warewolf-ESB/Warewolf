Feature: Switch
	In order to branch based on the data
	As Warewolf user
	I want tool has multiple branching decisions based on the data
	
@NeedsBlankWorkflow
Scenario: Drag toolbox switch onto a new workflow opens switch dialog
	When I "Drag_Toolbox_Switch_Onto_DesignSurface"
	Then I "Assert_Decision_Dialog_Done_Button_Exists"

#@NeedsSwitchDialog
#Scenario: Clicking Switch Dialog Done Button Creates a Switch on the Design Surface
	When I "Click_Switch_Dialog_Done_Button"
	Then I "Assert_Switch_Exists_OnDesignSurface"

@ignore
@Switch 
#Coded UI Tests
Scenario: Switch On Design Surface
	Given I have Switch large view opened
	And Variable to switch on "" is "Visible"
	And Display text "" is "Visible" 
	And Done button is "Visible"

@ignore	
Scenario: Adding Switch Variable in Large View
	Given I have Switch large view opened
	When I enter Variable to switch on "[[a]]"  
	Then Display text is updated as "[[a]]"
	When I click on Done 
	Then Switch large view is closed
	And Switch Small View is visible with display text as "[[a]]"

@ignore
Scenario: Change Display Text is updating in Small View
	Given Switch Small View is visible with display text as "[[a]]"
	When I open largeview 
	And Variable to switch on "[[a]]" is "Visible"
	And Display text "[[a]]" is "Visible" 
	When I edit Display text as "This is Switch" 
	And I click on Done
	And Switch sma ll view is visible with with display text as "This is Switch"