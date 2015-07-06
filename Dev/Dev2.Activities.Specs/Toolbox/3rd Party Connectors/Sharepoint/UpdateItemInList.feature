@sharepoint
Feature: UpdateItemInList
	In order to update and item in a SharePoint list
	As a Warewolf user
	I want to a tool that allows updating the item

Background: Setup for sharepoint scenerio
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And all items are deleted from the list
	And I map the list input fields as
	| Variable                      | Field Name         |
	| Warewolf Created Item Name 1  | Name               |
	| Warewolf Created Item Title 1 | Title              |
	| 1                             | IntField           |
	| 10.52                         | CurrencyField      |
	| 2015/06/12                    | DateField          |
	| 2015/06/12 09:00 AM           | DateTimeField      |
	| False                         | BoolField          |
	| Warewolf Created Text Field 1 | MultilineTextField |
	| Warewolf Required Field 1     | RequiredField      |	
	When the sharepoint create list item tool is executed
	Then the execution has "NO" error	
	And scenerio is clean
	And I map the list input fields as
	| Variable                      | Field Name         |
	| Warewolf Created Item Name 2  | Name               |
	| Warewolf Created Item Title 2 | Title              |
	| 2                             | IntField           |
	| 12.52                         | CurrencyField      |
	| 2015/06/11                    | DateField          |
	| 2015/06/11 11:00 AM           | DateTimeField      |
	| True                          | BoolField          |
	| Warewolf Created Text Field 2 | MultilineTextField |
	| Warewolf Required Field 2     | RequiredField      |
	When the sharepoint create list item tool is executed
	Then the execution has "NO" error	
	And scenerio is clean

Scenario: Update all items in list with static data
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And I map the list update fields as
	| Field Name | Variable                             |
	| Name       | Updated From Warewolf                |
	| Title      | My Updated Warewolf Acceptance Test Item |
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                 |
	| 1 | Name       | Updated From Warewolf                    |
	| 2 | Title      | My Updated Warewolf Acceptance Test Item |
	And the debug output as 
	|                      |
	| [[Result]] = Success |
	
Scenario: Update all items in list with static data and filter
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And I map the list update fields as
	| Field Name | Variable                             |
	| Name       | Updated From Warewolf                |
	| Title      | My Updated Warewolf Acceptance Test Item |
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField   | Equals      | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                 | Search Type | Value | Require All Criteria To Match |
	| 1 | Name       | Updated From Warewolf                    |             |       |                               |
	| 2 | Title      | My Updated Warewolf Acceptance Test Item |             |       |                               |
	| 3 | IntField   |                                          | =           | 2     | Yes                           |
	And the debug output as 
	|                      |
	| [[Result]] = Success |

Scenario: Update all items in list with static data and filter contains
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And I map the list update fields as
	| Field Name | Variable                             |
	| Name       | Updated From Warewolf                |
	| Title      | My Updated Warewolf Acceptance Test Item |
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Name       | Contains    | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                 | Search Type | Value | Require All Criteria To Match |
	| 1 | Name       | Updated From Warewolf                    |             |       |                               |
	| 2 | Title      | My Updated Warewolf Acceptance Test Item |             |       |                               |
	| 3 | Name       |                                          | Contains    | 2     | Yes                           |
	And the debug output as 
	|                      |
	| [[Result]] = Success |
	
Scenario: Update all items in list with static data and filter contains returns multiple items
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And I map the list update fields as
	| Field Name | Variable                             |
	| Name       | Updated From Warewolf                |
	| Title      | My Updated Warewolf Acceptance Test Item |
	And search criteria as
	| Field Name | Search Type | Value    | From | To |
	| Name       | Contains    | Warewolf |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                 | Search Type | Value    | Require All Criteria To Match |
	| 1 | Name       | Updated From Warewolf                    |             |          |                               |
	| 2 | Title      | My Updated Warewolf Acceptance Test Item |             |          |                               |
	| 3 | Name      |                                          | Contains    | Warewolf | Yes                           |
	And the debug output as 
	|                      |
	| [[Result]] = Success |
	
Scenario: Update all items in list with static data and filter has variable
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And I map the list update fields as
	| Field Name | Variable                             |
	| Name       | Updated From Warewolf                |
	| Title      | My Updated Warewolf Acceptance Test Item |
	And search criteria as
	| Field Name | Search Type | Value         | From | To |
	| IntField   | Equals      | [[filterVal]] |      |    |
	And I have a variable "[[filterVal]]" with a value "2"
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                 | Search Type | Value         | Require All Criteria To Match |
	| 1 | Name       | Updated From Warewolf                    |             |               |                               |
	| 2 | Title      | My Updated Warewolf Acceptance Test Item |             |               |                               |
	| 3 | IntField   |                                          | =      | [[filterVal]] = 2 | Yes                           |
	And the debug output as 
	|                      |
	| [[Result]] = Success |
	
Scenario: Update all items in list with scalar data and filter has variable
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And I map the list update fields as
	| Field Name | Variable        |
	| Name       | [[updateName]]  |
	| Title      | [[updateTitle]] |
	And search criteria as
	| Field Name | Search Type | Value         | From | To |
	| IntField   | Equals      | [[filterVal]] |      |    |
	And I have a variable "[[filterVal]]" with a value "2"
	And I have a variable "[[updateTitle]]" with a value "Updated Title from Variable"
	And I have a variable "[[updateName]]" with a value "Updated Name from Variable"
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                      | Search Type | Value             | Require All Criteria To Match |
	| 1 | Name       | [[updateName]] = Updated Name from Variable   |             |                   |                               |
	| 2 | Title      | [[updateTitle]] = Updated Title from Variable |             |                   |                               |
	| 3 | IntField   |                                               | =           | [[filterVal]] = 2 | Yes                           |
	And the debug output as 
	|                      |
	| [[Result]] = Success |
	
Scenario: Update all items in list with recordset data
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Update" list
	And I map the list update fields as
	| Field Name | Variable        |
	| Name       | [[rec(*).name]]  |
	| Title      | [[rec(*).title]] |
	And I have a variable "[[rec(1).title]]" with a value "Updated Title from Variable 1"
	And I have a variable "[[rec(1).name]]" with a value "Updated Name from Variable 1"
	And I have a variable "[[rec(2).title]]" with a value "Updated Title from Variable 2"
	And I have a variable "[[rec(2).name]]" with a value "Updated Name from Variable 2"
	And I have result variable as "[[Result]]"
	When the sharepoint update list item tool is executed
	Then the value of "[[Result]]" equals "Success"
	And the execution has "NO" error
	And the debug inputs as
	| # | Field Name | Variable                                         |
	| 1 | Name       | [[rec(1).name]] = Updated Name from Variable 1   |
	|   |            | [[rec(2).name]] = Updated Name from Variable 2   |
	| 2 | Title      | [[rec(1).title]] = Updated Title from Variable 1 |
	|   |            | [[rec(2).title]] = Updated Title from Variable 2 |
	And the debug output as 
	|                      |
	| [[Result]] = Success |
