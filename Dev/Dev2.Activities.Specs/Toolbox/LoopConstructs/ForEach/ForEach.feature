Feature: ForEach
	In order to loop through constructs
	As a Warewolf user
	I want to a tool that will allow me to execute other tools in an loop

#Scenario: Execute a foreach for every record in a recordset	using an activity
#	Given I there is a recordset in the datalist with this shape
#	| rs              | value |
#	| [[rs().row]]    | 1     |
#	| [[rs().row]]    | 2     |
#	| [[rs().row]]    | 3     |
#	And I Map the input recordset "[[rs().row]]" to "[[test().data]]"
#	And I Map the output recordset "[[test().data]]" to "[[res().data]]" 	
#	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
#	And the underlying dropped activity is a(n) "Activity"
#	When the foreach tool is executed
#	Then the recordset "[[res().data]]" will have data as 
#	| res            | data |
#	| [[res().data]] | 1    |
#	| [[res().data]] | 2    |
#	| [[res().data]] | 3    |
#	And the foreach execution has "NO" error

Scenario: Execute a foreach for every record in a recordset	using an tool number of records is 3
	Given I there is a recordset in the datalist with this shape
	| rs              | value |
	| [[rs().row]]    | 1     |
	| [[rs().row]]    | 2     |
	| [[rs().row]]    | 3     |	
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 3 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach for every record in a recordset	using an tool number of records is 4
	Given I there is a recordset in the datalist with this shape
	| rs              | value |
	| [[rs().row]]    | 1     |
	| [[rs().row]]    | 2     |
	| [[rs().row]]    | 3     |	
	| [[rs().row]]    | 6     |	
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed	
	Then the foreach executes 4 times
	And the foreach execution has "NO" error
	
Scenario: Execute a foreach in range from 0 to 0	
	And I have selected the foreach type as "InRange" from 0 to 0
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 0 times
	And the foreach execution has "AN" error

Scenario: Execute a foreach in range from 1 to 5	
	And I have selected the foreach type as "InRange" from 1 to 5
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 5 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach in range from 9 to 10	
	And I have selected the foreach type as "InRange" from 9 to 10
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 2 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach in csv 1,2,3
	And I have selected the foreach type as "InCSV" as "1,2,3"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 3 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach in csv 2,4,6
	And I have selected the foreach type as "InCSV" as "2,4,6"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 3 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach in csv 2
	And I have selected the foreach type as "InCSV" as "2"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 1 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach number of execution is 0
	And I have selected the foreach type as "NumOfExecution" as "0"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 0 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach number of execution is 1
	And I have selected the foreach type as "NumOfExecution" as "1"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 1 times
	And the foreach execution has "NO" error

Scenario: Execute a foreach number of execution is 101
	And I have selected the foreach type as "NumOfExecution" as "101"
	And the underlying dropped activity is a(n) "Tool"
	When the foreach tool is executed
	Then the foreach executes 101 times
	And the foreach execution has "NO" error

#Foreach type (For both activities and workflows)
		#* in Range
			# Range 0 to 0 (no executions)
			# Range 0 to 3 (4 executions)
			# Range 9 to 10 (3 executions)
			# out of range test
		#* in CSV
			# CSV index 0
			# CSV index 1
			#  out of range test
		#No of Execution
			# N = 0
			# N = any number
		#* in Recordset