Feature: ForEach
	In order to loop through constructs
	As a Warewolf user
	I want to a tool that will allow me to execute other tools in an loop

Scenario: Execute a foreach for every record in a recordset	using an activity
	Given I there is a recordset in the datalist with this shape
	| rs              | value |
	| [[rs().row]]    | 1     |
	| [[rs().row]]    | 2     |
	| [[rs().row]]    | 3     |
	And I Map the input recordset "[[rs().row]]" to "[[test().data]]"
	And I Map the output recordset "[[test().data]]" to "[[res().data]]" 	
	And I have selected the foreach type as "InRecordset" and used "[[rs()]]"	
	And the underlying dropped activity is a(n) "Activity"
	When the foreach tool is executed
	Then the recordset "[[res().data]]" will have data as 
	| res            | data |
	| [[res().data]] | 1    |
	| [[res().data]] | 2    |
	| [[res().data]] | 3    |


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
	
