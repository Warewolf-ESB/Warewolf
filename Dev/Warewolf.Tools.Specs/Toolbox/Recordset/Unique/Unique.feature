@Recordset
Feature: Unique
	In order to find unique records in a recordset
	As a Warewolf user
	I want tool that will allow me 

Scenario: Find unique records in a recordset
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 10  |
	| rs().row | 20  |
	| rs().row | 20  |
	| rs().row | 30  |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs().row]]"
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	|              | unique |
	| rec().unique | 10     |
	| rec().unique | 20     |
	| rec().unique | 30     |
	And the execution has "NO" error
	And the debug inputs as  
	| #           |                    | Return Fields  |
	| In Field(s) | [[rs(4).row]] = 30 | [[rs().row]] = |	
	And the debug output as 
	| # |                        |
	| 1 | [[rec(1).unique]] = 10 |
	|   | [[rec(2).unique]] = 20 |
	|   | [[rec(3).unique]] = 30 |

Scenario: Find unique records in an empty recordset
	Given I have the following empty recordset
	| rs       | val |	
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs().row]]"
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error


Scenario: Find unique records in a recordset and the in field is blank
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "" with the return field "[[rs().row]]"
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error
	And the debug inputs as  
	| #           |  | Return Fields  |
	| In Field(s) |  | [[rs().row]] = |
	And the debug output as 
	|  |                     |
	|  | [[rec(*).unique]] = |
		
Scenario: Find unique records in a recordset the return field is blank
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs().row]]" with the return field ""
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error
	And the debug inputs as  
	| #           |                   | Return Fields |
	| In Field(s) | [[rs(4).row]] = 3 | ""            |
	And the debug output as 
	|  |                     |
	|  | [[rec(*).unique]] = |
	
Scenario: Find unique records using a negative recordset index for In Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs(-1).row]]" with the return field "[[rs().row]]"
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error
	And the debug inputs as  
	| #           |                  | Return Fields   |
	| In Field(s) | [[rs(-1).row]] = |                 |
	|             |                  | [[rs().row]]  = |
	And the debug output as 
	|  |                    |
	|  | [[rec(*).unique]] = |

Scenario: Find unique records using a * for In Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs(*).row]]" with the return field "[[rs().row]]"
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rec          | unique |
	| rec().unique | 1      |
	| rec().unique | 2      |
	| rec().unique | 3      |
	And the execution has "NO" error
	And the debug inputs as  
	| #           |                   | Return Fields  |
	| In Field(s) | [[rs(1).row]] = 1 |                |
	|             | [[rs(2).row]] = 2 |                |
	|             | [[rs(3).row]] = 2 |                |
	|             | [[rs(4).row]] = 3 |                |
	|             |                   | [[rs().row]] = |
	And the debug output as 
	| # |                        |
	| 1 | [[rec(1).unique]] = 1 |
	|   | [[rec(2).unique]] = 2 |
	|   | [[rec(3).unique]] = 3 |

Scenario: Find unique records using a negative recordset index for Return Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs(-1).row]]"
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error	
	And the debug inputs as  
	| #           |                   | Return Fields    |
	| In Field(s) | [[rs(4).row]] = 3 | [[rs(-1).row]] = |	
	And the debug output as 
	|  |                    |
	|  | [[rec(*).unique]] = |

Scenario: Find unique records using a * for Return Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs(*).row]]"
	And The result variable is "[[rec().unique]]"
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	| rec().unique | 1   |
	| rec().unique | 2   |
	| rec().unique | 3   |
	And the execution has "NO" error
	And the debug inputs as  
	| #           |                   | Return Fields   |
	| In Field(s) | [[rs(4).row]] = 3 | [[rs(*).row]] = |	
	And the debug output as 
	| # |                        |
	| 1 | [[rec(1).unique]] = 1 |
	|   | [[rec(2).unique]] = 2 |
	|   | [[rec(3).unique]] = 3 |

Scenario: Executing Unique record tool with empty In Fields
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "" with the return field "[[rs(*).row]]"
	And The result variable is ""
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error	
	And the debug inputs as  
	| #           |  | Return Fields   |
	| In Field(s) |  | [[rs(*).row]] = |
	And the debug output as 
	|  |  |


Scenario: Executing Unique record tool with empty In Return and Result Field
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 2   |
	| rs().row | 3   |
	And I want to find unique in field "[[rs(*).row]]" with the return field ""
	And The result variable is ""
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error	
	And the debug inputs as  
	| #           |                   | Return Fields |
	| In Field(s) | [[rs(1).row]] = 1 |               |
	|             | [[rs(2).row]] = 2 |               |
	|             | [[rs(3).row]] = 2 |               |
	|             | [[rs(4).row]] = 3 | ""            |	
	And the debug output as 
	|  |  |

Scenario: Find unique records and assigning result to scalar must error
	Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 10  |
	| rs().row | 20  |
	| rs().row | 20  |
	| rs().row | 30  |
	And I want to find unique in field "[[rs().row]]" with the return field "[[rs().row]]"
	And The result variable is "[[a]]"
	When the unique tool is executed	
	Then the execution has "AN" error
	And the debug inputs as  
	| #           |                    | Return Fields  |
	| In Field(s) | [[rs(4).row]] = 30 | [[rs().row]] = |	

Scenario Outline: Invalid expressions
Given I have the following duplicated recordset
	| rs       | val |
	| rs().row | 10  |
	| rs().row | 20  |
	| rs().row | 20  |
	| rs().row | 30  |
	And I want to find unique in field "<InField>" with the return field "<Return>"
	And The result variable is "<Result>" equals "<value>"
	When the unique tool is executed	
	Then the unique result will be
	| rec       | unique |
	And the execution has "AN" error	
	And the debug inputs as  
	| # | InField   | Return   | Result   | Value   |
	| 1 | <InField> | <Return> | <Result> | <value> |
	Examples: 
	| InField      | Return       | Result                      | value                     |
	| asda         | [[rs().row]] | [[var]]                     | Error : scalar in unique  |
	| [[c]]        | [[rs().row]] | [[var]]                     | Error : scalar in unique  |
	|              | [[rs().row]] | [[var]]                     | Error : Invalid in Fields |
	| 99           | [[rs().row]] | [[rec(1).a]]                | Error : scalar in unique  |
	| [[v]]        | [[rs().row]] | [[rec([[int]]).a]], [[int]] | Error : scalar in unique  |
	| [[rs().row]] | [[v]]        | [[rec(1).a]]                | Error : scalar in unique  |
	| [[rs().row]] | 51           | [[rec(1).a]]                | Error : scalar in unique  |
	| [[rs().row]] | adas         | [[rec(1).a]]                | Error : scalar in unique  |

Scenario: Executing Unique record tool with NULL recordset
	Given I have the following duplicated recordset
	| rs       | val  |
	| rs().row | NULL |
	| rs().val | NULL |
	And I want to find unique in field "" with the return field "[[rs(*).row]]"
	And The result variable is "[[rs().val]]"
	When the unique tool is executed	
	Then the execution has "AN" error	


Scenario: Executing Unique record tool with non existent recordset
	Given I want to find unique in field "" with the return field "[[rs(*).row]]"
	And The result variable is "[[rs().val]]"
	When the unique tool is executed	
	Then the execution has "AN" error	
