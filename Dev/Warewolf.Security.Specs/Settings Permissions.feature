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
Scenario Outline: Setting Selected Resource Permissions for Users
        Given I have a server "localhost"
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should not have "<Rights>" 
Examples:
        | No | Resource Rights           | User Group | Resources | Rights | Resource                         | Permissions               |
        | 1  | View                      | Users      | All       | None   | Examples\Control Flow - Decision | View                      |
        | 2  | Execute                   | Users      | All       | None   | Examples\Control Flow - Decision | Execute                   |
        | 3  | Contribute, View, Execute | Users      | All       | None   | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 4  | View, Execute             | Users      | All       | None   | Examples\Control Flow - Decision | View, Execute             |

@OverlappingUserGroupsPermissionsSecurity
Scenario Outline: Setting Selected Resource Permissions for Users Overlapping Groups
		Given I have Public with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Given rights                                                     | Resource Rights           | User Group | Resources | Rights                                                           | Resource                                      | Permissions               |
        | 1  | View                                                             | View                      | Users      | All       | View                                                             | Examples\Control Flow - Decision | View                      |
        | 2  | Execute                                                          | View                      | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | View                      |
        | 3  | Contribute, View, Execute                                        | View                      | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | View                      |
        | 4  | View, Execute                                                    | View                      | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | View                      |
        | 5  | View, Execute, Contribute, Deploy To, Deploy From, Administrator | View                      | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Examples\Control Flow - Decision | View                      |
        | 6  | View                                                             | Execute                   | Users      | All       | View                                                             | Examples\Control Flow - Decision | Execute                   |
        | 7  | Execute                                                          | Execute                   | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | Execute                   |
        | 8  | Contribute, View, Execute                                        | Execute                   | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | Execute                   |
        | 9  | View, Execute                                                    | Execute                   | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | Execute                   |
        | 10 | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Execute                   | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Examples\Control Flow - Decision | Execute                   |
        | 11 | View                                                             | Contribute, View, Execute | Users      | All       | View                                                             | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 12 | Execute                                                          | Contribute, View, Execute | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 13 | Contribute, View, Execute                                        | Contribute, View, Execute | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 14 | View, Execute                                                    | Contribute, View, Execute | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 15 | View, Execute, Contribute, Deploy To, Deploy From                | Contribute, View, Execute | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 16 | View                                                             | View, Execute             | Users      | All       | View                                                             | Examples\Control Flow - Decision | View, Execute             |
        | 17 | Execute                                                          | View, Execute             | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | View, Execute             |
        | 18 | Contribute, View, Execute                                        | View, Execute             | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | View, Execute             |
        | 19 | View, Execute                                                    | View, Execute             | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | View, Execute             |
        | 20 | View, Execute, Contribute, Deploy To, Deploy From                | View, Execute             | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Examples\Control Flow - Decision | View, Execute             |
        | 21 | View                                                             | None                      | Users      | All       | View                                                             | Examples\Control Flow - Decision | None                      |
        | 22 | Execute                                                          | None                      | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | None                      |
        | 23 | Contribute, View, Execute                                        | None                      | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | None                      |
        | 24 | View, Execute                                                    | None                      | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | None                      |
        | 25 | View, Execute, Contribute, Deploy To, Deploy From, Administrator | None                      | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Examples\Control Flow - Decision | None                      |
        | 26 | None                                                             | Contribute, View, Execute | Users      | Users     | None                                                             | Examples\Control Flow - Decision | Contribute, View, Execute |

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
        | 1  | Users | Deploy To                                                        | View            | Users      | All       | Deploy To                                                        | Examples\Control Flow - Decision | View        |
        | 2  | Users | Deploy From                                                      | View            | Users      | All       | Deploy From                                                      | Examples\Control Flow - Decision | View        |
        | 3  | Users | View                                                             | View            | Users      | All       | View                                                             | Examples\Control Flow - Decision | View        |
        | 4  | Users | Execute                                                          | View            | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | View        |
        | 5  | Users | Contribute, View, Execute                                        | View            | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | View        |
        | 6  | Users | Deploy To, Deploy From                                           | View            | Users      | All       | Deploy To, Deploy From                                           | Examples\Control Flow - Decision | View        |
        | 7  | Users | View, Execute                                                    | View            | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | View        |
        | 8  | Users | View, Execute, Contribute, Deploy To, Deploy From, Administrator | View            | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Examples\Control Flow - Decision | View        |
        | 9  | Users | Deploy To, View                                                  | View            | Users      | All       | Deploy To, View                                                  | Examples\Control Flow - Decision | View        |
        | 10 | Users | Deploy To, Execute                                               | View            | Users      | All       | Deploy To, Execute                                               | Examples\Control Flow - Decision | View        |
        | 11 | Users | Deploy To, Contribute, View, Execute                             | View            | Users      | All       | Deploy To, Contribute, View, Execute                             | Examples\Control Flow - Decision | View        |
        | 12 | Users | Deploy From, View                                                | View            | Users      | All       | Deploy From, View                                                | Examples\Control Flow - Decision | View        |
        | 13 | Users | Deploy From, Execute                                             | View            | Users      | All       | Deploy From, Execute                                             | Examples\Control Flow - Decision | View        |
        | 14 | Users | Deploy From, Contribute, View, Execute                           | View            | Users      | All       | Deploy From, Contribute, View, Execute                           | Examples\Control Flow - Decision | View        |
        | 15 | Users | Deploy To, Deploy From, View                                     | View            | Users      | All       | Deploy To, Deploy From , View                                    | Examples\Control Flow - Decision | View        |
        | 16 | Users | Deploy To, Deploy From, Execute                                  | View            | Users      | All       | Deploy To, Deploy From , Execute                                 | Examples\Control Flow - Decision | View        |
        | 17 | Users | Deploy To, Deploy From, Contribute, View, Execute                | View            | Users      | All       | Deploy To, Deploy From , Contribute, View, Execute               | Examples\Control Flow - Decision | View        |

@ConflictingExecutePermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and conflicting with Execute permissions       
		Given I have Users with "<Given rights>" 
        And Resource "<Resource Name>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Group | Given rights                                                     | Resource Name                                 | Resource Rights | User Group | Resources | Rights                                                           | Resource                                      | Permissions |
        | 1  | Users | Deploy To                                                        | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To                                                        | Examples\Control Flow - Decision | Execute     |
        | 2  | Users | Deploy From                                                      | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy From                                                      | Examples\Control Flow - Decision | Execute     |
        | 3  | Users | View                                                             | Examples\Control Flow - Decision | Execute         | Users      | All       | View                                                             | Examples\Control Flow - Decision | Execute     |
        | 4  | Users | Execute                                                          | Examples\Control Flow - Decision | Execute         | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | Execute     |
        | 5  | Users | Contribute, View, Execute                                        | Examples\Control Flow - Decision | Execute         | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | Execute     |
        | 6  | Users | Deploy To, Deploy From                                           | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To, Deploy From                                           | Examples\Control Flow - Decision | Execute     |
        | 7  | Users | View, Execute                                                    | Examples\Control Flow - Decision | Execute         | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | Execute     |
        | 8  | Users | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Examples\Control Flow - Decision | Execute         | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Examples\Control Flow - Decision | Execute     |
        | 9  | Users | Deploy To, View                                                  | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To, View                                                  | Examples\Control Flow - Decision | Execute     |
        | 10 | Users | Deploy To, Execute                                               | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To, Execute                                               | Examples\Control Flow - Decision | Execute     |
        | 11 | Users | Deploy To, Contribute, View, Execute                             | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To, Contribute, View, Execute                             | Examples\Control Flow - Decision | Execute     |
        | 12 | Users | Deploy From, View                                                | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy From, View                                                | Examples\Control Flow - Decision | Execute     |
        | 13 | Users | Deploy From, Execute                                             | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy From, Execute                                             | Examples\Control Flow - Decision | Execute     |
        | 14 | Users | Deploy From, Contribute, View, Execute                           | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy From, Contribute, View, Execute                           | Examples\Control Flow - Decision | Execute     |
        | 15 | Users | Deploy To, Deploy From, View                                     | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To, Deploy From, View                                     | Examples\Control Flow - Decision | Execute     |
        | 16 | Users | Deploy To, Deploy From, Execute                                  | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To, Deploy From, Execute                                  | Examples\Control Flow - Decision | Execute     |
        | 17 | Users | Deploy To, Deploy From, Contribute, View, Execute                | Examples\Control Flow - Decision | Execute         | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute                | Examples\Control Flow - Decision | Execute     |

@ConflictingContributeViewExecutePermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and conflicting with Contribute, View and Execute permissions
       Given I have Users with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No |Given rights                                                     | Resource Rights           | User Group | Resources | Rights                                                           | Resource                                      | Permissions               |
        | 1  |Deploy To                                                        | Contribute, View, Execute | Users      | All       | Deploy To                                                        | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 2  |Deploy From                                                      | Contribute, View, Execute | Users      | All       | Deploy From                                                      | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 3  |View                                                             | Contribute, View, Execute | Users      | All       | View                                                             | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 4  |Execute                                                          | Contribute, View, Execute | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 5  |Contribute, View, Execute                                        | Contribute, View, Execute | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 6  |Deploy To, Deploy From                                           | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From                                           | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 7  |View, Execute                                                    | Contribute, View, Execute | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 8  |View, Execute, Contribute, Deploy To, Deploy From                | Contribute, View, Execute | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 9  |Deploy To, View                                                  | Contribute, View, Execute | Users      | All       | Deploy To, View                                                  | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 10 |Deploy To, Execute                                               | Contribute, View, Execute | Users      | All       | Deploy To, Execute                                               | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 11 |Deploy To, Contribute, View, Execute                             | Contribute, View, Execute | Users      | All       | Deploy To, Contribute, View, Execute                             | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 12 |Deploy From, View                                                | Contribute, View, Execute | Users      | All       | Deploy From, View                                                | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 13 |Deploy From, Execute                                             | Contribute, View, Execute | Users      | All       | Deploy From, Execute                                             | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 14 |Deploy From, Contribute, View, Execute                           | Contribute, View, Execute | Users      | All       | Deploy From, Contribute, View, Execute                           | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 15 |Deploy To, Deploy From, View                                     | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From, View                                     | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 16 |Deploy To, Deploy From, Execute                                  | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From, Execute                                  | Examples\Control Flow - Decision | Contribute, View, Execute |
        | 17 |Deploy To, Deploy From, Contribute, View, Execute, Administrator | Contribute, View, Execute | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute, Administrator | Examples\Control Flow - Decision | Contribute, View, Execute |

@ConflictingViewExecutePermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and conflicting with View and Execute permissions
        Given I have Users with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Given rights                                                     | Resource Rights | User Group | Resources | Rights                                                           | Resource                                      | Permissions   |
        | 1  | Deploy To                                                        | View, Execute   | Users      | All       | Deploy To                                                        | Examples\Control Flow - Decision | View, Execute |
        | 2  | Deploy From                                                      | View, Execute   | Users      | All       | Deploy From                                                      | Examples\Control Flow - Decision | View, Execute |
        | 3  | View                                                             | View, Execute   | Users      | All       | View                                                             | Examples\Control Flow - Decision | View, Execute |
        | 4  | Contribute, View, Execute                                        | View, Execute   | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | View, Execute |
        | 5  | Deploy To, Deploy From                                           | View, Execute   | Users      | All       | Deploy To, Deploy From                                           | Examples\Control Flow - Decision | View, Execute |
        | 6  | View, Execute                                                    | View, Execute   | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | View, Execute |
        | 7  | View, Execute, Contribute, Deploy To, Deploy From                | View, Execute   | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From                | Examples\Control Flow - Decision | View, Execute |
        | 8  | Deploy To, View                                                  | View, Execute   | Users      | All       | Deploy To, View                                                  | Examples\Control Flow - Decision | View, Execute |
        | 9  | Deploy To, Execute                                               | View, Execute   | Users      | All       | Deploy To, Execute                                               | Examples\Control Flow - Decision | View, Execute |
        | 10 | Deploy To, Contribute, View, Execute                             | View, Execute   | Users      | All       | Deploy To, Contribute, View, Execute                             | Examples\Control Flow - Decision | View, Execute |
        | 11 | Deploy From, View                                                | View, Execute   | Users      | All       | Deploy From, View                                                | Examples\Control Flow - Decision | View, Execute |
        | 12 | Deploy From, Execute                                             | View, Execute   | Users      | All       | Deploy From, Execute                                             | Examples\Control Flow - Decision | View, Execute |
        | 13 | Deploy From, Contribute, View, Execute                           | View, Execute   | Users      | All       | Deploy From, Contribute, View, Execute                           | Examples\Control Flow - Decision | View, Execute |
        | 14 | Deploy To, Deploy From, View                                     | View, Execute   | Users      | All       | Deploy To, Deploy From, View                                     | Examples\Control Flow - Decision | View, Execute |
        | 15 | Deploy To, Deploy From, Execute                                  | View, Execute   | Users      | All       | Deploy To, Deploy From, Execute                                  | Examples\Control Flow - Decision | View, Execute |
        | 16 | Deploy To, Deploy From, Contribute, View, Execute, Administrator | View, Execute   | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute, Administrator | Examples\Control Flow - Decision | View, Execute |

