Feature: Web Source
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


@WebSource
Scenario: Creating New Web Source 
   Given I open New Address
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And I Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Enabled"
   And TestDefault as "String" is "Visible"
   When I save the source
   Then the save dialog is opened
	
	
Scenario: Creating New Web Source  under auth type as user
   Given I open New Web Source
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "IntegrationTester"
   And Password field is "I73573r0"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Enabled"
   And TestDefault as "String" is "Visible"
   When I save the source
   Then the save dialog is opened
	

Scenario: Incorrect address wind auth type allowing to save
   Given I open New Web Source
   And I type Address as "sdfsdfd"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "IntegrationTester"
   And Password field is "I73573r0"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   Then "View Results in Browser" is "Not Visible"
   And "Save" is "Disabled"
   And TestDefault as "String" is "Visible"
  

  
Scenario: Incorrect  user auth type is not allowing to save
   Given I open New Web Source
   And I type Address as "sdfsdfd"
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "test"
   And Password field is "I73573r0"
   When Test Connecton is "UnSuccessful"
   And Validation message is thrown
   Then "View Results in Browser" is "Not Visible"
   And "Save" is "Disabled"
   And TestDefault as "String" is "Visible"


Scenario: Testing Auth type as Windows and swaping it resets the test connection 
   Given I open New Web Source
   And "Save" is "Disabled"
   And I type Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite" 
   And I type Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And I Select Authentication Type as "User"
   And Username field is "test"
   And Password field is "I73573r0"
   When Test Connecton is "Successful"
   And Validation message is Not thrown
   And "Save" is "Enabled"
   And I Select Authentication Type as "Anonymous"
   And Username field is "Invisible"
   And Password field is "Invisible"
   When Test Connecton is ""
   And Validation message is Not thrown
   And "Save" is "Disabled"
   When Test Connecton is "Successful"
   And "Save" is "Enabled"	 
   And I Select Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   When Test Connecton is ""
   And "Save" is "Disabled" 
	 	 

Scenario: Editing saved Web Source 
   Given I open "Edit Source - Test" 
   And Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   And TestDefault as "String" is "Visible"  
   When I Edit Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query as "/GetCountries.ashx?extensio
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is ""
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   When Test Connecton is "Successfull"
   Then "Save" is "Enabled" 
 


Scenario: Editing saved Web Source auth type
   Given I open "Edit Source - Test" 
   And Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   And TestDefault as "String" is "Visible"  
   When I edit Authentication Type as "User"
   And Username field is "Visible"
   And Password field is "Visible"
   When Test Connecton is ""
   And Username field is "IntegrationTester"
   And Password field is "I73573r0"
   And "Save" is "Disabled"
   When Test Connecton is "Successfull"
   Then "Save" is "Enabled" 


    
Scenario: Editing saved Web Source and canceling without saving
   Given I open "Edit Source - Test" 
   And Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query as "/GetCountries.ashx?extension=json&prefix=a"
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is "Successful"
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   And TestDefault as "String" is "Visible"  
   When I Edit Address as "http://RSAKLFSVRTFSBLD/IntegrationTestSite"
   And  Default Query as "/GetCountries.ashx?extensio
   And "Save" is "Disabled"
   And "Test Connection" is "Enabled"
   Then "View Results in Browser" is "Invisible"
   And TestDefault as "String" is "Invisible"
   And Select Authentication Type as "Anonymous"
   And Username field is "InVisible"
   And Password field is "InVisible"
   When Test Connecton is ""
   Then "View Results in Browser" is "Visible"
   And "Save" is "Disabled"
   When Test Connecton is "Successfull"
   Then "Save" is "Enabled" 
   When I cancel 
   Then Web Source ""Edit Source - Test" is closed 