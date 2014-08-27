Feature: SpecFlowFeature1
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
#
Scenario: TshepoPermissionsExample
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
