Feature: DeployFeature
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled12
	   Given I click "RIBBONDEPLOY"
	   ##Checking Validation Message When Source and Destination Servers Are Same
	   Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,UI_DeploySelectTB_AutoID" is visible
	   ##Checking Deploy Button Disabled When Nothing Selected To Deploy
	   Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,UI_Deploybtn_AutoID" is disabled
	   ##Checking Deploy Button Disabled When Source and Destination Servers are Same
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
	   Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,UI_Deploybtn_AutoID" is disabled
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,Expander"
	   ##Selecting Remote Server In Destination Connect Control Dropdown
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_Azure Public"
	   ##Checking Deploy Button Enabled When Resource Selected in Source Server
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
       Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,UI_Deploybtn_AutoID" is enabled
       ## Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,UI_DeploySelectTB_AutoID" is not visible


	
	