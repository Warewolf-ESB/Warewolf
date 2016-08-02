Feature: ComDll	
	In order to execute functions from a dll
	As a Warewolf user
	I want a tool that performs this action

Scenario: Creating a new workflow
Given I create New Workflow
	And I drag Comdll tool onto the design surface    
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Namespace refresh is disabled
	And Action is disabled
	And Action refresh is disabled
	And New button is Enabled	
	When I click new source
	Then the Comdll Source window is opened

Scenario: Editing existing Comdll tool
	Given I create New Workflow
	And I drag Comdll tool onto the design surface    
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled	
	When I select "ComDllSource" from source list as the source
	Then EditButton is Enabled
	And I click Edit source
	Then the Comdll Source window is opened with ComDllSource source

#
Scenario: Editing existing comdll tool then changing values
Given I create New Workflow
	And I drag Comdll tool onto the design surface    
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled	
	When I select "ComDllSource" from source list as the source
	Then EditButton is Enabled
	And I click Edit source

Scenario: Generating output
Given I create New Workflow
	And I drag Comdll tool onto the design surface    
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled
	And GenerateOutput is disabled
	When I select "ComDllSource" from source list as the source
	Then EditButton is Enabled
	And Namespace is Enabled
	And Action is Enabled
	And I select Action
	And GenerateOutput is disabled
	And I click Generate output
	Then Inputs windo is open

Scenario: Validating without selecting the source
Given I create New Workflow
	And I drag Comdll tool onto the design surface 	
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled	
	When I click Done to execute tool 
	Then Empty source error is Returned

Scenario: Validating with valid source
Given I create New Workflow
	And I drag Comdll tool onto the design surface 	
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled	
	When I select "ComDllSource" from source list as the source
	Then EditButton is Enabled
	And Namespace is Enabled
	And Action is Enabled
	And I select Action
	And GenerateOutput is disabled
	And I click Generate output
	Then Validation is successful

	
Scenario: Generating com outputs
Given I create New Workflow
	And I drag Comdll tool onto the design surface 	
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled	
	When I select "ComDllSource" from source list as the source
	Then EditButton is Enabled
	And Namespace is Enabled
	And Action is Enabled
	And I select Action
	And GenerateOutput is disabled
	And I click Generate output
	Then Validation is successful
	And I click fSix to Execute the tool the result is ""System.__ComObject""
		
Scenario: Executing com with valid source
Given I create New Workflow
	And I drag Comdll tool onto the design surface 	
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled	
	When I select "ComDllSource" from source list as the source
	Then EditButton is Enabled
	And Namespace is Enabled
	And Action is Enabled
	And I select Action
	And GenerateOutput is disabled
	And I click Generate output
	Then Validation is successful
	And I click fSix to Execute the tool the result is ""System.__ComObject""
		
Scenario: Executing com without method
Given I create New Workflow
	And I drag Comdll tool onto the design surface 	
	And EditButton is Disabled
	And Comdll Source is Enabled
	And Namespace is disabled
	And Action is disabled
	And New button is Enabled	
	When I select "ComDllSource" from source list as the source
	Then EditButton is Enabled
	And Namespace is Enabled
	And I click fSix to Execute the tool
	Then The result is returned with error "No Method Selected"
