@OracleSource
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

@OracleSource
Scenario: Creating New DB Source as User Auth   


@OracleSource
Scenario: Creating New DB Source as Windows Auth

@OracleSource
Scenario: Editing saved DB Source Remembers credentials
	