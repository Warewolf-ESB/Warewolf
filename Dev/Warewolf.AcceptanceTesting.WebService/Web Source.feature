@WebSource
Feature: Web Source
	In order to create a web source for web services
	As a Warewolf user
	I want to be able to manage web sources easily

Scenario: Creating New Web Source 
   Given I open New Web Source 
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And "TestDefault" is "Invisible"
   And I Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Enabled"
   And "TestDefault" is "Visible"
   When I save the source
   Then the save dialog is opened
	
	
Scenario: Creating New Web Source under auth type as user
   Given I open New Web Source
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And "TestDefault" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "IntegrationTester"
   And Password field is "I73573r0"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Enabled"
   And "TestDefault" is "Visible"
   When I save the source
   Then the save dialog is opened
	

Scenario: Incorrect address windows auth type not allowing save
   Given I open New Web Source
   And I type Address as "sdfsdfd"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And "TestDefault" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "IntegrationTester"
   And Password field is "I73573r0"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   Then "View Results in Browser" is "Not Visible"
   And "Save" is "Disabled"
   And "TestDefault" is "Visible"
     
Scenario: Incorrect address user auth type is not allowing to save
   Given I open New Web Source
   And I type Address as "sdfsdfd"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And "TestDefault" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "test"
   And Password field is "I73573r0"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   Then "View Results in Browser" is "Not Visible"
   And "Save" is "Disabled"
   And "TestDefault" is "Visible"


Scenario: Testing Auth type as Windows and swaping it resets the test connection 
   Given I open New Web Source
   And "Save" is "Disabled"
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite" 
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And "TestDefault" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "test"
   And Password field is "I73573r0"
   When Test Connecton is "Successful"
   And Validation message is Not thrown
   And "Save" is "Enabled"
   And I Select Authentication Type as "Anonymous"
   And Username field is "Invisible"
   And Password field is "Invisible"
   And "Save" is "Disabled"
   When Test Connecton is "Successful"
   And Validation message is Not thrown
   And "Save" is "Enabled"	 
   And I Select Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   And "Save" is "Disabled" 
	 	 

Scenario: Editing saved Web Source 
   Given I open "Edit Source - Test" web source
   And Address is "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query is "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And "TestDefault" is "Invisible"
   And Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   And "TestDefault" is "Visible"
   When I change Address to "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  I change Default Query to "/GetCountries.ashx?extensio
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And "TestDefault" is "Visible"  
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   When Test Connecton is "Successfull"
   Then "Save" is "Enabled" 
 


Scenario: Editing saved Web Source auth type
   Given I open "Edit Source - Test" web source
   And Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
  And "TestDefault" is "Invisible"
   And Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   And "TestDefault" is "Visible"  
   When I edit Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   When Test Connecton is ""
   And Username field is "IntegrationTester"
   And Password field is "I73573r0"
   And "Save" is "Disabled"
   When Test Connecton is "Successfull"
   Then "Save" is "Enabled" 