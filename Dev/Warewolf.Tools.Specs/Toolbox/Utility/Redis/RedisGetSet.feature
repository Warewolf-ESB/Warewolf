Feature: RedisGetSet
	In order to avoid reading data source every time I require data
	As a user
	I want to cache data for as long as I need to use it and delete and release the data key to be reused

@RedisGetSet
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
#@MSTest:DeploymentItem:Warewolf_Studio.exe
#@MSTest:DeploymentItem:Newtonsoft.Json.dll
#@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
#@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
#@MSTest:DeploymentItem:System.Windows.Interactivity.dll
#@MSTest:DeploymentItem:EnableDocker.txt
Scenario: No data in Cache
	Given Redis source "localhost"
	And I have a key "MyData"
	And No data in the cache
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the get/set tool
	Then the cache will contain
		| Key    | Data             |
		| MyData | "[[Var1]],Test1" |
	And output variables have the following values
		| var      | value   |
		| [[Var1]] | "Test1" |

@RedisGetSet
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
#@MSTest:DeploymentItem:Warewolf_Studio.exe
#@MSTest:DeploymentItem:Newtonsoft.Json.dll
#@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
#@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
#@MSTest:DeploymentItem:System.Windows.Interactivity.dll
#@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Data exists for given TTL not hit
	Given Redis source "localhost"
	And I have a key "MyData"
	And data exists (TTL not hit) for key "MyData" as
		| Key    | Data                     |
		| MyData | "[[Var1]],Data in cache" |
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the get/set tool
	Then the assign "dataToStore" is not executed
	And output variables have the following values
		| var      | value                    |
		| [[Var1]] | "[[Var1]],Data in cache" |

@RedisGetSet
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
#@MSTest:DeploymentItem:Warewolf_Studio.exe
#@MSTest:DeploymentItem:Newtonsoft.Json.dll
#@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
#@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
#@MSTest:DeploymentItem:System.Windows.Interactivity.dll
#@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Data Not Exist For Given Key (TTL exceeded) Spec
	Given Redis source "localhost"
	And I have a key "MyData"
	And data does not exist (TTL exceeded) for key "MyData" as
		| | |
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the get/set tool
	Then the assign "dataToStore" is executed
	Then the cache will contain
		| Key    | Data             |
		| MyData | "[[Var1]],Test1" |
	And output variables have the following values
		| var      | value   |
		| [[Var1]] | "Test1" |

@RedisGetSet
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Grids.XamGrid.v15.1.dll
#@MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll
#@MSTest:DeploymentItem:Warewolf_Studio.exe
#@MSTest:DeploymentItem:Newtonsoft.Json.dll
#@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
#@MSTest:DeploymentItem:Warewolf.Studio.Themes.Luna.dll
#@MSTest:DeploymentItem:System.Windows.Interactivity.dll
#@MSTest:DeploymentItem:EnableDocker.txt
Scenario: Delete Key From Cache
	Given Redis source "localhost"
	And I have a key "MyData"
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	Then The "MyData" Cache exists
	Then I have an existing key to delete "MyData"
	When I execute the delete tool
	Then The "MyData" Cache has been deleted