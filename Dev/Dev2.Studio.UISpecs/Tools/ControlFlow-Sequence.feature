Feature: ControlFlow-Sequence
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Sequence
Scenario: SequenceSmallViewControlFlowNotAllowedWorkflowOtherAllowedLargeViewControlFlowNotAllowedWorkflowOtherAllowed
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	And I drag "TOOLSEQUENCE" onto "WORKSURFACE,StartSymbol"
	#SmallViewControlFlowNotAllowedWorkflowOtherAllowed
	And I click point "40,10" on "TOOLDECISION"
	And I drag "TOOLDECISION" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),SmallViewContent,UI__DropPoint_AutoID"
	And I click point "40,10" on "TOOLCOUNT"
	And I drag "TOOLCOUNT" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),SmallViewContent,UI__DropPoint_AutoID"
	And I send "workflow" to "TOOLBOX,PART_SearchBox"
	And I click point "40,10" on "TOOLWORKFLOW"
	Given I drag "TOOLWORKFLOW" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Sequence(SequenceDesigner),SmallViewContent,UI__DropPoint_AutoID"
	#And I drag "TOOLWORKFLOW" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),SmallViewContent,UI__DropPoint_AutoID"
	And I type "decision" in "RESOURCEPICKERFILTER"
	And I click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	And I click "RESOURCEPICKEROKBUTTON"
	When I double click point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner)"
	Then "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,Count Records(CountRecordsDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,BARNEY\Decision Testing(ServiceDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,FlowDecisionDesigner" is invisible within "3" seconds	
	#LargeViewControlFlowNotAllowedWorkflowOtherAllowed
	When I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	And I click point "40,10" on "TOOLDECISION"
	And I drag "TOOLDECISION" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,sacd:VerticalConnector_1"
	And I click point "40,10" on "TOOLLENGTH"
	And I drag "TOOLLENGTH" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,sacd:VerticalConnector_1"
	And I send "workflow" to "TOOLBOX,PART_SearchBox"
	And I click point "40,10" on "TOOLWORKFLOW"
	And I drag "TOOLWORKFLOW" to point "5,5" on "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,sacd:VerticalConnector_1"
	And I type "javascript" in "RESOURCEPICKERFILTER"
	And I click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID"
	And I click "RESOURCEPICKEROKBUTTON"
	Then "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,Length(RecordsLengthDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,BARNEY\Javascript Testing(ServiceDesigner)" is visible
	And "WORKSURFACE,Sequence(SequenceDesigner),LargeViewContent,UI__ActivitiesPresenter_AutoID,FlowDecisionDesigner" is invisible within "3" seconds	
	
