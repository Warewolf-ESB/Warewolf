Feature: RemoteServerUISpecs
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Background: 
	   Given I click "EXPLORER,UI_localhost_AutoID"
	   Given I click "RIBBONSETTINGS"   
	   And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid" 
	   And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid" 
	   And "SECURITYPUBLICADMINISTRATOR" is unchecked 
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And I click "SECURITYSAVE" 
	   Given all tabs are closed


@RemtoeServer
Scenario: Testing Remote Server Connection Creating Remote Workflow and Executing
	Given I have Warewolf running
	And all tabs are closed
    Given I create a new remote connection "Test" as
       | Address               | AuthType | UserName | Password |
       | http://localhost:3142 | Public   |          |          |
	#Checking Explorer Icons
    Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID"   
	Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
    Then "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
    Then "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
	#Opening Remote Resouurce from Explorer
	Given I double click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "5" seconds
	# 12490 Opening and Debug A Remote Workflow When LocalWorkflow With SameName IsOpen Workflow is Executed
	Given I click "EXPLORER,UI_localhost_AutoID"   
	Given I click "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Given I double click "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "5" seconds
	And I send "{F6}" to ""
	Given "DEBUGOUTPUT,Assign" is visible within "15" seconds	
	# Creating A Workflow On Remote Server
	Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID"   
	And I click "RIBBONNEWENDPOINT"
	Given I send "Assign" to "TOOLBOX,PART_SearchBox"
	#Drag Assign Tool To Remote Design Surface
	And I drag "TOOLASSIGN" onto "WORKSURFACE,StartSymbol"
	Given I type "rec().a" in "TOOLASSIGNSMALLVIEWGRID,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID"
	And I send "{TAB}" to ""
	And I send "Warewolf" to ""
	And I send "{TAB}" to ""
	And I send "rec().a{TAB}" to ""
	And I send "TestRemote{TAB}" to ""
	When I double click point "5,5" on "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	When I click "WORKSURFACE,Assign (2)(MultiAssignDesigner),DoneButton"
	Then "WORKSURFACE,Assign (2)(MultiAssignDesigner),SmallViewContent" is visible
	#Drag Data Merge Toll To Remote Design Surface
    Given I double click "TOOLBOX,PART_SearchBox"
    Given I send "{Delete}" to ""
	Given I send "Data Merge" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLDATAMERGE" onto "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	Given I type "[[rec(1).a]]" in "TOOLDATAMERGESMALLVIEWGRID,UI__Row1_InputVariable_AutoID"
	And I send "{TAB}{TAB}" to ""
	And I send "8" to ""
	And I send "{TAB}" to ""
	And I send "[[rec(2).a]]" to ""
	And I send "{TAB}{TAB}" to ""
	And I send "10" to ""
	Given I type "[[result]]" in "WORKSURFACE,Data Merge (2)(DataMergeDesigner),SmallViewContent,UI__Resulttxt_AutoID"
	And I send "{F6}" to ""
	Given "DEBUGOUTPUT,Assign" is visible within "10" seconds	
	And "DEBUGOUTPUT,Assign" is visible "1" time
	#Saving Workflow On Remote Server
	Given I click "RIBBONSAVE"
	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}" to ""
	And I send "Remote1" to ""
	And I send "{TAB}{TAB}{TAB}" to ""
	And I send "{Enter}" to ""
    Given "WORKFLOWDESIGNER,Remote1(FlowchartDesigner)" is visible within "12" seconds
    And all tabs are closed
	#DragAndDropWorkflowFromRemoteServerOnALocalHostCreatedWorkflow_WorkFlowIsDroppedAndCanExecute
	Given I click "EXPLORER,UI_localhost_AutoID"   
	Given I click "EXPLORER,UI_localhost_AutoID,UI_EXAMPLES_AutoID,UI_Utility - Email_AutoID"
	Given I double click "EXPLORER,UI_localhost_AutoID,UI_EXAMPLES_AutoID,UI_Utility - Email_AutoID"
	Given "WORKFLOWDESIGNER,Utility - Email(FlowchartDesigner)" is visible within "5" seconds
	Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID"   
	Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Remote1_AutoID"
	Given I drag "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Remote1_AutoID" onto "WORKFLOWDESIGNER,Utility - Email(FlowchartDesigner),Email(EmailDesigner)"
	And I send "{F6}" to ""



Scenario: TestingRemoteServerUITests_EditRemoteDbService_DbServiceIsEdited1
    Given I have Warewolf running
	And all tabs are closed
    And I create a new remote connection "TestService" as
       | Address               | AuthType | UserName | Password |
       | http://localhost:3142 | Public   |          |          |
    And I click "EXPLORER,UI_TestService (http://localhost:3142/)_AutoID"   
	And I click "EXPLORER,UI_TestService (http://localhost:3142/)_AutoID,UI_REMOTEUITESTS_AutoID,UI_RemoteDBService_AutoID"
	And I double click "EXPLORER,UI_TestService (http://localhost:3142/)_AutoID,UI_REMOTEUITESTS_AutoID,UI_RemoteDBService_AutoID"
	And I click point "200,104" on "WebBrowserWindow"
	And I send "testingDBSrc{ENTER}" to ""
	And I click point "241,250" on "WebBrowserWindow"
	And I click point "891,118" on "WebBrowserWindow"
	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}" to ""
	And I send "{ENTER}" to ""
	#Given "dbo.FetchForEachMinMaxRunTimes" is Selected in "WebBrowserWindow"
	Given I double click "EXPLORER,UI_TestService (http://localhost:3142/)_AutoID,UI_REMOTEUITESTS_AutoID,UI_RemoteDBService_AutoID"
	Given I click point "138,232" on "WebBrowserWindow"
	Given I click point "891,118" on "WebBrowserWindow"
	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}" to ""
	And I send "{ENTER}" to ""
	Given I double click "EXPLORER,UI_TestService (http://localhost:3142/)_AutoID,UI_REMOTEUITESTS_AutoID,UI_RemoteDBService_AutoID"
	#Given "dbo.FieldWithDotlnName" is Selected in "WebBrowserWindow"

