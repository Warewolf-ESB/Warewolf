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

Scenario: Import data into Table with check contraint enabled
	Given I have this data
		| Col1 | Col2     | Col3                                 |
		| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
		| 1    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	And Check constraints is enabled
	When the tool is executed
	Then the new table will will have 0 of rows

Scenario: Import data into Table with keep identity disabled
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



Scenario: Import data into Table Batch size is 0
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Batch size is 0
	When the tool is executed
	Then  number of inserts is 1

Scenario: Import data into Table Batch size is 1
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Batch size is 1
	When the tool is executed
	Then  number of inserts is 3

Scenario: Import data into Table Batch size is 2
Given I have this data
	| Col1 | Col2     | Col3                                 |
	| 1    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |
	| 2    | TestData | b89416b9-5b24-4f95-bd11-25d9db8160a2 |
	| 3    | TestData | 279c690e-3304-47a0-8bde-5d3ca2520a34 |	
	And Batch size is 2
	When the tool is executed
	Then  number of inserts is 2

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
	

	#Not tested are :-			
		# Keep table lock
		# Use internal transaction