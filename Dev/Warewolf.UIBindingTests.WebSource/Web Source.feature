@WebSource
Feature: Web Source
	In order to create a web source for web services
	As a Warewolf user
	I want to be able to manage web sources easily

@WebSource
Scenario: Creating New Web Source 
   Given I open New Web Source 
   Then "New Web Service Source" tab is opened
   And title is "New Web Service Source"
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   Then "New Web Service Source *" tab is opened
   And TestQuery is "http://RSAKLFSVRTFSBLD/IntegrationTestSite/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And "Cancel Test" is "Disabled"
   And I Select Authentication Type as "Anonymous"
   And Username field is "Collapsed"
   And Password field is "Collapsed"
   When Test Connecton is "Successful"
   And "Save" is "Enabled"
   When I save as "Testing Resource Save"
   Then the save dialog is opened
   Then title is "Testing Resource Save"
   And "Testing Resource Save" tab is opened
   When I click TestQuery
   Then the browser window opens with "http://RSAKLFSVRTFSBLD/IntegrationTestSite/GetCountries.ashx?extension=json&prefix=a"
	
@WebSource
Scenario: Creating New Web Source under auth type as user
   Given I open New Web Source
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   And I type Username as "IntegrationTester"
   And I type Password as "I73573r0"
   When Test Connecton is "Successful"
   And "Save" is "Enabled"
   When I save the source
   Then the save dialog is opened
	
@WebSource
Scenario: Incorrect address anonymous auth type not allowing save
   Given I open New Web Source
   And I type Address as "sdfsdfd"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "Anonymous"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   And "Save" is "Enabled"
   	
@WebSource
Scenario: Incorrect address Shows correct error message
   Given I open New Web Source
   And I type Address as "sdfsdfd"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "Anonymous"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   And "Save" is "Enabled"
   And the error message is "Illegal characters in path."

@WebSource     
Scenario: Incorrect address user auth type is not allowing to save
   Given I open New Web Source
   And I type Address as "sdfsdfd"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "User"
   And I type Username as "test"
   And I type Password as "I73573r0"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   And "Save" is "Enabled"

@WebSource
Scenario: Testing Auth type as Anonymous and swaping it resets the test connection 
   Given I open New Web Source
   And "Save" is "Disabled"
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite" 
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And I Select Authentication Type as "User"
   And I type Username as "test"
   And I type Password as "I73573r0"
   When Test Connecton is "Successful"
   And Validation message is Not thrown
   And "Save" is "Enabled"
   And I Select Authentication Type as "Anonymous"
   And Username field is "Collapsed"
   And Password field is "Collapsed"
   And "Save" is "Enabled"
   When Test Connecton is "Successful"
   And Validation message is Not thrown
   And "Save" is "Enabled"	 
   And I Select Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   And "Save" is "Enabled" 
	 	 
@WebSource
Scenario: Editing saved Web Source 
   Given I open "Test" web source
   Then "Test" tab is opened
   And title is "Test"
   And Address is "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And Default Query is "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And Select Authentication Type as "Anonymous"
   And Username field is "Collapsed"
   And Password field is "Collapsed"
   And "Save" is "Enabled"
   When I change Address to "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=b"
   Then "Test *" tab is opened
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And "Save" is "Enabled"
   When Test Connecton is "Successfull"
   Then "Save" is "Enabled" 
   When I save the source
 
 @WebSource
 Scenario: Editing saved Web Source auth type
   Given I open "Test" web source
   Then "Test" tab is opened
   And Address is "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query is "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Enabled"
   And "Test Connection" is "Enabled"
   And Select Authentication Type as "Anonymous"
   And Username field is "Collapsed"
   And Password field is "Collapsed"
   When I edit Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   And Username field as "IntegrationTester"
   And Password field as "I73573r0"
   And "Save" is "Enabled"
   When Test Connecton is "Successfull"
   Then "Save" is "Enabled" 

@WebSource
Scenario: Cancel Seb Source Test
   Given I open New Web Source 
   Then "New Web Service Source" tab is opened
   And title is "New Web Service Source"
   And I type Address as "http://test-warewolf.cloudapp.net:3142/public/Hello%20World?Name=Me"
   When Test Connecton is "Long Running"
   And I Cancel the Test
   Then "Cancel Test" is "Disabled"
   And Validation message is thrown
   And Validation message is "Test Cancelled"

#wolf-1034
Scenario: Web Source returns text
   Given I open New Web Source
   Then "New Web Service Source" tab is opened
   And title is "New Web Service Source"
   And I type Address as "http://warewolf.io/version.txt"
   And "Test Connection" is "Enabled"
   When Test Connecton is "Successful"
   Then "Save" is "Enabled"	
   When I save as "Testing Return Text"
   Then the save dialog is opened
   Then title is "Testing Return Text"
   And "Testing Return Text" tab is opened