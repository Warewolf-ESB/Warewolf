Feature: Utility-Comment
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Comment
Scenario: CheckCommentInDebug
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "Comment" to ""
	And I drag "TOOLCOMMENT" onto "WORKSURFACE,StartSymbol"
	When I send "{F6}" to ""
	Then "DEBUGOUTPUT,Comment" is visible within "5" seconds



