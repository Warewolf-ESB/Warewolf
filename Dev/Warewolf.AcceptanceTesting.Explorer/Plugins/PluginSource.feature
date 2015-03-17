@Plugin
Feature: PluginSource
	In order to create plugins
	As a Warewolf User
	I want to be able to select dlls as a source to be used

Scenario: Default New Plugin Source
	Given I open New Plugin Source
	And file is selected
	And local drive "C" is visible in File window
	When I open "C" in File window
	And GAC is not selected
	And Assembly is ""
	And "Save" is "Disabled"

Scenario: Creating New Plugin by selecting from local system
	Given I open New Plugin Source
	And file is selected
	And local drive "C" is visible in "File" window
	When I open "C" in File window
	Then files in "C" is opened
	And GAC is not selected
	And Assembly is ""
	And "Save" is "Disabled"
	When I select "C:\Development\Dev\Binaries\MS Fakes\Microsoft QualityTools.Testing.Fakes.dll"
	Then Assembly is "C:\Development\Dev\Binaries\MS Fakes\Microsoft.QualityTools.Testing.Fakes.dll"
	And "Save" is "Enabled"
	When I save Plugin source
	Then Save Dialog is opened


Scenario: Creating New Plugin by selecting from GAC
	Given I open New Plugin Source
	And file is selected
	And local drive "C" is visible in "File" window
	And GAC is selected
	And file is not selected
	And Assembly is ""
	And "Save" is "Disabled"
	When I Search for "AuditPolicyGPMan"
	And I select "AuditPolicyGPManagedStubs.Interop, Version=6.1.0.0"
	Then Assembly is "GAC:CppCodeProvider, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
	And "Save" is "Enabled"
	When I save Plugin source
	Then Save Dialog is opened


Scenario: Editing Plugin Source
	Given I open "Edit Plugin Source - Test" plugin source
	And file is selected
	And local drive "C" is visible in "File" window
	And GAC is not selected
	And Assembly is "Plugins/Unlimited.Email.Plugin.dll"
	And "Save" is "Disabled"
	When I select "PrimativesTestDLL - Copy.dll"
	Then Assembly is "Z:\Plugins\PrimativesTestDLL - Copy.dll"
	And "Save" is "Enabled"
	When I save Plugin source
	Then Save Dialog is opened
