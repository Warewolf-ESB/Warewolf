Feature: Unique
	In order to find unique records in a recordset
	As a Warewolf user
	I want tool that will allow me 

Scenario: Find unique records in a  dataset
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And the unique execution has "NO" error
		
Scenario: Find unique records in a  dataset comma separated
	Given I have the following duplicated recordset
	| rs        | val |
	| rs().row  | 1   |
	| rs().row  | 2   |
	| rs().row  | 2   |
	| rs().row  | 3   |
	| rs().data | 1   |
	| rs().data | 2   |
	| rs().data | 2   |
	| rs().data | 3   |
	And I want to find unique in field "[[rs().row]],[[rs().data]]" with the return field "[[rs().row]]"
	And The result variable is "[[result().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
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
