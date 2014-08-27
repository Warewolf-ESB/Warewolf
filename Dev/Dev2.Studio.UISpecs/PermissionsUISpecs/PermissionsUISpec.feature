Feature: Security
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to setup permissions for my server

Scenario: Set server permission View
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   And I click "RIBBONSETTINGS"   
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
	   And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
	   And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICVIEW"
       And I click "SECURITYSAVE"
       Given I create a new remote connection "Server1" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
       Then I click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID"   
       Given I click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
       When I double click "EXPLORER,UI_Server1 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"      
       Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "7" seconds
       Then "RIBBONDEBUG" is visible
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONNEWENDPOINT" is disabled
       Then "RIBBONSCHEDULE" is disabled
	

Scenario: Set server permission execute
       Given I click "EXPLORER,UI_localhost_AutoID" 
	   And I click "RIBBONSETTINGS"   
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
	   And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
	   And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICEXECUTE"
       And I click "SECURITYSAVE"
       When I create a new remote connection "Server2" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	   And I click "EXPLORER,UI_Server2 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Then "EXPLORER,UI_Server2 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
	   And "EXPLORER,UI_Server2 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	   When I double click "EXPLORER,UI_Server2 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID"
    Then "WINDOWDEBUG" is not visible
	Then "WINDOWVIEWINBROWSER" is visible
	Then I click "WINDOWCANCEL"
	Then "RIBBONNEWDATABASECONNECTOR" is not visible
	Then "RIBBONSCHEDULE" is not visible
    Then "RIBBONNEWPLUGINCONNECTOR" is not visible
	Then "RIBBONNEWWEBCONNECTOR" is not visible
	Then "RIBBONNEWENDPOINT" is not visible
	
	
Scenario: Set server permission Contribute
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   And I click "RIBBONSETTINGS"   
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
	   And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
	   And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICCONTRIBUTE"
       And I click "SECURITYSAVE"
       When I create a new remote connection "Server3" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	   And I click "EXPLORER,UI_ExplorerTree_AutoID,UI_Server3 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Then "Explorer,UI_Server3 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
	   Then "Explorer,UI_Server3 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
	   When I double click "Explorer,UI_Server3 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "RIBBONDEBUG" is visible
	   Then "RIBBONNEWDATABASECONNECTOR" is visible
	   Then "RIBBONSCHEDULE" is not visible
       Then "RIBBONNEWPLUGINCONNECTOR" is visible
	   Then "RIBBONNEWWEBCONNECTOR" is visible
	   Then "RIBBONNEWENDPOINT" is visible
	   Then "RIBBONSCHEDULE" is not visible



Scenario: Set server permission Deploy To FFFF
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   And I click "RIBBONSETTINGS"   
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
	   And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
	   And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICDEPLOYTO"
       And I click "SECURITYSAVE"
       Given I create a new remote connection "Server5" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	   Given I click "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   And "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
	   #And "EXPLORER,UI_Server4 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is not visible
	   #And "EXPLORER,UI_Server4 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	   And I click "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Then "RIBBONNEWENDPOINT" is disabled
	   Then "RIBBONSETTINGS" is disabled
	   Then "RIBBONNEWDATABASECONNECTOR" is disabled
	   Then "RIBBONSCHEDULE" is disabled		
	   Then "RIBBONNEWPLUGINCONNECTOR" is disabled
	   Then "RIBBONNEWWEBCONNECTOR" is disabled
	Then "RIBBONDEPLOY" is visible
	Then I click "RIBBONDEPLOY"
	   Then I click "ACTIVETAB,DeployUserControl,ConnectUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_Server5"
	   Then "DEPLOYSOURCE,UI_SourceServer_UI_Server5 (http://localhost:3142/)_AutoID_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible

Scenario: Set server permission Deploy From
       Given I have Warewolf running
	   And I click "EXPLORER,UI_localhost_AutoID" 
	   And I click "RIBBONSETTINGS"   
	   And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
	   And "SECURITYPUBLICEXECUTE" is unchecked
	   And "SECURITYPUBLICDEPLOYTO" is unchecked
	   And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICDEPLOYFROM"
       And I click "SECURITYSAVE"  
       And I create a new remote connection "Server5" as
	| Address               | AuthType | UserName | Password |
	| http://localhost:3142 | Public   |          |          |
	   And I click "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   Then "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
	   #Given "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is disabled
	   Given I click "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID" 
	   Then "RIBBONNEWENDPOINT" is disabled
	   Then "RIBBONSETTINGS" is disabled
	   Then "RIBBONNEWDATABASECONNECTOR" is disabled
	   Then "RIBBONSCHEDULE" is disabled			
	   Then "RIBBONNEWPLUGINCONNECTOR" is disabled
	   Then "RIBBONNEWWEBCONNECTOR" is disabled
	Then "RIBBONDEPLOY" is visible
	Then I click "RIBBONDEPLOY"
	   Given I click "UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_Server5"
	   Then "DEPLOYDESTINATION,UI_SourceServer_UI_Server5 (http://localhost:3142/)_AutoID_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible
	   And close the Studio and Server


