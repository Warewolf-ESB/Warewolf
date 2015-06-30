Feature: DeleteItemFromList
	In order to delete an item from a Sharepoint List
	As a Warewolf User
	I want a tool that will allow be to provide criteria to find and item 
	  and map the number of found items to the result
	  and delete the found  items from the Sharepoint List

Scenario: Delete Item from list with no criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I map the list input fields as
	| Field Name | Variable           |
	| Name       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "Created From Warewolf"
	And I have a variable "[[items(1).title]]" with value "My New Warewolf Acceptance Test Item"
	And I have a variable "[[items(2).name]]" with value "Created From Warewolf 2"
	And I have a variable "[[items(2).title]]" with value "My New Warewolf Acceptance Test Item 2"
	#And I have a variable "[[items(3).name]]" with value "Created From Warewolf 3"
	#And I have a variable "[[items(3).title]]" with value "My New Warewolf Acceptance Test Item 3"
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

Scenario: Delete Item from Sharepoint list with Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| Name       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "1"
	And I have a variable "[[items(1).title]]" with value "My New Warewolf Acceptance Test Item"
	And I have a variable "[[items(2).name]]" with value "2"
	And I have a variable "[[items(2).title]]" with value "My New Warewolf Acceptance Test Item 2"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Equals      | "My New Warewolf Acceptance Test Item"     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug output as 
	|                                             |
	| [[Result]] = 1 |

Scenario: Delete Item from Sharepoint list with Greater Than criteria
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
	When the sharepoint delete item from list tool is executed	
	Then the value of "[[list(1).id]]" equals 2
	Then the value of "[[list(1).name]]" equals "name2"
	Then the value of "[[list(1).title]]" equals "Do not read item 2"
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
	| 3 | [[list(1).title]] = Do not read item 2 |

Scenario: Delete Item from list with Greater Than Equal criteria
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
	When the sharepoint delete item from list tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not read item 2"
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
	| 3 | [[list(1).title]] = Do not read this item |
	|   | [[list(2).title]] = Do not read item 2    |

Scenario: Delete Item from Sharepoint list with Less Than criteria
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
	When the sharepoint delete item from list tool is executed	
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
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
	| 3 | [[list(1).title]] = Do not read this item |

Scenario: Delete Item from list with Less Than Equal criteria
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
	When the sharepoint delete item from list tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not read item 2"
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
	| 3 | [[list(1).title]] = Do not read this item |
	|   | [[list(2).title]] = Do not read item 2    |

Scenario: Delete Item from Sharepoint list with Not Equal criteria
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
	When the sharepoint delete item from list tool is executed	
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
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
	| 3 | [[list(1).title]] = Do not read this item |

Scenario: Delete Item from list with Contains criteria
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
	When the sharepoint delete item from list tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not read item 2"
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
	| 3 | [[list(1).title]] = Do not read this item |
	|   | [[list(2).title]] = Do not read item 2    |

Scenario: Delete Item from list with Begins With criteria
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
	When the sharepoint delete item from list tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not read item 2"
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
	| 3 | [[list(1).title]] = Do not read this item |
	|   | [[list(2).title]] = Do not read item 2    |

Scenario: Delete Item from list with Multiple criteria return multiple results
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
	When the sharepoint delete item from list tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
	Then the value of "[[list(2).id]]" equals 2
	Then the value of "[[list(2).name]]" equals "name2"
	Then the value of "[[list(2).title]]" equals "Do not read item 2"
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
	| 3 | [[list(1).title]] = Do not read this item |
	|   | [[list(2).title]] = Do not read item 2    |

Scenario: Delete Item from list with Multiple criteria return single results
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
	When the sharepoint delete item from list tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
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
	| 3 | [[list(1).title]] = Do not read this item |

Scenario: Delete Item from list with Multiple criteria do not match all criteria
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
	When the sharepoint delete item from list tool is executed
	Then the value of "[[list(1).id]]" equals 1
	Then the value of "[[list(1).name]]" equals "name1"
	Then the value of "[[list(1).title]]" equals "Do not read this item"
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
	| 3 | [[list(1).title]] = Do not read this item |
