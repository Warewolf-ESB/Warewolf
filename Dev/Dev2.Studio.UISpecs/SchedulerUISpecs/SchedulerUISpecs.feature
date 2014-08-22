Feature: SchedulerUISpecs
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: CreateSaveDeleteSchedule
	#Given I click "RIBBONSCHEDULE"
	#And I click "SCHEDULERNEWBUTTON"
	#And I click "SCHEDULERWORKFLOWSELECTORBUTTON"
	#And I send "webget" to "RESOURCEPICKERFILTER"
	#And I wait for "1" seconds
	#And I double click "RESOURCEPICKERFOLDERS,UI_TESTS_AutoID,UI_WebGetRequest_AutoID"
	#And I click "RESOURCEPICKEROKBUTTON"
	#And I click "SCHEDULEREDITTRIGGERBUTTON"
	#And I wait for "1" seconds
	Given I click on 'okBtn' in "TriggerEditDialog"
	#And I send "{TAB} {TAB} {TAB} {TAB} {TAB} {TAB} {TAB} {TAB} {ENTER}" to "TriggerEditDialog"
	#And I click "SCHEDULERSAVEBUTTON"
	#And I wait for "3" seconds
	#And I click "UI_OkButton_AutoID"
	#And I send "IntegrationTester" to "SCHEDULERUSERNAMEINPUT"
	#And I send "I73573r0" to "SCHEDULERPASSWORDINPUT"
	And I click "SCHEDULERSAVEBUTTON"
	And I wait for "1" seconds
	Then "SCHEDULERSAVEBUTTON" is disabled
	And "SCHEDULERNEWBUTTON" is enabled
	#Given I click "SCHEDULERDELETEBUTTON"
	#And I wait for "2" seconds
	#And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	#And I wait for "1" seconds
	#Then "SCHEDULERDELETEBUTTON" is disabled
	#And "SCHEDULERSAVEBUTTON" is disabled
	#And "SCHEDULERNEWBUTTON" is enabled

Scenario: CreateSchedule
	Given I click "RIBBONSCHEDULE"
	# 5 secs
	#And I click on 'UI_NewTaskButton_AutoID' in "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_SchedulerView_AutoID"
	#And I click on 'UI_DeleteTaskButton_AutoID' in "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_SchedulerView_AutoID"
	# 5 secs
	#And I click on 'UI_NewTaskButton_AutoID' in "ACTIVETAB,UI_SchedulerView_AutoID"
	#And I click on 'UI_DeleteTaskButton_AutoID' in "ACTIVETAB,UI_SchedulerView_AutoID"
	# 9 secs
	#And I click on 'UI_NewTaskButton_AutoID' in "ACTIVETAB"
	#And I click on 'UI_DeleteTaskButton_AutoID' in "ACTIVETAB"
	# 19 SEC
	# 4 SEC
	#And I click "ACTIVETAB,UI_SchedulerView_AutoID,UI_NewTaskButton_AutoID"
	#And I click "ACTIVETAB,UI_SchedulerView_AutoID,UI_DeleteTaskButton_AutoID"