Scenario: Set server permission View and Resource permission execute for public
	  # Given I click "EXPLORER,UI_localhost_AutoID" 
	  # And I click "RIBBONSETTINGS"   
	  # And I click "SECURITYPUBLICADMINISTRATOR"  
   #    And "SECURITYPUBLICVIEW" is unchecked
	  # And "SECURITYPUBLICEXECUTE" is unchecked
	  # And "SECURITYPUBLICDEPLOYTO" is unchecked
	  # And "SECURITYPUBLICDEPLOYFROM" is unchecked
   #    And I click "SECURITYPUBLICVIEW" is unchecked
	  #
	   #Given I send "UI Testing Group" to "ACTIVETAB,SETTINGSFORGROUP"

	   #Given I click "SETTINGSSERVERPERMISSIONSGRID,UI_ServerPermissionsGrid_Row_2_AutoID,UI_ServerViewPermissions_Row_2_Cell_AutoID,UI_UI Testing Group_ViewPermissionCheckBox_AutoID"
	   
	    #Given I click "SETTINGSRESOURECESELECT"


		#And I double click "RESOURCEPICKERFOLDERS,UI_TESTS_AutoID,UI_WebGetRequest_AutoID"
	    Given I click "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_ExplorerTree_AutoID,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"

    #   And I click "SECURITYSAVE"
    #   Given I create a new remote connection "Server6" as
    #   | Address               | AuthType | UserName          | Password |
    #   | http://localhost:3142 | User     | Integrationtester | I73573r0 |
	   #Given I click "EXPLORER,UI_Server6 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   #Then "EXPLORER,UI_Server6 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
	   #Then "EXPLORER,UI_Server6 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
	   #Then I double click "EXPLORER,UI_Server6 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	   #Then "RIBBONNEWENDPOINT" is not visible
	   #Then "RIBBONSETTINGS" is not visible
	   #Then "RIBBONNEWDATABASECONNECTOR" is not visible
	   #Then "RIBBONSCHEDULE" is not visible			
	   #Then "RIBBONNEWPLUGINCONNECTOR" is not visible
	   #Then "RIBBONNEWWEBCONNECTOR" is not visible
	   #Then "RIBBONDEPLOY" is visible
	   #Then I click "RIBBONDEPLOY"
	   #And I click "U_UI_DestinationServercbx_AutoID_Server6' in "ACTIVETAB,DeployUserControl,ConnectUserControl,UI_DestinationServercbx_AutoID"
	   #Then "UI_DestinationServer_Server6 (http://localhost:3142/)_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible
	
	

Scenario: TshepoCodedUISpecTestForMurali
	 Given all tabs are closed
       And I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICVIEW"
       And I click "SECURITYSAVE"
       Given I create a new remote connection "SVR003" as
       | Address               | AuthType | UserName | Password |
       | http://localhost:3142 | Public   |          |          |
       Then I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID"   
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
       Given I double click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"      
       Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "3" seconds
       Then "RIBBONDEBUG" is visible
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONNEWENDPOINT" is disabled
       Then "RIBBONSCHEDULE" is disabled
	   #Then close the Studio and Server
       #Set server permission execute
       Given I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICEXECUTE"
       And I click "SECURITYSAVE"
	  #Refresh local host
	   Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
       And I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is not visible
       And "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
       When I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID"	   
       And I wait for "5" seconds
	   Then "WINDOWDEBUG" is visible within "1" seconds
       Then "WINDOWDEBUGBUTTON" is disabled
       Then "WINDOWVIEWINBROWSER" is visible
       Then I click "WINDOWCANCEL"
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONNEWENDPOINT" is disabled
       #Set server permission Contribute
       Given I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICCONTRIBUTE"
       And I click "SECURITYSAVE"
       #Refresh
	   Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
	   Then I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"       
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
       When I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "RIBBONDEBUG" is visible
       Then "RIBBONNEWDATABASECONNECTOR" is visible
       Then "RIBBONSCHEDULE" is disabled
       Then "RIBBONNEWPLUGINCONNECTOR" is visible
       Then "RIBBONNEWWEBCONNECTOR" is visible
       Then "RIBBONNEWENDPOINT" is visible
       Then "RIBBONSCHEDULE" is disabled
       #Set server permission Deploy To
       Given I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICDEPLOYTO"
       And I click "SECURITYSAVE"
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       And "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible      
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
	   And I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "RIBBONNEWENDPOINT" is disabled
       Then "RIBBONSETTINGS" is disabled
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled            
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONDEPLOY" is visible
       Then I click "RIBBONDEPLOY"
       Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_SVR003"
       Then "DEPLOYSOURCE,UI_SourceServer_SVR003 (http://localhost:3142/)_AutoID,UI_Unautherized_DeployFrom_AutoID" is visible
       #Set server permission Deploy From
       And I click "EXPLORER,UI_localhost_AutoID" 
       And I click "RIBBONSETTINGS"   
       And I click "SECURITYPUBLICADMINISTRATOR"  
       And "SECURITYPUBLICVIEW" is unchecked
       And "SECURITYPUBLICEXECUTE" is unchecked
       And "SECURITYPUBLICDEPLOYTO" is unchecked
       And "SECURITYPUBLICDEPLOYFROM" is unchecked
       And I click "SECURITYPUBLICDEPLOYFROM"
       And I click "SECURITYSAVE" 
       And I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       Then "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
       #Given "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is disabled
       Given I click "EXPLORER,UI_SVR003 (http://localhost:3142/)_AutoID" 
       Then "RIBBONNEWENDPOINT" is disabled
       Then "RIBBONSETTINGS" is disabled
       Then "RIBBONNEWDATABASECONNECTOR" is disabled
       Then "RIBBONSCHEDULE" is disabled                   
       Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       Then "RIBBONNEWWEBCONNECTOR" is disabled
       Then "RIBBONDEPLOY" is visible
       Then I click "RIBBONDEPLOY"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_SVR003"
       Then "DEPLOYDESTINATION,UI_DestinationServer_SVR003 (http://localhost:3142/)_AutoID,UI_Unautherized_DeployToText_AutoID" is visible
	