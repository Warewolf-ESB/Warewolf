@DbSource
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

@DbSource
Scenario: Creating New DB Source as User Auth
   Given I open New database Source
   Then "New Database Source" tab is Opened
   And Title is "New Database Source"
   When I Type Server as "Localhost"
   Then the Intellisense contains these options
   | Options         |
   | Test |
   | admin |
   | postgres |
   And Type options contains
   | Options              |
   | Microsoft SQL Server |
   | MySQL                |
   | PostgreSql Database                |
   And I Type Select The Server as "Localhost"
   And Type options has "Microsoft SQL Server" as the default
   And database dropdown is "Collapsed"
   And I Select Authentication type as "User"
   And "Save" Save is "Disabled"
   And "Test Connection" Connection is "Enabled"
   Then Username Is "Collapsed"
   And Password  Is "Collapsed"
   Then Database dropdown is "Collapsed"
   And "Test Connection" Con Is "Enabled"
   And "Cancel Test" Cancel Is "Disabled"
   When I Click test connnection "Test Connection"
   Then "Cancel Test" Disabled Is "Disabled"
   Then Test Connecton Is "Successful"
   Then Database dropdown is "Visible"
   Then I select "postgres" As Database
   And "Save" Save Is Enabled "Enabled"   
   When I save the source As "SavedDBSource"
   Then The save dialog is opened
   Then "SavedDBSource" Save Tab is opened
   And Title is "SavedDBSource"
   When I Type Server as "RSA"
   Then "SavedDBSource *" Is the tab Header

    @DbSource
Scenario: Incorrect PostgreSQL Server Address Doesnt Allow save user Auth
      Given I open New database Source
      And I Type Server as "LocalHostTest"
      And I Select Authentication type as "User"
      Then username field is "Visible"
      And password field is "Visible"
	  When I Type Username as "testuser"
	  And I Type Password as "test123"
      Then database dropdown is "Collapsed"
      And "Test Connection" Connection is "Enabled"
      When Test connecton is "Unsuccessful"
      Then database dropdown is "Collapsed"
      And "Save" Save is "Disabled"

	  @DbSource
Scenario: Editing saved PostgreSQL Source Remembers credentials
	Given I Open "Database Source - testPostgres" 
    And server as "localhost"
    And Authentication type is selected as "User"
    And Username field Is "testuser"
    And Password field Is "******"
    And "Test Connection" connection is "Enabled"
	And Database "postgres" Is selected 
    And "Save" save is "Disabled"
	When I Edit server as "192.168.1.1"
	Then Authentication type is selected as "User"
    Then Username Is "testuser"
    And Password  Is "******"
    Then "Test Connection" Connection is "Enabled" 
    And "Save" Save is "Disabled"
    Then Database dropdown is "Collapsed"
    And "Test Connection" Connection is "Enabled"
    When Test connecton is "Successful"
	Then "Save" Save is "Enabled"
	
@DbSource
Scenario: Cancel PostGreSQL Test
   Given I open New database Source
   And I Type Server as "LocalHostTest"
   And "Save" save is "Disabled"
   And "Test Connection" connection is "Enabled"
   And I Select Authentication type as "User"
   When I Type Username as "testuser"
   And I Type Password as "******"
   When Test connecton is "Long Running"
   And I cancel the Test
   Then the Validation message as "Test Cancelled" 
   Then Test Connecton is "Enabled"
   And "Save" Save is "Disabled"
  