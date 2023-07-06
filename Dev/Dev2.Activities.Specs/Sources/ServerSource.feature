﻿@ServerSourceTests
Feature: ServerSource
	In order to create server source
	As a Warewolf user
	I want to be able to use three authentication types

@COMIPCSaxonCSandStudioTests
Scenario: Create Windows Server Source
	Given I create a server source as
	| Address               | AuthenticationType |
	| http://localhost:3142 | Windows            |
	And I save as "WinServerSource"
	When I Test "WinServerSource"
	Then The result is "success"
	And I delete serversource 

@COMIPCSaxonCSandStudioTests
Scenario: Create User Server Source
	Given I create a server source as
	| Address                                 | AuthenticationType |
	| http://tst-ci-remote.premier.local:3142 | Public             |
	When I Test the connection
	Then The result is "success"

Scenario: Create Bad User Server Source
	Given I create a server source as
	| Address               | AuthenticationType |
	| http://localhost:3142 | User               |
	And User as "BadUser" and with "Dev2@dmin123" as password
	When I Test the connection
	Then The result is "Connection Error :One or more errors occurred. (Response status code does not indicate success: 401 (Unauthorized).)"

@COMIPCSaxonCSandStudioTests
Scenario: Create Public Server Source
	Given I create a server source as
	| Address                             | AuthenticationType |
	| http://wolfs-den.premier.local:3142 | Public             |
	When I Test the connection
	Then The result is "success"
