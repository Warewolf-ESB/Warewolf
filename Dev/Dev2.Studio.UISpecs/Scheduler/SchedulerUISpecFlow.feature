Feature: SchedulerSpecFlowFeature
	In order to Create a Scheduled Workflow Tasks in warewolf studio
	As a Warewolf User
	I want to be able to Create Tasks in Warewolf Scheduler Tab

@Scheduler
Scenario: Creating Scheduler Task Without password and expected error
     Given I have Warewolf running
     Given all tabs are closed
	 And I click "EXPLORER,UI_localhost_AutoID" 
	 #Opening Schedule Tab 
	 And I click "RIBBONSCHEDULE" 
	 #Creating New Schedule  
	 And I click "SCHEDULERNEWBUTTON" 
	 Then "SCHEDULERNEWBUTTON" is disabled
	 #Testing Risource picker Is Allowing Only Workflows to Schedule
	 And I click "SCHEDULERWORKFLOWSELECTORBUTTON" 
	 And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Write File_AutoID"
	 And I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
     And I type "IntegrationTester" in "SCHEDULERUSERNAMEINPUT"
	 And I type "CIScheduleTest" in "SCHEDULERNAMEINPUT"
	 #Checking Edit Trigger Dialog Visible 
	 When I click "SCHEDULEREDITTRIGGERBUTTON"
     Then "TriggerEditDialog" is visible
	 And I click point "524,508" on "TriggerEditDialog"
	 And I type "20" in "SCHEDULERHISTORYTOKEEPINPUT"
	 And I send "{TAB}{ENTER}" to ""
	 #And I click "SCHEDULERHISTORYTAB"
	 #And I click "SCHEDULERHELPBUTTON"
	 ##And "SCHEDULERHELPBUTTON" contains text "Each trigger that is executed"
	 #Saving Task Without Password And Expected Error
	 Given I click "SCHEDULERSAVEBUTTON"
	 And I click point "199,264" on ""
	 And "UI_MessageBox_AutoID,UI_OkButton_AutoID" is visible within "2" seconds
	 Given I click "UI_MessageBox_AutoID,UI_OkButton_AutoID"
	 Then "SCHEDULERNEWBUTTON" is disabled
	 ##Closing The Tab Without Saving and Testing For validations
	Given I click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,closeBtn"
	 And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	 And I click point "199,264" on ""
	 And "UI_MessageBox_AutoID,UI_OkButton_AutoID" is visible within "2" seconds
	 Given I click "UI_MessageBox_AutoID,UI_OkButton_AutoID"
	 And I click "SCHEDULERTAB,UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID"
	 And "SCHEDULERNAMEINPUT" contains text "CIScheduleTest"
	 ##Correcting Error And Saving The Task
	 And I click "SCHEDULERSAVEBUTTON"
	 Given I send "I73573r0" to ""
	 And I click point "199,264" on ""
	 ##Deleting The Task
	 And "SCHEDULERSAVEBUTTON" is disabled
	 And I click "SCHEDULERDELETEBUTTON"
	 And I click "SCHEDULERDELETECONFIRMATIONYESBUTTON"
	 ##And "SCHEDULERTAB,UI_DataGridRow__AutoID,UI_DataGridCell_AutoID" contains text "Empty"
	


