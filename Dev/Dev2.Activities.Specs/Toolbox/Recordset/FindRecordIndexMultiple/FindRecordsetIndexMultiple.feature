Feature: FindRecordsetIndexMultiple
	In order to search for pieces of data in a recordset
	As a Warewolf user
	I want a tool I can use to find an index 

Scenario: Find an index of data in a recordset with Starts With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

#Scenario: Find an index of data in a recordset with Is Between
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs().row | 1|
#	| rs().row | 15|
#	| rs().row | 20|	
#	| rs().row | 34|	
#	And search the recordset with type "Is Between" and criteria is "16" and "33"
#	When the find records index multiple tool is executed
#	Then the find records index multiple result should be 3
	
Scenario: Find an index of data in a recordset with Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |	
	| rs().row | d2FyZXdvbGY= |	
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset search type is Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Greater Than
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Greater Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Greater Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0
	
Scenario: Find an index of data in a recordset search type is Less Than
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Less Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Less Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0
	
Scenario: Find an index of data in a recordset search type is Not Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<>" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<>" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<>" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Greater Or Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Greater Or Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Greater Or Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Less Or Equal
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Less Or Equal multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Less Or Equal result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Starts With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Starts With multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Starts With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Ends With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Ends With multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Ends With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Ends With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Contains
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Contains multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Contains result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Contains" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Doesn't Contain
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Doesn't Contain multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Doesn't Contain result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Doesn't Contain" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Alphanumeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Base64" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Base64 multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Base64" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Base64 result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Base64" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

	Scenario: Find an index of data in a recordset search type is Is Date
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Date" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Email
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Email" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Numeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Regex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Regex multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Regex result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Regex" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Text
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Text" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is XML
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is XML" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Date
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Date" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Email
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Email" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Numeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Text
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Text" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not XML
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not XML" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0