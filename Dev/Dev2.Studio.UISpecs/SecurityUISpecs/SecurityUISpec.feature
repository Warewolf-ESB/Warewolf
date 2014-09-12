Feature: Security
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to setup permissions for my server
	
Background: 
	   Given I click "EXPLORER,UI_localhost_AutoID"
	   Given I click "RIBBONSETTINGS"   
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked       
       And "SECURITYPUBLICADMINISTRATOR" is unchecked  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICCONTRIBUTE" is unchecked
       And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid" 
	   And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid" 
	   And I click "SECURITYSAVE" 
	   Given all tabs are closed

Scenario: Testing Different Server Permissions For Public
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
       Given I create a new remote connection "REM" as
       | Address               | AuthType | UserName | Password |
       | http://localhost:3142 | Public   |          |          |
	   #Checking Explorer Icons
       Then I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID"   
       Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	   #Opening Resouurce from Explorer
       Given I double click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"      
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
	   Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID" 
       And I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is not visible
       And "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
       ##Given I double click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID"	          
#	   #Checking Debug Window Buttons
	   ###Given "WINDOWDEBUG" is visible within "25" seconds
       ###Then "WINDOWDEBUGBUTTON" is disabled
       ##Then "WINDOWVIEWINBROWSER" is visible within "25" seconds
       ##Then I click "WINDOWCANCEL"
	   #Checking Ribbon Icons
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONNEWENDPOINT" is disabled
 #Test-3 Set server permission Contribute
       Given all tabs are closed
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
	   Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID" 
	   Then I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"       
       Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
       When I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   #Ribbon Icons
       Then "RIBBONDEBUG" is visible
       Then "RIBBONNEWDATABASECONNECTOR" is visible
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is visible
       Then "RIBBONNEWWEBCONNECTOR" is visible
       Then "RIBBONNEWENDPOINT" is visible
       Then "RIBBONSCHEDULE" is disabled
 #Test-4 Set server permission Deploy To
        Given all tabs are closed
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
       Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       And "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible      
       Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID" 
	   And I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
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
       Given I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_REM"
       Then "DEPLOYSOURCE,UI_SourceServer_UI_REM (http://localhost:3142/)_AutoID_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible
 #Test-5 Set server permission Deploy From
      Given all tabs are closed
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
       And I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
       Given "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
       Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID" 
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
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_REM"
       Then "DEPLOYDESTINATION,REM*,UI_Unautherized_DeployToText_AutoID" is visible
	

	# Before Running this Test make sure IntegrationTester account is not ADMIN on your Box
Scenario: Testing Server Permission And Resource permission for Specific Group
 #Test-1 Set up Public Remote Server permissions View and Resource selected with Execute right only   
       Given all tabs are closed
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   #Open Settings Tab
	   And I click "RIBBONSETTINGS" 
	   #SetUp Server Permissions View for Public
       And I click "SECURITYPUBLICVIEW"
	   #SetUp Server Permissions Execute for "UI Testing Group" Specific Group
	   Given I type "UI Testing Group" in "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI__AddWindowsGroupTextBox_AutoID"
       And I click "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI__ViewPermissionCheckBox_AutoID"
	   #SetUp Resource Permissions for "UI Testing Group" Specific Group
      Given I click "SETTINGSRESOURECESELECT"
       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "UI Testing Group" to ""
       And I click "SETTINGSRESOURCEROW1,UI__ExecutePermissionCheckBox_AutoID"
       And I click "SECURITYSAVE"
	   #Create Remote Connection as User "Integration Tester" 
       Given I create a new remote connection "TestingPermisions" as
       | Address               | AuthType | UserName          | Password |
       | http://localhost:3142 | User     | Integrationtester | I73573r0 |
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
	   Given I click "SETTINGSRESOURCEROW1,UI_AddRemovebtn_AutoID"
	   And I click "SECURITYSAVE"
	    Given I click "SETTINGSRESOURECESELECT"
       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "UI Testing Group" to ""
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
       Given "DEPLOYDESTINATION,TestingPermisions*,UI_Unautherized_DeployToText_AutoID" is not visible
       Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_TestingPermisions"
	   Then "DEPLOYSOURCE,UI_Unautherized_DeployFrom_AutoID" is not visible	
#End Test - 2
 #Test-3 Testing  Remote Server permissions which is having Server Blank and Resource For Specific Group Contribute   
       Given all tabs are closed
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   #Open Settings Tab
	   And I click "RIBBONSETTINGS" 
	   #SetUp Server Permissions Blank
	   And I clear table "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid" 
	   Given I click "SETTINGSRESOURCEROW1,UI_AddRemovebtn_AutoID"
	   And I click "SECURITYSAVE"
	   #SetUp Resource Permissions Contribute for "UI Testing Group" Specific Group
	    Given I click "SETTINGSRESOURECESELECT"
       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "UI Testing Group" to ""
       And I click "SETTINGSRESOURCEROW1,UI__ContributePermissionCheckBox_AutoID"
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
	   Given all tabs are closed
	   #Checking Deploy Permissions "UnAuthorized"
	   Given I click "RIBBONDEPLOY"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_TestingPermisions"
       Given "DEPLOYDESTINATION,TestingPermisions*,UI_Unautherized_DeployToText_AutoID" is visible
       Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_TestingPermisions"
       Then "DEPLOYSOURCE,UI_Unautherized_DeployFrom_AutoID" is visible
