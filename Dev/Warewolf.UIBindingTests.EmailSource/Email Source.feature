@EmailSource
Feature: Email Source
	In order to share settings
	I want to save my Email source Settings
	So that I can reuse them

# Ensure New Email source tab is opened when click on "New Email source" button.
# Ensure Title is saved and displayed correctly
# Ensure From defaults to UserName
# Ensure User is able to Edit saves From and To
# Ensure system is throwing validation message when send is unsuccessful


@EmailSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Create New Email source
	Given I open New Email Source
	Then "New Email Source" tab is opened
	And the title is "New Email Source"
	And "Host" input is ""
	And "User Name" input is ""
	And "Password" input is ""
	And "Enable SSL" input is "False"
	And "Port" input is "25"
	And "Timeout" input is "10000"
	And "From" input is ""
	And "To" input is ""
	And "Send" is "Disabled"
	And "Save" is "Disabled"

@EmailSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: From Defaults to User Name But Not After Change
	Given I open New Email Source
	Then "New Email Source" tab is opened
	When I type Username as "warewolf@dev2.co.za"
	Then From input is "warewolf@dev2.co.za"
	When I type From as "info@dev2.co.za"
	Then "User Name" input is "warewolf@dev2.co.za"

@EmailSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Enable Send and Enable Save With Validation
	Given I open New Email Source
	Then "New Email Source" tab is opened
	And I type Host as "smtp.gmail.com"
	And I type Username as "warewolf@dev2.co.za"
	And I type Password as "Dev_tech*"
	And "Enable SSL" input is "False"
	And "Port" input is "25"
	And "Timeout" input is "10000"
	And "Send" is "Enabled"
	And "Save" is "Disabled"
	And I type To as "info@dev2.co.za"
	And "Send" is "Enabled"
	When I click "Send"
	And Send is "Successful"
	When I save as "TestEmail"
	And the save dialog is opened
	
@EmailSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Fail Send
	Given I open New Email Source
	Then "New Email Source" tab is opened
	And I type Host as "smtp.gmail.com"
	And I type Username as "warewolf@dev2.co.za"
	And I type Password as "Dev_tech*"
	And "Enable SSL" input is "False"
	And "Port" input is "25"
	And "Timeout" input is "10000"
	And "Send" is "Enabled"
	And "Save" is "Disabled"
	And I type From as "warewolf@dev2.co.za"
	And I type To as "queries@dev2.co.za"
	Then "Send" is "Enabled"
	And Send is "Unsuccessful"
	Then Send is "Failed to Send: One or more errors occurred"
	And "Save" is "Disabled"

@EmailSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Edit saves From and To
	Given I open "Test Email Source"
	Then "Test Email Source" tab is opened
	And "Host" input is "smtp.gmail.com"
	And "User Name" input is "warewolf@dev2.co.za"
	And "Password" input is "Dev_tech*"
	And "Enable SSL" input is "False"
	And "Port" input is "25"
	And "Timeout" input is "100"
	And "From" input is "warewolf@dev2.co.za"
	And "To" input is "info@dev2.co.za"
	And "Send" is "Enabled"
	And "Save" is "Disabled"
	
