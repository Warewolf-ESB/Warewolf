@PluginSource
Feature: PluginSource
	In order to create plugins
	As a Warewolf User
	I want to be able to select dlls as a source to be used

## New Plugin Source File
## New Plugin Source GAC
## Editing Plugin Source


Scenario: New Plugin Source File
	Given I open New Plugin Source
	Then "New DotNet Plugin Source" tab is opened
	And title is "New DotNet Plugin Source"
	And ConfigFile textbox is "Disabled"
	And ConfigFileButton button is "Disabled"
	And "Save" is "Disabled"
	When I type "C:\Development\Dev\Binaries\MS Fakes\Microsoft.QualityTools.Testing.Fakes.dll" in "AssemblyName"
	Then ConfigFile textbox is "Enabled"
	Then ConfigFileButton button is "Enabled"
	Then "Save" is "Enabled"
	When I type "C:\Development\Dev\Binaries\MS Fakes\Microsoft.QualityTools.Testing.Fakes.dl" in "AssemblyName"
	Then ConfigFile textbox is "Disabled"
	Then ConfigFileButton button is "Disabled"
	Then "Save" is "Disabled"
	When I type "C:\Development\Dev\Binaries\MS Fakes\Microsoft.QualityTools.Testing.Fakes.dll" in "AssemblyName"
	Then ConfigFile textbox is "Enabled"
	Then ConfigFileButton button is "Enabled"
	Then "Save" is "Enabled"
	When I save as "Testing Resource Save"
    Then the save dialog is opened
	Then title is "Testing Resource Save"
	And "Testing Resource Save" tab is opened
	

Scenario: New Plugin Source GAC
Given I open New Plugin Source
	Then "New DotNet Plugin Source" tab is opened
	And title is "New DotNet Plugin Source"
	And ConfigFile textbox is "Disabled"
	And ConfigFileButton button is "Disabled"
	And "Save" is "Disabled"
	When I type "GAC:AuditPolicyGPManagedStubs, Version=6.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a.dll" in "GacAssemblyName"
	Then ConfigFile textbox is "Disabled"
	Then ConfigFileButton button is "Disabled"
	Then "Save" is "Enabled"
	When I save as "Testing Resource Save"
    Then the save dialog is opened
	Then title is "Testing Resource Save"
	And "Testing Resource Save" tab is opened

	 
Scenario: Editing Plugin Source
	Given I open "Test" plugin source
	Then title is "Test"
	And "GacAssemblyName" value is "GAC:AuditPolicyGPManagedStubs, Version=6.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
	When I type "GAC:AuditPolicyGPManagedStubs, Version=6.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3" in "GacAssemblyName"
	And "Save" is "Enabled"
	When I save Plugin source
