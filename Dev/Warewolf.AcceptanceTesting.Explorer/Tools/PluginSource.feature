Feature: PluginSource
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Plugin
Scenario: Default New Plugin Source
	Given I open New Plugin Source
	And file is "Selected" 
	And local drive "C" is visible in "File" window
	When I open "C" in "File" window
	Then scroll bar is Visible
	And GAC is "UnSelected"
	And Assembly is ""
	And Save Plugin source is "Disabled"

Scenario: Creating New Plugin by selecting from local system
	Given I open New Plugin Source
	And file is "Selected" 
	And local drive "C" is visible in "File" window
	When I open "C" in "File" window
	Then files in c is opened
	Then scroll bar is Visible
	And GAC is "UnSelected"
	And Assembly is ""
	And Save Plugin source is "Disabled"
	When I select a dll "c\Development\Dev\Binaries\MS Fakes\Microsoft QualityTools.Testing.Fakes.dll"
	Then Assembly is "C:\Development\Dev\Binaries\MS Fakes\Microsoft.QualityTools.Testing.Fakes.dll"
	And Save Plugin source is "Enabled"
	When I save Plugin source
	Then "save" dialogbox is opened


Scenario: Creating New Plugin by selecting from GAC
	Given I open New Plugin Source
	And file is "Selected" 
	And local drive "C" is visible in "File" window
	And GAC is "Selected"
	Then scroll bar is Visible
	And file is "UnSelected"
	And Assembly is ""
	And Save Plugin source is "Disabled"
	When I Search for "AuditPolicyGPMan"
	And I select "AuditPolicyGPManagedStubs.Interop, Version=6.1.0.0"
	Then Assembly is "GAC:CppCodeProvider, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
	And Save Plugin source is "Enabled"
	When I save Plugin source
	Then "save" dialogbox is opened


Scenario: Editing Plugin Source
	Given I open "Edit Plugin Source - Test"
	And file is "Selected" 
	And local drive "C" is visible in "File" window
	And GAC is "UnSelected"
	Then scroll bar is Visible
	And file is "UnSelected"
	And Assembly is "Plugins/Unlimited.Email.Plugin.dll"
	And Save Plugin source is "Disabled"	
	When I select "PrimativesTestDLL - Copy.dll"
	Then Assembly is "Z:\Plugins\PrimativesTestDLL - Copy.dll"
	And Save Plugin source is "Enabled"
	When I save Plugin source
	Then "save" dialogbox is opened