#End Test - 3
 

 Scenario: Testing Setiings Tab Duplicate Resource Permissions HelP Text
        #SecuritySettingsUiTestsAdd10ResourcesMakeSureScrollBarIsThere
        #SecuritySettingsUiTestsAddResourcesAndRelatedPriviledgesResourcesAreAddedSuccessfullyAndSaveButtonDisabled
        #SecuritySettingsUiTestsOpenHelpAdornersHelpAdornersOpenedAndClosedSuccessfully
       Given all tabs are closed
       And I click "RIBBONSETTINGS" 
	   #Adding Resource Permissions Row1
       Given I click "SETTINGSRESOURECESELECT"
       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "UI Testing Group" to ""
       And I click "SETTINGSRESOURCEROW1,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row2
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_1_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_1_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row3
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_2_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Unzip_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_2_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row4
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_3_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Write File_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_3_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row5
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_4_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Recordset - Count Records_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_4_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row6
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_5_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Recordset - Find Records_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_5_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row7
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_6_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   Given I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_6_AutoID,UI__ExecutePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row8
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_7_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Recordset - SQL Bulk Insert_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_7_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row9
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_8_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Recordset - Unique Records_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_8_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row10
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_9_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Scripting - CMD Line_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "Testing" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_9_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Adding Resource Permissions Row11
	   Given I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_10_AutoID,UI__AddResourceButton_AutoID"
	   And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	   And I send "{TAB}" to ""
	   And I send "UI Testing Group" to ""
	   And I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_10_AutoID,UI__ContributePermissionCheckBox_AutoID"
	   #Saving Security Settings
       And I click "SECURITYSAVE"
	   #Checking Validation Message Of Duplicate Permissions
	   Given "SETTINGSTAB,UI_Errors_AutoID" contains text "There are duplicate permissions"
	   #Deleting Duplicate Row and Checking ScrollBar
       Given I click "SETTINGSADDRESOURCE,UI_PermissionsGrid_Row_0_AutoID,UI_AddRemovebtn_AutoID"
	   And I click "SECURITYSAVE"
	   #Checking Help Text for Server Permissions and Resource Permissions
	   #Given I click "SECURITYSERVERHELP"
	   #Given "SETTINGSTAB,UI_HelpText_AutoID" contains text "To set server"
	   #And I click "SECURITYRESOURCEHELP"
	   #Given "SETTINGSTAB,UI_HelpText_AutoID" contains text "To set specific"
	   #

Scenario: RemoteWorkflowWithDifferentPermissionedItemsInIt
		Given I click "EXPLORER,UI_localhost_AutoID" 
		#Setup permissions for test
		And I click "RIBBONSETTINGS"   
		Given I click "SETTINGSRESOURECESELECT"
        And I double click "RESOURCEPICKERFOLDERS,UI_MO_AutoID,UI_TestForEachOutput_AutoID"
	    Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	    And I send "{TAB}" to ""
	    And I send "Public" to ""
		And I click "SECURITYPUBLICVIEW"
		And I click "SECURITYPUBLICEXECUTE"	   
		And I click "SECURITYSAVE"
		#Setup remote connection
		And I create a new remote connection "REM" as
		| Address               | AuthType | UserName | Password |
		| http://localhost:3142 | Public   |          |          |
		#Open NewForEachNestedForEachTest and check if nested workflow (MO\TestForEachOutput) has proper permissions 
        And I send "NewForEachNestedForEachTest" to "EXPLORERFILTER"
		And I double click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_INTEGRATION TEST SERVICES_AutoID,UI_NewForEachNestedForEachTest_AutoID"
		And I click point "240,10" on "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,NewForEachNestedForEachTest(FlowchartDesigner),For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,Mo\TestForEachOutput(ServiceDesigner)"
		Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,TestForEachOutput(FlowchartDesigner)" is invisible within "5" seconds
		And I double click "EXPLORERFILTER"
		And I send "{DELETE}" to ""
		When I click "EXPLORERFILTERREFRESHBUTTON"
		And I send "NewForEachNestedForEachTest" to "EXPLORERFILTER"
	    Then "EXPLORERFOLDERS,UI_INTEGRATION TEST SERVICES_AutoID,UI_NewForEachNestedForEachTest_AutoID" is visible within "3" seconds
		And I double click "EXPLORERFILTER"
		And I send "{DELETE}" to ""
		And I send "TestForEachOutput" to "EXPLORERFILTER"
	    Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_MO_AutoID,UI_TestForEachOutput_AutoID,UI_NotAutherized_AutoID" is visible


		
