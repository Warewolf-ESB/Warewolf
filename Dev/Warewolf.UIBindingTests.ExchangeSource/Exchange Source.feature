@ExchangeSource
Feature: Exchange Source
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@ExchangeSource
Scenario: Creating new exchange source
	Given I open a new exchange source
	Then "New Exchange Source" tab is Opened
	And Title is "New Exchange Source"
	When I Type Auto Discover as "https://outlook.office365.com/EWS/Exchange.asmx"
	When I Type User Name as "TestUser"
	When I Type Password as "TestUser"
	When I Type TimeOut as "1000"
	When I Type To Email as "test@gmsil.com"

Scenario: Testing new exchange source
Given I open a new exchange source
	Then "New Exchange Source" tab is Opened
	And Title is "New Exchange Source"
	When I Type Auto Discover as "https://outlook.office365.com/EWS/Exchange.asmx"
	When I Type User Name as "TestUser"
	When I Type Password as "TestUser"
	When I Type TimeOut as "1000"
	When I Type To Email as "test@gmsil.com"
	Then I click on the Test Button

@ExchangeSource
Scenario: Fail Send Shows correct error message
	Given I open a new exchange source
	Then "New Exchange Source" tab is Opened
	And Title is "New Exchange Source"
	When I Type Auto Discover as "https://outlook.office365.com/EWS/Exchange.asmx"
	When I Type User Name as "TestUser"
	When I Type Password as "TestUser"
	When I Type TimeOut as "1000"
	When I Type To Email as "test@gmsil.com"
	And Send is "Unsuccessful"
	Then Send is "The request failed. The remote server returned an error: (401) Unauthorized."
	And "Save" is "Disabled"