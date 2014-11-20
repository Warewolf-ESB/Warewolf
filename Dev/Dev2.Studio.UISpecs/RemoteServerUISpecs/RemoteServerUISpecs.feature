Feature: RemoteServerUISpecs
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


Background: 
      Given I click "EXPLORERFILTERCLEARBUTTON"
	  And I click "EXPLORER,UI_localhost_AutoID"
	  When I click "RIBBONSETTINGS"   
	  And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid" 
	  And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid" 
	  And "SECURITYPUBLICADMINISTRATOR" is unchecked 
	  And I click "SECURITYPUBLICADMINISTRATOR"  
      And I click "SECURITYSAVE" 
	  And all tabs are closed


@RemtoeServer
Scenario: Testing Remote Server Connection Creating Remote Workflow and Executing
	   Given I have Warewolf running
	   And all tabs are closed
	   Given I click "EXPLORER,UI_localhost_AutoID"
       Given I create a new remote connection "Test" as
          | Address               | AuthType | UserName | Password |
          | http://localhost:3142 | Public   |          |          |
	   #Checking Explorer Icons
       Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID"   
	   Given I send "Decision Testing" to "EXPLORERFILTER"
	   Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
	   #Opening Remote Resouurce from Explorer
	   Given I double click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID"
	   Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "5" seconds
	   # 12490 Opening and Debug A Remote Workflow When LocalWorkflow With SameName IsOpen Workflow is Executed
	   Given I click "EXPLORER,UI_localhost_AutoID,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID"
	   Given I double click "EXPLORER,UI_localhost_AutoID,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID"
	   Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "5" seconds
	   And I send "{F6}" to ""
	   #Given "DEBUGOUTPUT,Assign[1]" is visible within "25" seconds	
	   ## Creating A Workflow On Remote Server
	   Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID"   
	   And I click "RIBBONNEWENDPOINT"
	   Given I send "Assign" to "TOOLBOX,PART_SearchBox"
	   #Drag Assign Tool To Remote Design Surface
	   And I drag "TOOLASSIGN" onto "WORKSURFACE,StartSymbol"
	   ##Given I type "rec().a" in "TOOLASSIGNSMALLVIEWGRID,UI_TextBox_AutoID"
	   Given I send "rec().a" to "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Warewolf" to ""
	   And I send "{TAB}" to ""
	   And I send "rec().a{TAB}" to ""
	   And I send "TestRemote{TAB}" to ""
	   When I double click point "5,5" on "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	   When I click "WORKSURFACE,Assign (2)(MultiAssignDesigner),DoneButton"
	   Then "WORKSURFACE,Assign (2)(MultiAssignDesigner),SmallViewContent" is visible
	   #Drag Data Merge Tool To Remote Design Surface
       Given I double click "TOOLBOX,PART_SearchBox"
       Given I send "{Delete}" to ""
	   Given I send "Data Merge" to "TOOLBOX,PART_SearchBox"
       Given I drag "TOOLDATAMERGE" onto "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	   ##Given I type "[[rec(1).a]]" in "TOOLDATAMERGESMALLVIEWGRID,UI__Row1_InputVariable_AutoID"
	   Given I send "[[rec(1).a]]" to "TOOLDATAMERGESMALLVIEWGRID,UI__Row1_InputVariable_AutoID"
	   And I send "{TAB}{TAB}" to ""
	   And I send "8" to ""
	   And I send "{TAB}" to ""
	   And I send "[[rec(2).a]]" to ""
	   And I send "{TAB}{TAB}" to ""
	   And I send "10" to ""
	   And I send "[[result]]" to "WORKSURFACE,Data Merge (2)(DataMergeDesigner),SmallViewContent,UI__Resulttxt_AutoID"
	   And I send "{F6}" to ""
	   #Given "DEBUGOUTPUT,Assign[1]" is visible within "10" seconds	
	   #Saving Workflow On Remote Server
	   Given I click "RIBBONSAVE"
	   And I send "{TAB}{TAB}{TAB}{TAB}{TAB}" to ""
	   And I send "Remote1" to ""
	   And I send "{TAB}{TAB}{TAB}" to ""
	   And I send "{Enter}" to ""
       Given "WORKFLOWDESIGNER,Remote1(FlowchartDesigner)" is visible within "12" seconds
       And all tabs are closed
	   #DragAndDropWorkflowFromRemoteServerOnALocalHostCreatedWorkflow_WorkFlowIsDroppedAndCanExecute
	   Given I click "EXPLORERFILTERCLEARBUTTON"   
	   Given I send "Utility - Email" to "EXPLORERFILTER"
	   Given I click "EXPLORER,UI_localhost_AutoID,UI_Examples_AutoID,UI_Utility - Email_AutoID"
	   Given I double click "EXPLORER,UI_localhost_AutoID,UI_Examples_AutoID,UI_Utility - Email_AutoID"
	   Given "WORKFLOWDESIGNER,Utility - Email(FlowchartDesigner)" is visible within "5" seconds
	   Given I click "EXPLORERFILTERCLEARBUTTON"  
	   Given I send "Remote1" to "EXPLORERFILTER"  
	   Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Remote1_AutoID"
	   Given I drag "EXPLORER,UI_Test (http://localhost:3142/)_AutoID,UI_Remote1_AutoID" onto "WORKFLOWDESIGNER,Utility - Email(FlowchartDesigner),Email(EmailDesigner)"
	   Given "WORKFLOWDESIGNER,Utility - Email(FlowchartDesigner),Remote1(ServiceDesigner)" is visible within "5" seconds
	   And I send "{F6}" to ""
	   Given "DEBUGOUTPUT,DsfActivity" is visible within "20" seconds
	#Cleanup (Deleting the created server and Resource)
	   And all tabs are closed
	   And I right click "EXPLORER,UI_ExplorerTree_AutoID,UI_Test (http://localhost:3142/)_AutoID,UI_Remote1_AutoID"
	   And I send "{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	   And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	   Given I click "EXPLORERFILTERCLEARBUTTON"   
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   Given I click "EXPLORER,UI_Test (http://localhost:3142/)_AutoID" 
	   Given I click "EXPLORERCONNECTBUTTON"
	   Given I click "EXPLORER,UI_SourceServerRefreshbtn_AutoID"
       Given I send "Test" to "EXPLORERFILTER"
	   And I right click "EXPLORERFOLDERS,UI_Test_AutoID"
	   And I send "{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	   And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"




