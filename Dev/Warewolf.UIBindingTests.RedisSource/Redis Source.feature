@WebSource
@RedisSource
Feature: Redis Source
	In order to create a Redis source for accessing a Redis Cache Server
	As a Warewolf user
	I want to be able to manage Redis sources easily

@RedisSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt	
Scenario: Creating New Redis Source 
   Given I open New Redis Source 
   Then "New Redis Source" tab is opened
   And title is "New Redis Source"
   And I type HostName as "localhost"
   And I type port number as "6379"
   Then "New Redis Source *" tab is opened
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And "Cancel Test" is "Disabled"
   And I Select Authentication Type as "Anonymous"
   And Password field is "Collapsed"
   When Test Connecton is "Successful"
   And "Save" is "Enabled"
   When I save as "Testing Redis Source Save"
   Then the save dialog is opened
   Then title is "Testing Redis Source Save"
   And "Testing Redis Source Save" tab is opened

@RedisSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt	   
Scenario: Creating New Redis Source under password
   Given I open New Redis Source
   And I type HostName as "localhost"
   And I type port number as "6379"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "Password"
   And Password field is "Visible"
   And I type Password
   When Test Connecton is "Successful"
   And "Save" is "Enabled"
   When I save the source
   Then the save dialog is opened

@RedisSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt		
Scenario: Incorrect hostname anonymous auth type not allowing save
   Given I open New Redis Source
   And I type HostName as "sdfsdfd"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "Anonymous"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   And "Save" is "Enabled"

@RedisSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt	   	
Scenario: Testing Auth type as Anonymous and swaping it resets the test connection 
   Given I open New Redis Source
   And "Save" is "Disabled"
   And I type HostName as "localhost" 
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

@RedisSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
@MSTest:DeploymentItem:EnableDocker.txt		 	 
Scenario: Editing saved Redis Source 
   Given I open "Test-Redis" redis source
   Then "Test-Redis" tab is opened
   And title is "Test-Redis"
   And HostName is "localhost"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And Select Authentication Type as "Anonymous"
   And Password field is "Collapsed"
   And "Save" is "Enabled"
   When I change HostName to "anotherredisserver"
   Then "Test-Redis *" tab is opened
   And "Test Connection" is "Enabled"
   And "Save" is "Enabled"