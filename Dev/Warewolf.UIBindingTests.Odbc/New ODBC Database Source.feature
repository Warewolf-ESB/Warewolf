
@DbSource
Feature: New ODBC Source
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

@ODBCSource
Scenario: Creating New DB Source General Testing
   Given I open New Database Source
   Then "New ODBC Source" tab is opened
   And title is "New ODBC Source"   
   And Database dropdown is "Collapsed"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then Database dropdown is "Collapsed"
   And "Test Connection" is "Enabled"
   And "Cancel Test" is "Disabled"
   When I click "Test Connection"
   Then "Cancel Test" is "Disabled"
   Then Test Connecton is "Successful"
   Then Database dropdown is "Visible"
   Then I select "Dev2TestingDB" as Database
   And "Save" is "Enabled"   
   When I save the source as "SavedDBSource"
   Then the save dialog is opened
   Then "SavedDBSource" tab is opened
   And title is "SavedDBSource"

@ODBCSource
Scenario: Cancel DB Source Test
   Given I open New Database Source
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   When Test Connecton is "Long Running"
   And I Cancel the Test
   Then the validation message as "Test Cancelled" 
   Then "Test Connection" is "Enabled"
   And "Save" is "Disabled"

@ODBCSource
Scenario: Creating New DB Source as Windows Auth
	Given I open New Database Source
	And "Save" is "Disabled"
	And "Test Connection" is "Enabled"
	Then Test Connecton is "Successful"	
	And "Save" is "Disabled"
	When I select "Dev2TestingDB" as Database
	Then "Save" is "Enabled" 
	
