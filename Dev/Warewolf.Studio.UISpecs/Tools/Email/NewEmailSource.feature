Feature: NewEmailSource
	In order to share settings
	I want to save my Email source Settings
	So that I can reuse them
	
@ignore
@EmailSource
Scenario: Creating New Email Source Opened
	Given I have New Email Source tab opened
	And "Host Textbox" is focussed 
	And User Name is "visible"
	And Password is "visible"
	And Enable SSL radio button is selected as "NO"
	And port is set to "25"
	And Timeout is set to "100"
	And "Test Connection" is "Disabled"
	And Save Email Source is "Disabled"
	And cancel is "Enabled"
	
@ignore
Scenario: Creating Email Source with valid credentials
	Given I have New Email Source tab opened
	And "Host Textbox" is focussed 
	When I enter host as "smtp.gmail.com"
	Then User Name is "visible"
	And Password is "visible"
	And Enable SSL radio button is selected as "NO"
	And port is set to "25"
	And Timeout is set to "100"
	And "Test Connection" is "Disabled"
	And Save Email Source is "Disabled"
	And cancel is "Enabled"
	When I enter username as ""
	Then "Test Connection" is "Disabled"
	And Save Email Source is "Disabled"
	When I enter password as ""
	Then "Test Connection" is "Enabled"
	When I click Test Connection
	Then Send Test Email popup is opened
	And from text box is as ""
	And To text box as ""
	When I enter To email as ""
	Then send is "Enabled"
	And cancel is "Disabled"
	When Test Connecton is "Successful"
	Then Save Email Source is "Enabled"
		
@ignore
Scenario: Not allowing to save email source with test connection is unsuccessful
	Given I have New Email Source tab opened
	And "Host Textbox" is focussed 
	When I enter host as "smtp.gmail.com"
	Then User Name is "visible"
	And Password is "visible"
	And Enable SSL radio button is selected as "NO"
	And port is set to "25"
	And Timeout is set to "100"
	And "Test Connection" is "Disabled"
	And Save Email Source is "Disabled"
	And cancel is "Enabled"
	When I enter username as ""
	Then "Test Connection" is "Disabled"
	And Save Email Source is "Disabled"
	When I enter password as ""
	Then "Test Connection" is "Enabled"
	When I click Test Connection
	Then Send Test Email popup is opened
	And from text box is as ""
	And To text box as ""
	When I enter To email as ""
	Then send is "Enabled"
	And cancel is "Disabled"
	When Test Connecton is "UnSuccessful"
	Then Save Email Source is "Disabled"