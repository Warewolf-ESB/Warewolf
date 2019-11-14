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
   And server port is "6379" 
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
   And I type port number as "6379"
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
   And I type port number as "6379"
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
   And HostName is "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type port number as "6379"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And Select Authentication Type as "Anonymous"
   And Password field is "Collapsed"
   And "Save" is "Enabled"
   When I change HostName to "anotherredisserver"
   Then "Test-Redis *" tab is opened
   And "Test Connection" is "Enabled"
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
Scenario: No data in Cache
	Given Redis source "localhost"
	And I have a key "MyData"
	And No data in the cache
	And an assign "dataToStore" as 
	| var | value |
	|  [[Var1]]   |  "Test1" |
	When I execute the tool
	Then the cache will contain
	| Key    | Data             |
	| MyData | "[[Var1]],Test1" |
	And output variables have the following values
	| var      | value   |
	| [[Var1]] | "Test1" |

	
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
	Scenario: Data exists for given TTL not hit
	Given Redis source "localhost" 
	And I have a key "MyData"
	And data exists (TTL not hit) for key "MyData" as
	| Key | Data |
	| MyData | "[[Var1]],Data in cache" |
	And an assign "dataToStore" as
	| var | value |
	| [[Var1]] | "Test1" |
	When I execute the tool
	Then the assign "dataToStore" is not executed
	And output variables have the following values
	| var | value |
	| [[Var1]] | "[[Var1]],Data in cache" |

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
	Scenario: Data Not Exist For Given Key (TTL exceeded) Spec
	Given Redis source "localhost" 
	And I have a key "MyData"
	And data does not exist (TTL exceeded) for key "MyData" as
	| | |
	And an assign "dataToStore" as
	| var | value |
	| [[Var1]] | "Test1" |
	When I execute the tool
	Then the assign "dataToStore" is executed
	Then the cache will contain
	| Key | Data |
	| MyData | "[[Var1]],Test1" |
	And output variables have the following values
	| var | value |
	| [[Var1]] | "Test1" |

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
Scenario: Delete Key From Cache
	Given Redis source "localhost" 
	And I have a key "MyData"
	And No data in the cache
	And an assign "dataToStore" as 
	| var      | value   |
	| [[Var1]] | "Test1" |
	Then I have an existing key to delete "MyData"
	When I execute the delete tool
	Then Cache has been deleted