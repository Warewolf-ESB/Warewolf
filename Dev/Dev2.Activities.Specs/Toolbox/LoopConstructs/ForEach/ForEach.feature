Feature: ForEach
	In order to loop through constructs
	As a Warewolf user
	I want to a tool that will allow me to execute other tools in an loop

Scenario: Execute a foreach over a tool using a recordset with 3 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |	
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 3 times
	And the execution has "NO" error
	And the debug inputs as
    |                | Recordset           |
    | * in Recordset | [[rs(1).field]] = 1 |
    |                | [[rs(2).field]] = 2 |
    |                | [[rs(3).field]] = 3 |  

Scenario: Execute a foreach over a tool using a recordset with 4 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	| [[rs().field]] | 6     |	
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed	
	Then the foreach executes 4 times
	And the execution has "NO" error
	And the debug inputs as
	|                | Recordset           |
	| * in Recordset | [[rs(1).field]] = 1 |
	|                | [[rs(2).field]] = 2 |
	|                | [[rs(3).field]] = 3 |
	|                | [[rs(4).field]] = 6 |
	
#This Scenarios should pass after the bug 12021 is fixed
#Scenario: Execute a foreach over a tool using a recordset with invalid
#	Given There is a recordset in the datalist with this shape
#	| rs             | value |
#	| [[rs().field]] | 1     |
#	| [[rs().field]] | 2     |
#	| [[rs().field]] | 3     |
#	| [[rs().field]] | 6     |	
#	And I have selected the foreach type as "InRecordset" and used "[[rs()]]+1"	
#	And the underlying dropped activity is a(n) "Tool"
#	When the foreach tool is executed	
#	Then the foreach executes 4 times
#	And the execution has "AN" error
#	And the debug inputs as
#	|                | Recordset           |
#
#Scenario: Execute a foreach using a recordset with invalid
#	Given There is a recordset in the datalist with this shape
#	| rs             | value |
#	| [[rs().field]] | 1     |
#	| [[rs().field]] | 2     |
#	| [[rs().field]] | 3     |
#	| [[rs().field]] | 6     |	
#	And I have selected the foreach type as "InRecordset" and used "[[rs().a]]"	
#	And the underlying dropped activity is a(n) "Tool"
#	When the foreach tool is executed	
#	Then the foreach executes 4 times
#	And the execution has "AN" error
#	And the debug inputs as
#	|                | Recordset           |
#
#Scenario: Execute a foreach using a recordset with special character
#	Given There is a recordset in the datalist with this shape
#	| rs             | value |
#	| [[rs().field]] | 1     |
#	| [[rs().field]] | 2     |
#	| [[rs().field]] | 3     |
#	| [[rs().field]] | 6     |	
#	And I have selected the foreach type as "InRecordset" and used "[[rs()]]#$%3"	
#	And the underlying dropped activity is a(n) "Tool"
#	When the foreach tool is executed	
#	Then the foreach executes 4 times
#	And the execution has "AN" error
#	And the debug inputs as
#	|                | Recordset           |

Scenario: Execute a foreach over a tool for range 0 to 0
	And I have selected the foreach type as "InRange" from 0 to 0
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 0 times
	And the execution has "AN" error
	And the debug inputs as
	|            | From | To |
	| * in Range | 0    | 0  |	

Scenario: Execute a foreach over a tool for range 1 to 5
	And I have selected the foreach type as "InRange" from 1 to 5
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 5 times
	And the execution has "NO" error
	And the debug inputs as
	|            | From | To |
	| * in Range | 1    | 5  |

Scenario: Execute a foreach over a tool for range 9 to 10
	And I have selected the foreach type as "InRange" from 9 to 10
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 2 times
	And the execution has "NO" error
	And the debug inputs as
	|           | From | To |
	| * in Range | 9    | 10 |

