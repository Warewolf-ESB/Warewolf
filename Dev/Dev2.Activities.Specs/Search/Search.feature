@WarewolfSearch
Feature: Search
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Search Workflow Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsWorkflowNameSelected" checkbox
	When I search for "SearchWorkflowForSpecs"
	Then the search result contains
	  | ResourceId                           | Name                   | Path                                         | Type         | Match                  |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | WorkflowName | SearchWorkflowForSpecs |

Scenario: Search TestName Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsTestNameSelected" checkbox
	When I search for "TestForSearchSpecs"
	Then the search result contains
	  | ResourceId                           | Name                   | Path                                         | Type     | Match              |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | TestName | TestForSearchSpecs |

Scenario: Search ScalarName Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsScalarNameSelected" checkbox
	When I search for "SearchVar"
	Then the search result contains
	  | ResourceId                           | Name      | Path                                         | Type   | Match     |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | Scalar | SearchVar |

Scenario: Search ObjectName Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsObjectNameSelected" checkbox
	When I search for "SearchObject"
	Then the search result contains
	  | ResourceId                           | Name                   | Path                                         | Type   | Match         |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | Object | @SearchObject |

Scenario: Search RecSetName Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsRecSetNameSelected" checkbox
	When I search for "SearchRec"
	Then the search result contains
	  | ResourceId                           | Name      | Path                                         | Type      | Match     |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | RecordSet | SearchRec |

Scenario: Search ToolTitle Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsToolTitleSelected" checkbox
	When I search for "Search Tool"
	Then the search result contains
	  | ResourceId                           | Name        | Path                                         | Type      | Match       |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | ToolTitle | Search Tool |

Scenario: Search InputVariable Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsInputVariableSelected" checkbox
	When I search for "SearchVar"
	Then the search result contains
	  | ResourceId                           | Name      | Path                                         | Type        | Match     |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | ScalarInput | SearchVar |

Scenario: Search OutputVariable Name
	Given I have a localhost server
	And I have the Search View open
	And I check the "IsOutputVariableSelected" checkbox
	When I search for "SearchRec"
	Then the search result contains
	  | ResourceId                           | Name      | Path                                         | Type         | Match     |
	  | c494711c-c6a4-44d5-abb9-c0339cd88bae | SearchWorkflowForSpecs | SearchFolderForSpecs\\SearchWorkflowForSpecs | RecordSetOutput | SearchRec |