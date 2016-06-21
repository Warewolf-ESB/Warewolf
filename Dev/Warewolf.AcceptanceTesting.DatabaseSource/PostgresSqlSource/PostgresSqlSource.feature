@PostgreSource
Feature: New PostgresSql Database Source
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
##/ REQUIREMENTS
## Ensure User allows to save server source with windows credentials
## Ensure user allows to save server source as specfic user.
## Ensure user is able to select Authonication type as Windows or User
## Ensure UserName and Password fields are visible when user selects authentication type as User
## Ensure UserName and Password fields are Disappear when user changes authentication type from User to Windows.
## Ensure user is testing the source before saving
## Ensure user is allowing to save server source when test connection is successfull
## Ensure user is allowing to save server source when test connection is Unsuccessfull
## Ensure system is throwing validation message when test connection is unsuccessfull
## Ensure save button is disabled before user clicks on test connection
## Ensure save button is Enabled when test connection is successfull
## Ensure user is able to cancel Test connection.
## Ensur Cancel Tesrt button is in disabled when Test Connection button is enabled 
## Ensur Tesrt Connection is in Enabled when Cancel Test button is Disabled 
## Ensure Database dropdown is visible when test connection is successfull
## Ensure user is able to select database from the database dropdown 

@PostgreSource
Scenario: Creating New DB Source as User Auth
   Given I open New database Source
   Then "New Database Source" tab is Opened
   And Title is "New Database Source"
   And Type options has "Microsoft SQL Server" as the default
   And I select type "PostgreSql Database"
   Then Authentication type "Windows" is "Disabled"
   And I Select Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   And Database dropdown is "Collapsed"
   When I Type Server as "Localhost"
   Then the Intellisense contains these options
   | Options |
   |         |
   And Type options contains
   | Options              |
   | Microsoft SQL Server |
   | MySQL                |
   | PostgreSql Database  |
   When I type Username as "testuser"
   And I type Password as "test123"
   Then database dropdown is "Collapsed"
   And "Test Connection" is "Enabled"
   When Test Connecton is "Successful"
   Then Database dropdown is "Visible"
   Then I select "Dev2TestingDB" as Database
   Then "Save" is "Enabled" 
   When I save the source as "SavedDBSource"
   Then the save dialog is opened
   Then "SavedDBSource" tab is opened
   And title is "SavedDBSource"
   When I type Server as "RSA"
   Then "SavedDBSource *" is the tab Header
   And title is "SavedDBSource"

@PostgreSource
Scenario: Editing saved DB Source Remembers credentials
	Given I open "Database Source - testPostgres" 
	And type option has "PostgreSql Database" selected
    And Server as "localhost"
    And Authentication Type is selected as "User"
    And Username field is "testuser"
    And Password field is "******"
	And Database "Dev2TestingDB" is selected 
    And "Save" is "Disabled"
	When I Select Authentication Type as "Windows"
    Then "Test Connection" is "Enabled" 
    And "Save" is "Disabled"
    And Database dropdown is "Collapsed"
    And "Test Connection" is "Enabled"
    And Test Connecton is "Successful"
    And "Save" is "Enabled"
    And Database dropdown is "Visible"
    And I select "Dev2TestingDB2" as Database
    And "Save" is "Enabled" 