#PBi 12200
#Scenario:Testing Remote Server Deploy Rights
# #Test-1 Set server permission Deploy To
#       Given all tabs are closed
#       Given I click "EXPLORER,UI_localhost_AutoID" 
#       And I click "RIBBONSETTINGS"   
#       And I click "SECURITYPUBLICDEPLOYTO"
#       And I click "SECURITYSAVE"
#	   Given I create a new remote connection "REM" as
#       | Address               | AuthType | UserName | Password |
#       | http://localhost:3142 | Public   |          |          |
#	   #Checking Explorer Can't See Any Resources Under Remote Server
#       Given "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID" is not visible
#	   Given "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_Examples_AutoID,UI_Utility - Calculate_AutoID" is not visible
#       Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID" 
#	   #Ribbon Icons
#       Then "RIBBONNEWENDPOINT" is disabled
#       Then "RIBBONSETTINGS" is disabled
#       Then "RIBBONNEWDATABASECONNECTOR" is disabled
#       Then "RIBBONSCHEDULE" is disabled            
#       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
#       Then "RIBBONNEWWEBCONNECTOR" is disabled
#       Then "RIBBONDEPLOY" is visible
#	   #Checking Deploy Validaion Message "UnAuthorized"
#       Then I click "RIBBONDEPLOY"
#       Given I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_REM"
#       Then "DEPLOYSOURCE,UI_SourceServer_UI_REM (http://localhost:3142/)_AutoID_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible
# #Test-2 Set server permission Deploy From
#      Given all tabs are closed
#       And I click "EXPLORER,UI_localhost_AutoID" 
#       And I click "RIBBONSETTINGS"   
#       And I click "SECURITYPUBLICADMINISTRATOR"  
#       And "SECURITYPUBLICVIEW" is unchecked
#       And "SECURITYPUBLICEXECUTE" is unchecked
#       And "SECURITYPUBLICDEPLOYTO" is unchecked
#       And I click "SECURITYSAVE" 
#	   #Exploer Icons
#       Given "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID" is not visible
#	   Given "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_Examples_AutoID,UI_Utility - Calculate_AutoID" is not visible
#       Given I click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID" 
#	   #Ribbon Icons
#       Then "RIBBONNEWENDPOINT" is disabled
#       Then "RIBBONSETTINGS" is disabled
#       Then "RIBBONNEWDATABASECONNECTOR" is disabled
#       Then "RIBBONSCHEDULE" is disabled                   
#       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
#       Then "RIBBONNEWWEBCONNECTOR" is disabled
#       Then "RIBBONDEPLOY" is visible
#	   #Checking Deploy UnAuthorized
#       Then I click "RIBBONDEPLOY"
#       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_REM"
#       Then "DEPLOYDESTINATION,REM*,UI_Unautherized_DeployToText_AutoID" is visible
#	   Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_REM"
#       Then "DEPLOYSOURCE,UI_Unautherized_DeployFrom_AutoID" is visible
##Test-3 Set server permission Deploy From and Specific Resource View
#        Given all tabs are closed
#       Given I click "EXPLORER,UI_localhost_AutoID" 
#       And I click "RIBBONSETTINGS"   
#       And I click "SECURITYPUBLICADMINISTRATOR"  
#       And "SECURITYPUBLICEXECUTE" is unchecked
#       And "SECURITYPUBLICDEPLOYTO" is unchecked
#	   #SetUp Resource Permissions Contribute for "Public" Specific Group
#	   Given I click "SETTINGSRESOURECESELECT"
#       And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
#	   Given I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
#	   And I send "{TAB}" to ""
#	   And I send "Public" to ""
#       And I click "SETTINGSRESOURCEROW1,UI__ContributePermissionCheckBox_AutoID"
#       And I click "SECURITYSAVE"
#	   #Checking Explorer Resources
#	   And "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_Examples_AutoID,UI_Utility - Calculate_AutoID" is not visible
#	   Then "EXPLORER,UI_TestingPermisions (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID" is not visible
#	   And "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID" is visible
#	   #Checking Resource Icons
#	   Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
#       Then "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
#       #Opening Resource
#	   Given I double click "EXPLORER,UI_REM (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"      
#       Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "3" seconds
#	   #Ribbon Icons
#       Then "RIBBONNEWENDPOINT" is disabled
#       Then "RIBBONSETTINGS" is disabled
#       Then "RIBBONNEWDATABASECONNECTOR" is disabled
#       Then "RIBBONSCHEDULE" is disabled            
#       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
#       Then "RIBBONNEWWEBCONNECTOR" is disabled
#       Then "RIBBONDEPLOY" is visible
#	   #Checking Deploy UnAuthorized
#       Then I click "RIBBONDEPLOY"
#       Given I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_REM"
#       Then "DEPLOYSOURCE,UI_SourceServer_UI_REM (http://localhost:3142/)_AutoID_AutoID,UI_Unautherized_DeployFrom_AutoID" is not visible
#	   Then "DEPLOYSOURCE,UI_SourceServer_UI_REM (http://localhost:3142/)_AutoID_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID" is visible
#	   Then "DEPLOYSOURCE,UI_SourceServer_UI_REM (http://localhost:3142/)_AutoID_AutoID,UI_BARNEY_AutoID,UI_Javascript Testing_AutoID" is visible
#	   Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_REM"
#       Then "DEPLOYDESTINATION,REM*,UI_Unautherized_DeployToText_AutoID" is visible