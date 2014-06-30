Feature: Settings
	In order to set permissions for my server
	As a Warewolf user
	I want to setup a Server Permissions

#Scenario Outline: Server Permissions 
#        Given I have a server “localhost”
#        And it has ‘<Group>’ with rights '<Given rights>' 
#        When connected as user part of ‘<UserGroup>’
#        Then '<Resources>' resources are visible
#        And resources should have '<Rights>' rights
#Examples:
#        | No | Group  | Given rights                                            | User Group | Resources | rights                                            |
#        | 1  | Public | Deploy To                                               | User       | All       | Deploy To                                         |
#        | 2  | Public | Deploy From                                             | User       | All       | Deploy From                                       |
#        | 3  | Public | View                                                    | User       | All       | View                                              |
#        | 4  | Public | Execute                                                 | User       | All       | Execute                                           |
#        | 5  | Public | Contribute, View, Execute                               | User       | All       | Contribute, View, Execute                         |
#        | 6  | Public | Deploy To, Deploy From                                  | User       | All       | Deploy To, Deploy From                            |
#        | 7  | Public | View, Execute                                           | User       | All       | View, Execute                                     |
#        | 8  | Public | View, Execute, Contribute, Deploy To, Deploy From,Admin | User       | All       | View, Execute, Contribute, Deploy To, Deploy From |
#        | 9  | Public | Deploy To, View                                         | User       | All       | Deploy To, View                                   |
#        | 10 | Public | Deploy To, Execute                                      | User       | All       | Deploy To, Execute                                |
#        | 11 | Public | Deploy To, Contribute, View, Execute                    | User       | All       | Deploy To, Contribute, View, Execute              |
#        | 12 | Public | Deploy From, View                                       | User       | All       | Deploy From, View                                 |
#        | 13 | Public | Deploy From, Execute                                    | User       | All       | Deploy From, Execute                              |
#        | 14 | Public | Deploy From, Contribute, View, Execute                  | User       | All       | Deploy From, Contribute, View, Execute            |
#        | 15 | Public | Deploy To, Deploy From, View                            | User       | All       | Deploy To, Deploy From, View                      |
#        | 16 | Public | Deploy To, Deploy From, Execute                         | User       | All       | Deploy To, Deploy From, Execute                   |
#        | 17 | Public |                                                         | User       |           |                                                   |
#
#
#Scenario Outline: Setting Selected Resource Permissions for users 
#        Given I have a server “localhost”
#        And it has ‘<Group>’ with rights '<Given rights>' 
#        And Resource ‘<Resource Name>’ has rights ‘<Resource Rights>’
#        When connected as user part of ‘<UserGroup>’
#        Then '<Resources>' resources are visible
#        And resources should have '<Rights>' rights 
#        And selected resource ‘<Resource>’ should have ‘<Permissions>’
#Examples: 
#        | No | Group  | Given rights                                              | Resource Name                     | Resource Rights          | UserGroup | Resources | Rights                                             | Resource         | Permissions               |
#        | 1  | Public | Deploy To                                                 | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To                                          | DECISION TESTING | View                      |
#        | 2  | Public | Deploy From                                               | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy From                                        | DECISION TESTING | View                      |
#        | 3  | Public | View                                                      | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | View                                               | DECISION TESTING | View                      |
#        | 4  | Public | Execute                                                   | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Execute                                            | DECISION TESTING | View                      |
#        | 5  | Public | Contribute, View, Execute                                 | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Contribute, View, Execute                          | DECISION TESTING | View                      |
#        | 6  | Public | Deploy To, Deploy From                                    | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To, Deploy From                             | DECISION TESTING | View                      |
#        | 7  | Public | View, Execute                                             | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | View, Execute                                      | DECISION TESTING | View                      |
#        | 8  | Public | View, Execute, Contribute, Deploy To, Deploy From , Admin | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | View, Execute, Contribute, Deploy To, Deploy From  | DECISION TESTING | View                      |
#        | 9  | Public | Deploy To, View                                           | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To , View                                   | DECISION TESTING | View                      |
#        | 10 | Public | Deploy To, Execute                                        | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To , Execute                                | DECISION TESTING | View                      |
#        | 11 | Public | Deploy To, Contribute, View, Execute                      | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To , Contribute, View, Execute              | DECISION TESTING | View                      |
#        | 12 | Public | Deploy From, View                                         | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy From , View                                 | DECISION TESTING | View                      |
#        | 13 | Public | Deploy From, Execute                                      | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy From , Execute                              | DECISION TESTING | View                      |
#        | 14 | Public | Deploy From, Contribute, View, Execute                    | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy From , Contribute, View, Execute            | DECISION TESTING | View                      |
#        | 15 | Public | Deploy To, Deploy From, View                              | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To, Deploy From , View                      | DECISION TESTING | View                      |
#        | 16 | Public | Deploy To, Deploy From, Execute                           | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To, Deploy From , Execute                   | DECISION TESTING | View                      |
#        | 17 | Public | Deploy To, Deploy From, Contribute, View, Execute         | WORKFLOWS\BARNEY\DECISION TESTING | View                     | User      | All       | Deploy To, Deploy From , Contribute, View, Execute | DECISION TESTING | View                      |
#        | 18 | Public | Deploy To                                                 | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To                                          | DECISION TESTING | Execute                   |
#        | 19 | Public | Deploy From                                               | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy From                                        | DECISION TESTING | Execute                   |
#        | 20 | Public | View                                                      | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | View                                               | DECISION TESTING | Execute                   |
#        | 21 | Public | Execute                                                   | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Execute                                            | DECISION TESTING | Execute                   |
#        | 22 | Public | Contribute, View, Execute                                 | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Contribute, View, Execute                          | DECISION TESTING | Execute                   |
#        | 23 | Public | Deploy To, Deploy From                                    | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To, Deploy From                             | DECISION TESTING | Execute                   |
#        | 24 | Public | View, Execute                                             | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | View, Execute                                      | DECISION TESTING | Execute                   |
#        | 25 | Public | View, Execute, Contribute, Deploy To, Deploy From, Admin  | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | View, Execute, Contribute, Deploy To, Deploy From  | DECISION TESTING | Execute                   |
#        | 26 | Public | Deploy To, View                                           | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To, View                                    | DECISION TESTING | Execute                   |
#        | 27 | Public | Deploy To, Execute                                        | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To, Execute                                 | DECISION TESTING | Execute                   |
#        | 28 | Public | Deploy To, Contribute, View, Execute                      | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To, Contribute, View, Execute               | DECISION TESTING | Execute                   |
#        | 29 | Public | Deploy From, View                                         | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy From, View                                  | DECISION TESTING | Execute                   |
#        | 30 | Public | Deploy From, Execute                                      | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy From, Execute                               | DECISION TESTING | Execute                   |
#        | 31 | Public | Deploy From, Contribute, View, Execute                    | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy From, Contribute, View, Execute             | DECISION TESTING | Execute                   |
#        | 32 | Public | Deploy To, Deploy From, View                              | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To, Deploy From, View                       | DECISION TESTING | Execute                   |
#        | 33 | Public | Deploy To, Deploy From, Execute                           | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To, Deploy From, Execute                    | DECISION TESTING | Execute                   |
#        | 34 | Public | Deploy To, Deploy From, Contribute, View, Execute         | WORKFLOWS\BARNEY\DECISION TESTING | Execute                  | User      | All       | Deploy To, Deploy From, Contribute, View, Execute  | DECISION TESTING | Execute                   |
#        | 35 | Public | Deploy To                                                 | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To                                          | DECISION TESTING | Contribute, View, Execute |
#        | 36 | Public | Deploy From                                               | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy From                                        | DECISION TESTING | Contribute, View, Execute |
#        | 37 | Public | View                                                      | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | View                                               | DECISION TESTING | Contribute, View, Execute |
#        | 38 | Public | Execute                                                   | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Execute                                            | DECISION TESTING | Contribute, View, Execute |
#        | 39 | Public | Contribute, View, Execute                                 | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Contribute, View, Execute                          | DECISION TESTING | Contribute, View, Execute |
#        | 40 | Public | Deploy To, Deploy From                                    | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To, Deploy From                             | DECISION TESTING | Contribute, View, Execute |
#        | 41 | Public | View, Execute                                             | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | View, Execute                                      | DECISION TESTING | Contribute, View, Execute |
#        | 42 | Public | View, Execute, Contribute, Deploy To, Deploy From         | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | View, Execute, Contribute, Deploy To, Deploy From  | DECISION TESTING | Contribute, View, Execute |
#        | 43 | Public | Deploy To, View                                           | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To, View                                    | DECISION TESTING | Contribute, View, Execute |
#        | 44 | Public | Deploy To, Execute                                        | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To, Execute                                 | DECISION TESTING | Contribute, View, Execute |
#        | 45 | Public | Deploy To, Contribute, View, Execute                      | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To, Contribute, View, Execute               | DECISION TESTING | Contribute, View, Execute |
#        | 46 | Public | Deploy From, View                                         | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy From, View                                  | DECISION TESTING | Contribute, View, Execute |
#        | 47 | Public | Deploy From, Execute                                      | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy From, Execute                               | DECISION TESTING | Contribute, View, Execute |
#        | 48 | Public | Deploy From, Contribute, View, Execute                    | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy From, Contribute, View, Execute             | DECISION TESTING | Contribute, View, Execute |
#        | 49 | Public | Deploy To, Deploy From, View                              | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To, Deploy From, View                       | DECISION TESTING | Contribute, View, Execute |
#        | 50 | Public | Deploy To, Deploy From, Execute                           | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To, Deploy From, Execute                    | DECISION TESTING | Contribute, View, Execute |
#        | 51 | Public | Deploy To, Deploy From, Contribute, View, Execute, Admin  | WORKFLOWS\BARNEY\DECISION TESTING | Cotribute, View, Execute | User      | All       | Deploy To, Deploy From, Contribute, View, Execute  | DECISION TESTING | Contribute, View, Execute |
#        | 52 | Public | Deploy To                                                 | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To                                          | DECISION TESTING | View, Execute             |
#        | 53 | Public | Deploy From                                               | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy From                                        | DECISION TESTING | View, Execute             |
#        | 54 | Public | View                                                      | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | View                                               | DECISION TESTING | View, Execute             |
#        | 55 | Public | Execute                                                   | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Execute                                            | DECISION TESTING | View, Execute             |
#        | 56 | Public | Contribute, View, Execute                                 | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Contribute, View, Execute                          | DECISION TESTING | View, Execute             |
#        | 57 | Public | Deploy To, Deploy From                                    | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To, Deploy From                             | DECISION TESTING | View, Execute             |
#        | 58 | Public | View, Execute                                             | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | View, Execute                                      | DECISION TESTING | View, Execute             |
#        | 59 | Public | View, Execute, Contribute, Deploy To, Deploy From         | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | View, Execute, Contribute, Deploy To, Deploy From  | DECISION TESTING | View, Execute             |
#        | 60 | Public | Deploy To, View                                           | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To, View                                    | DECISION TESTING | View, Execute             |
#        | 61 | Public | Deploy To, Execute                                        | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To, Execute                                 | DECISION TESTING | View, Execute             |
#        | 62 | Public | Deploy To, Contribute, View, Execute                      | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To, Contribute, View, Execute               | DECISION TESTING | View, Execute             |
#        | 63 | Public | Deploy From, View                                         | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy From, View                                  | DECISION TESTING | View, Execute             |
#        | 64 | Public | Deploy From, Execute                                      | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy From, Execute                               | DECISION TESTING | View, Execute             |
#        | 65 | Public | Deploy From, Contribute, View, Execute                    | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy From, Contribute, View, Execute             | DECISION TESTING | View, Execute             |
#        | 66 | Public | Deploy To, Deploy From, View                              | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To, Deploy From, View                       | DECISION TESTING | View, Execute             |
#        | 67 | Public | Deploy To, Deploy From, Execute                           | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To, Deploy From, Execute                    | DECISION TESTING | View, Execute             |
#        | 68 | Public | Deploy To, Deploy From, Contribute, View, Execute, Admin  | WORKFLOWS\BARNEY\DECISION TESTING | View, Execute            | User      | All       | Deploy To, Deploy From, Contribute, View, Execute  | DECISION TESTING | View, Execute             |
#        | 69 | Public | Deploy To                                                 | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To                                          | DECISION TESTING | Unauthorised              |
#        | 70 | Public | Deploy From                                               | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy From                                        | DECISION TESTING | Unauthorised              |
#        | 71 | Public | View                                                      | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | View                                               | DECISION TESTING | Unauthorised              |
#        | 72 | Public | Execute                                                   | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Execute                                            | DECISION TESTING | Unauthorised              |
#        | 73 | Public | Contribute, View, Execute                                 | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Contribute, View, Execute                          | DECISION TESTING | Unauthorised              |
#        | 74 | Public | Deploy To, Deploy From                                    | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To, Deploy From                             | DECISION TESTING | Unauthorised              |
#        | 75 | Public | View, Execute                                             | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | View, Execute                                      | DECISION TESTING | Unauthorised              |
#        | 76 | Public | View, Execute, Contribute, Deploy To, Deploy From, Admin  | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | View, Execute, Contribute, Deploy To, Deploy From  | DECISION TESTING | Unauthorised              |
#        | 77 | Public | Deploy To, View                                           | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To, View                                    | DECISION TESTING | Unauthorised              |
#        | 78 | Public | Deploy To, Execute                                        | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To, Execute                                 | DECISION TESTING | Unauthorised              |
#        | 79 | Public | Deploy To, Contribute, View, Execute                      | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To, Contribute, View, Execute               | DECISION TESTING | Unauthorised              |
#        | 80 | Public | Deploy From, View                                         | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy From, View                                  | DECISION TESTING | Unauthorised              |
#        | 81 | Public | Deploy From, Execute                                      | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy From, Execute                               | DECISION TESTING | Unauthorised              |
#        | 82 | Public | Deploy From, Contribute, View, Execute                    | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy From, Contribute, View, Execute             | DECISION TESTING | Unauthorised              |
#        | 83 | Public | Deploy To, Deploy From, View                              | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To, Deploy From, View                       | DECISION TESTING | Unauthorised              |
#        | 84 | Public | Deploy To, Deploy From, Execute                           | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To, Deploy From, Execute                    | DECISION TESTING | Unauthorised              |
#        | 85 | Public | Deploy To, Deploy From, Contribute, View, Execute, Admin  | WORKFLOWS\BARNEY\DECISION TESTING | None                     | User      | All       | Deploy To, Deploy From, Contribute, View, Execute  | DECISION TESTING | Unauthorised              |
#        
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
