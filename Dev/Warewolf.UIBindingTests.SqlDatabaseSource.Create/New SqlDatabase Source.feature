
@DbSource
Feature: New SqlDatabase Source
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

@SQLDbSource
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Creating New SQL Server DB Source as Windows Auth
    Given I open New Database Source
    And I type Server as "RSAKLFSVRDEV"
    And I Select Authentication Type as "Windows"
    Then Username field is "Collapsed"
    And Password field is "Collapsed"
    And "Save" is "Disabled"
    Then "Test Connection" is "Enabled" 
    And "Save" is "Disabled"
    Then Database dropdown is "Collapsed"
    And "Test Connection" is "Enabled"
    Then Test Connecton is "Successful"
    And "Save" is "Disabled"
    And Database dropdown is "Visible"
    When I select "Dev2TestingDB" as Database
    Then "Save" is "Enabled" 
    
