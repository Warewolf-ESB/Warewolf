Feature: ReadItemFromList
	In order to read an item from a Sharepoint List
	As a Warewolf User
	I want a tool that will allow be to provide criteria to find and item 
	  and map the item properties to Warewolf variables

Scenario: Read Item from list with no criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	|   | [[list(2).id]] = 2                          |
	| 2 | [[list(1).name]] = name1                    |
	|   | [[list(2).name]] = name2                    |
	| 3 | [[list(1).title]] = Do not delete this item |
	|   | [[list(2).title]] = Do not delete item 2    |

Scenario: Read Item from Sharepoint list with Equal criteria
Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| ID         | Equals      | 2     |      |    |
	When the sharepoint tool is executed	
	Then the value of "[[list(1).id]]" equals 2
	Then the value of "[[list(1).name]]" equals "name2"
	Then the value of "[[list(1).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                          |
	| 1 | [[list(1).id]] = 2                       |
	| 2 | [[list(1).name]] = name2                 |
	| 3 | [[list(1).title]] = Do not delete item 2 |

Scenario: Read Item from Sharepoint list with Greater Than criteria
Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| ID         | >           | 1     |      |    |
	When the sharepoint tool is executed	
	Then the value of "[[list(1).id]]" equals 2
	Then the value of "[[list(1).name]]" equals "name2"
	Then the value of "[[list(1).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                          |
	| 1 | [[list(1).id]] = 2                       |
	| 2 | [[list(1).name]] = name2                 |
	| 3 | [[list(1).title]] = Do not delete item 2 |

Scenario: Read Item from list with Greater Than Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
		And search criteria as
	| Field Name | Search Type | Value | From | To |
	| ID         | >=           | 1     |      |    |
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	|   | [[list(2).id]] = 2                          |
	| 2 | [[list(1).name]] = name1                    |
	|   | [[list(2).name]] = name2                    |
	| 3 | [[list(1).title]] = Do not delete this item |
	|   | [[list(2).title]] = Do not delete item 2    |

Scenario: Read Item from Sharepoint list with Less Than criteria
Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| ID         | <           | 2     |      |    |
	When the sharepoint tool is executed	
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	| 2 | [[list(1).name]] = name1                    |
	| 3 | [[list(1).title]] = Do not delete this item |

Scenario: Read Item from list with Less Than Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
		And search criteria as
	| Field Name | Search Type | Value | From | To |
	| ID         | <=           | 2     |      |    |
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	|   | [[list(2).id]] = 2                          |
	| 2 | [[list(1).name]] = name1                    |
	|   | [[list(2).name]] = name2                    |
	| 3 | [[list(1).title]] = Do not delete this item |
	|   | [[list(2).title]] = Do not delete item 2    |

Scenario: Read Item from Sharepoint list with Not Equal criteria
Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| ID         | <>      | 2     |      |    |
	When the sharepoint tool is executed	
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	| 2 | [[list(1).name]] = name1                    |
	| 3 | [[list(1).title]] = Do not delete this item |

Scenario: Read Item from list with Contains criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
		And search criteria as
	| Field Name | Search Type | Value  | From | To |
	| Title      | Contains    | delete |      |    |
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	|   | [[list(2).id]] = 2                          |
	| 2 | [[list(1).name]] = name1                    |
	|   | [[list(2).name]] = name2                    |
	| 3 | [[list(1).title]] = Do not delete this item |
	|   | [[list(2).title]] = Do not delete item 2    |

Scenario: Read Item from list with Begins With criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
		And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Begins With           | Do     |      |    |
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	|   | [[list(2).id]] = 2                          |
	| 2 | [[list(1).name]] = name1                    |
	|   | [[list(2).name]] = name2                    |
	| 3 | [[list(1).title]] = Do not delete this item |
	|   | [[list(2).title]] = Do not delete item 2    |

Scenario: Read Item from list with Multiple criteria return multiple results
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
		And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title      | Contains    | Do    |      |    |
	| ID         | <=          | 2     |      |    |
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not delete item 2"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	|   | [[list(2).id]] = 2                          |
	| 2 | [[list(1).name]] = name1                    |
	|   | [[list(2).name]] = name2                    |
	| 3 | [[list(1).title]] = Do not delete this item |
	|   | [[list(2).title]] = Do not delete item 2    |

Scenario: Read Item from list with Multiple criteria return single results
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
		And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title      | Contains    | Do    |      |    |
	| ID         | <           | 2     |      |    |
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	| 2 | [[list(1).name]] = name1                    |
	| 3 | [[list(1).title]] = Do not delete this item |

Scenario: Read Item from list with Multiple criteria do not match all criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list fields as
		| Variable         | Field Name |
		| [[list().id]]    | ID         |
		| [[list().name]]  | Name       |
		| [[list().title]] | Title      |
		And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title      | Contains    | Do    |      |    |
	| ID         | <           | 2     |      |    |
	And do not require all criteria to match
	When the sharepoint tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not delete this item"
	And the execution has "NO" error
	And the debug inputs as
	| # | Variable           | Field Name |
	| 1 | [[list().id]] =    | ID         |
	| 2 | [[list().name]] =  | Name       |
	| 3 | [[list().title]] = | Title      |
	And the debug output as 
	| # |                                             |
	| 1 | [[list(1).id]] = 1                          |
	| 2 | [[list(1).name]] = name1                    |
	| 3 | [[list(1).title]] = Do not delete this item |