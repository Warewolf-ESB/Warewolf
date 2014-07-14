Feature: Settings Permissions
	In order to set permissions for my server
	As a Warewolf user
	I want to setup a Server Permissions
	
Scenario Outline: Server Permissions 
        Given I have a server "localhost"
        And it has '<Group>' with '<Given rights>' 
        When connected as user part of '<User Group>'
        Then '<Resources>' resources are visible
        And resources should have '<Rights>'
Examples:
        | No | Group  | Given rights                                      | User Group | Resources | Rights                                            |
        | 1  | Public | Deploy To                                         | Users      | All       | Deploy To                                         |
        | 2  | Public | Deploy From                                       | Users      | All       | Deploy From                                       |
        | 3  | Public | View                                              | Users      | All       | View                                              |
        | 4  | Public | Execute                                           | Users      | All       | Execute                                           |
        | 5  | Public | Contribute, View, Execute                         | Users      | All       | Contribute, View, Execute                         |
        | 6  | Public | Deploy To, Deploy From                            | Users      | All       | Deploy To, Deploy From                            |
        | 7  | Public | View, Execute                                     | Users      | All       | View, Execute                                     |
        | 8  | Public | View, Execute, Contribute, Deploy To, Deploy From | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From |
        | 9  | Public | Deploy To, View                                   | Users      | All       | Deploy To, View                                   |
        | 10 | Public | Deploy To, Execute                                | Users      | All       | Deploy To, Execute                                |
        | 11 | Public | Deploy To, Contribute, View, Execute              | Users      | All       | Deploy To, Contribute, View, Execute              |
        | 12 | Public | Deploy From, View                                 | Users      | All       | Deploy From, View                                 |
        | 13 | Public | Deploy From, Execute                              | Users      | All       | Deploy From, Execute                              |
        | 14 | Public | Deploy From, Contribute, View, Execute            | Users      | All       | Deploy From, Contribute, View, Execute            |
        | 15 | Public | Deploy To, Deploy From, View                      | Users      | All       | Deploy To, Deploy From, View                      |
        | 16 | Public | Deploy To, Deploy From, Execute                   | Users      | All       | Deploy To, Deploy From, Execute                   |

Scenario Outline: Setting Selected Resource Permissions for users 
        Given I have a server "localhost"
        And it has '<Group>' with '<Given rights>' 
        And Resource '<Resource Name>' has rights '<Resource Rights>' for '<User Group>'
        When connected as user part of '<User Group>'
        Then '<Resources>' resources are visible
        And '<Resource>' should have '<Permissions>'
		And resources should have '<Rights>' 
Examples: 
        | No | Group     | Given rights              | Resource Name           | Resource Rights          | User Group | Resources | Rights                    | Resource                | Permissions               |
        | 1  | TestGroup | View                      | BARNEY\DECISION TESTING | View                     | Users      | All       | View                      | BARNEY\DECISION TESTING | View                      |
        | 2  | TestGroup | Execute                   | BARNEY\DECISION TESTING | View                     | Users      | All       | Execute                   | BARNEY\DECISION TESTING | View, Execute             |
        | 3  | TestGroup | Contribute, View, Execute | BARNEY\DECISION TESTING | View                     | Users      | All       | Contribute, View, Execute | BARNEY\DECISION TESTING | View, Contribute, Execute |
        | 4  | TestGroup | View, Execute             | BARNEY\DECISION TESTING | View                     | Users      | All       | View, Execute             | BARNEY\DECISION TESTING | View, Execute             |
        | 5  | TestGroup | View                      | BARNEY\DECISION TESTING | Execute                  | Users      | All       | View                      | BARNEY\DECISION TESTING | View, Execute             |
        | 6  | TestGroup | Execute                   | BARNEY\DECISION TESTING | Execute                  | Users      | All       | Execute                   | BARNEY\DECISION TESTING | Execute                   |
        | 7  | TestGroup | Contribute, View, Execute | BARNEY\DECISION TESTING | Execute                  | Users      | All       | Contribute, View, Execute | BARNEY\DECISION TESTING | View, Contribute, Execute |
        | 7  | TestGroup | View, Execute             | BARNEY\DECISION TESTING | Execute                  | Users      | All       | View, Execute             | BARNEY\DECISION TESTING | Execute, View             |
        | 8  | TestGroup | View                      | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | View                      | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 9  | TestGroup | Execute                   | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | Execute                   | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 10 | TestGroup | Contribute, View, Execute | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | Contribute, View, Execute | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 11 | TestGroup | View, Execute             | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | View, Execute             | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 12 | TestGroup | View                      | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | View                      | BARNEY\DECISION TESTING | View, Execute             |
        | 13 | TestGroup | Execute                   | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | Execute                   | BARNEY\DECISION TESTING | View, Execute             |
        | 14 | TestGroup | Contribute, View, Execute | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | Contribute, View, Execute | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 15 | TestGroup | View, Execute             | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | View, Execute             | BARNEY\DECISION TESTING | View, Execute             |
        | 16 | TestGroup | View                      | BARNEY\DECISION TESTING | None                     | Users      | All       | View                      | BARNEY\DECISION TESTING | Unauthorised              |
        | 17 | TestGroup | Execute                   | BARNEY\DECISION TESTING | None                     | Users      | All       | Execute                   | BARNEY\DECISION TESTING | Unauthorised              |
        | 18 | TestGroup | Contribute, View, Execute | BARNEY\DECISION TESTING | None                     | Users      | All       | Contribute, View, Execute | BARNEY\DECISION TESTING | Unauthorised              |
        | 19 | TestGroup | View, Execute             | BARNEY\DECISION TESTING | None                     | Users      | All       | View, Execute             | BARNEY\DECISION TESTING | Unauthorised              |

