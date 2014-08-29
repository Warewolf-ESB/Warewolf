Feature: Security
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to setup permissions for my server
#	
#Background: 
#	   Given I click "EXPLORER,UI_localhost_AutoID"
#	   Given I click "RIBBONSETTINGS"   
#	   And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid" 
#	   And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid" 
#	   And "SECURITYPUBLICADMINISTRATOR" is unchecked  
#       And "SECURITYPUBLICVIEW" is unchecked
#       And "SECURITYPUBLICEXECUTE" is unchecked
#       And "SECURITYPUBLICDEPLOYTO" is unchecked
#       And "SECURITYPUBLICDEPLOYFROM" is unchecked       
#	   And "SECURITYPUBLICCONTRIBUTE" is unchecked
#       And I click "SECURITYSAVE" 
#	   Given all tabs are closed

Scenario: Set server permission View
       Given all tabs are closed
       And I click "EXPLORER,UI_localhost_AutoID" 
#Test -1 Setup Public Server Permissions View
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICVIEW"
       And I click "SECURITYSAVE"
	   #Creating New Remote Connection
       Given I create a new remote connection "SVR003" as
       | Address               | AuthType | UserName | Password |
       | http://localhost:3142 | Public   |          |          |
	   #Checking Explorer Icons
       Then I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID"   
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	   #Opening Resouurce from Explorer
       Given I double click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"      
       Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "3" seconds
	   #Checking Ribbon Icons
       Then "RIBBONDEBUG" is visible
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONNEWENDPOINT" is disabled
       Then "RIBBONSCHEDULE" is disabled
	   Given all tabs are closed
  #Test-2 Set server permission execute
       Given I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICEXECUTE"
       And I click "SECURITYSAVE"
	   #Checking Explorer Icons
	   Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
       And I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is not visible
       And "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
       When I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID"	          
	   #Checking Debug Window Buttons
	   Then I wait for "2" seconds
	   Given "WINDOWDEBUG" is visible within "3" seconds
       Then "WINDOWDEBUGBUTTON" is disabled
       Then "WINDOWVIEWINBROWSER" is visible
       Then I click "WINDOWCANCEL"
	   #Checking Ribbon Icons
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONNEWENDPOINT" is disabled
 #Test-3 Set server permission Contribute
       Given I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICCONTRIBUTE"
       And I click "SECURITYSAVE"
       #Checking Explorer Icons
	   Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
	   Then I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"       
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
       When I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   #Ribbon Icons
       Then "RIBBONDEBUG" is visible
       Then "RIBBONNEWDATABASECONNECTOR" is visible
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is visible
       Then "RIBBONNEWWEBCONNECTOR" is visible
       Then "RIBBONNEWENDPOINT" is visible
       Then "RIBBONSCHEDULE" is disabled
 #Test-4 Set server permission Deploy To
       Given I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICDEPLOYTO"
       And I click "SECURITYSAVE"
	   #Checking Explorer Icons
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       And "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible      
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
	   And I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   #Ribbon Icons
       Then "RIBBONNEWENDPOINT" is disabled
       Then "RIBBONSETTINGS" is disabled
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled            
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONDEPLOY" is visible
	   #Checking Deploy UnAuthorized
       Then I click "RIBBONDEPLOY"
       Given I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_SVR003"
       Then "DEPLOYSOURCE,UI_SourceServer_SVR003 (http://localhost:3142/)_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible
 #Test-4 Set server permission Deploy From
       And I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICDEPLOYFROM"
       And I click "SECURITYSAVE" 
	   #Exploer Icons
       And I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
       Given "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is disabled
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
	   #Ribbon Icons
       Then "RIBBONNEWENDPOINT" is disabled
       Then "RIBBONSETTINGS" is disabled
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled                   
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONDEPLOY" is visible
	   #Checking Deploy UnAuthorized
       Then I click "RIBBONDEPLOY"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_SVR003"
       Then "DEPLOYDESTINATION,SVR003*,UI_Unautherized_DeployToText_AutoID" is visible
	


Scenario: Testing Server Permission And Resource permission for Specific Group
 #Test-1 Set up Public Remote Server permissions View and Resource selected with Execute right only   
       Given all tabs are closed
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   #Open Settings Tab
	   And I click "RIBBONSETTINGS" 
	   #SetUp Server Permissions View for Public
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
	   And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
	   And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICVIEW"
	   #SetUp Server Permissions Execute for "UI Testing Group" Specific Group
	   Given I type "UI Testing Group" in "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI__AddWindowsGroupTextBox_AutoID"
       And I click "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI__ViewPermissionCheckBox_AutoID"
	   #SetUp Resource Permissions for "UI Testing Group" Specific Group
       Given I click "SETTINGSRESOURECESELECT"
       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   Given I type "UI Testing Group" in "UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid,UI_PermissionsGrid_Row_0_AutoID,UI__AddWindowsGroupsTextBox_AutoID"
       And I click "SETTINGSRESOURCEEXECUTE,UI__ExecutePermissionCheckBox_AutoID"
       And I click "SECURITYSAVE"
	   And I wait for "2" seconds
	   #Create Remote Connection as User "Integration Tester" 
       Given I create a new remote connection "TestingPermisions" as
       | Address               | AuthType | UserName          | Password |
       | http://localhost:3142 | User     | Integrationtester | I73573r0 |
	   And I wait for "5" seconds
	   #Checking Remote Server Resource Icon in Explorer  
	   Given I click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
	   #Opening Remote Server Resource in Explorer 
	   Then I double click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   And "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "7" seconds
	   # Ribbon Icons
	   Then "RIBBONNEWENDPOINT" is disabled
	   Then "RIBBONSETTINGS" is disabled
	   Then "RIBBONNEWDATABASECONNECTOR" is disabled
	   Then "RIBBONSCHEDULE" is disabled		
	   Then "RIBBONNEWPLUGINCONNECTOR" is disabled
	   Then "RIBBONNEWWEBCONNECTOR" is disabled
	   Then "RIBBONSAVE" is visible
	   Then "RIBBONDEPLOY" is visible
	   Then "RIBBONDEBUG" is visible
	   #Checking Deploy Permissions "UnAuthorized"
	   Given I click "RIBBONDEPLOY"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_TestingPermisions"
       Given "DEPLOYDESTINATION,TestingPermisions*,UI_Unautherized_DeployToText_AutoID" is visible
       Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_TestingPermisions"
       Then "DEPLOYSOURCE,UI_Unautherized_DeployFrom_AutoID" is visible		
