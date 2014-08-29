Feature: SpecFlowFeature1
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

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

Scenario: TshepoPermissionsExample
	  # Given all tabs are closed
   #    And I click "EXPLORER,UI_localhost_AutoID" 
   #    And I click "RIBBONSETTINGS"   
   #    And I click "SECURITYPUBLICADMINISTRATOR"  
   #    And "SECURITYPUBLICVIEW" is unchecked
   #    And "SECURITYPUBLICEXECUTE" is unchecked
   #    And "SECURITYPUBLICDEPLOYTO" is unchecked
   #    And "SECURITYPUBLICDEPLOYFROM" is unchecked
   #    And I click "SECURITYPUBLICVIEW"
   #    And I click "SECURITYSAVE"
       #Given I create a new remote connection "SWR41" as
       #| Address               | AuthType | UserName | Password |
       #| http://localhost:3142 | Public   |          |          |
   #    Then I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID"   
   #    Given I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
   #    Then "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
   #    Then "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is not visible
   #    Given I double click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"      
   #    Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "3" seconds
   #    Then "RIBBONDEBUG" is visible
   #    Then "RIBBONNEWDATABASECONNECTOR" is disabled
   #    Then "RIBBONSCHEDULE" is disabled
   #    Then "RIBBONNEWPLUGINCONNECTOR" is disabled
   #    Then "RIBBONNEWWEBCONNECTOR" is disabled
   #    Then "RIBBONNEWENDPOINT" is disabled
   #    Then "RIBBONSCHEDULE" is disabled
	  # #Then close the Studio and Server
   #    #Set server permission execute
   #    Given I click "EXPLORER,UI_localhost_AutoID" 
   #    And I click "RIBBONSETTINGS"   
   #    And I click "SECURITYPUBLICADMINISTRATOR"  
   #    And "SECURITYPUBLICVIEW" is unchecked
   #    And "SECURITYPUBLICEXECUTE" is unchecked
   #    And "SECURITYPUBLICDEPLOYTO" is unchecked
   #    And "SECURITYPUBLICDEPLOYFROM" is unchecked
   #    And I click "SECURITYPUBLICEXECUTE"
   #    And I click "SECURITYSAVE"
	  ##Refresh local host
	  # Given I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID" 
   #    And I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
   #    Then "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is not visible
   #    And "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
   #    When I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID"	   
   #    And I wait for "5" seconds
	  # Then "WINDOWDEBUG" is visible within "1" seconds
   #    Then "WINDOWDEBUGBUTTON" is disabled
   #    Then "WINDOWVIEWINBROWSER" is visible
   #    Then I click "WINDOWCANCEL"
   #    Then "RIBBONNEWDATABASECONNECTOR" is disabled
   #    Then "RIBBONSCHEDULE" is disabled
   #    Then "RIBBONNEWPLUGINCONNECTOR" is disabled
   #    Then "RIBBONNEWWEBCONNECTOR" is disabled
   #    Then "RIBBONNEWENDPOINT" is disabled
   #    #Set server permission Contribute
   #    Given I click "EXPLORER,UI_localhost_AutoID" 
   #    And I click "RIBBONSETTINGS"   
   #    And I click "SECURITYPUBLICADMINISTRATOR"  
   #    And "SECURITYPUBLICVIEW" is unchecked
   #    And "SECURITYPUBLICEXECUTE" is unchecked
   #    And "SECURITYPUBLICDEPLOYTO" is unchecked
   #    And "SECURITYPUBLICDEPLOYFROM" is unchecked
   #    And I click "SECURITYPUBLICCONTRIBUTE"
   #    And I click "SECURITYSAVE"
   #    #Refresh
	  # Given I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID" 
	  # Then I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"       
   #    Then "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
   #    Then "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
   #    When I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
   #    Then "RIBBONDEBUG" is visible
   #    Then "RIBBONNEWDATABASECONNECTOR" is visible
   #    Then "RIBBONSCHEDULE" is disabled
   #    Then "RIBBONNEWPLUGINCONNECTOR" is visible
   #    Then "RIBBONNEWWEBCONNECTOR" is visible
   #    Then "RIBBONNEWENDPOINT" is visible
   #    Then "RIBBONSCHEDULE" is disabled
   #    #Set server permission Deploy To
   #    Given I click "EXPLORER,UI_localhost_AutoID" 
   #    And I click "RIBBONSETTINGS"   
   #    And I click "SECURITYPUBLICADMINISTRATOR"  
   #    And "SECURITYPUBLICVIEW" is unchecked
   #    And "SECURITYPUBLICEXECUTE" is unchecked
   #    And "SECURITYPUBLICDEPLOYTO" is unchecked
   #    And "SECURITYPUBLICDEPLOYFROM" is unchecked
   #    And I click "SECURITYPUBLICDEPLOYTO"
   #    And I click "SECURITYSAVE"
   #    Given I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
   #    And "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible      
   #    Given I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID" 
	  # And I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
   #    Then "RIBBONNEWENDPOINT" is disabled
   #    Then "RIBBONSETTINGS" is disabled
   #    Then "RIBBONNEWDATABASECONNECTOR" is disabled
   #    Then "RIBBONSCHEDULE" is disabled            
   #    Then "RIBBONNEWPLUGINCONNECTOR" is disabled
   #    Then "RIBBONNEWWEBCONNECTOR" is disabled
   #    Then "RIBBONDEPLOY" is visible
   #    Then I click "RIBBONDEPLOY"
   #    Then I click "ACTIVETAB,DeployUserControl,UI_SourceServercbx_AutoID,U_UI_SourceServercbx_AutoID_SWR41"
   #    Then "DEPLOYSOURCE,SWR41*,UI_Unautherized_DeployFrom_AutoID" is visible						  
   #    #Set server permission Deploy From
   #    And I click "EXPLORER,UI_localhost_AutoID" 
   #    And I click "RIBBONSETTINGS"   
   #    And I click "SECURITYPUBLICADMINISTRATOR"  
   #    And "SECURITYPUBLICVIEW" is unchecked
   #    And "SECURITYPUBLICEXECUTE" is unchecked
   #    And "SECURITYPUBLICDEPLOYTO" is unchecked
   #    And "SECURITYPUBLICDEPLOYFROM" is unchecked
   #    And I click "SECURITYPUBLICDEPLOYFROM"
   #    And I click "SECURITYSAVE" 
   #    And I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
   #    Then "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
   #    #Given "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is disabled
   #    Given I click "EXPLORER,UI_SWR41 (http://localhost:3142/)_AutoID" 
   #    Then "RIBBONNEWENDPOINT" is disabled
   #    Then "RIBBONSETTINGS" is disabled
   #    Then "RIBBONNEWDATABASECONNECTOR" is disabled
   #    Then "RIBBONSCHEDULE" is disabled                   
   #    Then "RIBBONNEWPLUGINCONNECTOR" is disabled
   #    Then "RIBBONNEWWEBCONNECTOR" is disabled
   #    Then "RIBBONDEPLOY" is visible
   #    Then I click "RIBBONDEPLOY"
   #    Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_SWR41"

