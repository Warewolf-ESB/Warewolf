Feature: DeployFeature
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled12
	   Given I have Warewolf running
	   And all tabs are closed	
	   Given I click "RIBBONDEPLOY"
	   ##Checking Validation Message When Source and Destination Servers Are Same
	   Given "DEPLOYERROR" is visible
	   ##Checking Deploy Button Disabled When Nothing Selected To Deploy
	   Given "DEPLOYBUTTON" is disabled
	   ##Checking Deploy Button Disabled When Source and Destination Servers are Same
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
	   Given "DEPLOYBUTTON" is disabled
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,Expander"
	   ##Selecting Remote Server In Destination Connect Control Dropdown
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
       Given I click "ACTIVETAB,DeployUserControl,UI_DestinationServercbx_AutoID,U_UI_DestinationServercbx_AutoID_Azure Public"
	   ##Checking Deploy Button Enabled When Resource Selected in Source Server
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_BARNEY_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
       Given "DEPLOYBUTTON" is disabled
       ## Given "DEPLOYERROR" is not visible


	
	