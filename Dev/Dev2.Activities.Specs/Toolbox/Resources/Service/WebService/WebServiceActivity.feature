@ignore
Feature: WebServiceActivity
	In order to use data from a webservice
	As a Warewolf user
	I want to be able to execute a webservice and use the data

Scenario: Execute a webservice
	Given I have the Get Cities Webservice
	When I execute the activity
	Then the result should be "Success"
