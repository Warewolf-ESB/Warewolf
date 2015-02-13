Feature: DBService Inspector
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Action Inspector
Scenario: Opening Action Inspector and closing
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   And "" is focused
   And "1 Data Source" is "Enabled"
   And "2 Select Action" is "Enabled"
   And "3 Test Connector and Calculate Outputs" is "Enabled" 
   And "inspect Data Connector" hyper link is "Visible"
   When I click on "inspect Data Connector"
   Then Action Inspector is opened
   And "" is focused
   And it contains
   |CREATE PROCEDURE dbo.FetchHtmlFragment |
   And "close" button is visible
   And "Action Inspector" popup is "movable"
   When I close Action inspector
   Then Action Inspector is closed



   