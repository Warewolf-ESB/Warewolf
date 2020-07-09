@WebSource
@ElasticsearchSource
Feature: Elasticsearch Source
	In order to share settings
	I want to save my Elasticsearch source Settings
	So that I can reuse them

@ElasticsearchSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Creating New Elasticsearch Source
	Given I open New Elasticsearch Source
	Then "New Elasticsearch Source" tab is opened
	And title is "New Elasticsearch Source"
	Given I type HostName as a valid anonymous Elasticsearch server
	Then server port is "9200"
	And I type port number as "9200"
	And I type search index as "warewolflogs"
	Then "New Elasticsearch Source *" tab is opened
	And "Save" is "Enabled"
	And "Test Connection" is "Enabled" 
	And "Cancel Test" is "Disabled"
	And I Select Authentication Type as "Anonymous"
	And Password field is "Collapsed"
	When Test Connecton is "Successful"
	And "Save" is "Enabled"
	When I save as "Testing Elasticsearch Source Save"
	Then the save dialog is opened
	Then title is "Testing Elasticsearch Source Save"
	And "Testing Elasticsearch Source Save" tab is opened

@ElasticsearchSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Creating New Elasticsearch Source under password
	Given I open New Elasticsearch Source
	And I type HostName as a valid Elasticsearch server
	And I type port number as "9200"
	Then I type search index as "warewolflogs"
	And "Save" is "Enabled"
	And "Test Connection" is "Enabled"
	And I Select Authentication Type as "Password"
	And Password field is "Visible"
	And I type Password
	When Test Connecton is "Successful"
	And "Save" is "Enabled"
	When I save the source
	Then the save dialog is opened

@ElasticsearchSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Incorrect Elasticsearch hostname anonymous auth type not allowing save
	Given I open New Elasticsearch Source
	And I type HostName as "sdfsdfd"
	And I type port number as "9200"
	Then I type search index as "warewolflogs"
	And "Save" is "Enabled"
	And "Test Connection" is "Enabled"
	And I Select Authentication Type as "Anonymous"
	When Test Connecton is "UnSuccessful"
	And Validation message is thrown
	And "Save" is "Enabled"

@ElasticsearchSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Testing Elasticsearch Auth type as Anonymous and swaping it resets the test connection
	Given I open New Elasticsearch Source
	And "Save" is "Disabled"
	And I type HostName as a valid Elasticsearch server
	And I type port number as "9200"
	Then I type search index as "warewolflogs"
	And "Save" is "Enabled"
	And "Test Connection" is "Enabled"
	And I Select Authentication Type as "Password"
	And I type Password
	When Test Connecton is "Successful"
	And Validation message is Not thrown
	And "Save" is "Enabled"
	And I Select Authentication Type as "Anonymous"
	And Password field is "Collapsed"
	And "Save" is "Enabled"
	When Test Connecton is "Successful"
	And Validation message is Not thrown
	And "Save" is "Enabled"
	And I Select Authentication Type as "Password"
	And Password field is "Visible"
	And "Save" is "Enabled"

@ElasticsearchSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Editing saved Elasticsearch Source
	Given I open "Test-Elasticsearch" Elasticsearch source
	Then "Test-Elasticsearch" tab is opened
	And title is "Test-Elasticsearch"
	And HostName is "http://rsaklfwynand"
	And I type port number as "9200"
	And I type search index as "warewolflogs"
	And "Save" is "Enabled"
	And "Test Connection" is "Enabled"
	And Select Authentication Type as "Anonymous"
	And Password field is "Collapsed"
	And "Save" is "Enabled"
	When I change HostName to "anotherElasticsearchserver"
	Then "Test-Elasticsearch *" tab is opened
	And "Test Connection" is "Enabled"
	And "Save" is "Enabled"