#Scenario: RemoteWorkflowWithNestedRemoteWorkflowReturnsFastAccurateData
#	Given I have Warewolf running
#	And all tabs are closed
#	And I click "RIBBONNEWENDPOINT"
#	#A long wait has been put in here (5 seconds) it would be better to try for up to 5 seconds and then fail -  please look at plumbing that
#    And I create a new remote connection "Azure" as
#     | Address                                    | AuthType | UserName | Password         |
#     | http://dev2-warewolf.cloudapp.net:3142/dsf | User     | dev2user | VisualService8us |
#   And I click "EXPLORERFILTERCLEARBUTTON"
#	And I click "EXPLORER,UI_Azure (http://dev2-warewolf.cloudapp.net:3142/)_AutoID"   
#	And I send "Call Nested Workflow" to "EXPLORERFILTER"
#	#This remote workflow has another remote workflow nested inside it.
#	And I drag "EXPLORER,UI_Azure (http://dev2-warewolf.cloudapp.net:3142/)_AutoID,UI_External Access Testing_AutoID,UI_Call Nested Workflow_AutoID" onto "WORKSURFACE,StartSymbol"
#	And I send "{F6}" to ""
#	And I send "Suc" to "UI_DebugInputWindow_AutoID,TabItems,UI_InputDataTab_AutoID,DataListInputs,UI_InputData_Row_0_AutoID,UI_InputData_Row_0_Cell_AutoID,UI_Inputtxt_AutoID"
#	And I click "WINDOWDEBUGBUTTON"
#	#NOTE: This step needs to be plumbed in properly. Right now that AutoID appears twice in the debug output (at the top and the bottom) and they should be different.
#	#NOTE: Also cannot currently check for text in debug output items
#	And "WfApplicationUtils" contains text "Success"






