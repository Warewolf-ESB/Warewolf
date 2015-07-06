Feature: DeleteItemFromList
	In order to delete an item from a Sharepoint List
	As a Warewolf User
	I want a tool that will allow be to provide criteria to find and item 
	  and map the number of found items to the result
	  and delete the found  items from the Sharepoint List

Background: Setup for sharepoint scenerio
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
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
	

Scenario: Delete Item from list with no criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug output as 
	|                |
	| [[Result]] = 2 |

Scenario: Delete Item from Sharepoint list with Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField   | Equals      | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField   | =           | 2     | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 1 |

Scenario: Delete Item from Sharepoint list with Greater Than criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name    | Search Type | Value | From | To |
	| CurrencyField | >           | 10    |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name    | Search Type | Value | Require All Criteria To Match |
	| 1 | CurrencyField | >           | 10    | Yes                           |
	And the debug output as 
	|                                             |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Greater Than Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField   | >=          | 1     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField   | >=          | 1     | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 2 |

Scenario: Delete Item from Sharepoint list with Less Than criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField   | <           | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField   | <           | 2     | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 1 |

Scenario: Delete Item from list with Less Than Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField   | <=          | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField   | <=          | 2     | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 2 |

Scenario: Delete Item from Sharepoint list with Not Equal criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| IntField   | <>          | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | IntField   | <>          | 2     | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 1 |

Scenario: Delete Item from list with Contains criteria
		Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value    | From | To |
	| Title      | Contains    | Warewolf |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value    | Require All Criteria To Match |
	| 1 | Title      | Contains    | Warewolf | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Begins With criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value    | From | To |
	| Title      | Begins With | Warewolf |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value    | Require All Criteria To Match |
	| 1 | Title      | Begins With | Warewolf | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Multiple criteria return multiple results
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title      | Contains    | o     |      |    |
	| IntField   | <=          | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains    | o     | Yes                           |
	| 2 | IntField   | <=          | 2     | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 2 |

Scenario: Delete Item from list with Multiple criteria return single results
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title      | Contains    | 1     |      |    |
	| IntField   | <=          | 2     |      |    |
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "1"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains    | 1     | Yes                           |
	| 2 | IntField   | <=          | 2     | Yes                           |
	And the debug output as 
	|                |
	| [[Result]] = 1 |

Scenario: Delete Item from list with Multiple criteria do not match all criteria
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title      | Contains    | 2     |      |    |
	| IntField   | <           | 2   |      |    |
	And do not require all criteria to match
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "2"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains    | 2     | No                           |
	| 2 | IntField   | <           | 2   | No                            |
	And the debug output as 
	|                |
	| [[Result]] = 2 |

	
Scenario: Delete Item from list with Multiple criteria match all criteria finds nothing
	Given I have a sharepoint source to "http://rsaklfsvrsharep/"
	And I select "AcceptanceTesting_Delete" list
	And search criteria as
	| Field Name | Search Type | Value | From | To |
	| Title      | Contains    | z     |      |    |
	| IntField   | >           | 200   |      |    |
	And do not require all criteria to match
	And I have result variable as "[[Result]]"
	When the sharepoint delete item from list tool is executed
	Then the value of "[[Result]]" equals "0"
	And the execution has "NO" error
	And the debug inputs as 
	| # | Field Name | Search Type | Value | Require All Criteria To Match |
	| 1 | Title      | Contains    | z     | No                            |
	| 2 | IntField   | >           | 200   | No                            |
	And the debug output as 
	|                |
	| [[Result]] = 0 |
	