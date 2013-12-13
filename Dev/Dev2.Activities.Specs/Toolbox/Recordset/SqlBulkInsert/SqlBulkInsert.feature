Feature: SqlBulkInsert
	In order to quickly insert large amounts of data in a sql server database
	As a Warewolf user
	I want a tool that performs this action

Scenario: Import data into table with check contraint disabled
	Given I have this data
		| Col1 | Col2     | Col3                                 |
		| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
		| 1    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	And Check constraints is disabled
	When the tool is executed
	Then the new table will have
		| Col1 | Col2     | Col3                                 |
		| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
		| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	And the sqlbulkinsert execution has "NO" error
    

Scenario: Import data into Table with check constraint enabled
#Col3 is a foreign key that does not exist in the primary key table.
	Given I have this data
		| Col1 | Col2     | Col3                                 |
		| 1    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	And Check constraints is enabled
	When the tool is executed
	Then the new table will will have 0 of rows
	And the sqlbulkinsert execution has "AN" error
	

Scenario: Import data into Table with keep identity disabled
#Given that the table is truncated i.e. seed is 1 and increment is 1
	Given I have this data
		| Col1 | Col2     | Col3                                 |
		| 4    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
		| 6    | TestData | bc7a9611-102e-4899-82b8-97ff1517d268 |
		| 8    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Keep identity is disabled
	When the tool is executed
	Then the new table will have
		| Col1 | Col2     | Col3                                 |
		| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
		| 2    | TestData | bc7a9611-102e-4899-82b8-97ff1517d268 |
		| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	And the sqlbulkinsert execution has "NO" error		

Scenario: Import data into Table with keep identity enabled
	Given I have this data
		| Col1 | Col2     | Col3                                 |
		| 4    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
		| 6    | TestData | bc7a9611-102e-4899-82b8-97ff1517d268 |
		| 8    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Keep identity is enabled
	When the tool is executed
	Then the new table will have
		| Col1 | Col2     | Col3                                 |
		| 4    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
		| 6    | TestData | bc7a9611-102e-4899-82b8-97ff1517d268 |
		| 8    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And the sqlbulkinsert execution has "NO" error	

Scenario: Import data into Table with skip blank rows disabled
#Note the second row is blank from the source data
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	|      |          |										 |
	| 2    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Skip rows is disabled
	When the tool is executed
	Then the new table will will have 0 of rows
	And the sqlbulkinsert execution has "AN" error

Scenario: Import data into Table with skip blank rows enabled
#Note the second row is blank from the source data
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	|      |          |										 |
	| 2    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Skip rows is enabled
	When the tool is executed
	Then the new table will will have 3 of rows		
	And the sqlbulkinsert execution has "NO" error

Scenario: Import data into Table with fire triggers disabled
#A trigger exists in the table [SqlBulkInsertSpecFlowTestTable] against the column [Col2] to add a default value of XXXXXXXX.
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    |          |	b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Fire triggers is disabled
	When the tool is executed
	Then the new table will have
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    |          |	b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And the sqlbulkinsert execution has "NO" error

Scenario: Import data into Table with fire triggers enabled
#A trigger exists in the table [SqlBulkInsertSpecFlowTestTable] against the column [Col2] to add a default value of XXXXXXXX.
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    |          |	b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Fire triggers is enabled
	When the tool is executed
	Then the new table will have
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | XXXXXXXX |	b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And the sqlbulkinsert execution has "NO" error

Scenario: Import data into Table Batch size is 0
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Batch size is 0
	When the tool is executed
	Then  number of inserts is 1
	And the sqlbulkinsert execution has "NO" error

Scenario: Import data into Table Batch size is 1
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Batch size is 1
	When the tool is executed
	Then  number of inserts is 3
	And the sqlbulkinsert execution has "NO" error

Scenario: Import data into Table Batch size is 2
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Batch size is 2
	When the tool is executed
	Then  number of inserts is 2
	And the sqlbulkinsert execution has "NO" error

Scenario: Import data into Table timeout after 3 second
#Note there is a trigger to wait for 2 seconds to simulate inserting large data
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Timeout in 3 seconds
	When the tool is executed
	Then  number of inserts is 1
	And the sqlbulkinsert execution has "NO" error

Scenario: Import data into Table timeout after 1 second
#Note there is a trigger to wait for 2 seconds to simulate inserting large data
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Timeout in 1 seconds
	When the tool is executed
	Then  number of inserts is 0
	And the sqlbulkinsert execution has "AN" error
	
Scenario: Import data into table with blank data
	Given I have this data
		| Col1 | Col2     | Col3                           |
	When the tool is executed
	Then the new table will have
		| Col1 | Col2     | Col3                           |
	And the sqlbulkinsert execution has "AN" error
    


	#Not tested are :-			
		# Keep table lock
		# Use internal transaction