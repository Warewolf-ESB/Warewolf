Feature: ControlFlow-Sequence
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Sequence
Scenario: SequenceSmallViewControlFlowNotAllowedWorkflowOtherAllowedLargeViewControlFlowNotAllowedWorkflowOtherAllowed
	Given I have Warewolf running
	And all tabs are closed
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	And I drag "TOOLSEQUENCE" onto "WORKSURFACE,StartSymbol"
	#SmallViewControlFlowNotAllowedWorkflowOtherAllowed
	And I drag "TOOLDECISION" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),SmallViewContent,UI__DropPoint_AutoID"
	And I drag "TOOLCOUNT" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),SmallViewContent,UI__DropPoint_AutoID"
	And I send "workflow" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLWORKFLOW" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),SmallViewContent,UI__DropPoint_AutoID"
	And I type "decision" in "RESOURCEPICKERFILTER"
	And I click "RESOURCEPICKERFOLDERS,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID"
	And I click "RESOURCEPICKEROKBUTTON"
	When I double click point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner)"
	Then "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,Count Records(CountRecordsDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,Acceptance Testing Resources\Decision Testing(ServiceDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,FlowDecisionDesigner" is invisible within "3" seconds	
	#LargeViewControlFlowNotAllowedWorkflowOtherAllowed
	When I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	And I drag "TOOLDECISION" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,sacd:VerticalConnector_1"
	And I drag "TOOLLENGTH" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,sacd:VerticalConnector_1"
	And I send "workflow" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLWORKFLOW" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,sacd:VerticalConnector_1"
	And I type "javascript" in "RESOURCEPICKERFILTER"
	And I click "RESOURCEPICKERFOLDERS,UI_Acceptance Testing Resources_AutoID,UI_Javascript Testing_AutoID"
	And I click "RESOURCEPICKEROKBUTTON"
	Then "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,Length(RecordsLengthDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,Acceptance Testing Resources\Javascript Testing(ServiceDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,FlowDecisionDesigner" is invisible within "3" seconds	
	
