Feature: Security
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to setup permissions for my server

Scenario: Set server permission View
	Given I click "RIBBONSETTINGS"
	And I send "SECURITYPUBLICVIEW" to "ACTIVETAB,UI_SettingsView_AutoID"
	#And I click on 'SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerViewPermissions_Row_1_Cell_AutoID,UI_Public_ViewPermissionCheckBox_AutoID' in "ACTIVETAB,UI_SettingsView_AutoID"
	And I click on 'SECURITYSAVE' in "ACTIVETAB,UI_SettingsView_AutoID"
	When I create a new remote connection "Server1" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	And I click "UI_DocManager_AutoID,Explorer,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,UI_NavigationViewUserControl_AutoID,UI_ExplorerTree_AutoID,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "UI_DocManager_AutoID,Explorer,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,UI_NavigationViewUserControl_AutoID,UI_ExplorerTree_AutoID,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
	Then "UI_DocManager_AutoID,Explorer,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,UI_NavigationViewUserControl_AutoID,UI_ExplorerTree_AutoID,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	When I double click "UI_DocManager_AutoID,Explorer,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,UI_NavigationViewUserControl_AutoID,UI_ExplorerTree_AutoID,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "RIBBONNEWENDPOINT" is not visible
	Then "RIBBONSETTINGS" is not visible
	Then "RIBBONNEWDATABASECONNECTOR" is not visible
	Then "RIBBONSCHEDULE" is not visible			
	Then "RIBBONNEWPLUGINCONNECTOR" is not visible
	Then "RIBBONNEWWEBCONNECTOR" is not visible
	

Scenario: Set server permission execute
	Given I click "RIBBONSETTINGS"
	And I send "SECURITYPUBLICEXECUTE" to "ACTIVETAB,UI_SettingsView_AutoID"
	#And I click on 'SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerViewPermissions_Row_1_Cell_AutoID,UI_Public_ViewPermissionCheckBox_AutoID' in "ACTIVETAB,UI_SettingsView_AutoID"
	And I click on 'SECURITYSAVE' in "ACTIVETAB,UI_SettingsView_AutoID"
	And I create a new remote connection "Server1" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	Given I click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
	And "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	When I double click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID"
    Then "WINDOWDEBUG" is not visible
	Then "WINDOWVIEWINBROWSER" is visible
	Then I click "WINDOWCANCEL"
	Then "RIBBONNEWDATABASECONNECTOR" is not visible
	Then "RIBBONSCHEDULE" is not visible
    Then "RIBBONNEWPLUGINCONNECTOR" is not visible
	Then "RIBBONNEWWEBCONNECTOR" is not visible
	Then "RIBBONNEWENDPOINT" is not visible
	
	
Scenario: Set server permission Contribute
	#Given I click "RIBBONSETTINGS"
	#And I send "SECURITYPUBLICCONTRIBUTE" to "ACTIVETAB,UI_SettingsView_AutoID"
	##And I click on 'SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerViewPermissions_Row_1_Cell_AutoID,UI_Public_ViewPermissionCheckBox_AutoID' in "ACTIVETAB,UI_SettingsView_AutoID"
	#And I click on 'SECURITYSAVE' in "ACTIVETAB,UI_SettingsView_AutoID"
	And I create a new remote connection "Server1" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	#Given I click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Given I click "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
	Then "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
	When I double click "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
 #   Then "RIBBONDEBUG" is visible
	#Then "RIBBONNEWDATABASECONNECTOR" is visible
	#Then "RIBBONSCHEDULE" is not visible
 #   Then "RIBBONNEWPLUGINCONNECTOR" is visible
	#Then "RIBBONNEWWEBCONNECTOR" is visible
	#Then "RIBBONNEWENDPOINT" is visible
	#Then "RIBBONSCHEDULE" is not visible



