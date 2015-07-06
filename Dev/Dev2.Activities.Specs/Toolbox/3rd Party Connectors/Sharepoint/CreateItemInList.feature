Feature: CreateItemInList
	In order to add items to a Sharepoint List
	As a Warewolf user
	I want to a tool that willl show me what fields need to be captured

Background: Setup for sharepoint scenerio
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Create" list
	And all items are deleted from the list	
	Then scenerio is clean

Scenario: Create Item static data
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Create" list
	And I map the list input fields as
	| Field Name | Variable                             |
	| Name       | Created From Warewolf                |
	| Title      | My New Warewolf Acceptance Test Item |
	And I have result variable as "[[Result]]"
	When the sharepoint create list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                             |
	| 1 | Name       | Created From Warewolf                |
	| 2 | Title      | My New Warewolf Acceptance Test Item |
	And the debug output as 
	|                      |
	| [[Result]] = Success |

Scenario: Create Item scalar data
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Create" list
	And I map the list input fields as
	| Field Name | Variable  |
	| Name       | [[name]]  |
	| Title      | [[title]] |
	And I have result variable as "[[Result]]"
	And I have a variable "[[name]]" with value "Created From Warewolf"
	And I have a variable "[[title]]" with value "My New Warewolf Acceptance Test Item"
	When the sharepoint create list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                             |
	| 1 | Name       | [[name]] = Created From Warewolf                |
	| 2 | Title      | [[title]] = My New Warewolf Acceptance Test Item |
	And the debug output as 
	|                      |
	| [[Result]] = Success |

Scenario: Create Item recordset last record
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Create" list
	And I map the list input fields as
	| Field Name | Variable          |
	| Name       | [[items().name]]  |
	| Title      | [[items().title]] |
	And I have result variable as "[[Result]]"
	And I have a variable "[[items(1).name]]" with value "Created From Warewolf"
	And I have a variable "[[items(1).title]]" with value "My New Warewolf Acceptance Test Item"
	When the sharepoint create list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                             |
	| 1 | Name       | [[items(1).name]] = Created From Warewolf                |
	| 2 | Title      | [[items(1).title]] = My New Warewolf Acceptance Test Item |
	And the debug output as 
	|                      |
	| [[Result]] = Success |

Scenario: Create Item recordset given record index 
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Create" list
	And I map the list input fields as
	| Field Name | Variable           |
	| Name       | [[items(1).name]]  |
	| Title      | [[items(1).title]] |
	And I have result variable as "[[Result]]"
	And I have a variable "[[items(1).name]]" with value "Created From Warewolf"
	And I have a variable "[[items(1).title]]" with value "My New Warewolf Acceptance Test Item"
	When the sharepoint create list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                             |
	| 1 | Name       | [[items(1).name]] = Created From Warewolf                |
	| 2 | Title      | [[items(1).title]] = My New Warewolf Acceptance Test Item |
	And the debug output as 
	|                      |
	| [[Result]] = Success |

Scenario: Create Item recordset all records
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Create" list
	And I map the list input fields as
	| Field Name | Variable           |
	| Name       | [[items(*).name]]  |
	| Title      | [[items(*).title]] |
	And I have result variable as "[[Result]]"
	And I have a variable "[[items(1).name]]" with value "Created From Warewolf"
	And I have a variable "[[items(1).title]]" with value "My New Warewolf Acceptance Test Item"
	And I have a variable "[[items(2).name]]" with value "Created From Warewolf 2"
	And I have a variable "[[items(2).title]]" with value "My New Warewolf Acceptance Test Item 2"
	When the sharepoint create list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                                    |
	| 1 | Name       | [[items(1).name]] = Created From Warewolf                   |
	|   |            | [[items(2).name]] = Created From Warewolf 2                 |
	| 2 | Title      | [[items(1).title]] = My New Warewolf Acceptance Test Item   |
	|   |            | [[items(2).title]] = My New Warewolf Acceptance Test Item 2 |
	And the debug output as 
	|                      |
	| [[Result]] = Success |
