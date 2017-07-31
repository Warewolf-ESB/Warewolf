@WcfSource
Feature: Wcf Source
	In order to share settings
	I want to save my Wcf source Settings
	So that I can reuse them

@WcfSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Create New Wcf source
	Given I open New Wcf Source
	Then "New WCF Service Source" tab is opened
	And the title is "New WCF Service Source"
	And "WCF Endpoint Url" input is ""
	And "Test Connection" is "Disabled"
	And "Save" is "Disabled"

@WcfSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Fail Send Shows correct error message
	Given I open New Wcf Source
	Then "New WCF Service Source" tab is opened
	And I type WCF Endpoint Url as "test"
	And "Test Connection" is "Enabled"
	And "Save" is "Disabled"
	And Send is "Unsuccessful"
	Then Send is "Invalid URI: The format of the URI could not be determined."
	And "Save" is "Disabled"