Scenario Outline: Setting Selected Resource Permissions for users overlapping groups
        Given I have a server "localhost"
        And it has '<Group>' with '<Given rights>' 
        And Resource '<Resource Name>' has rights '<Resource Rights>' for '<User Group>'
        When connected as user part of '<User Group>'
        Then '<Resources>' resources are visible
        And '<Resource>' should have '<Permissions>'
		And resources should have '<Rights>' 
Examples: 
        | No | Group  | Given rights                                              | Resource Name           | Resource Rights          | User Group | Resources | Rights                                            | Resource                | Permissions               |
        | 1  | Public | View                                                      | BARNEY\DECISION TESTING | View                     | Users      | All       | View                                              | BARNEY\DECISION TESTING | View                      |
        | 2  | Public | Execute                                                   | BARNEY\DECISION TESTING | View                     | Users      | All       | Execute                                           | BARNEY\DECISION TESTING | View                      |
        | 3  | Public | Contribute, View, Execute                                 | BARNEY\DECISION TESTING | View                     | Users      | All       | Contribute, View, Execute                         | BARNEY\DECISION TESTING | View                      |
        | 4  | Public | View, Execute                                             | BARNEY\DECISION TESTING | View                     | Users      | All       | View, Execute                                     | BARNEY\DECISION TESTING | View                      |
        | 5  | Public | View, Execute, Contribute, Deploy To, Deploy From , Admin | BARNEY\DECISION TESTING | View                     | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From | BARNEY\DECISION TESTING | View                      |
        | 6  | Public | View                                                      | BARNEY\DECISION TESTING | Execute                  | Users      | All       | View                                              | BARNEY\DECISION TESTING | Execute                   |
        | 7  | Public | Execute                                                   | BARNEY\DECISION TESTING | Execute                  | Users      | All       | Execute                                           | BARNEY\DECISION TESTING | Execute                   |
        | 8  | Public | Contribute, View, Execute                                 | BARNEY\DECISION TESTING | Execute                  | Users      | All       | Contribute, View, Execute                         | BARNEY\DECISION TESTING | Execute                   |
        | 9  | Public | View, Execute                                             | BARNEY\DECISION TESTING | Execute                  | Users      | All       | View, Execute                                     | BARNEY\DECISION TESTING | Execute                   |
        | 10 | Public | View, Execute, Contribute, Deploy To, Deploy From, Admin  | BARNEY\DECISION TESTING | Execute                  | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From | BARNEY\DECISION TESTING | Execute                   |
        | 11 | Public | View                                                      | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | View                                              | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 12 | Public | Execute                                                   | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | Execute                                           | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 13 | Public | Contribute, View, Execute                                 | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | Contribute, View, Execute                         | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 14 | Public | View, Execute                                             | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | View, Execute                                     | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 15 | Public | View, Execute, Contribute, Deploy To, Deploy From         | BARNEY\DECISION TESTING | Cotribute, View, Execute | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From | BARNEY\DECISION TESTING | Contribute, View, Execute |
        | 16 | Public | View                                                      | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | View                                              | BARNEY\DECISION TESTING | View, Execute             |
        | 17 | Public | Execute                                                   | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | Execute                                           | BARNEY\DECISION TESTING | View, Execute             |
        | 18 | Public | Contribute, View, Execute                                 | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | Contribute, View, Execute                         | BARNEY\DECISION TESTING | View, Execute             |
        | 19 | Public | View, Execute                                             | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | View, Execute                                     | BARNEY\DECISION TESTING | View, Execute             |
        | 20 | Public | View, Execute, Contribute, Deploy To, Deploy From         | BARNEY\DECISION TESTING | View, Execute            | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From | BARNEY\DECISION TESTING | View, Execute             |
        | 21 | Public | View                                                      | BARNEY\DECISION TESTING | None                     | Users      | All       | View                                              | BARNEY\DECISION TESTING | Unauthorised              |
        | 22 | Public | Execute                                                   | BARNEY\DECISION TESTING | None                     | Users      | All       | Execute                                           | BARNEY\DECISION TESTING | Unauthorised              |
        | 23 | Public | Contribute, View, Execute                                 | BARNEY\DECISION TESTING | None                     | Users      | All       | Contribute, View, Execute                         | BARNEY\DECISION TESTING | Unauthorised              |
        | 24 | Public | View, Execute                                             | BARNEY\DECISION TESTING | None                     | Users      | All       | View, Execute                                     | BARNEY\DECISION TESTING | Unauthorised              |
        | 25 | Public | View, Execute, Contribute, Deploy To, Deploy From, Admin  | BARNEY\DECISION TESTING | None                     | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From | BARNEY\DECISION TESTING | Unauthorised              |