Feature: SpecFlowFeature1
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: MakeMuraliHide
	#Given I click "EXPLORERFILTERCLEARBUTTON"
	#And I send "Recordset" to "EXPLORERFILTER"
	#Given I double click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	#Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is visible within "1" seconds
	#Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is invisible within "1" seconds
	#Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is enabled within "1" seconds
	#Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is disabled within "1" seconds
	#Given I click "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Assign (2)(MultiAssignDesigner)"
	##When I send "{DELETE}" to "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Assign (2)(MultiAssignDesigner)"
	#Then I double click "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Assign (2)(MultiAssignDesigner),Assign (2)*"
	#When I send "Assign Variables" to "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Assign (2)(MultiAssignDesigner),Assign (2)*"
	#Given I click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,UI_SourceServercbx_AutoID"
	#Given I click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_Server8"
	#Given I click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,U_UI_SourceServercbx_AutoID_Server8"
	#Given I click "UI_DocManager_AutoID,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,ConnectUserControl,UI_ExplorerServerCbx_AutoID"
	#Then I click "UI_DocManager_AutoID,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,ConnectUserControl,UI_ExplorerServerCbx_AutoID,Remote Connection Integration*"

Scenario: TshepoCodedUISpecTestForMurali
	 # Given I click "RIBBONSETTINGS"    
  #     And "SECURITYVIEWCONTENT" is unchecked
  #     And I click "SECURITYVIEWCONTENT"
  #     And I click "SECURITYSAVE"
  #     Given I create a new remote connection "Svr4" as
  #     | Address               | AuthType | UserName | Password |
  #     | http://localhost:3142 | Public   |          |          |    
  #     Given I click "EXPLORER,UI_Svr4 (http://localhost:3142/)_AutoID"   
  #     Then I click "EXPLORER,UI_Svr4 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
  #     Then "EXPLORER,UI_Svr4 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanEdit_AutoID" is visible
  #     Then "EXPLORER,UI_Svr4 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is visible
  #     When I double click "EXPLORER,UI_Svr4 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"      
  #     Given "WORKFLOWDESIGNER,Decision Testing(FlowchartDesigner)" is visible within "6" seconds
		#Then "RIBBONDEBUG" is visible
  #    Then "RIBBONNEWDATABASECONNECTOR" is disabled
  #     Then "RIBBONSCHEDULE" is disabled
		#Then "RIBBONNEWPLUGINCONNECTOR" is disabled
  #     Then "RIBBONNEWWEBCONNECTOR" is disabled
  #     Then "RIBBONNEWENDPOINT" is disabled
  #     Then "RIBBONSCHEDULE" is disabled
       
     #Given I click "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_tshepo_AutoID"
     #Given I right click "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_tshepo_AutoID"
     #Then I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	 Given "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_tshepo_AutoID,v.1*,UI_CanEdit_AutoID" is disabled
	 #Given all tabs are closed
	 #Given "RIBBONSCHEDULE" is enabled within "1" seconds
	 # Given I double click "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_tshepo_AutoID"
	 #Given "EXPLORER,UI_localhost_AutoID,UI_BARNEY_AutoID,UI_tshepo_AutoID,UI_CanEdit_AutoID" is not visible

Scenario: MuraliAgainWithAutoIDS
       #Given I have Warewolf running
       #And I click "EXPLORER,UI_localhost_AutoID" 
       #And I click "RIBBONSETTINGS"   
       #And I click "SECURITYPUBLICADMINISTRATOR"  
       #And "SECURITYPUBLICVIEW" is unchecked
       #And "SECURITYPUBLICEXECUTE" is unchecked
       #And "SECURITYPUBLICDEPLOYTO" is unchecked
       #And "SECURITYPUBLICDEPLOYFROM" is unchecked
       #And I click "SECURITYPUBLICDEPLOYFROM"
       #And I click "SECURITYSAVE"  
       ##And I create a new remote connection "Svr0054" as
       #| Address               | AuthType | UserName | Password |
       #| http://localhost:3142 | Public   |          |          |
       #And I click "EXPLORER,UI_Svr0054 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
       #Then "EXPLORER,UI_Svr0054 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_NotAutherized_AutoID" is visible
       ##Given "EXPLORER,UI_Svr0054 (http://localhost:3142/)_AutoID,UI_BARNEY_AutoID,UI_Decision Testing_AutoID,UI_CanExecute_AutoID" is disabled
       #Given I click "EXPLORER,UI_Server5 (http://localhost:3142/)_AutoID" 
       #Then "RIBBONNEWENDPOINT" is disabled
       #Then "RIBBONSETTINGS" is disabled
       #Then "RIBBONNEWDATABASECONNECTOR" is disabled
       #Then "RIBBONSCHEDULE" is disabled                   
       #Then "RIBBONNEWPLUGINCONNECTOR" is disabled
       #Then "RIBBONNEWWEBCONNECTOR" is disabled
       #Then "RIBBONDEPLOY" is visible
       #Then I click "RIBBONDEPLOY"
       Given I click "UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_Svr0054"
       Then "DEPLOYDESTINATION,UI_DestinationServer_Svr0054 (http://localhost:3142/)_AutoID,UI_Unautherized_DeployToText_AutoID" is visible

Scenario: BarneyTest1
	Given I click "EXPLORERFILTERCLEARBUTTON"	
	And I type "Utility" in "EXPLORERFILTER"
	#Given I double click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	#Then "UI_There is no version history to display_AutoID" is visible within "2" seconds
	#And  I click "EXPLORERFILTERCLEARBUTTON"
	#And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	#When I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	#Then "UI_There is no version history to display_AutoID" is visible within "2" seconds
	Given I click "RIBBONSAVE"
	#And I click "RIBBONSAVE"
