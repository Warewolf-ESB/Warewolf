Feature: SharepointRead
	In order to Read Sharepoint enteries on a sharepoint server
	As a Warewolf user
	I want a tool that performs this action


Scenario: Opening new Sharepoint Read Tool
	Given I have a new Workflow
	And I drag Sharepoint Read Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When I press New
	Then the sharepoint source window is opened

Scenario: Edit and Existing sharepoint Source
	Given I have a new Workflow
	And I drag Sharepoint Read Tool onto the design surface 
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
	And I drag Sharepoint Read Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And App is selected as the list
	When I click Refresh
	Then the Sharepoint Read Tool is refreshed

Scenario: Insert values to sharepoint service
	Given I have a new Workflow
	And I drag Sharepoint Read Tool onto the design surface 
	And Sharepoint Server source is Enabled
	And Sharepoint List is Enabled
	And Edit is Disabled
	And Refresh is Enabled
	And New is Enabled
	When SharepointTestServer is selected as the data source
	And App is selected as the list
	And Sharepoint Read variables are
	| From Field         | [[Variable]]                    |
	| ID                 | [[appdata(*).ID]]               |
	| Content Type       | [[appdata(*).ContentType]]      |
	| Title              | [[appdata(*).Title]]            |
	| Modified           | [[appdata(*).Modified]]         |
	| Create             | [[appdata(*).Created]]          |
	| Created By         | [[appdata(*).CreatedBy]]        |
	| Modified By        | [[appdata(*).ModifiedBy]]       |
	| Version            | [[appdata(*).Version]]          |
	| Attachments        | [[appdata(*).Attachments]]      |
	| Edit               | [[appdata(*).Edit]]             |
	| Title              | [[appdata(*).Title]]            |
	| Type               | [[appdata(*).Type]]             |
	| Item Child Count   | [[appdata(*).ItemChildCount]]   |
	| Folder Child Count | [[appdata(*).FolderChildCount]] |
	| App Create By      | [[appdata(*).AppCreatedBy]]     |
	| App Modified By    | [[appdata(*).AppModifiedBy]]    |