Feature: SharepointUpdate
	In order to Update Sharepoint enteries on a sharepoint server
	As a Warewolf user
	I want a tool that performs this action


Scenario: Opening new Sharepoint Update Tool
	Given I drag Sharepoint Update Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When I press New
	Then the sharepoint source window is opened

Scenario: Edit and Existing sharepoint Source
	Given I drag Sharepoint Update Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And I press Edit
	Then the SharepointTestServer Sharepoint source window is opened

Scenario: Refresh Sharepoint list
	Given I drag Sharepoint Update Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And App is selected as the list
	When I click Refresh
	Then the Sharepoint Update Tool is refreshed

Scenario Outline: Insert values to sharepoint service
	Given I drag Sharepoint Update Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And App is selected as the list
	And Sharepoint Update variables are
	| [[Variable                 | To Field     | Type |
	| [[appdata(*).ContentType]] | Content Type | Text |
	| [[appdata(*).Title         | Title        | Text |
	| [[appdata(*).Attachments]] | Attachments  | Text |
	And Sharepoint Update Input variables are
	| Field Name | Match Type | Value   |
	| Title      | =          | <Value> |
Examples: 
  | Value                |
  | [[appdata(*).Title]] |
  | [[scalar]]           |
  | This is value        |
