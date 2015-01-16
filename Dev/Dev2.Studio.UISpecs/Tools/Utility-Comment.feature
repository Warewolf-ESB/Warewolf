Feature: Utility-Comment
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Comment
Scenario: CheckCommentInDebug
	Given I have Warewolf running 
	And all tabs are closed
	Given I click "EXPLORERFILTERCLEARBUTTON"  
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Searching Comment Tool in Tool Search Box
	Given I send "Comment" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLCOMMENT" onto "WORKSURFACE,StartSymbol"
	When I send "{F6}" to ""
	Then "DEBUGOUTPUT,Comment" is visible within "5" seconds
