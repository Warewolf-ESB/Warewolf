Feature: QuikcVariableInput
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@QVI
Scenario: QVIAddToMoreThanOneAndReplace
	Given I have Warewolf running
	And all tabs are closed
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}Merge" to ""
	And I drag "TOOLDATAMERGE" onto "WORKSURFACE,StartSymbol"
	##QVIAddTo
	And I click point "248,10" on "WORKSURFACE,Data Merge (1)(DataMergeDesigner)"
	And I send "aa,bb,cc,dd,ee,ff,gg,hh,ii,jj" to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),QuickVariableInputContent,QviVariableListBox"
	And I send "," to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),QuickVariableInputContent,QviSplitOnCharacter"
	When I send "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	Then "WORKSURFACE,Data Merge (10)(DataMergeDesigner)" is visible within "1" seconds
	#MoreThanOne
	And I drag "TOOLDATAMERGE" onto "WORKSURFACE,Data Merge (10)(DataMergeDesigner)"
	And I click point "248,10" on "WORKSURFACE,Data Merge (1)(DataMergeDesigner)"
	And I send "aa,bb,cc,dd,ee,ff,gg,hh,ii" to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),QuickVariableInputContent,QviVariableListBox"
	And I send "," to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),QuickVariableInputContent,QviSplitOnCharacter"
	When I send "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	Then "WORKSURFACE,Data Merge (9)(DataMergeDesigner)" is visible within "1" seconds
	#AndReplace
	And I click point "248,10" on "WORKSURFACE,Data Merge (10)(DataMergeDesigner)"
	And I send "XX,YY,ZZ" to "WORKSURFACE,Data Merge (10)(DataMergeDesigner),QuickVariableInputContent,QviVariableListBox"
	And I send "," to "WORKSURFACE,Data Merge (10)(DataMergeDesigner),QuickVariableInputContent,QviSplitOnCharacter"
	And I click "WORKSURFACE,Data Merge (10)(DataMergeDesigner),QuickVariableInputContent,ReplaceOption"
	When I send "{TAB}{TAB}{ENTER}" to ""
	Then "WORKSURFACE,Data Merge (3)(DataMergeDesigner)" is visible within "1" seconds