Scenario: Set server permission Deploy To
	Given I click "RIBBONSETTINGS"
	And I send "SECURITYDEPLOYTO" to "ACTIVETAB,UI_SettingsView_AutoID"
	#And I click on 'SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerViewPermissions_Row_1_Cell_AutoID,UI_Public_ViewPermissionCheckBox_AutoID' in "ACTIVETAB,UI_SettingsView_AutoID"
	And I click on 'SECURITYSAVE' in "ACTIVETAB,UI_SettingsView_AutoID"
	When I create a new remote connection "Server1" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	And I click "Explorer,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "Explorer,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
	Then "Explorer,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is not visible
	Then "Explorer,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	Then I double click "Explorer,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "RIBBONNEWENDPOINT" is not visible
	Then "RIBBONSETTINGS" is not visible
	Then "RIBBONNEWDATABASECONNECTOR" is not visible
	Then "RIBBONSCHEDULE" is not visible			
	Then "RIBBONNEWPLUGINCONNECTOR" is not visible
	Then "RIBBONNEWWEBCONNECTOR" is not visible
	Then "RIBBONDEPLOY" is visible
	Then I click "RIBBONDEPLOY"
	And I click on 'U_UI_SourceServercbx_AutoID_Server1' in "ACTIVETAB,DeployUserControl,ConnectUserControl,UI_SourceServercbx_AutoID"
	Then "UI_DestinationServer_sE (http://localhost:3142/)_AutoID,UI_Unautherized_DeployToText_AutoID" is visible

Scenario: Set server permission Deploy From
	Given I click "RIBBONSETTINGS"
	And I send "SECURITYDEPLOYFROM" to "ACTIVETAB,UI_SettingsView_AutoID"
	#And I click on 'SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerViewPermissions_Row_1_Cell_AutoID,UI_Public_ViewPermissionCheckBox_AutoID' in "ACTIVETAB,UI_SettingsView_AutoID"
	And I click on 'SECURITYSAVE' in "ACTIVETAB,UI_SettingsView_AutoID"
	When I create a new remote connection "Server1" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	Given I click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
	Then "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	Then I double click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "RIBBONNEWENDPOINT" is not visible
	Then "RIBBONSETTINGS" is not visible
	Then "RIBBONNEWDATABASECONNECTOR" is not visible
	Then "RIBBONSCHEDULE" is not visible			
	Then "RIBBONNEWPLUGINCONNECTOR" is not visible
	Then "RIBBONNEWWEBCONNECTOR" is not visible
	Then "RIBBONDEPLOY" is visible
	Then I click "RIBBONDEPLOY"
	And I click on 'U_UI_DestinationServercbx_AutoID_Server1' in "ACTIVETAB,DeployUserControl,ConnectUserControl,UI_DestinationServercbx_AutoID"
	Then "UI_DestinationServer_Server1 (http://localhost:3142/)_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible


Scenario: Set server permission View and Resource permission execute for public
	Given I click "RIBBONSETTINGS"
	And I send "SECURITYDEPLOYFROM" to "ACTIVETAB,UI_SettingsView_AutoID"
	And I click on 'SECURITYSAVE' in "ACTIVETAB,UI_SettingsView_AutoID"
	When I create a new remote connection "Server1" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	Given I click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
	Then "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	Then I double click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	Then "RIBBONNEWENDPOINT" is not visible
	Then "RIBBONSETTINGS" is not visible
	Then "RIBBONNEWDATABASECONNECTOR" is not visible
	Then "RIBBONSCHEDULE" is not visible			
	Then "RIBBONNEWPLUGINCONNECTOR" is not visible
	Then "RIBBONNEWWEBCONNECTOR" is not visible
	Then "RIBBONDEPLOY" is visible
	Then I click "RIBBONDEPLOY"
	And I click on 'U_UI_DestinationServercbx_AutoID_Server1' in "ACTIVETAB,DeployUserControl,ConnectUserControl,UI_DestinationServercbx_AutoID"
	Then "UI_DestinationServer_Server1 (http://localhost:3142/)_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible
	
	