@NoConflictingPermissionsSecurity		
Scenario Outline: Setting Selected Resource Permissions for users and no conflicting permissions
        Given I have Users with "<Given rights>" 
        And Resource "<Resource>" has rights "<Resource Rights>" for "<User Group>"
        When connected as user part of "<User Group>"
        Then "<Resource>" should have "<Permissions>"
		And resources should have "<Rights>" 
Examples: 
        | No | Given rights                                                     | Resource Rights | User Group | Resources | Rights                                                           | Resource                                      | Permissions |
        | 1  | Deploy To                                                        | None            | Users      | All       | Deploy To                                                        | Examples\Control Flow - Decision | None        |
        | 2  | Deploy From                                                      | None            | Users      | All       | Deploy From                                                      | Examples\Control Flow - Decision | None        |
        | 3  | View                                                             | None            | Users      | All       | View                                                             | Examples\Control Flow - Decision | None        |
        | 4  | Execute                                                          | None            | Users      | All       | Execute                                                          | Examples\Control Flow - Decision | None        |
        | 5  | Contribute, View, Execute                                        | None            | Users      | All       | Contribute, View, Execute                                        | Examples\Control Flow - Decision | None        |
        | 6  | Deploy To, Deploy From                                           | None            | Users      | All       | Deploy To, Deploy From                                           | Examples\Control Flow - Decision | None        |
        | 7  | View, Execute                                                    | None            | Users      | All       | View, Execute                                                    | Examples\Control Flow - Decision | None        |
        | 8  | View, Execute, Contribute, Deploy To, Deploy From, Administrator | None            | Users      | All       | View, Execute, Contribute, Deploy To, Deploy From, Administrator | Examples\Control Flow - Decision | None        |
        | 9  | Deploy To, View                                                  | None            | Users      | All       | Deploy To, View                                                  | Examples\Control Flow - Decision | None        |
        | 10 | Deploy To, Execute                                               | None            | Users      | All       | Deploy To, Execute                                               | Examples\Control Flow - Decision | None        |
        | 11 | Deploy To, Contribute, View, Execute                             | None            | Users      | All       | Deploy To, Contribute, View, Execute                             | Examples\Control Flow - Decision | None        |
        | 12 | Deploy From, View                                                | None            | Users      | All       | Deploy From, View                                                | Examples\Control Flow - Decision | None        |
        | 13 | Deploy From, Execute                                             | None            | Users      | All       | Deploy From, Execute                                             | Examples\Control Flow - Decision | None        |
        | 14 | Deploy From, Contribute, View, Execute                           | None            | Users      | All       | Deploy From, Contribute, View, Execute                           | Examples\Control Flow - Decision | None        |
        | 15 | Deploy To, Deploy From, View                                     | None            | Users      | All       | Deploy To, Deploy From, View                                     | Examples\Control Flow - Decision | None        |
        | 16 | Deploy To, Deploy From, Execute                                  | None            | Users      | All       | Deploy To, Deploy From, Execute                                  | Examples\Control Flow - Decision | None        |
        | 17 | Deploy To, Deploy From, Contribute, View, Execute, Administrator | None            | Users      | All       | Deploy To, Deploy From, Contribute, View, Execute, Administrator | Examples\Control Flow - Decision | None        |