Scenario: Execute a foreach over a tool with Csv Indexes 1,2,3
	And I have selected the foreach type as "InCSV" as "1,2,3"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	|         | Csv Indexes |
	| * in CSV | 1,2,3       |

Scenario: Execute a foreach over a tool with Csv Indexes 2,4,6
	And I have selected the foreach type as "InCSV" as "2,4,6"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 3 times
	And the execution has "NO" error
	And the debug inputs as
	|         | Csv Indexes |
	| * in CSV | 2,4,6       |

Scenario: Execute a foreach over a tool with csv index 2
	And I have selected the foreach type as "InCSV" as "2"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 1 times
	And the execution has "NO" error
    And the debug inputs as
	|          | Csv Indexes |
	| * in CSV | 2           |

Scenario: Execute a foreach over a tool with number of executions equals 0
	And I have selected the foreach type as "NumOfExecution" as "0"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 0 times
	And the execution has "NO" error
	And the debug inputs as
	|                 | Number |
	| No. of Executes | 0      |

Scenario: Execute a foreach over a tool with number of executions equals 1
	And I have selected the foreach type as "NumOfExecution" as "1"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 1 times		
	And the execution has "NO" error
	And the debug inputs as
	|                 | Number |
	| No. of Executes | 1      |

Scenario: Execute a foreach over a tool with number of executions equals 8
	And I have selected the foreach type as "NumOfExecution" as "8"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 8 times
	And the execution has "NO" error
	And the debug inputs as	
	|                 | Number |
	| No. of Executes | 8      |

Scenario: Execute a foreach over an activity using a recordset with 3 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |	
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| 1     |
	| 2     |
	| 3     |	
	And the execution has "NO" error
	And the debug inputs as
    |                | Recordset           |
    | * in Recordset | [[rs(1).field]] = 1 |
    |                | [[rs(2).field]] = 2 |
    |                | [[rs(3).field]] = 3 |

Scenario: Execute a foreach over an activity using a recordset with 4 rows
	Given There is a recordset in the datalist with this shape
	| rs             | value |
	| [[rs().field]] | 1     |
	| [[rs().field]] | 2     |
	| [[rs().field]] | 3     |
	| [[rs().field]] | 6     |	
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed	
	Then The mapping uses the following indexes
	| index |
	| 1     |
	| 2     |
	| 3     |	
	| 4     |
	And the execution has "NO" error
	And the debug inputs as
    |                | Recordset           |
    | * in Recordset | [[rs(1).field]] = 1 |
    |                | [[rs(2).field]] = 2 |
    |                | [[rs(3).field]] = 3 |
    |                | [[rs(4).field]] = 6 |
	
Scenario: Execute a foreach over an activity for range 0 to 0
	And I have selected the foreach type as "InRange" from 0 to 0
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then the foreach executes 0 times
	And the execution has "AN" error
	And the debug inputs as
	|           | From | To |
	| * in Range | 0    | 0  |

Scenario: Execute a foreach over an activity for range 1 to 5
	And I have selected the foreach type as "InRange" from 1 to 5
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| 1     |
	| 2     |
	| 3     |	
	| 4     |	
	| 5     |	
	Then the execution has "NO" error
	And the debug inputs as
	|            | From | To |
	| * in Range | 1    | 5  |

Scenario: Execute a foreach over an activity for range 9 to 10
	And I have selected the foreach type as "InRange" from 9 to 10
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| 9     |
	| 10    |
	And the execution has "NO" error
	And the debug inputs as
	|           | From | To |
	| * in Range | 9    | 10 |

Scenario: Execute a foreach over an activity with Csv Indexes 1,2,3
	And I have selected the foreach type as "InCSV" as "1,2,3"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| 1     |
	| 2     |
	| 3     |
	| 4     |
	| 5     |
	And the execution has "NO" error
	And the debug inputs as
	|         | Csv Indexes |
	| * in CSV | 1,2,3       |

