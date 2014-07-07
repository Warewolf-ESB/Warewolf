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
	|               | unique |
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
	And the debug inputs as  
	|             |  | Return Fields |
	And the debug output as 
	|  |                    |
	|  | [[rec().unique]] = |

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
	|  | [[rec().unique]] = |
		
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
	|  | [[rec().unique]] = |
	
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
	|  | [[rec().unique]] = |

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
	|  |                     |
	|  | [[rec().unique]] = |

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

##This Test Scenario should be passed after the bug 11994 is fixed
#Scenario: Find unique records and assigning result in two variables
#	Given I have the following duplicated recordset
#	| rs       | val |
#	| rs().row | 10  |
#	| rs().row | 20  |
#	| rs().row | 20  |
#	| rs().row | 30  |
#	And I want to find unique in field "[[rs().row]]" with the return field "[[rs().row]]"
#	And The result variable is "[[a]],[[b]]"
#	When the unique tool is executed	
#	Then the unique result will be
#	| rec          | unique |
#	| rec().unique | 10     |
#	| rec().unique | 20     |
#	| rec().unique | 30     |
#	And the execution has "NO" error
#	And the debug inputs as  
#	| #           |                    | Return Fields  |
#	| In Field(s) | [[rs(4).row]] = 30 | [[rs().row]] = |	
#	And the debug output as 
#	| # |                  |
#	| 1 | [[a]] = 10,20,30 |
#	| 2 | [[b]] = 10,20,30 |
