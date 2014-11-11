Feature: DeployFeature
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DeployTab
Scenario: IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled12
	   Given I have Warewolf running
	   Given I start Studio as "" with password ""
	   Given I click "RIBBONDEPLOY"
	   ##Checking Validation Message When Source and Destination Servers Are Same
	   Given "DEPLOYERROR" is visible
	   ##Checking Deploy Button Disabled When Nothing Selected To Deploy
	   Given "DEPLOYBUTTON" is disabled
	   ##Checking Deploy Button Disabled When Source and Destination Servers are Same
	   Given I type "Decision Testing" in "DEPLOYSOURCEFILTER"
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
	   Given "DEPLOYBUTTON" is disabled
	  ##  And I click "DEPLOYSOURCEFILTERCLEAR"
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,Expander"
	   ##Selecting Remote Server In Destination Connect Control Dropdown
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
	   Given I create a new remote connection as "Deployrem" in Deploy Destination 
       | Address                  | AuthType | UserName      | Password |
       | http://TST7X64W:3142/dsf | User     | Administrator |          |
	   ##Checking Deploy Button Enabled When Resource Selected in Source Server
	   Given I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
       Given "DEPLOYBUTTON" is enabled
       ## Given "DEPLOYERROR" is not visible
	  ##Cleanup (Deleting the created server)
	   Given I click "EXPLORER,UI_localhost_AutoID" 
	   Given I click "EXPLORER,UI_Deployrem (http://tst7x64w:3142/)_AutoID" 
	   Given I click "EXPLORERCONNECTBUTTON"
	   Given I click "EXPLORER,UI_SourceServerRefreshbtn_AutoID"
       Given I send "Deployrem" to "EXPLORERFILTER"
	   And I right click "EXPLORERFOLDERS,UI_Deployrem_AutoID"
	   And I send "{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	   And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"












	   











