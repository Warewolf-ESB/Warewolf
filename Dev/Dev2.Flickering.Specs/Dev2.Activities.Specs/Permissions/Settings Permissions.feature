@Security
Feature: Settings Permissions
	In order to set permissions for my server
	As a Warewolf user
	I want to setup a Server Permissions

@Security
Scenario Outline: Setting Selected Resource Permissions for users 
        Given I have a server "localhost"
        And it has '<Group>' with '<Given rights>' 
        And Resource '<Resource Name>' has rights '<Resource Rights>' for '<User Group>'
        When connected as user part of '<User Group>'
        Then '<Resource>' should have '<Permissions>'
		And resources should not have '<Rights>' 
Examples: 
        | No | Group                   | Given rights              | Resource Name                                 | Resource Rights           | User Group | Resources | Rights | Resource                                      | Permissions               |
        | 7  | Warewolf Administrators | Contribute, View, Execute | Acceptance Testing Resources\DECISION TESTING | Execute                   | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Execute                   |

#@Security
Scenario Outline: Setting Selected Resource Permissions for users overlapping groups
        Given I have a server "localhost"
        And it has '<Group>' with '<Given rights>' 
        And Resource '<Resource Name>' has rights '<Resource Rights>' for '<User Group>'
        When connected as user part of '<User Group>'
        Then '<Resource>' should have '<Permissions>'
		And resources should have '<Rights>' 
Examples: 
        | No | Group  | Given rights                                                     | Resource Name                                 | Resource Rights           | User Group | Resources | Rights                                                           | Resource                                      | Permissions               |
        | 18 | Public | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | View, Execute             | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | View, Execute             |