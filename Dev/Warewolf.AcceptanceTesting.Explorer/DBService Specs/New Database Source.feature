@CreatingNewDBSource
Feature: New Database Source
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


##// REQUIREMENTS
##* Ensure User allows to save server source with windows credentials
##* Ensure user allows to save server source as specfic user.
##* Ensure user is testing the source before saving
##* Ensure user is allowing to save server source when test connection is successfull
##* Ensure user is allowing to save server source when test connection is Unsuccessfull
##* Ensure system is throwing validation message when test connection is unsuccessfull
##* Ensure save button is disabled before user clicks on test connection
##* Ensure Database dropdown is visible when test connection is successfull
##* Ensure user is able to select database from the database dropdown 



Scenario: Creating New DB Source 
   Given I open New Database Source
   And I type Server as "RSAKLFSVRGENDEV"
   And Database dropdown is "Invisible"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "Windows"
   Then Username field is "InVisible"
   And Password field is "InVisible"
   Then Database dropdown is "InVisible"
   And "Test Connection" is "Enabled"
   When Test Connecton is "Successful"
   Then Database dropdown is "Visible"
   Then I select "Dev2TestingDB" as Database
   And "Save" is "Enabled"   
   When I save the source
   Then the save dialog is opened
	
	
#Scenario: Creating New DB Source under auth type as user
#    Given I open New Database Source
#    And I type Server as "RSAKLFSVRGENDEV"
#    And select "Database" dropdown is "Invisible"
#    And "Save" is "Disabled"
#    And "Cancel" is "Enabled"
#    And "Test Connection" is "Enabled"
#    And I Select Authentication Typ as
#	 | Windows    | User     |
#	 | UnSelected | Selected |
#    Then Username field is "Visible"
#    And Password field is "Visible"
#    And "Test Connection" is "Disbled"
#    And "Save" is "Disabled"
#    When I type Username as "testuser"
#    And I type Password as "test123"
#    Then "Test Connection" is "Enabled" 
#    And "Save" is "Enabled"
#    Then select "Database" dropdown is "InVisible"
#    And "Test Connection" is "Enabled"
#    When I Test Connection
#    Then Test Connecton is "Successful"
#    And "Save" is "Enabled"
#    And select "Database" dropdown is "Visible"
#    When I selct "Dev2TestingDB" as Database
#    Then "Save" is "Enabled" 
#    When I save the source
#    Then the save dialog is opened
#
#
#
#Scenario: Incorrect Server address wind auth type allowing to save
#      Given I open New Database Source
#	  And "Save" is "Disabled"
#      And I type Server as "Incorrect"
#      And select "Database" dropdown is "Invisible"
#      And "Save" is "Enabled"
#      And I Select Authentication Typ as
#	   | Windows  | User       |
#	   | Selected | Unselected |
#      Then Username field is "InVisible"
#      And Password field is "InVisible"
#      Then select "Database" dropdown is "InVisible"
#      And "Test Connection" is "Enabled"
#      When I Test Connection
#      Then Test Connecton is "Unsuccessful"
#      And the validation message as ""
#      Then select "Database" dropdown is "InVisible"
#      And "Save" is "Enabled"
#      When I save the source
#      Then the save dialog is opened
#
#  
#Scenario: Incorrect Server source with user auth type is not allowing to save
#      Given I open New Database Source
#      And I type Server as "Incorrect"
#      And select "Database" dropdown is "Invisible"
#      And "Save" is "Enabled"
#      And I Select Authentication Typ as
#	   | Windows    | User     |
#	   | UnSelected | Selected |
#      Then Username field is "InVisible"
#      And Password field is "InVisible"
#      Then select "Database" dropdown is "InVisible"
#      And "Test Connection" is "Enabled"
#      When I Test Connection
#      Then Test Connecton is "Unsuccessful"
#      And "Save" is "Enabled"
#      Then select "Database" dropdown is "InVisible"
#      When I save the source
#      Then the save dialog is opened
#
#
#
#Scenario: Testing as Windows and swaping it resets the test connection 
#      Given I open New Database Source
#      And "Save" is "Disabled"
#      And "Cancel" is "Enabled"
#      And I type Server as "RSAKLFSVRGENDEV"
#      And select "Database" dropdown is "Invisible"
#      And "Save" is "Enabled"
#      And "Cancel" is "Enabled"
#      And "Test Connection" is "Enabled"
#      And I Select Authentication Typ as
#	   | Windows    | User     |
#	   | UnSelected | Selected |
#      Then Username field is "Visible"
#      And Password field is "Visible"
#      And "Test Connection" is "Disabled"
#      And "Save" is "Enabled"
#      When I type Username as "testuser"
#      And I type Password as "test123"
#      Then "Test Connection" is "Enabled" 
#      And "Save" is "Enabled"
#      Then select "Database" dropdown is "InVisible"
#      And "Test Connection" is "Enabled"
#      When I Test Connection
#      Then Test Connecton is "Successful"
#      And "Save" is "Enabled"
#      And select "Database" dropdown is "Visible"
#      When I selct "Dev2TestingDB" as Database
#      And I Select Authentication Typ as
#	   | Windows  | User       |
#	   | Selected | UnSelected |
#      Then "Test Connection" is "Enabled" 
#      And "Save" is "Enabled"
#      Then Test Connecton is ""
#      Then select "Database" dropdown is "InVisible"
#      And "Test Connection" is "Enabled"
#      When I Select Authentication Typ as
#	   | Windows    | User     |
#	   | UnSelected | Selected |
#      Then Username field is "Visible" with username as ""
#      And Password field is "Visible"with with password as ""
#      And "Test Connection" is "Disabled"
#      And "Save" is "Disabled"
   