Scenario: Execute a foreach over an activity with Csv Indexes 2,4,6
	And I have selected the foreach type as "InCSV" as "2,4,6"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| 2     |
	| 4     |
	| 6     |	
	And the execution has "NO" error
	And the debug inputs as
	|         | Csv Indexes |
	| * in CSV | 2,4,6       |

Scenario: Execute a foreach over an activity with csv index 2
	And I have selected the foreach type as "InCSV" as "2"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| 2     |
	And the execution has "NO" error
	And the debug inputs as
	|          | Csv Indexes |
	| * in CSV | 2           |

Scenario: Execute a foreach over an activity with number of executions equals 0
	And I have selected the foreach type as "NumOfExecution" as "0"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then the foreach executes 0 times
	And the execution has "NO" error
	And the debug inputs as
	|                 | Number |
	| No. of Executes | 0      |

Scenario: Execute a foreach over an activity with number of executions equals 1
	And I have selected the foreach type as "NumOfExecution" as "1"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| *     |	
	And the execution has "NO" error
	And the debug inputs as
	|                 | Number |
	| No. of Executes | 1      |

Scenario: Execute a foreach over an activity with number of executions equals 8
	And I have selected the foreach type as "NumOfExecution" as "8"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then the execution has "NO" error
	And The mapping uses the following indexes
	| index |
	| *     |
	| *     |
	| *     |
	| *     |
	| *     |
	| *     |
	| *     |
	| *     |			
	And the debug inputs as
	|                 | Number |
	| No. of Executes | 8      |

#Scenario: Execute a foreach over an activity with number of executions equals +1 invalid
#	And I have selected the foreach type as "NumOfExecution" as "+1"
#	And the underlying dropped activity is a(n) "Activity"
#	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
#	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
#	When the foreach tool is executed
#	Then The mapping uses the following indexes
#	| index |
#	| *     |	
#	And the execution has "AN" error
#	And the debug inputs as
#	|                 | Number |

#Scenario: Execute a foreach over an activity with number of executions equals +1 invalid
#	And I have selected the foreach type as "NumOfExecution" as "+1"
#	And the underlying dropped activity is a(n) "Activity"
#	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
#	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
#	When the foreach tool is executed
#	Then The mapping uses the following indexes
#	| index |
#	| *     |	
#	And the execution has "AN" error
#	And the debug inputs as
#	|                 | Number |     

Scenario: Execute a foreach with number of executions equals @#$1 invalid
	And I have selected the foreach type as "NumOfExecution" as "+1"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| *     |	
	And the execution has "AN" error
	And the debug inputs as
	|                 | Number |

Scenario: Execute a foreach with number of executions as recordset with star
	And I have selected the foreach type as "NumOfExecution" as "[[rec(*).a]]"
	And the underlying dropped activity is a(n) "Activity"
	And I Map the input recordset "[[rs(*).field]]" to "[[test(*).data]]"
	And I Map the output recordset "[[test(*).data]]" to "[[res(*).data]]" 	
	When the foreach tool is executed
	Then The mapping uses the following indexes
	| index |
	| *     |	
	And the execution has "AN" error
	And the debug inputs as
	|                 | Number |   

#Scenario: Execute a foreach using In Recordset with incorrect recordset
#	Given There is a recordset in the datalist with this shape
#	| rs             | value |
#	| [[rs().field]] | 1     |
#	| [[rs().field]] | 2     |
#	| [[rs().field]] | 3     |
#	| [[rs().field]] | 6     |	
#	And I have selected the foreach type as "InRecordset" and used "[[rs()]%^&]"	
#	And the underlying dropped activity is a(n) "Tool"
#	When the foreach tool is executed	
#	Then the foreach executes 4 times
#	And the execution has "AN" error
#	And the debug inputs as
#	|                | Recordset           |
#	| * in Recordset | [[rs(1).field]] = 1 |
#	|                | [[rs(2).field]] = 2 |
#	|                | [[rs(3).field]] = 3 |
#	|                | [[rs(4).field]] = 6 |









