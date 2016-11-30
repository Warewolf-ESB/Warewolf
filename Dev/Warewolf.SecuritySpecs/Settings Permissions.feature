@Security
Feature: Settings Permissions
	In order to set permissions for my server
	As a Warewolf user
	I want to setup a Server Permissions

	Background: Initialize Feature Level settings
	
@ServerPermissionsSecurity
Scenario Outline: Server Permissions 
        Given I have Public with "<Given rights>" 
        Then resources should have "<Rights>"
Examples:
        | No |  Given rights                                      |  Rights                                            |
        | 1  |  Deploy To                                         |  Deploy To                                         |
        | 2  |  Deploy From                                       |  Deploy From                                       |
        | 3  |  View                                              |  View                                              |
        | 4  |  Execute                                           |  Execute                                           |
        | 5  |  Contribute, View, Execute                         |  Contribute, View, Execute                         |
        | 6  |  Deploy To, Deploy From                            |  Deploy To, Deploy From                            |
        | 7  |  View, Execute                                     |  View, Execute                                     |
        | 8  |  View, Execute, Contribute, Deploy To, Deploy From |  View, Execute, Contribute, Deploy To, Deploy From |
        | 9  |  Deploy To, View                                   |  Deploy To, View                                   |
        | 10 |  Deploy To, Execute                                |  Deploy To, Execute                                |
        | 11 |  Deploy To, Contribute, View, Execute              |  Deploy To, Contribute, View, Execute              |
        | 12 |  Deploy From, View                                 |  Deploy From, View                                 |
        | 13 |  Deploy From, Execute                              |  Deploy From, Execute                              |
        | 14 |  Deploy From, Contribute, View, Execute            |  Deploy From, Contribute, View, Execute            |
        | 15 |  Deploy To, Deploy From, View                      |  Deploy To, Deploy From, View                      |
        | 16 |  Deploy To, Deploy From, Execute                   |  Deploy To, Deploy From, Execute                   |

@ResourcePermissionsSecurity
Scenario Outline: Setting Selected Resource Permissions for users 
        Given I have a server "localhost"
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should not have "<Rights>" 
Examples: 
        | No | Resource Rights           | User Group | Resources | Rights | Resource                                      | Permissions               |
        | 1  | View                      | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 2  | View                      | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 3  | View                      | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 4  | View                      | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 5  | Execute                   | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 6  | Execute                   | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 7  | Execute                   | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 7  | Execute                   | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 8  | Contribute, View, Execute | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 9  | Contribute, View, Execute | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 10 | Contribute, View, Execute | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 11 | Contribute, View, Execute | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 12 | View, Execute             | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 13 | View, Execute             | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 14 | View, Execute             | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 15 | View, Execute             | Users      | All       | None   | Acceptance Testing Resources\DECISION TESTING | View, Execute             |

@OverlappingUserGroupsPermissionsSecurity
Scenario Outline: Setting Selected Resource Permissions for users overlapping groups
		Given I have Public with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Given rights                                                     | Resource Rights           | User Group | Resources | Rights                                                           | Resource                                      | Permissions               |
        | 1  | View                                                             | View                      | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 2  | Execute                                                          | View                      | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 3  | Contribute, View, Execute                                        | View                      | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 4  | View, Execute                                                    | View                      | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 5  | View, Execute, Contribute, Deploy To, Deploy From, Administrator | View                      | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Acceptance Testing Resources\DECISION TESTING | View                      |
        | 6  | View                                                             | Execute                   | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 7  | Execute                                                          | Execute                   | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 8  | Contribute, View, Execute                                        | Execute                   | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 9  | View, Execute                                                    | Execute                   | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 10 | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Execute                   | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Acceptance Testing Resources\DECISION TESTING | Execute                   |
        | 11 | View                                                             | Contribute, View, Execute | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 12 | Execute                                                          | Contribute, View, Execute | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 13 | Contribute, View, Execute                                        | Contribute, View, Execute | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 14 | View, Execute                                                    | Contribute, View, Execute | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 15 | View, Execute, Contribute, Deploy To, Deploy From                | Contribute, View, Execute | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 16 | View                                                             | View, Execute             | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 17 | Execute                                                          | View, Execute             | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 18 | Contribute, View, Execute                                        | View, Execute             | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 19 | View, Execute                                                    | View, Execute             | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 20 | View, Execute, Contribute, Deploy To, Deploy From                | View, Execute             | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Acceptance Testing Resources\DECISION TESTING | View, Execute             |
        | 21 | View                                                             | None                      | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | None                      |
        | 22 | Execute                                                          | None                      | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | None                      |
        | 23 | Contribute, View, Execute                                        | None                      | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | None                      |
        | 24 | View, Execute                                                    | None                      | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | None                      |
        | 25 | View, Execute, Contribute, Deploy To, Deploy From, Administrator | None                      | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Acceptance Testing Resources\DECISION TESTING | None                      |
        | 26 | None                                                             | Contribute, View, Execute | Users      | Users     | None                                                             | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |

@ConflictingViewPermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users conflicting with View permissions
        Given I have a server "localhost"
        And it has "<Group>" with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Group | Given rights                                                     | Resource Rights | User Group | Resources | Rights                                                           | Resource                                      | Permissions |
        | 1  | Users | Deploy To                                                        | View            | Users      | All       | Deploy To                                                        | Acceptance Testing Resources\DECISION TESTING | View        |
        | 2  | Users | Deploy From                                                      | View            | Users      | All       | Deploy From                                                      | Acceptance Testing Resources\DECISION TESTING | View        |
        | 3  | Users | View                                                             | View            | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | View        |
        | 4  | Users | Execute                                                          | View            | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | View        |
        | 5  | Users | Contribute, View, Execute                                        | View            | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | View        |
        | 6  | Users | Deploy To, Deploy From                                           | View            | Users      | All       | Deploy To, Deploy From                                           | Acceptance Testing Resources\DECISION TESTING | View        |
        | 7  | Users | View, Execute                                                    | View            | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | View        |
        | 8  | Users | View, Execute, Contribute, Deploy To, Deploy From, Administrator | View            | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Acceptance Testing Resources\DECISION TESTING | View        |
        | 9  | Users | Deploy To, View                                                  | View            | Users      | All       | Deploy To, View                                                  | Acceptance Testing Resources\DECISION TESTING | View        |
        | 10 | Users | Deploy To, Execute                                               | View            | Users      | All       | Deploy To, Execute                                               | Acceptance Testing Resources\DECISION TESTING | View        |
        | 11 | Users | Deploy To, Contribute, View, Execute                             | View            | Users      | All       | Deploy To, Contribute, View, Execute                             | Acceptance Testing Resources\DECISION TESTING | View        |
        | 12 | Users | Deploy From, View                                                | View            | Users      | All       | Deploy From, View                                                | Acceptance Testing Resources\DECISION TESTING | View        |
        | 13 | Users | Deploy From, Execute                                             | View            | Users      | All       | Deploy From, Execute                                             | Acceptance Testing Resources\DECISION TESTING | View        |
        | 14 | Users | Deploy From, Contribute, View, Execute                           | View            | Users      | All       | Deploy From, Contribute, View, Execute                           | Acceptance Testing Resources\DECISION TESTING | View        |
        | 15 | Users | Deploy To, Deploy From, View                                     | View            | Users      | All       | Deploy To, Deploy From , View                                    | Acceptance Testing Resources\DECISION TESTING | View        |
        | 16 | Users | Deploy To, Deploy From, Execute                                  | View            | Users      | All       | Deploy To, Deploy From , Execute                                 | Acceptance Testing Resources\DECISION TESTING | View        |
        | 17 | Users | Deploy To, Deploy From, Contribute, View, Execute                | View            | Users      | All       | Deploy To, Deploy From , Contribute, View, Execute               | Acceptance Testing Resources\DECISION TESTING | View        |

@ConflictingExecutePermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and conflicting with Execute permissions       
		Given I have Users with "<Given rights>" 
        And Resource "<Resource Name>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Group | Given rights                                                     | Resource Name                                 | Resource Rights | User Group | Resources | Rights                                                           | Resource                                      | Permissions |
        | 1  | Users | Deploy To                                                        | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To                                                        | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 2  | Users | Deploy From                                                      | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy From                                                      | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 3  | Users | View                                                             | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 4  | Users | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 5  | Users | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 6  | Users | Deploy To, Deploy From                                           | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To, Deploy From                                           | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 7  | Users | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 8  | Users | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 9  | Users | Deploy To, View                                                  | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To, View                                                  | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 10 | Users | Deploy To, Execute                                               | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To, Execute                                               | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 11 | Users | Deploy To, Contribute, View, Execute                             | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To, Contribute, View, Execute                             | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 12 | Users | Deploy From, View                                                | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy From, View                                                | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 13 | Users | Deploy From, Execute                                             | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy From, Execute                                             | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 14 | Users | Deploy From, Contribute, View, Execute                           | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy From, Contribute, View, Execute                           | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 15 | Users | Deploy To, Deploy From, View                                     | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To, Deploy From, View                                     | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 16 | Users | Deploy To, Deploy From, Execute                                  | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To, Deploy From, Execute                                  | Acceptance Testing Resources\DECISION TESTING | Execute     |
        | 17 | Users | Deploy To, Deploy From, Contribute, View, Execute                | Acceptance Testing Resources\DECISION TESTING | Execute         | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute                | Acceptance Testing Resources\DECISION TESTING | Execute     |

@ConflictingContributeViewExecutePermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and conflicting with Contribute, View and Execute permissions
       Given I have Users with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No |Given rights                                                     | Resource Rights           | User Group | Resources | Rights                                                           | Resource                                      | Permissions               |
        | 1  |Deploy To                                                        | Contribute, View, Execute | Users      | All       | Deploy To                                                        | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 2  |Deploy From                                                      | Contribute, View, Execute | Users      | All       | Deploy From                                                      | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 3  |View                                                             | Contribute, View, Execute | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 4  |Execute                                                          | Contribute, View, Execute | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 5  |Contribute, View, Execute                                        | Contribute, View, Execute | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 6  |Deploy To, Deploy From                                           | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From                                           | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 7  |View, Execute                                                    | Contribute, View, Execute | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 8  |View, Execute, Contribute, Deploy To, Deploy From                | Contribute, View, Execute | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 9  |Deploy To, View                                                  | Contribute, View, Execute | Users      | All       | Deploy To, View                                                  | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 10 |Deploy To, Execute                                               | Contribute, View, Execute | Users      | All       | Deploy To, Execute                                               | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 11 |Deploy To, Contribute, View, Execute                             | Contribute, View, Execute | Users      | All       | Deploy To, Contribute, View, Execute                             | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 12 |Deploy From, View                                                | Contribute, View, Execute | Users      | All       | Deploy From, View                                                | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 13 |Deploy From, Execute                                             | Contribute, View, Execute | Users      | All       | Deploy From, Execute                                             | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 14 |Deploy From, Contribute, View, Execute                           | Contribute, View, Execute | Users      | All       | Deploy From, Contribute, View, Execute                           | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 15 |Deploy To, Deploy From, View                                     | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From, View                                     | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 16 |Deploy To, Deploy From, Execute                                  | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From, Execute                                  | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |
        | 17 |Deploy To, Deploy From, Contribute, View, Execute, Administrator | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute, Administrator | Acceptance Testing Resources\DECISION TESTING | Contribute, View, Execute |

@ConflictingViewExecutePermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and conflicting with View and Execute permissions
        Given I have Users with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Given rights                                                     | Resource Rights | User Group | Resources | Rights                                                           | Resource                                      | Permissions   |
        | 1  | Deploy To                                                        | View, Execute   | Users      | All       | Deploy To                                                        | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 2  | Deploy From                                                      | View, Execute   | Users      | All       | Deploy From                                                      | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 3  | View                                                             | View, Execute   | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 4  | Contribute, View, Execute                                        | View, Execute   | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 5  | Deploy To, Deploy From                                           | View, Execute   | Users      | All       | Deploy To, Deploy From                                           | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 6  | View, Execute                                                    | View, Execute   | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 7  | View, Execute, Contribute, Deploy To, Deploy From                | View, Execute   | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 8  | Deploy To, View                                                  | View, Execute   | Users      | All       | Deploy To, View                                                  | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 9  | Deploy To, Execute                                               | View, Execute   | Users      | All       | Deploy To, Execute                                               | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 10 | Deploy To, Contribute, View, Execute                             | View, Execute   | Users      | All       | Deploy To, Contribute, View, Execute                             | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 11 | Deploy From, View                                                | View, Execute   | Users      | All       | Deploy From, View                                                | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 12 | Deploy From, Execute                                             | View, Execute   | Users      | All       | Deploy From, Execute                                             | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 13 | Deploy From, Contribute, View, Execute                           | View, Execute   | Users      | All       | Deploy From, Contribute, View, Execute                           | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 14 | Deploy To, Deploy From, View                                     | View, Execute   | Users      | All       | Deploy To, Deploy From, View                                     | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 15 | Deploy To, Deploy From, Execute                                  | View, Execute   | Users      | All       | Deploy To, Deploy From, Execute                                  | Acceptance Testing Resources\DECISION TESTING | View, Execute |
        | 16 | Deploy To, Deploy From, Contribute, View, Execute, Administrator | View, Execute   | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute, Administrator | Acceptance Testing Resources\DECISION TESTING | View, Execute |

@NoConflictingPermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and no conflicting permissions
        Given I have Users with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Given rights                                                     | Resource Rights | User Group | Resources | Rights                                                           | Resource                                      | Permissions |
        | 1  | Deploy To                                                        | None            | Users      | All       | Deploy To                                                        | Acceptance Testing Resources\DECISION TESTING | None        |
        | 2  | Deploy From                                                      | None            | Users      | All       | Deploy From                                                      | Acceptance Testing Resources\DECISION TESTING | None        |
        | 3  | View                                                             | None            | Users      | All       | View                                                             | Acceptance Testing Resources\DECISION TESTING | None        |
        | 4  | Execute                                                          | None            | Users      | All       | Execute                                                          | Acceptance Testing Resources\DECISION TESTING | None        |
        | 5  | Contribute, View, Execute                                        | None            | Users      | All       | Contribute, View, Execute                                        | Acceptance Testing Resources\DECISION TESTING | None        |
        | 6  | Deploy To, Deploy From                                           | None            | Users      | All       | Deploy To, Deploy From                                           | Acceptance Testing Resources\DECISION TESTING | None        |
        | 7  | View, Execute                                                    | None            | Users      | All       | View, Execute                                                    | Acceptance Testing Resources\DECISION TESTING | None        |
        | 8  | View, Execute, Contribute, Deploy To, Deploy From, Administrator | None            | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Acceptance Testing Resources\DECISION TESTING | None        |
        | 9  | Deploy To, View                                                  | None            | Users      | All       | Deploy To, View                                                  | Acceptance Testing Resources\DECISION TESTING | None        |
        | 10 | Deploy To, Execute                                               | None            | Users      | All       | Deploy To, Execute                                               | Acceptance Testing Resources\DECISION TESTING | None        |
        | 11 | Deploy To, Contribute, View, Execute                             | None            | Users      | All       | Deploy To, Contribute, View, Execute                             | Acceptance Testing Resources\DECISION TESTING | None        |
        | 12 | Deploy From, View                                                | None            | Users      | All       | Deploy From, View                                                | Acceptance Testing Resources\DECISION TESTING | None        |
        | 13 | Deploy From, Execute                                             | None            | Users      | All       | Deploy From, Execute                                             | Acceptance Testing Resources\DECISION TESTING | None        |
        | 14 | Deploy From, Contribute, View, Execute                           | None            | Users      | All       | Deploy From, Contribute, View, Execute                           | Acceptance Testing Resources\DECISION TESTING | None        |
        | 15 | Deploy To, Deploy From, View                                     | None            | Users      | All       | Deploy To, Deploy From, View                                     | Acceptance Testing Resources\DECISION TESTING | None        |
        | 16 | Deploy To, Deploy From, Execute                                  | None            | Users      | All       | Deploy To, Deploy From, Execute                                  | Acceptance Testing Resources\DECISION TESTING | None        |
        | 17 | Deploy To, Deploy From, Contribute, View, Execute, Administrator | None            | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute, Administrator | Acceptance Testing Resources\DECISION TESTING | None        |
