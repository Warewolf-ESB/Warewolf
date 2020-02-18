@ServerSourceTests
Feature: ServerSource
	In order to create server source
	As a Warewolf user
	I want to be able to use three authentication types

Scenario: Create Windows Server Source
	Given I create a server source as
	| Address               | AuthenticationType |
	| http://localhost:3142 | Windows            |
	And I save as "WinServerSource"
	When I Test "WinServerSource"
	Then The result is "success"
	And I delete serversource 

Scenario: Create User Server Source
	Given I create a server source as
	| Address                                 | AuthenticationType |
	| http://tst-ci-remote.premier.local:3142 | User               |
	And User as "WarewolfAdmin" and with "W@rEw0lf@dm1n" as password
	When I Test the connection
	Then The result is "success"

Scenario: Create Bad User Server Source
	Given I create a server source as
	| Address               | AuthenticationType |
	| http://localhost:3142 | User               |
	And User as "BadUser" and with "Dev2@dmin123" as password
	When I Test the connection
	Then The result is "Connection Error :Unauthorized"

@Ignore
#TODO: re-introduce this test when there is enough RAM in the build rig to host the container it depends on
Scenario: Create Public Server Source
	Given I create a server source as
	| Address                             | AuthenticationType |
	| http://wolfs-den.premier.local:3142 | Public             |
	When I Test the connection
	Then The result is "success"
