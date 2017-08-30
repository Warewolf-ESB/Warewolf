@ServerSourceTests
Feature: ServerSource
	In order to create server source
	As a Warewolf user
	I want to be able to use three authentication types

Scenario: Create Windows Server Source
	Given I create a server source as
	| Address                   | AuthenticationType |
	| http://tst-ci-remote:3142 | Windows            |
	And I save as "WinServerSource"
	When I Test "WinServerSource"
	Then The result is "success"
	And I delete serversource 

Scenario: Create User Server Source
	Given I create a server source as
	| Address                   | AuthenticationType |
	| http://tst-ci-remote:3142 | User               |
	And User details as 
	| username               | Password |
	| dev2\integrationtester | I73573r0 |
	And I save as "WinServerSource1"
	When I Test "WinServerSource1"
	Then The result is "success"
	And I delete serversource 

Scenario: Create BadUser Server Source
	Given I create a server source as
	| Address               | AuthenticationType |
	| http://localhost:3142 | User               |
	And User details as 
	| username | Password |
	| BadUser  | I73573r0 |
	And I save as "WinServerSource2"
	Then The result is "Connection Error :Unauthorized"	

Scenario: Create Public Server Source
	Given I create a server source as
	| Address                   | AuthenticationType |
	| http://tst-ci-remote:3142 | Public             |
	And I save as "WinServerSource2"
	When I Test "WinServerSource2"
	Then The result is "success"
	And I delete serversource 


