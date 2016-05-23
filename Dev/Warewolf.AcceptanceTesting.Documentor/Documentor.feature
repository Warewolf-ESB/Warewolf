Feature: Documentor
	In order to provide a description for each component to the user 
	As a Warewolf user
	I want to be shown all aspects of item clicked

Scenario: Context menu
	Given the explorer is visible
	And I right click on "Hello World" 
	Then a context menu is visible
	And "Open" is "Visible"
	And "Deploy Hello World" is "Visible"
	And "Rename" is "Visible"
	And "Delete" is "Visible"
	And "Show Dependancies" is "Visible"
	And "Show Document" is "Visible"
	And "Debug" is "Visible"
	And "Show Version History" is "Visible"
	When I click "Show Document"
	Then the Document window is opened
	And a description of the "Hello World" workflow is visible

Scenario: Message displayed in Documentor window
	Given I click "New DataBase Service Connector"
	Then "New DB Connector" tab is opened
	And Data Source is focused
	And "1 Data Source" is "Enabled"
	When I select "DemoDB" as data source
	Then the Documentor window is updated with the message 
	| "Database Service Source Types             |
	| Allows you to select a database souce type |