# End Test - 1
#Test-2 Setup Specific Group  Remote Server permissions Deploy To, Deploy From only and Resource Selected With No Permissions   
       Given all tabs are closed
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   #Opening Settings Tab
	   And I click "RIBBONSETTINGS" 
	   #Setup Server Permissions Nothing for Public
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
	   And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
	   And "SECURITYPUBLICDEPLOYFROM" is unchecked
	   #SetUp Server Permissions Deploy To, Deploy From for "UI Testing Group" Specific Group
	   Given  "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI_UI Testing Group_ViewPermissionCheckBox_AutoID" is unchecked
	   Given I click "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI_UI Testing Group_DeployToPermissionCheckBox_AutoID" 
	   Given I click "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI_UI Testing Group_DeployFromPermissionCheckBox_AutoID" 
	   #Setup Blank permission to selected resource
	   Given I click "SETTINGSRESOURCEEXECUTE,UI_AddRemovebtn_AutoID"
	   And I click "SECURITYSAVE"
	   Given I click "SETTINGSRESOURECESELECT"
       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   Given I type "UI Testing Group" in "UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid,UI_PermissionsGrid_Row_0_AutoID,UI__AddWindowsGroupsTextBox_AutoID"
	   And I click "SECURITYSAVE"
	   #Checking Resource Icon in Explorer 
	   Given I click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is not visible
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID,UI_NotAutherized_AutoID" is visible
	   Given I click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID"
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID,UI_CanEdit_AutoID" is not visible
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID,UI_NotAutherized_AutoID" is visible
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID,UI_CanExecute_AutoID" is not visible
	   #Checking Ribbon Icons
	   Then "RIBBONNEWENDPOINT" is disabled
	   Then "RIBBONSETTINGS" is disabled
	   Then "RIBBONNEWDATABASECONNECTOR" is disabled
	   Then "RIBBONSCHEDULE" is disabled		
	   Then "RIBBONNEWPLUGINCONNECTOR" is disabled
	   Then "RIBBONNEWWEBCONNECTOR" is disabled
	   Then "RIBBONSAVE" is disabled
	   Then "RIBBONDEPLOY" is visible
	   Then "RIBBONDEBUG" is disabled
	   #Checking Deploy Permissions "Authorized"
	   Given I click "RIBBONDEPLOY"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_TestingPermisions"
       Given "DEPLOYDESTINATION,TestingPermisions*,UI_Unautherized_DeployFrom_AutoID" is not visible
       Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_TestingPermisions"
	   Then "DEPLOYSOURCE,UI_Unautherized_DeployFrom_AutoID" is not visible	
#End Test - 2
 #Test-3 Testing  Remote Server permissions which is having Server Blank and Resource For Specific Group Contribute   
       Given all tabs are closed
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   #Open Settings Tab
	   And I click "RIBBONSETTINGS" 
	   #SetUp Server Permissions View for "UI Testing Group" Specific Group
	   Given I click "SETTINGSRESOURCEEXECUTE,UI_AddRemovebtn_AutoID"
	   And I click "SECURITYSAVE"
	   #SetUp Resource Permissions Contribute for "UI Testing Group" Specific Group
	   Given I click "SETTINGSRESOURECESELECT"
       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   Given I type "UI Testing Group" in "UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid,UI_PermissionsGrid_Row_0_AutoID,UI__AddWindowsGroupsTextBox_AutoID"
       And I click "SETTINGSRESOURCEEXECUTE,UI__ContributePermissionCheckBox_AutoID"
       And I click "SECURITYSAVE"
	   #Checking Resource Icon in Explorer 
	   Given I click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Then I double click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   And "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID" is visible within "7" seconds
	   Given I click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
	   #Opening Remote Server Resource From Explorer 
	   Then I double click "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   And "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "7" seconds
	   # Ribbon Icons
	   Then "RIBBONSETTINGS" is disabled
	   Then "RIBBONNEWDATABASECONNECTOR" is disabled
	   Then "RIBBONSCHEDULE" is disabled		
	   Then "RIBBONNEWPLUGINCONNECTOR" is disabled
	   Then "RIBBONNEWWEBCONNECTOR" is disabled
	   Then "RIBBONSAVE" is visible
	   Then "RIBBONDEPLOY" is visible
	   Then "RIBBONDEBUG" is visible
	   #Checking Deploy Permissions "UnAuthorized"
	    Given I click "RIBBONDEPLOY"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_TestingPermisions"
       Given "DEPLOYDESTINATION,TestingPermisions*,UI_Unautherized_DeployToText_AutoID" is visible
       Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_TestingPermisions"
       Then "DEPLOYSOURCE,UI_Unautherized_DeployFrom_AutoID" is visible		
#End Test - 3
