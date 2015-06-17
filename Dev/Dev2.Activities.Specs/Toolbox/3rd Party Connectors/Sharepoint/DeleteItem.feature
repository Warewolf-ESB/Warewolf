Feature: DeleteItemFromList
	In order to delete an item from a Sharepoint List
	As a Warewolf User
	I want a tool that will allow be to provide criteria to find and item 
	  and map the number of found items to the result
	  and delete the found  items from the Sharepoint List

Background: Clear out the Sharepoint list we will use for testing
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the execution has "NO" error
	And clear the activity
	

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
	| RequiredField      | [[items(*).required]] |
	And I have a variable "[[items(1).name]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(1).required]]" with value "One"
	And I have a variable "[[items(2).name]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(2).required]]" with value "Two"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Equals      | Two     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | =      | Two   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 1 |

Scenario: Delete Item from Sharepoint list with Greater Than criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| IntField       | [[items(*).intfield]]  |
	| Title      | [[items(*).title]] |
	| RequiredField      | [[items(*).title]] |
	And I have a variable "[[items(1).intfield]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).intfield]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).intfield]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField         | >      | 100     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField      | >      | 100   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Greater Than Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| IntField       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	| RequiredField      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).name]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).name]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField         | >=      | 100     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "3"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField      | >=      | 100   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 3 |

Scenario: Delete Item from Sharepoint list with Less Than criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| IntField       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	| RequiredField      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).name]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).name]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField         | <      | 200     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField      | <      | 200   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 1 |

Scenario: Delete Item from list with Less Than Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| IntField       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	| RequiredField      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).name]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).name]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField         | <=      | 200     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField      | <=      | 200   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

Scenario: Delete Item from Sharepoint list with Not Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| IntField       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	| RequiredField      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).name]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).name]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField         | <>      | 200     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField      | <>      | 200   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Contains criteria
		Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| IntField       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	| RequiredField      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "one"
	And I have a variable "[[items(2).name]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "one two"
	And I have a variable "[[items(3).name]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "three"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Contains      | one     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains      | one   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Begins With criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name | Variable           |
	| IntField       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	| RequiredField      | [[items(*).title]] |
	And I have a variable "[[items(1).name]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "one"
	And I have a variable "[[items(2).name]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "one two"
	And I have a variable "[[items(3).name]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "three"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Begins With      | one     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Begins With      | one   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Multiple criteria return multiple results
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name    | Variable              |
	| IntField      | [[items(*).intfield]] |
	| Title         | [[items(*).title]]    |
	| RequiredField | [[items(*).title]]    |
	And I have a variable "[[items(1).intfield]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).intfield]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).intfield]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And I have a variable "[[items(4).intfield]]" with value "400"
	And I have a variable "[[items(4).title]]" with value "4th"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Contains      | o     |      |    |
	| IntField         | <=      | 300     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "3"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains      | o   | Yes                           |
	| 2 | IntField      | <>      | 300   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 3 |

Scenario: Delete Item from list with Multiple criteria return single results
		Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name    | Variable              |
	| IntField      | [[items(*).intfield]] |
	| Title         | [[items(*).title]]    |
	| RequiredField | [[items(*).title]]    |
	And I have a variable "[[items(1).intfield]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).intfield]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).intfield]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And I have a variable "[[items(4).intfield]]" with value "400"
	And I have a variable "[[items(4).title]]" with value "4th"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Contains      | e     |      |    |
	| IntField         | Equals      | 300     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains      | e   | Yes                           |
	| 2 | IntField      | =      | 300   | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 1 |

Scenario: Delete Item from list with Multiple criteria do not match all criteria
			Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name    | Variable              |
	| IntField      | [[items(*).intfield]] |
	| Title         | [[items(*).title]]    |
	| RequiredField | [[items(*).title]]    |
	And I have a variable "[[items(1).intfield]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).intfield]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).intfield]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And I have a variable "[[items(4).intfield]]" with value "400"
	And I have a variable "[[items(4).title]]" with value "4th"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Contains      | o     |      |    |
	| IntField         | <      | 200     |      |    |
	And do not require all criteria to match
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains      | o   | No                           |
	| 2 | IntField      | <      | 200   | No                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

	
Scenario: Delete Item from list with Multiple criteria match all criteria finds nothing
			Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting" list
		And I map the list input fields as
	| Field Name    | Variable              |
	| IntField      | [[items(*).intfield]] |
	| Title         | [[items(*).title]]    |
	| RequiredField | [[items(*).title]]    |
	And I have a variable "[[items(1).intfield]]" with value "100"
	And I have a variable "[[items(1).title]]" with value "One"
	And I have a variable "[[items(2).intfield]]" with value "200"
	And I have a variable "[[items(2).title]]" with value "Two"
	And I have a variable "[[items(3).intfield]]" with value "300"
	And I have a variable "[[items(3).title]]" with value "Three"
	And I have a variable "[[items(4).intfield]]" with value "400"
	And I have a variable "[[items(4).title]]" with value "4th"
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title         | Contains      | n     |      |    |
	| IntField         | >      | 200     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	And the activity is cleared
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "0"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains      | n   | No                           |
	| 2 | IntField      | >      | 200   | No                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 0 |