#       Given "DEPLOYDESTINATION,SWR41*,UI_Unautherized_DeployToText_AutoID" is visible
	#Given I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	#And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	#And I click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.1*,UI_CanEdit_AutoID"
	   
	   #Given I click "RIBBONSETTINGS"   
   
   #Given I click point "50,50" on "RIBBONSETTINGS"
   #Given I right click point "80,250" on "EXPLORERFOLDERS"
   #Given I double click point "80,250" on "EXPLORERFOLDERS"

#	Given I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
#	And I click "UI_RenameContextMenuItem_AutoID"
#	And I send "Recordset - Records Length RENAME{ENTER}" to ""
#	And I type "Recordset - Records Length RENAME" in "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,UI_RenameTexbox_AutoID"

Given "WINDOWDEBUG" is visible within "5" seconds


Scenario: TshepoDesignerExample
	#Given I have Warewolf running
	#And all tabs are closed
	#And I click "RIBBONNEWENDPOINT"
	#And I double click "TOOLBOX,PART_SearchBox"
	#And I send "Assign" to ""
	#Given I drag "TOOLMULTIASSIGN" onto "WORKSURFACE,StartSymbol"
	#And "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID" is visible within "1" seconds
	#Given I type "myvar" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID"
	#And I send "{TAB}" to ""
	#Given I type "HELLO" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID"
	#And I send "{TAB}" to ""
	#Given I click "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID"
	#Given I double click "WORKSURFACE,Assign (1)(MultiAssignDesigner)"
	Given "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.1*" is visible within "2" seconds

Scenario: TshepoDragandDropExample
	#Given I double click "TOOLBOX,PART_SearchBox"
	#And I send "for Each" to ""
	#When I drag "TOOLFOREACH" onto "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	Given I drag "WORKSURFACE,Assign (2)(MultiAssignDesigner)" to point "50,70" on "WORKSURFACE,For Each(ForeachDesigner)"
