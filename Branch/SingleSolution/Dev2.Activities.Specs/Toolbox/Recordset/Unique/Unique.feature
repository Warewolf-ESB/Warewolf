Feature: Unique
	In order to find unique records in a recordset
	As a Warewolf user
	I want tool that will allow me 

Scenario: Find unique records in a  dataset
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 10   |
	| rs().row | 20   |
	| rs().row | 20   |
	| rs().row | 30   |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| result       | unique |
	| result().unique | 10   |
	| result().unique | 20   |
	| result().unique | 30   |
	And the unique execution has "NO" error
		
Scenario: Find unique records in a  dataset comma separated
	Given I have the following duplicated recordset
	| rs        | val |
	| rs().row  | 10   |
	| rs().row  | 20   |
	| rs().row  | 20   |
	| rs().row  | 30   |
	| rs().data | 10   |
	| rs().data | 20   |
	| rs().data | 20   |
	| rs().data | 30   |
	And I want to find unique in field "[[rs().row]],[[rs().data]]" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	| rs().row | 10   |
	| rs().row | 20   |
	| rs().row | 30   |
	And the unique execution has "NO" error

Scenario: Find unique records in an empty dataset
	Given I have the following empty recordset
	| rs       | val |	
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	And the unique execution has "AN" error

Scenario: Find unique records in a  dataset and the in field is blank
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	And the unique execution has "AN" error
		
Scenario: Find unique records in a  dataset the return field is blank
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs().row]]" with the return field ""
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	And the unique execution has "AN" error

Scenario: Find unique records using a negative recordset index for In Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs(-1).row]]" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	And the unique execution has "AN" error

Scenario: Find unique records using a * for In Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs(*).row]]" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And the unique execution has "NO" error

Scenario: Find unique records using a negative recordset index for Return Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs(-1).row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	And the unique execution has "AN" error

Scenario: Find unique records using a * for Return Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs(*).row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And the unique execution has "NO" error
