Feature: New Database Source
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@CreatingNewDBSource
Scenario: Opening New DB Source Popup
    Given I click "New" on "New Database Service"
	Then I have New Database Source opened
	And type is selected as "SqlDatabase"
	And Server textbox is "visible"
	And I Select Authentication Typ as
	| Windows  | User       |
	| Selected | Unselected |
	And "Test Connection" is "Disabled"
	And select "Database" dropdown is "Invisible"
	And "Save" is "Disabled"
	And "Cancel" is "Enabled"


Scenario: Creating New DB Source Popup
   Given I have New Database Source opened
   And I type Server as "RSAKLFSVRGENDEV"
   And select "Database" dropdown is "Invisible"
   And "Save" is "Disabled"
   And "Cancel" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Typ as
	| Windows  | User       |
	| Selected | Unselected |
   And "Test Connection" is "Enabled"
   When I Test Connection
   Then Test Connecton is "Successful"
   Then select "Database" dropdown is "Visible"
   When I selct "Dev2TestingDB" as Database

	
	