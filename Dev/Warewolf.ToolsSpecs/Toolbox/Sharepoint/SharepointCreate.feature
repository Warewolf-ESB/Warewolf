Feature: SharepointCreate
	In order to Create new Sharepoint enteries to a sharepoint server
	As a Warewolf user
	I want a tool that performs this action


Scenario: Opening new Sharepoint Create Tool
	Given I have a new Workflow
	And I drag Sharepoint Create Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When I press New
	Then the sharepoint source window is opened

Scenario: Edit and Existing sharepoint Source
	Given I have a new Workflow
	And I drag Sharepoint Create Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And I press Edit
	Then the SharepointTestServer Sharepoint source window is opened

Scenario: Refresh Sharepoint list
    Given I have a new Workflow
	And I drag Sharepoint Create Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And App is selected as the list
	When I click Refresh
	Then the Sharepoint Create Tool is refreshed

Scenario Outline: Insert values to sharepoint service
	Given I have a new Workflow
	And I drag Sharepoint Create Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And App is selected as the list
	And Sharepoint Create Input variables are
	| Content Type | Title   | Attachments   |
	| <Content>    | <Title> | <Attachments> |
Examples: 
| Content                    | Title                | Attachments                |
| [[appdata(*).ContentType]] | [[appdata(*).Title]] | [[appdata(*).Attachments]] |
| [[Variable]]               | [[var]]              | [[scalar]]                 |
| Test                       | Mr                   | This is value              |