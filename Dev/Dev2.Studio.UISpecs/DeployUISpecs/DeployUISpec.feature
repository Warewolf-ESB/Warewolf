Feature: DeployFeature
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

#3This is migrated in new UI Specs
 #@DeployTab
#Scenario: IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled12
#	   Given I have Warewolf running
#	   And all tabs are closed
#	   And I click "EXPLORERFILTERCLEARBUTTON"   
#	   And I click "RIBBONDEPLOY"
#	   ##Checking Validation Message When Source and Destination Servers Are Same
#	   And "DEPLOYERROR" is visible
#	   ##Checking Deploy Button Disabled When Nothing Selected To Deploy
#	   And "DEPLOYBUTTON" is disabled
#	   ##Checking Deploy Button Disabled When Source and Destination Servers are Same
#	   And I type "Decision Testing" in "DEPLOYSOURCEFILTER"
#	   And I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
#	   And "DEPLOYBUTTON" is disabled
#	  ##  And I click "DEPLOYSOURCEFILTERCLEAR"
#	   And I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,Expander"
#	   ##Selecting Remote Server In Destination Connect Control Dropdown
#	   And I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
#	   Given I create a new remote connection as "Deployrem" in Deploy Destination 
#       | Address                      | AuthType | UserName          | Password |
#       | http://SANDBOX-DEV2:3142/dsf | User     | IntegrationTester | I73573r0 |  
#	   ##Checking Deploy Button Enabled When Resource Selected in Source Server
#	   And I click "DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_AutoID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing_AutoID"
#       And "DEPLOYBUTTON" is enabled
#       ## Given "DEPLOYERROR" is not visible
#	  ##Cleanup (Deleting the created server)
#	   Given I click "EXPLORER,UI_localhost_AutoID" 
#	   Given I click "EXPLORER,UI_Deployrem (http://sandbox-dev2:3142/)_AutoID" 
#	   Given I click "EXPLORERCONNECTBUTTON"
#	   Given I click "EXPLORER,UI_SourceServerRefreshbtn_AutoID"
#       Given I send "Deployrem" to "EXPLORERFILTER"
#	   And I right click "EXPLORERFOLDERS,UI_Deployrem_AutoID"
#	   And I send "{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
#	   And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"













	   











