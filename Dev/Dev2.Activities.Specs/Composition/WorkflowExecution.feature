Feature: WorkflowExecution
	In order to execute a workflow
	As a Warewolf user
	I want to be able to build workflows and execute them against the server
	 
Background: Setup for workflow execution
			Given Debug events are reset
			And Debug states are cleared

@WorkflowExecution
Scenario: Workflow with multiple tools executing against the server
	  Given I have a workflow "WorkflowWithAssignAndCount"
	  And "WorkflowWithAssignAndCount" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | yes   |
	  | [[rec().a]] | no    |
	  And "WorkflowWithAssignAndCount" contains Count Record "CountRec" on "[[rec()]]" into "[[count]]"
	  When "WorkflowWithAssignAndCount" is executed
	  Then the workflow execution has "NO" error
	  And the "Rec To Convert" in WorkFlow "WorkflowWithAssignAndCount" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | yes       |
	  | 2 | [[rec().a]] = | no        |
	  And the "Rec To Convert" in Workflow "WorkflowWithAssignAndCount" debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
	  And the "CountRec" in WorkFlow "WorkflowWithAssignAndCount" debug inputs as
	  | Recordset            |
	  | [[rec(1).a]] = yes |
	  | [[rec(2).a]] = no |
	  And the "CountRec" in Workflow "WorkflowWithAssignAndCount" debug outputs as    
	  |               |
	  | [[count]] = 2 |

@WorkflowExecution
Scenario: Workflow with ForEach InRecordset Not entered
Given I have a workflow "WFWithForEachRecordsetNotentered"
And "WFWithForEachRecordsetNotentered" contains a Foreach "ForEachTest1" as "InRecordset" executions ""
When "WFWithForEachRecordsetNotentered" is executed
Then the workflow execution has "AN" error
And Workflow "WFWithForEachRecordsetNotentered" has errors
		| Error                                     |
		| The Recordset Field is Required           |
		| Cannot execute a For Each with no content |
	
@WorkflowExecution
Scenario: Workflow with ForEach InRange Not entered
	Given I have a workflow "WFWithForEachInRangeNotentered"
	And "WFWithForEachInRangeNotentered" contains a Foreach "ForEachTest1" as "InRange" executions ""
	When "WFWithForEachInRangeNotentered" is executed
	Then the workflow execution has "AN" error
	And Workflow "WFWithForEachInRangeNotentered" has errors
			| Error                                     |
			| The FROM field is Required                |
			| Cannot execute a For Each with no content |				

@WorkflowExecution
Scenario: Workflow with ForEach NumberOfExecutes Not entered
	Given I have a workflow "WFWithForEachNumberOfExecutesNotentered"
	And "WFWithForEachNumberOfExecutesNotentered" contains a Foreach "ForEachTest1" as "NumOfExecution" executions ""
	When "WFWithForEachNumberOfExecutesNotentered" is executed
	Then the workflow execution has "AN" error
	And Workflow "WFWithForEachNumberOfExecutesNotentered" has errors
			| Error                                                     |
			| Number of executes must be a whole number from 1 onwards. |
			| Cannot execute a For Each with no content                 |

@WorkflowExecution
Scenario: Workflow with ForEach InCsv Not entered
	Given I have a workflow "WFWithForEachInCsvNotentered"
	And "WFWithForEachInCsvNotentered" contains a Foreach "ForEachTest1" as "InCSV" executions ""
	When "WFWithForEachInCsvNotentered" is executed
	Then the workflow execution has "AN" error
	And Workflow "WFWithForEachInCsvNotentered" has errors
			| Error                                     |
			| The CSV Field is Required               |
			| Cannot execute a For Each with no content |
	          
@WorkflowExecution
Scenario: Workflow with ForEach which contains assign
      Given I have a workflow "WFWithForEachContainingAssign"
	  And "WFWithForEachContainingAssign" contains an Assign "Rec To Convert" as
	    | variable    | value |
	    | [[Warewolf]] | bob   |
	  And "WFWithForEachContainingAssign" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2"
	  And "ForEachTest" contains an Assign "MyAssign" as
	    | variable    | value |
	    | [[rec().a]] | Test  |
      When "WFWithForEachContainingAssign" is executed
	  Then the workflow execution has "NO" error
	  And the "ForEachTest" in WorkFlow "WFWithForEachContainingAssign" debug inputs as 
	    |                 | Number |
	    | No. of Executes | 2      |
      And the "ForEachTest" in WorkFlow "WFWithForEachContainingAssign" has  "2" nested children 
	  And the "MyAssign" in step 1 for "ForEachTest" debug inputs as
	    | # | Variable      | New Value |
	    | 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 1 for "ForEachTest" debug outputs as
		| # |                     |
		| 1 | [[rec(1).a]] = Test |
	  And the "MyAssign" in step 2 for "ForEachTest" debug inputs as
		| # | Variable      | New Value |
		| 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 2 for "ForEachTest" debug outputs as
		| # |                     |
		| 1 | [[rec(2).a]] = Test |
		
@WorkflowExecution
Scenario: Gather System Info returns values
	Given I have a workflow "WorkflowWithGatherSystemInfo"
	And "WorkflowWithGatherSystemInfo" contains Gather System Info "System info" as
	| Variable                   | Selected                             |
	| [[ComputerName]]           | Computer Name                        |
	| [[OperatingSystemVersion]] | Operating System Version             |
	| [[VirtualMemoryAvailable]] | Virtual Memory Available (MB)        |
	| [[VirtualMemoryTotal]]     | Virtual Memory Total (MB)            |
	| [[MacAddress]]             | MAC Addresses                        |
	| [[GateWayAddress]]         | Defaut Gateway Addresses             |
	| [[DNSAddress]]             | DNS Server Addresses                 |
	| [[IPv4Address]]            | IPv4 Addresses                       |
	| [[IPv6Address]]            | IPv6 Addresses                       |
	| [[WarewolfMemory]]         | Warewolf Memory Usage                |
	| [[WarewolfCPU]]            | Warewolf Total CPU Usage (All Cores) |
	| [[WarewolfServerVersion]]  | Warewolf Server Version              |
	 When "WorkflowWithGatherSystemInfo" is executed
	  Then the workflow execution has "NO" error
	  And the "System info" in WorkFlow "WorkflowWithGatherSystemInfo" debug inputs as
	  | #  |                              |                                      |
	  | 1  | [[ComputerName]] =           | Computer Name                        |
	  | 2  | [[OperatingSystemVersion]] = | Operating System Version             |
	  | 3  | [[VirtualMemoryAvailable]] = | Virtual Memory Available (MB)        |
	  | 4  | [[VirtualMemoryTotal]] =     | Virtual Memory Total (MB)            |
	  | 5  | [[MacAddress]] =             | MAC Addresses                        |
	  | 6  | [[GateWayAddress]] =         | Defaut Gateway Addresses             |
	  | 7  | [[DNSAddress]] =             | DNS Server Addresses                 |
	  | 8  | [[IPv4Address]] =            | IPv4 Addresses                       |
	  | 9  | [[IPv6Address]] =            | IPv6 Addresses                       |
	  | 10 | [[WarewolfMemory]] =         | Warewolf Memory Usage                |
	  | 11 | [[WarewolfCPU]] =            | Warewolf Total CPU Usage (All Cores) |
	  | 12 | [[WarewolfServerVersion]] =  | Warewolf Server Version              |
	  And the "System info" in Workflow "WorkflowWithGatherSystemInfo" debug outputs as
	  | #  |                                     |
	  | 1  | [[ComputerName]] = String           |
	  | 2  | [[OperatingSystemVersion]] = String |
	  | 3  | [[VirtualMemoryAvailable]] = String |
	  | 4  | [[VirtualMemoryTotal]] = String     |
	  | 5  | [[MacAddress]] = String             |
	  | 6  | [[GateWayAddress]] = String         |
	  | 7  | [[DNSAddress]] = String             |
	  | 8  | [[IPv4Address]] = String            |
	  | 9  | [[IPv6Address]] = String            |
	  | 10 | [[WarewolfMemory]] = String         |
	  | 11 | [[WarewolfCPU]] = String            |
	  | 12 | [[WarewolfServerVersion]] = String  |


@WorkflowExecution
Scenario: Workflow with ForEach which contains Sequence
      Given I have a workflow "WorkflowWithForEachContainingSeq"
	  And "WorkflowWithForEachContainingSeq" contains an Assign "RecVal" as
	  | variable     | value |
	  | [[rec(1).a]] | 123   |
	  | [[rec(1).b]] | 456   |
	  And "WorkflowWithForEachContainingSeq" contains a Foreach "ForEachTest1" as "NumOfExecution" executions "2"
	  And "ForEachTest1" contains a Sequence "Seq1" as
	  And 'Seq1' in "ForEachTest1" contains Data Merge "Data Merge" into "[[rec(1).c]]" as
	  | Variable     | Type | Using | Padding | Alignment |
	  | [[rec(1).a]] | None |       |         | Left      |
	  | [[rec(1).b]] | None |       |         | Left      |
	   And 'Seq1' in "ForEachTest1" contains Gather System Info "System info" as
	  | Variable     | Selected    |
	  | [[rec(1).d]] | Date & Time |
	  When "WorkflowWithForEachContainingSeq" is executed
	  Then the workflow execution has "NO" error
	  And the "RecVal" in WorkFlow "WorkflowWithForEachContainingSeq" debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | 123       |
	  | 2 | [[rec(1).b]] = | 456       |
	  And the "RecVal" in Workflow "WorkflowWithForEachContainingSeq" debug outputs as 
	  | # |                      |
	  | 1 | [[rec(1).a]]  =  123 |
	  | 2 | [[rec(1).b]]  =  456 |
	   And the "ForEachTest1" in WorkFlow "WorkflowWithForEachContainingSeq" debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
      And the "ForEachTest1" in WorkFlow "WorkflowWithForEachContainingSeq" has  "2" nested children 
	  And the "Data Merge" in 'Seq1' in step 1 for "ForEachTest1" debug inputs as
	  | # |                    | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 123 | None | ""    | ""  | Left  |
	  | 2 | [[rec(1).b]] = 456 | None | ""    | ""  | Left  |
	   And the "Data Merge" in 'Seq1' in step 1 for "ForEachTest1" debug outputs as
	  |                       |
	  | [[rec(1).c]] = 123456 |
	  And the "System info" in 'Seq1' in step 1 for "ForEachTest1" debug inputs as
	  | # |                |             |
	  | 1 | [[rec(1).d]] = | Date & Time |
	  And the "System info" in 'Seq1' in step 1 for "ForEachTest1" debug outputs as
	  | # |                       |
	  | 1 | [[rec(1).d]] = String | 
	  And the "Data Merge" in 'Seq1' in step 2 for "ForEachTest1" debug inputs as
	  | # |                    | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 123 | None | ""    | ""  | Left  |
	  | 2 | [[rec(1).b]] = 456 | None | ""    | ""  | Left  |
	   And the "Data Merge" in 'Seq1' in step 2 for "ForEachTest1" debug outputs as
	  |                       |
	  | [[rec(1).c]] = 123456 |
	  And the "System info" in 'Seq1' in step 2 for "ForEachTest1" debug inputs as
	  | # |                |             |
	  | 1 | [[rec(1).d]] = | Date & Time |
	  And the "System info" in 'Seq1' in step 2 for "ForEachTest1" debug outputs as
	  | # |                       |
	  | 1 | [[rec(1).d]] = String |	


@WorkflowExecution
Scenario: Executing ForEach in Rec with star which contains Sequence
      Given I have a workflow "WorkFWithForEachwithRecContainingSequence"
	  And "WorkFWithForEachwithRecContainingSequence" contains an Assign "RecVal" as
	  | variable     | value    |
	  | [[rec(1).a]] | 123      |
	  | [[rec(1).b]] | 456      |
	  | [[rec(2).a]] | Test     |
	  | [[rec(2).b]] | Warewolf |
	  And "WorkFWithForEachwithRecContainingSequence" contains a Foreach "ForEachTest1" as "InRecordset" executions "[[rec(*)]]"
	  And "ForEachTest1" contains a Sequence "Seq1" as
	  And 'Seq1' in "ForEachTest1" contains Data Merge "Data Merge" into "[[rec(*).c]]" as
	  | Variable     | Type | Using | Padding | Alignment |
	  | [[rec(*).a]] | None |       |         | Left      |
	  | [[rec(*).b]] | None |       |         | Left      |
	  And 'Seq1' in "ForEachTest1" contains Gather System Info "System info" as
	  | Variable     | Selected    |
	  | [[rec(*).d]] | Date & Time |
	  When "WorkFWithForEachwithRecContainingSequence" is executed
	  Then the workflow execution has "NO" error
	  And the "RecVal" in WorkFlow "WorkFWithForEachwithRecContainingSequence" debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | 123       |
	  | 2 | [[rec(1).b]] = | 456       |
	  | 3 | [[rec(2).a]] = | Test      |
	  | 4 | [[rec(2).b]] = | Warewolf  |
	  And the "RecVal" in Workflow "WorkFWithForEachwithRecContainingSequence" debug outputs as 
	  | # |                          |
	  | 1 | [[rec(1).a]]  =  123     |
	  | 2 | [[rec(1).b]]  =  456     |
	  | 3 | [[rec(2).a]] =  Test     |
	  | 4 | [[rec(2).b]] =  Warewolf |
	  And the "ForEachTest1" in WorkFlow "WorkFWithForEachwithRecContainingSequence" debug inputs as 
	  |                | Recordset      |
	  | * in Recordset |                |
	  |                | [[rec(1)]] = |
	  |                | [[rec(2)]] = |
      And the "ForEachTest1" in WorkFlow "WorkFWithForEachwithRecContainingSequence" has  "2" nested children
	  And the "Data Merge" in 'Seq1' in step 1 for "ForEachTest1" debug inputs as
	  | # |                    | With | Using | Pad | Align |
	  | 1 | [[rec(1).a]] = 123 | None | ""    | ""  | Left  |
	  | 2 | [[rec(1).b]] = 456 | None | ""    | ""  | Left  |
	  And the "Data Merge" in 'Seq1' in step 1 for "ForEachTest1" debug outputs as
	  |                       |
	  | [[rec(1).c]] = 123456 |
       And the "System info" in 'Seq1' in step 1 for "ForEachTest1" debug inputs as
	  | # |                |             |
	  | 1 | [[rec(1).d]] = | Date & Time |
	   And the "System info" in 'Seq1' in step 1 for "ForEachTest1" debug outputs as
	  | # |                       |
	  | 1 | [[rec(1).d]] = String |
	  And the "Data Merge" in 'Seq1' in step 2 for "ForEachTest1" debug inputs as
	  | # |                         | With | Using | Pad | Align |
	  | 1 | [[rec(2).a]] = Test     | None | ""    | ""  | Left  |
	  | 2 | [[rec(2).b]] = Warewolf | None | ""    | ""  | Left  |
	  And the "Data Merge" in 'Seq1' in step 2 for "ForEachTest1" debug outputs as
	  |                             |
	  | [[rec(2).c]] = TestWarewolf |
      And the "System info" in 'Seq1' in step 2 for "ForEachTest1" debug inputs as
	  | # |                |             |
	  | 1 | [[rec(2).d]] = | Date & Time |
	   And the "System info" in 'Seq1' in step 2 for "ForEachTest1" debug outputs as
	  | # |                       |
	  | 1 | [[rec(2).d]] = String |
	  
@NestedForEachExecution
 Scenario: Workflow with ForEach in Rec with star which contains Dot Net DLL
      Given I have a workflow "WFWithForEachContainingDotNetDLL"	
	   And "WFWithForEachContainingDotNetDLL" contains an Assign "RecVal" as
	  | variable         | value |
	  | [[rec().number]] | 1     |
	  | [[rec().number]] | 2     |
	  | [[rec().number]] | 3     |
	  | [[rec().number]] | 4     |
	  And "WFWithForEachContainingDotNetDLL" contains a Foreach "ForEachTest" as "InRecordset" executions "[[rec(*)]]" 		
	  And "ForEachTest" contains an DotNet DLL "DotNetService" as
	     | Source                   | ClassName                       | ObjectName | Action    | ActionOutputVaribale |
	     | New DotNet Plugin Source | TestingDotnetDllCascading.Human | [[@human]] | BuildInts | [[rec1().num]]       |
	  And "DotNetService" constructorinputs 0 with inputs as
	  | parameterName | value |type|
	  
      When "WFWithForEachContainingDotNetDLL" is executed
	  Then the workflow execution has "NO" error
	   And the "RecVal" in WorkFlow "WFWithForEachContainingDotNetDLL" debug inputs as 
		  | # | Variable           | New Value |
		  | 1 | [[rec().number]] = | 1         |
		  | 2 | [[rec().number]] = | 2         |
		  | 3 | [[rec().number]] = | 3         |
		  | 4 | [[rec().number]] = | 4         |	  
      And the "ForEachTest" in WorkFlow "WFWithForEachContainingDotNetDLL" has  "4" nested children 	 	  
	  And the dotnetdll "BuildInts" in 'DotNet DLL' in step 1 for "ForEachTest" debug inputs as
		 | label | Variable          | value | operater |
		 | a     | [[rec(1).number]] | 1     | =        |
		 | b     | [[rec(1).number]] | 1     | =        |
		 | c     | [[rec(1).number]] | 1     | =        |
		 | d     | [[rec(1).number]] | 1     | =        |
	  And the dotnetdll "BuildInts" in 'DotNet DLL' in step 2 for "ForEachTest" debug inputs as
			 | label | Variable          | value | operater |
			 | a     | [[rec(2).number]] | 2     | =        |
			 | b     | [[rec(2).number]] | 2     | =        |
			 | c     | [[rec(2).number]] | 2     | =        |
			 | d     | [[rec(2).number]] | 2     | =        |
	 And the dotnetdll "BuildInts" in 'DotNet DLL' in step 3 for "ForEachTest" debug inputs as
		 	 | label | Variable          | value | operater |
		 	 | a     | [[rec(3).number]] | 3     | =        |
		 	 | b     | [[rec(3).number]] | 3     | =        |
		 	 | c     | [[rec(3).number]] | 3     | =        |
		 	 | d     | [[rec(3).number]] | 3     | =        |
	 And the dotnetdll "BuildInts" in 'DotNet DLL' in step 4 for "ForEachTest" debug inputs as
		 	 | label | Variable          | value | operater |
		 	 | a     | [[rec(4).number]] | 4     | =        |
		 	 | b     | [[rec(4).number]] | 4     | =        |
		 	 | c     | [[rec(4).number]] | 4     | =        |
		 	 | d     | [[rec(4).number]] | 4     | =        |
	And the dotnetdll "BuildInts" in "DotNet DLL" in step 1 for "ForEachTest" debug output as
		 | label | Variable        | value | operater |
	   	 |       | [[rec1(4).num]] | 1     | =        |
    And the dotnetdll "BuildInts" in "DotNet DLL" in step 2 for "ForEachTest" debug output as
		 | label | Variable        | value | operater |
		 |       | [[rec1(8).num]] | 2     | =        |
    And the dotnetdll "BuildInts" in "DotNet DLL" in step 3 for "ForEachTest" debug output as
		 | label | Variable         | value | operater |
		 |       | [[rec1(12).num]] | 3     | =        |
    And the dotnetdll "BuildInts" in "DotNet DLL" in step 4 for "ForEachTest" debug output as
		 | label | Variable         | value | operater |
		 |       | [[rec1(16).num]] | 4     | =        |

	
		

@WorkflowExecution
Scenario: Executing 2 ForEach"s inside a ForEach which contains Assign only
      Given I have a workflow "WFContainsForEachInsideforEach"
	  And "WFContainsForEachInsideforEach" contains a Foreach "ForEachTest1" as "NumOfExecution" executions "2"
	  And "ForEachTest1" contains a Foreach "ForEachTest2" as "NumOfExecution" executions "2"
	  And "ForEachTest2" contains a Foreach "ForEachTest3" as "NumOfExecution" executions "2"
	  And "ForEachTest3" contains an Assign "Testingoutput" as
	  | variable     | value    |
	  | [[rec(1).a]] | 123      |
	  When "WFContainsForEachInsideforEach" is executed
	  Then the workflow execution has "NO" error
	  And the "ForEachTest1" in WorkFlow "WFContainsForEachInsideforEach" debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
	  And the "ForEachTest1" in WorkFlow "WFContainsForEachInsideforEach" has  "2" nested children
      And the "ForEachTest2" in step 1 for "ForEachTest1" debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
      And the "ForEachTest2" in WorkFlow "ForEachTest1" has  "2" nested children
	  And the "ForEachTest3" in step 1 for "ForEachTest2" debug inputs as 
	  |                 | Number |
	  | No. of Executes | 2      |
	  And the "ForEachTest3" in WorkFlow "ForEachTest2" has  "2" nested children
	  And the "Testingoutput" in step 1 for "ForEachTest3" debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | 123       |
	  And the "Testingoutput" in step 1 for "ForEachTest3" debug outputs as
	  | # |                          |
	  | 1 | [[rec(1).a]]  =  123     |
	  And the "Testingoutput" in step 2 for "ForEachTest3" debug inputs as
	  | # | Variable           | New Value |
	  | 1 | [[rec(1).a]] = 123 | 123       |
	  And the "Testingoutput" in step 2 for "ForEachTest3" debug outputs as
	  | # |                    |
	  | 1 | [[rec(1).a]] = 123 |	  		

@NestedForEachExecution
  Scenario: Executing 2 ForEach"s inside a ForEach which contains Assign only Large Execution
      Given I have a workflow "WFForEachInsideforEachLargeTenFifty"
	  And "WFForEachInsideforEachLargeTenFifty" contains a Foreach "ForEachTest1" as "NumOfExecution" executions "10"
	  And "ForEachTest1" contains a Foreach "ForEachTest2" as "NumOfExecution" executions "50"
	  And "ForEachTest2" contains an Assign "Testingoutput" as
	  | variable    | value         |
	  | [[rec().a]] | 123asda       |
	  | [[rec().b]] | aaaaa         |
	  | [[rec().c]] | rrrrrrr       |
	  | [[rec().d]] | 123asda       |
	  | [[rec().e]] | sdfsdrf45456  |
	  | [[rec().f]] | cvbcb1123     |
	  | [[rec().g]] | aasdww2323    |
	  | [[rec().h]] | oooooo9999    |
	  | [[rec().i]] | sdfsdf3434    |
	  | [[rec().j]] | asda123123    |
	  | [[rec().k]] | sssdff444     |
	  | [[rec().l]] | asdvvvbbg3333 |
	  | [[rec().m]] | aasdasd       |
	  | [[rec().n]] | aasdd222      |
	  | [[rec().o]] | 22323asda     |
	  And I get the server memory
	  When "WFForEachInsideforEachLargeTenFifty" is executed
	  Then the workflow execution has "NO" error
	  And the "ForEachTest1" in WorkFlow "WFForEachInsideforEachLargeTenFifty" debug inputs as 
	  |                 | Number |
	  | No. of Executes | 10      |
	  And the "ForEachTest1" in WorkFlow "WFForEachInsideforEachLargeTenFifty" has at least "5" nested children
      And the "ForEachTest2" in step 1 for "ForEachTest1" debug inputs as 
	  |                 | Number |
	  | No. of Executes | 50      |
      And the "ForEachTest2" in WorkFlow "ForEachTest1" has  "50" nested children	 
	  And the "Testingoutput" in step 50 for "ForEachTest2" debug inputs as
	  | #  | Variable      | New Value     |
	  | 1  | [[rec().a]] = | 123asda       |
	  | 2  | [[rec().b]] = | aaaaa         |
	  | 3  | [[rec().c]] = | rrrrrrr       |
	  | 4  | [[rec().d]] = | 123asda       |
	  | 5  | [[rec().e]] = | sdfsdrf45456  |
	  | 6  | [[rec().f]] = | cvbcb1123     |
	  | 7  | [[rec().g]] = | aasdww2323    |
	  | 8  | [[rec().h]] = | oooooo9999    |
	  | 9  | [[rec().i]] = | sdfsdf3434    |
	  | 10 | [[rec().j]] = | asda123123    |
	  | 11 | [[rec().k]] = | sssdff444     |
	  | 12 | [[rec().l]] = | asdvvvbbg3333 |
	  | 13 | [[rec().m]] = | aasdasd       |
	  | 14 | [[rec().n]] = | aasdd222      |
	  | 15 | [[rec().o]] = | 22323asda     |
	  And the "Testingoutput" in step 50 for "ForEachTest2" debug outputs as
	  | #  |                               |
	  | 1  | [[rec(50).a]] = 123asda       |
	  | 2  | [[rec(50).b]] = aaaaa         |
	  | 3  | [[rec(50).c]] = rrrrrrr       |
	  | 4  | [[rec(50).d]] = 123asda       |
	  | 5  | [[rec(50).e]] = sdfsdrf45456  |
	  | 6  | [[rec(50).f]] = cvbcb1123     |
	  | 7  | [[rec(50).g]] = aasdww2323    |
	  | 8  | [[rec(50).h]] = oooooo9999    |
	  | 9  | [[rec(50).i]] = sdfsdf3434    |
	  | 10 | [[rec(50).j]] = asda123123    |
	  | 11 | [[rec(50).k]] = sssdff444     |
	  | 12 | [[rec(50).l]] = asdvvvbbg3333 |
	  | 13 | [[rec(50).m]] = aasdasd       |
	  | 14 | [[rec(50).n]] = aasdd222      |
	  | 15 | [[rec(50).o]] = 22323asda     |
		
@WorkflowExecution
Scenario: Workflow Assign and Find Record index tool with two variables in reult field expect error
      Given I have a workflow "WFWithAssignandFindRecordindexy"
	  And "WFWithAssignandFindRecordindexy" contains an Assign "Record" as
      | # | variable     | value    |
      | # | [[rec(1).a]] | Warewolf |
	  And "WFWithAssignandFindRecordindexy" contains Find Record Index "FindRecord0" into result as "[[a]][[b]]"
      | # | In Field    | # | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().a]] | 1 | =          | Warewolf | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexy" is executed
	  Then the workflow execution has "AN" error
	  And the "Record" in WorkFlow "WFWithAssignandFindRecordindexy" debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | Warewolf  | 
	  And the "Record" in Workflow "WFWithAssignandFindRecordindexy" debug outputs as   
	  | # |                                  |
	  | 1 | [[rec(1).a]]         =  Warewolf |


@WorkflowExecution
Scenario: Workflow Assign and Find Record index
      Given I have a workflow "WFWithAssignandFindRecordindexTool"
	  And "WFWithAssignandFindRecordindexTool" contains an Assign "Record" as
      | # | variable     | value    |
      | # | [[rec(1).a]] | Warewolf |
	  And "WFWithAssignandFindRecordindexTool" contains Find Record Index "FindRecord0" into result as "[[a]]*]]"
      | # | In Field    | # | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().a]] | 1 | =          | Warewolf | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexTool" is executed
	  Then the workflow execution has "AN" error
	  And the "Record" in WorkFlow "WFWithAssignandFindRecordindexTool" debug inputs as 
	  | # | Variable       | New Value |
	  | 1 | [[rec(1).a]] = | Warewolf  | 
	  And the "Record" in Workflow "WFWithAssignandFindRecordindexTool" debug outputs as   
	  | # |                                  |
	  | 1 | [[rec(1).a]]         =  Warewolf |	 	 
	  
@WorkflowExecution
Scenario: Workflow Assign and Find Record index expected not greater than
      Given I have a workflow "WFWithAssignandFindRecordindexTool"
	  And "WFWithAssignandFindRecordindexTool" contains an Assign "Record" as
      | # | variable        | value |
      | # | [[rec(1).Name]] | 1Bob  |
	  And "WFWithAssignandFindRecordindexTool" contains Find Record Index "FindRecord0" into result as "[[asdf]]"
      | # | In Field       | # | Match Type | Match | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().Name]] | 1 | >          | 1     | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexTool" is executed
	  Then the workflow execution has "" error
	  And the "Record" in WorkFlow "WFWithAssignandFindRecordindexTool" debug inputs as 
	  | # | Variable          | New Value |
	  | 1 | [[rec(1).Name]] = | 1Bob      |
	  And the "Record" in Workflow "WFWithAssignandFindRecordindexTool" debug outputs as   
	  | # |                                 |
	  | 1 | [[rec(1).Name]]         =  1Bob |
	  And the "FindRecord0" in Workflow "WFWithAssignandFindRecordindexTool" debug outputs as   
	  |                        |
	  | [[asdf]]         =  -1 |
	  
@WorkflowExecution
Scenario: Workflow Assign and Find Record index expected not less than
      Given I have a workflow "WFWithAssignandFindRecordindexTool"
	  And "WFWithAssignandFindRecordindexTool" contains an Assign "Record" as
      | # | variable        | value |
      | # | [[rec(1).Name]] | 1Bob  |
	  And "WFWithAssignandFindRecordindexTool" contains Find Record Index "FindRecord0" into result as "[[asdf]]"
      | # | In Field       | # | Match Type | Match | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().Name]] | 1 | <          | 1     | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexTool" is executed
	  Then the workflow execution has "" error
	  And the "Record" in WorkFlow "WFWithAssignandFindRecordindexTool" debug inputs as 
	  | # | Variable          | New Value |
	  | 1 | [[rec(1).Name]] = | 1Bob      |
	  And the "Record" in Workflow "WFWithAssignandFindRecordindexTool" debug outputs as   
	  | # |                                 |
	  | 1 | [[rec(1).Name]]         =  1Bob |
	  And the "FindRecord0" in Workflow "WFWithAssignandFindRecordindexTool" debug outputs as   
	  |                        |
	  | [[asdf]]         =  -1 |

@WorkflowExecution
Scenario: Workflow Assign and Find Record index expected is greater than
      Given I have a workflow "WFWithAssignandFindRecordindexTool"
	  And "WFWithAssignandFindRecordindexTool" contains an Assign "Record" as
      | variable       | value |
      | [[rec().Name]] | 1Bob  |
      | [[rec().Age]]  | 2     |
      | [[rec().Name]] | 2Bob  |
      | [[rec().Age]]  | 23    |
	  And "WFWithAssignandFindRecordindexTool" contains Find Record Index "FindRecord0" into result as "[[expectedResult]]"
      | # | In Field                     | # | Match Type | Match | Require All Matches To Be True | Require All Fields To Match |
      | # | [[rec().Name]],[[rec().Age]] | 1 | >          | 2     | NO                             | NO                          |
	  When "WFWithAssignandFindRecordindexTool" is executed
	  Then the workflow execution has "" error
	  And the "FindRecord0" in Workflow "WFWithAssignandFindRecordindexTool" debug outputs as   
	  |                       |
	  | [[expectedResult]]         =  2 |

@WorkflowExecution
Scenario Outline: Testing Length with two variables in Result field
      Given I have a workflow "WorkflowforLength"
      And "WorkflowforLength" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | 1213  |
	  | [[rec().a]] | 4561  |
	  And "WorkflowforLength" contains Length "Len" on "[[rec(*)]]" into "<Variable>"
	  When "WorkflowforLength" is executed
	  Then the workflow execution has "AN" error	
      And the "Rec To Convert" in WorkFlow "WorkflowforLength" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | 1213      |
	  | 2 | [[rec().a]] = | 4561      |
	  And the "Rec To Convert" in Workflow "WorkflowforLength" debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = 1213 |
	  | 2 | [[rec(2).a]] = 4561 |
	  And the "Len" in WorkFlow "WorkflowforLength" debug inputs as
	  | Recordset           |
	  | [[rec(1).a]] = 1213 |
	  | [[rec(2).a]] = 4561 |
	  And the "Len" in Workflow "WorkflowforLength" debug outputs as    
	  |                |
	  |                |
Examples: 
      | No | Variable               |
      | 1  | [[length]][[a]]        |
      | 2  | [[a]]*]]               |
      | 3  | [[var@]]               |
      | 4  | [[var]]00]]            |
      | 5  | [[(1var)]]             |
      | 6  | [[var[[a]]]]           |
      | 7  | [[var.a]]              |
      | 8  | [[#var]]               |
      | 9  | [[var 1]]              |
      | 10 | [[rec(1).[[rec().1]]]] |
      | 11 | [[rec(@).a]]           |
      | 12 | [[rec"()".a]]          |
      | 13 | [[rec([[[[b]]]]).a]]   |


@WorkflowExecution
Scenario: Testing Data Split with two variables in Result field
      Given I have a workflow "WorkflowforDatasplit"
      And "WorkflowforDatasplit" contains an Assign "Rec To Convert" as
	  | variable    | value    |
	  | [[rec().a]] | Test     |
	  | [[rec().a]] | Warewolf |
	  And "WorkflowforDatasplit" contains Data Split "Data Split" as
	  | String       | Variable        | Type  | At | Include    | Escape |
	  | [[rec(1).a]] | [[fr().a]][[a]] | Index | 2  | Unselected |        |
	  |              | [[fr().b]][[b]] | Index | 2  | Unselected |        |
	  When "WorkflowforDatasplit" is executed  	  
	  Then the workflow execution has "AN" error	
      And the "Rec To Convert" in WorkFlow "WorkflowforDatasplit" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | Test      |
	  | 2 | [[rec().a]] = | Warewolf  |
	  And the "Rec To Convert" in Workflow "WorkflowforDatasplit" debug outputs as    
	  | # |                         |
	  | 1 | [[rec(1).a]] = Test     |
	  | 2 | [[rec(2).a]] = Warewolf |
	 And the "Data Split" in WorkFlow "WorkflowforDatasplit" debug inputs as 
	  | String to Split     | Process Direction | Skip blank rows | # |                   | With  | Using | Include | Escape |
	  | [[rec(1).a]] = Test | Forward           | No              | 1 | [[fr().a]][[a]] = | Index | 2     | No      |        |
	  |                     |                   |                 | 2 | [[fr().b]][[b]] = | Index | 2     | No      |        |
	  And the "Data Split" in Workflow "WorkflowforDatasplit" debug outputs as  
	  | # |                    |

@WorkflowExecution
Scenario Outline: Testing Format Numbers with two variables in Result
     Given I have a workflow "Workflowforfn"
	  And "Workflowforfn" contains an Assign "Values" as
	  | variable | value |
	  | [[a]]    | 1     |
	  | [[b]]    | 2     |
	  And "Workflowforfn" contains Format Number "Fnumber" as 
	  | Number  | Rounding Selected | Rounding To | Decimal to show | Result       |
	  | 123.568 | Up                | 2           | 2               | "<Variable>" |
	  When "Workflowforfn" is executed  	  
	  Then the workflow execution has "AN" error	
	  And the "Fnumber" in WorkFlow "Workflowforfn" debug inputs as 	
	  | Number  | Rounding | Rounding Value | Decimals to show |
	  | 123.568 | Up       | 2              | 2                |	  
Examples: 
       | No | Variable               |
       | 1  | [[a]][[Result]]        |

@WorkflowExecution
Scenario Outline: Testing Random Numbers with two variables in Result
      Given I have a workflow "Workflowforrandom123"
	  And "Workflowforrandom123" contains an Assign "Values" as
	  | variable | value |
	  | [[a]]    | 1     |
	  | [[b]]    | 10     |
	  And "Workflowforrandom123" contains Random "Randoms" as
	  | Type    | From | To | Result       |
	  | Numbers | 1    | 10 | "<Variable>" |
	  When "Workflowforrandom123" is executed  	  
	  Then the workflow execution has "AN" error
	   And the "Values" in WorkFlow "Workflowforrandom123" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | 1         |
	  | 2 | [[b]] =  | 10        |
	  And the "Values" in Workflow "Workflowforrandom123" debug outputs as    
	  | # |             |
	  | 1 | [[a]] =  1  |
	  | 2 | [[b]] =  10 |
	  And the "Randoms" in WorkFlow "Workflowforrandom123" debug inputs as 
	    | Random  | From | To |
	    | Numbers | 1    | 10  |
      And the "Randoms" in Workflow "Workflowforrandom123" debug outputs as
	  |                |
	  | "<Variable>" = |
Examples: 
      | No | Variable               |
      | 1  | [[a]][[Result]]        |

@WorkflowExecution
Scenario Outline: Testing Date and Time with two variables in Result field
      Given I have a workflow "WorkflowforDT"
      And "WorkflowforDT" contains an Assign "Convert2" as
	  | variable    | value      |
	  | [[rec().a]] | 12/01/2001 |
	  And "WorkflowforDT" contains Date and Time "AddDates" as
      | Input       | Input Format | Add Time | Output Format | Result       |
      | [[rec().a]] | dd/mm/yyyy   | 1        | dd/mm/yyyy    | "<Variable>" |	
	  When "WorkflowforDT" is executed  	  
	  Then the workflow execution has "AN" error	
      And the "Convert2" in WorkFlow "WorkflowforDT" debug inputs as
	  | # | Variable      | New Value  |
	  | 1 | [[rec().a]] = | 12/01/2001 |
	  And the "Convert2" in Workflow "WorkflowforDT" debug outputs as    
	  | # |                           |
	  | 1 | [[rec(1).a]] = 12/01/2001 |
	  And the "AddDates" in WorkFlow "WorkflowforDT" debug inputs as
	   | Input                     | Input Format | Add Time |   | Output Format |
	   | [[rec(1).a]] = 12/01/2001 | dd/mm/yyyy   | Years    | 1 | dd/mm/yyyy    |	
	  And the "AddDates" in Workflow "WorkflowforDT" debug outputs as   
	   |                |
	   | "<Variable>" = |
Examples: 
      | No | Variable               |
      | 1  | [[a]][[Result]]        |      

@WorkflowExecution
Scenario: Convert an recordset to Upper by using index as scalar
	Given I have a workflow "ConvertUsingScalarWithRecursiveEvalution"
	And "ConvertUsingScalarWithRecursiveEvalution" contains an Assign "Records" as
	  | variable     | value    |
	  | [[rs().row]] | warewolf |
	  | [[a]]        | 1        |
	And "ConvertUsingScalarWithRecursiveEvalution" contains case convert "Case to Convert" as
	  | Variable          | Type  |
	  | [[rs([[a]]).row]] | UPPER |
	When "ConvertUsingScalarWithRecursiveEvalution" is executed
	Then the workflow execution has "NO" error
	And the "Records" in WorkFlow "ConvertUsingScalarWithRecursiveEvalution" debug inputs as
	  | # | Variable       | New Value |
	  | 1 | [[rs().row]] = | warewolf  |
	  | 2 | [[a]] =        | 1         |
	And the "Records" in Workflow "ConvertUsingScalarWithRecursiveEvalution" debug outputs as  
	  | # |                           |
	  | 1 | [[rs(1).row]] =  warewolf |
	  | 2 | [[a]] =  1                |
	And the "Case to Convert" in WorkFlow "ConvertUsingScalarWithRecursiveEvalution" debug inputs as
	  | # | Convert                  | To    |
	  | 1 | [[rs(1).row]] = warewolf | UPPER |
	And the "Case to Convert" in Workflow "ConvertUsingScalarWithRecursiveEvalution" debug outputs as
	  | # |                          |
	  | 1 | [[rs(1).row]] = WAREWOLF |

@WorkflowExecution
Scenario: Convert an recordset to Upper by using index as recordset
	Given I have a workflow "ConvertUsingRecSetInRecursiveEvalution"
	And "ConvertUsingRecSetInRecursiveEvalution" contains an Assign "Records" as
	  | variable       | value    |
	  | [[rs().row]]   | warewolf |
	  | [[rs().index]] | 1        |
	And "ConvertUsingRecSetInRecursiveEvalution" contains case convert "Case to Convert" as
	  | Variable                    | Type  |
	  | [[rs([[rs(1).index]]).row]] | UPPER |
	When "ConvertUsingRecSetInRecursiveEvalution" is executed
	Then the workflow execution has "NO" error
	And the "Records" in WorkFlow "ConvertUsingRecSetInRecursiveEvalution" debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[rs().row]] =   | warewolf  |
	  | 2 | [[rs().index]] = | 1         |
	And the "Records" in Workflow "ConvertUsingRecSetInRecursiveEvalution" debug outputs as  
	  | # |                          |
	  | 1 | [[rs(1).row]] = warewolf |
	  | 2 | [[rs(1).index]] = 1      |
	And the "Case to Convert" in WorkFlow "ConvertUsingRecSetInRecursiveEvalution" debug inputs as
	  | # | Convert                  | To    |
	  | 1 | [[rs(1).row]] = warewolf | UPPER |
	And the "Case to Convert" in Workflow "ConvertUsingRecSetInRecursiveEvalution" debug outputs as
	  | # |                          |
	  | 1 | [[rs(1).row]] = WAREWOLF |

@WorkflowExecution
Scenario: Base Convert two varibles on one row 
	Given I have a workflow "BaseConvertUsingRecSetInRecursiveEvalution"
	And "BaseConvertUsingRecSetInRecursiveEvalution" contains an Assign "Records" as
	  | variable    | value |
	  | [[rs().a]]  | 1     |
	  | [[rec().a]] | 2     |
	And "BaseConvertUsingRecSetInRecursiveEvalution" contains Base convert "Base to Convert" as
	  | Variable               | From | To      |
	  | [[rec([[rs(1).a]]).a]] | Text | Base 64 |
	When "BaseConvertUsingRecSetInRecursiveEvalution" is executed
	Then the workflow execution has "NO" error
	And the "Records" in WorkFlow "BaseConvertUsingRecSetInRecursiveEvalution" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs().a]] =  | 1         |
	  | 2 | [[rec().a]] = | 2         |
	And the "Records" in Workflow "BaseConvertUsingRecSetInRecursiveEvalution" debug outputs as  
	  | # |                  |
	  | 1 | [[rs(1).a]] = 1  |
	  | 2 | [[rec(1).a]] = 2 |
	And the "Base to Convert" in WorkFlow "BaseConvertUsingRecSetInRecursiveEvalution" debug inputs as
	  | # | Convert           | From | To     |
	  | 1 | [[rec(1).a]] = 2 | Text | Base 64 |
    And the "Base to Convert" in Workflow "BaseConvertUsingRecSetInRecursiveEvalution" debug outputs as  
	  | # |                     |
	  | 1 | [[rec(1).a]] = Mg== |

@WorkflowExecution
Scenario: Workflow by using For Each with Random in it
      Given I have a workflow "WFWithForEachContainsRandom"
	  And "WFWithForEachContainsRandom" contains a Foreach "ForEachTest123" as "NumOfExecution" executions "5"
	  And "ForEachTest123" contains Random "Random" as
	    | Type    | From | To | Result       |
	    | Numbers | 1    | 5  | [[rec(*).a]] |
      When "WFWithForEachContainsRandom" is executed
	  Then the workflow execution has "NO" error
	  And the "ForEachTest123" in WorkFlow "WFWithForEachContainsRandom" debug inputs as 
	    |                 | Number |
	    | No. of Executes | 5      |
      And the "ForEachTest123" in WorkFlow "WFWithForEachContainsRandom" has  "5" nested children 
	   And the "Random" in step 1 for "ForEachTest123" debug inputs as
	    | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the "Random" in step 1 for "ForEachTest123" debug outputs as
        |                      |
	    | [[rec(1).a]] = Int32 |
	  And the "Random" in step 2 for "ForEachTest123" debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the "Random" in step 2 for "ForEachTest123" debug outputs as
        |                      |
	    | [[rec(1).a]] = Int32 |
       And the "Random" in step 3 for "ForEachTest123" debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the "Random" in step 3 for "ForEachTest123" debug outputs as
         |                      |
	    | [[rec(1).a]] = Int32 |
      And the "Random" in step 4 for "ForEachTest123" debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	  And the "Random" in step 4 for "ForEachTest123" debug outputs as
       |                      |
	    | [[rec(1).a]] = Int32 |
       And the "Random" in step 5 for "ForEachTest123" debug inputs as
        | Random  | From | To |
	    | Numbers | 1    | 5  |
	And the "Random" in step 5 for "ForEachTest123" debug outputs as
         |                      |
         | [[rec(1).a]] = Int32 |

@WorkflowExecution
Scenario: Workflow with Calculation using Star notation
      Given I have a workflow "WorkflowWithAssignCalculationUsingStar"
      And "WorkflowWithAssignCalculationUsingStar" contains an Assign "Records" as
	  | variable    | value |
	  | [[rs(1).a]] | 19    |
	  | [[rs(2).a]] | 20    |
	  | [[rs(3).a]] | 40    |	 
	  And "WorkflowWithAssignCalculationUsingStar" contains an Assign "Calculation" as
	  | variable      | value          |
	  | [[rec().sum]] | =[[rs(*).a]]+1 |
	  When "WorkflowWithAssignCalculationUsingStar" is executed
	  Then the workflow execution has "NO" error
	  And the "Records" in WorkFlow "WorkflowWithAssignCalculationUsingStar" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rs(1).a]] = | 19        |
	  | 2 | [[rs(2).a]] = | 20        |
	  | 3 | [[rs(3).a]] = | 40        |
	  And the "Records" in Workflow "WorkflowWithAssignCalculationUsingStar" debug outputs as  
	  | # |                   |
	  | 1 | [[rs(1).a]] =  19 |
	  | 2 | [[rs(2).a]] =  20 |
	  | 3 | [[rs(3).a]] =  40 |
	   And the "Calculation" in WorkFlow "WorkflowWithAssignCalculationUsingStar" debug inputs as
	  | # | Variable        | New Value            |
	  | 1 | [[rec().sum]] = | [[rs(1).a]]+1 = 19+1 |
	  |   |                 | [[rs(2).a]]+1 = 20+1 |
	  |   |                 | [[rs(3).a]]+1 = 40+1 |
	  And the "Calculation" in Workflow "WorkflowWithAssignCalculationUsingStar" debug outputs as  
	  | # |                     |
	  | 1 | [[rec(3).sum]] = 41 |

@WorkflowExecution
Scenario: Workflow Saving with Different Versions 
	 Given I have a workflow "WorkflowWithVersionAssignTest"
	 And "WorkflowWithVersionAssignTest" contains an Assign "VarsAssign" as
	  | variable    | value |
	  | [[rec().a]] | New   |
	  | [[rec().a]] | Test  |	 
	  When workflow "WorkflowWithVersionAssignTest" is saved "1" time
	  Then workflow "WorkflowWithVersionAssignTest" has "0" Versions in explorer
	  When workflow "WorkflowWithVersionAssignTest" is saved "2" time
	  Then workflow "WorkflowWithVersionAssignTest" has "2" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.2 DateTime        |
	  | v.1 DateTime        |
	  When workflow "WorkflowWithVersionAssignTest" is saved "3" time
	  Then workflow "WorkflowWithVersionAssignTest" has "5" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.5 DateTime Save   |
	  | v.4 DateTime Save   |
	  | v.3 DateTime Save   |
	  | v.2 DateTime Save   |
	  | v.1 DateTime Save   |
	  And workflow "WorkflowWithVersionAssignTest" is deleted as cleanup

@WorkflowExecution
Scenario: Executing workflow of different versions
	 Given I have a workflow "WorkflowWithVersionAssignExecuted2"
	 And "WorkflowWithVersionAssignExecuted2" contains an Assign "VarsAssign" as
	  | variable    | value |
	  | [[rec().a]] | New   |
	  | [[rec().a]] | Test  |	 
	  When workflow "WorkflowWithVersionAssignExecuted2" is saved "1" time
	  Then workflow "WorkflowWithVersionAssignExecuted2" has "0" Versions in explorer
	  When "WorkflowWithVersionAssignExecuted2" is executed without saving
	  Then the workflow execution has "NO" error
	  And the "VarsAssign" in WorkFlow "WorkflowWithVersionAssignExecuted2" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | New       |
	  | 2 | [[rec().a]] = | Test      |
	  And the "VarsAssign" in Workflow "WorkflowWithVersionAssignExecuted2" debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = New  |
	  | 2 | [[rec(2).a]] = Test | 
	  When workflow "WorkflowWithVersionAssignExecuted2" is saved "2" time
	  Then workflow "WorkflowWithVersionAssignExecuted2" has "2" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.2 DateTime        |
	  | v.1 DateTime        |
	 And "WorkflowWithVersionAssignExecuted2" contains an Assign "VarsAssign2" as
	  | variable    | value |
	  | [[rec().a]] | New   |
	  | [[rec().a]] | Test  |
	  | [[rec().a]] | V1    |
	 When workflow "WorkflowWithVersionAssignExecuted2" is saved "1" time
	 When "WorkflowWithVersionAssignExecuted2" is executed without saving
	 Then the workflow execution has "NO" error
	 And the "VarsAssign2" in WorkFlow "WorkflowWithVersionAssignExecuted2" debug inputs as
	  | # | Variable      | New Value |
	  | 1 | [[rec().a]] = | New       |
	  | 2 | [[rec().a]] = | Test      |
	  | 3 | [[rec().a]] = | V1        |	  
	 When workflow "WorkflowWithVersionAssignExecuted2" is saved "1" time
	  Then workflow "WorkflowWithVersionAssignExecuted2" has "4" Versions in explorer
	  And explorer as 
	  | Explorer           |
	  | WorkflowWithAssign |
	  | v.4 DateTime        |
	  | v.3 DateTime        |
	  | v.2 DateTime        |
	  | v.1 DateTime        |	
	  When I rollback "WorkflowWithVersionAssignExecuted2" to version "1"
	  When "WorkflowWithVersionAssignExecuted2" is executed without saving
	  Then the workflow execution has "NO" error
	  And the "VarsAssign" in Workflow "WorkflowWithVersionAssignExecuted2" debug outputs as    
	  | # |                     |
	  | 1 | [[rec(1).a]] = New  |
	  | 2 | [[rec(2).a]] = Test |
	  And workflow "WorkflowWithVersionAssignExecuted2" is deleted as cleanup

@WorkflowExecution
Scenario: Calculate testing variable values that hasn"t been assigned
      Given I have a workflow "WorkflowforCalTestingUnassignedvariablevalue"
      And "WorkflowforCalTestingUnassignedvariablevalue" contains an Assign "Values34" as
	  | variable | value |
	  | [[Val]]    | 1     |
	 And "WorkflowforCalTestingUnassignedvariablevalue" contains Calculate "Calculate1" with formula "[[Val1]]+1" into "[[res]]"
	  When "WorkflowforCalTestingUnassignedvariablevalue" is executed  	  
	  Then the workflow execution has "AN" error	
      And the "Values34" in WorkFlow "WorkflowforCalTestingUnassignedvariablevalue" debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[Val]] = | 1         |
	  And the "Values34" in Workflow "WorkflowforCalTestingUnassignedvariablevalue" debug outputs as    
	  | # |             |
	  | 1 | [[Val]] = 1 |
	  And the "Calculate1" in WorkFlow "WorkflowforCalTestingUnassignedvariablevalue" debug inputs as 
      | fx =         |
      | [[Val1]]+1 = |           
      And the "Calculate1" in Workflow "WorkflowforCalTestingUnassignedvariablevalue" debug outputs as  
	  |           |
	  | [[res]] = |


@WorkflowExecution
Scenario: Calculate testing variable values that hasn"t been assigned2
      Given I have a workflow "WorkflowforCalTestingUnassignedvariablevalue2"
      And "WorkflowforCalTestingUnassignedvariablevalue2" contains an Assign "Values34" as
	  | variable | value |
	  | [[Val]]  | 1     |
	 And "WorkflowforCalTestingUnassignedvariablevalue2" contains Calculate "Calculate1" with formula "[[Val1]]23+1" into "[[res]]"
	  When "WorkflowforCalTestingUnassignedvariablevalue2" is executed  	  
	  Then the workflow execution has "AN" error	
      And the "Values34" in WorkFlow "WorkflowforCalTestingUnassignedvariablevalue2" debug inputs as
	  | # | Variable  | New Value |
	  | 1 | [[Val]] = | 1         |
	  And the "Values34" in Workflow "WorkflowforCalTestingUnassignedvariablevalue2" debug outputs as    
	  | # |             |
	  | 1 | [[Val]] = 1 |
	  And the "Calculate1" in WorkFlow "WorkflowforCalTestingUnassignedvariablevalue2" debug inputs as 
      | fx =           |
      | [[Val1]]23+1 = |           
      And the "Calculate1" in Workflow "WorkflowforCalTestingUnassignedvariablevalue2" debug outputs as  
	  |           |
	  | [[res]] = |

@WorkflowExecution
Scenario: Gather System tool throws error when debug with 2 variables in one row 
	  Given I have a workflow "WorkflowW"
	  And "WorkflowW" contains an Assign "IndexVal" as
	  | variable | value   |
	  | [[a]]    | b       |
	   And "WorkflowW" contains Gather System Info "System info" as
	  | Variable   | Selected    |
	  | [[a]][[b]] | Date & Time |
	  When "WorkflowW" is executed
	  Then the workflow execution has "AN" error
	   And the "IndexVal" in WorkFlow "WorkflowW" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  And the "IndexVal" in Workflow "WorkflowW" debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  And the "System info" in WorkFlow "WorkflowW" debug inputs as
	  | # |              |             |
	 And the "System info" in Workflow "WorkflowW" debug outputs as    
	  | # |              |
	  | 1 | [[a]][[b]] = |

	   
	  
	   
@WorkflowExecution
Scenario: Gather System tool throws error when debug with invalid variableb
	  Given I have a workflow "WorkflowW1"
	  And "WorkflowW1" contains an Assign "IndexVal" as
	  | variable | value   |
	  | [[a]]    | b       |
	   And "WorkflowW1" contains Gather System Info "System info" as
	  | Variable         | Selected    |
	  | [[a]][[rec().a]] | Date & Time |
	  When "WorkflowW1" is executed
	  Then the workflow execution has "AN" error
	   And the "IndexVal" in WorkFlow "WorkflowW1" debug inputs as
	  | # | Variable | New Value |
	  | 1 | [[a]] =  | b         |
	  And the "IndexVal" in Workflow "WorkflowW1" debug outputs as  
	  | # |                 |
	  | 1 | [[a]] = b       |
	  And the "System info" in WorkFlow "WorkflowW1" debug inputs as
	  | # |                      |             |
	 And the "System info" in Workflow "WorkflowW1" debug outputs as    
	  | # |                      |
	  | 1 | [[a]][[rec().a]] = |

@WorkflowExecution
Scenario: Workflow Base Convert and Case Convert passing invalid variable through execution
	  Given I have a workflow "WorkflowWithBaseCase1"
	  And "WorkflowWithBaseCase1" contains an Assign "Assign1" as
	  | variable       | value    |
	  | [[a]]          | 1        |
	  | [[rec(1).a]]   | Warewolf |
	  | [[rec(2).a]]   | Test     |
	  | [[index(1).a]] | a$*      |
	  And "WorkflowWithBaseCase1" contains case convert "Case1" as
	  | Variable                  | Type  |
	  | [[rec([[index(1).a]]).a]] | UPPER |
	  And "WorkflowWithBaseCase1" contains Base convert "Base1" as
	  | Variable                  | From | To      |
	  | [[rec([[index(1).a]]).a]] | Text | Base 64 |
	  When "WorkflowWithBaseCase1" is executed
	  Then the workflow execution has "AN" error
	  And the "Assign1" in WorkFlow "WorkflowWithBaseCase1" debug inputs as
	  | # | Variable         | New Value |
	  | 1 | [[a]] =          | 1         |
	  | 2 | [[rec(1).a]] =   | Warewolf  |
	  | 3 | [[rec(2).a]] =   | Test      |
	  | 4 | [[index(1).a]] = | a$*       |
	   And the "Assign1" in Workflow "WorkflowWithBaseCase1" debug outputs as   
	  | # |                            |
	  | 1 | [[a]]         =  1         |
	  | 2 | [[rec(1).a]]   =  Warewolf |
	  | 3 | [[rec(2).a]]  =  Test      |
	  | 4 | [[index(1).a]] =  a$*      |
	  And the "Case1" in WorkFlow "WorkflowWithBaseCase1" debug inputs as
	  | # | Convert                     | To    |
	  | 1 | [[rec(a$*).a]] = | UPPER |
	  And the "Case1" in Workflow "WorkflowWithBaseCase1" debug outputs as  
	  | # |                     |
	  And the "Base1" in WorkFlow "WorkflowWithBaseCase1" debug inputs as
	  | # | Convert          | From | To      |
	  | 1 | [[rec(a$*).a]] = | Text | Base 64 |
      And the "Base1" in Workflow "WorkflowWithBaseCase1" debug outputs as  
	  | # |                     |

@WorkflowExecution
Scenario: Workflow Base Convert coverting same variable multiple times
	 Given I have a workflow "WorkflowWithBaseConvertUsingSameVariable"
	 And "WorkflowWithBaseConvertUsingSameVariable" contains an Assign "Assign1" as
	 | variable | value |
	 | [[test]] | data  |
	 And "WorkflowWithBaseConvertUsingSameVariable" contains Base convert "Base12" as
	 | Variable | From    | To      |
	 | [[test]] | Text    | Base 64 |
	 | [[test]] | Base 64 | Text    |
	 When "WorkflowWithBaseConvertUsingSameVariable" is executed
	 Then the workflow execution has "NO" error
	 And the "Assign1" in WorkFlow "WorkflowWithBaseConvertUsingSameVariable" debug inputs as
	 | # | Variable   | New Value |
	 | 1 | [[test]] = | data      |
	  And the "Assign1" in Workflow "WorkflowWithBaseConvertUsingSameVariable" debug outputs as   
	 | # |                  |
	 | 1 | [[test]] =  data |
	 And the "Base12" in WorkFlow "WorkflowWithBaseConvertUsingSameVariable" debug inputs as
	 | # | Convert             | From    | To      |
	 | 1 | [[test]] = data     | Text    | Base 64 |
	 | 2 | [[test]] = ZGF0YQ== | Base 64 | Text    |
    And the "Base12" in Workflow "WorkflowWithBaseConvertUsingSameVariable" debug outputs as  
	 | # |                     |
	 | 1 | [[test]] = ZGF0YQ== |
	 | 2 | [[test]] = data     |

@WorkflowExecution
Scenario: Workflow Assign and Find Record Index executing with incorrect format of Inputs 
      Given I have a workflow "WFWithAssignandFindRecordindexs"
	  And "WFWithAssignandFindRecordindex" contains an Assign "Record" as
      | variable     | value |
      | [[rec(1).a]] | 23    |
      | [[rec(2).a]] | 34    |
      | [[xr(1).a]]  | 10    |
	  And "WFWithAssignandFindRecordindexs" contains Find Record Index "FindRecord0" into result as "[[a]][[b]]"
	  | # | In Field              | # | Match Type | Match    | Require All Matches To Be True | Require All Fields To Match |
	  | # | [[rec().a]][[xr().a]] | 1 | =          | Warewolf | YES                            | NO                          |
	  When "WFWithAssignandFindRecordindexs" is executed
	  Then the workflow execution has "AN" error
	  
@WorkflowExecution
Scenario: Database PostgreSql Database service inputs and outputs
     Given I have a workflow "PostgreSqlGetCountries"
	 And "PostgreSqlGetCountries" contains a postgre tool using "get_countries" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable           |
	  | Prefix           | s             | Id                  | [[countries(*).Id]]   |
	  |                  |               | Name                | [[countries(*).Name]] |
      When "PostgreSqlGetCountries" is executed
     Then the workflow execution has "NO" error
	 And the "get_countries" in Workflow "PostgreSqlGetCountries" debug outputs as
	  |                                       |
	  | [[countries(1).Id]] = 1               |
	  | [[countries(2).Id]] = 3               |
	  | [[countries(1).Name]] = United States |
	  | [[countries(2).Name]] = South Africa  |

@WorkflowExecution
Scenario Outline: Database MySqlDB Database service using * indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                  | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestMySqlWFWithMySqlStarIndex | MySqlEmail  | [[rec(*).name]] | [[rec(*).email]] | NO           |

@WorkflowExecution
Scenario: Database MySqlDB Database service using char in param name
     Given I have a workflow "TestMySqlWFWithMySqlCharParamName"
	 And "TestMySqlWFWithMySqlCharParamName" contains a mysql database service "procWithCharNoOutput" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable |
	  | id               | 445           |                     |             |
	  | val              | bart01        |                     |             |
      When "TestMySqlWFWithMySqlCharParamName" is executed
     Then the workflow execution has "NO" error


@WorkflowExecution
Scenario Outline: Database MySqlDB Database service using int indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs is
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                 | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestMySqlWFWithMySqlIntIndex | MySqlEmail  | [[rec(1).name]] | [[rec(1).email]] | NO           |

@WorkflowExecution
Scenario Outline: Database MySqlDB Database service last  indexes
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs is
	  |                                       |
	  | [[rec(1).name]] = Monk                |
	  | [[rec(1).email]] = dora@explorers.com |
Examples: 
    | WorkflowName                  | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestMySqlWFWithMySqlLastIndex | MySqlEmail  | [[rec().name]] | [[rec().email]] | NO           |

@WorkflowExecution
Scenario Outline: Database MySqlDB Database service scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name     | <nameVariable>  |
	  |                  |               | email    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                |
	  | [[name]] = Monk                |
	  | [[email]] = dora@explorers.com |
Examples: 
    | WorkflowName               | ServiceName | nameVariable | emailVariable | errorOccured |
    | TestMySqlWFWithMySqlScalar | MySqlEmail  | [[name]]     | [[email]]     | NO           |

@WorkflowExecution
Scenario Outline: Database MySqlDB Database service Error outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
Examples: 
    | WorkflowName                                 | ServiceName | nameVariable         | emailVariable | errorOccured |
    | TestMySqlWFWithMySqlMailsInvalidIndex        | MySqlEmail  | [[rec(-1).name]]     | [[email]]     | YES          |
    | TestMySqlWFWithMySqlMailsInvalidVar          | MySqlEmail  | [[123]]              | [[email]]     | YES          |
    | TestMySqlWFWithMySqlMailsInvalidVarWithIndex | MySqlEmail  | [[rec(-1).name.bob]] | [[email]]     | YES          |

@WorkflowExecution
Scenario Outline: Database MySqlDB Database service inputs and outputs
	Given I depend on a valid MySQL server
    And I have a workflow "<WorkflowName>"
	And "<WorkflowName>" contains a mysql database service "<ServiceName>" with mappings as
	 | Input to Service | From Variable | Output from Service | To Variable     |
	 | name             | afg%          | countryid           | <nameVariable>  |
	 |                  |               | description         | <emailVariable> |
    When "<WorkflowName>" is executed
    Then the workflow execution has "<errorOccured>" error
	And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	 |                                            |
	 | [[countries(1).id]] = 1                    |
	 | [[countries(2).id]] = 1                    |
	 | [[countries(1).description]] = Afghanistan |
	 | [[countries(2).description]] = Afghanistan |
Examples: 
    | WorkflowName                  | ServiceName           | nameVariable        | emailVariable                | errorOccured |
    | TestMySqlWFWithMySqlCountries | Pr_CitiesGetCountries | [[countries(*).id]] | [[countries(*).description]] | NO           |

@MSSql
Scenario Outline: Database SqlDB Database service inputs and outputs
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  | Prefix           | afg           | countryid           | <nameVariable>  |
	  |                  |               | description         | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                            |
	  | [[countries(1).id]] = 1                    |
	  | [[countries(1).description]] = Afghanistan |
Examples: 
    | WorkflowName                    | ServiceName               | nameVariable        | emailVariable                | errorOccured |
    | TestSqlWFWithSqlServerCountries | dbo.Pr_CitiesGetCountries | [[countries(*).id]] | [[countries(*).description]] | NO           |

@MSSql
Scenario Outline: Database SqlDB  service DBErrors
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
Examples: 
     | WorkflowName                      | ServiceName     | nameVariable | emailVariable | errorOccured |
     | TestWFWithDBSqlServerErrorProcSql | dbo.willalwayserror | [[name]]     | [[email]]     | YES          |

@MSSql
Scenario Outline: Database SqlDB  service using int indexes 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name     | <nameVariable>  |
	  |                  |               | email    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                         |
	  | [[rec(1).name]] = dora                  |
	  | [[rec(1).email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName                  | ServiceName | nameVariable    | emailVariable    | errorOccured |
    | TestWFWithDBSqlServerIntIndex | dbo.SQLEmail    | [[rec(1).name]] | [[rec(1).email]] | NO           |

@MSSql
Scenario Outline: Database SqlDB  service using last indexes 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name     | <nameVariable>  |
	  |                  |               | email    | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                         |
	  | [[rec(1).name]] = dora                  |
	  | [[rec(1).email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName              | ServiceName | nameVariable   | emailVariable   | errorOccured |
    | TestWFWithDBSqlServerLastIndex | dbo.SQLEmail    | [[rec().name]] | [[rec().email]] | NO           |

@MSSql
Scenario Outline: Database SqlDB  service using scalar outputs 
     Given I have a workflow "<WorkflowName>"
	 And "<WorkflowName>" contains a sqlserver database service "<ServiceName>" with mappings as
	  | Input to Service | From Variable | Output from Service | To Variable     |
	  |                  |               | name                | <nameVariable>  |
	  |                  |               | email               | <emailVariable> |
      When "<WorkflowName>" is executed
     Then the workflow execution has "<errorOccured>" error
	 And the "<ServiceName>" in Workflow "<WorkflowName>" debug outputs as
	  |                                  |
	  | [[name]] = dora                  |
	  | [[email]] = dora@explorers.co.za |
Examples: 
    | WorkflowName                | ServiceName  | nameVariable | emailVariable | errorOccured |
    | TestWFWithDBSqlServerScalar | dbo.SQLEmail | [[name]]     | [[email]]     | NO           |

@WorkflowExecution
Scenario: Executing unsaved workflow should execute by ID
	Given I create a new unsaved workflow with name "Unsaved 1"
	And "Unsaved 1" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec(1).a]] | yes   |
	  | [[rec(2).a]] | no    |	 
	  When '1' unsaved WF "Unsaved 1" is executed
	  Then the workflow execution has "NO" error
	  And the "Rec To Convert" in Workflow "Unsaved 1" debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = yes |
	  | 2 | [[rec(2).a]] = no  |
	  Then I create a new unsaved workflow with name "Unsaved 1"
	  And "Unsaved 1" contains an Assign "Assign 1" as
	  | variable    | value |
	  | [[rec(1).a]] | 1   |
	  | [[rec(2).a]] | 2    |	 
	  When '2' unsaved WF "Unsaved 1" is executed	 
	  And the "Assign 1" in Workflow "Unsaved 1" debug outputs as    
	  | # |                    |
	  | 1 | [[rec(1).a]] = 1 |
	  | 2 | [[rec(2).a]] = 2  |

@WorkflowExecution
Scenario:WF with DsfRabbitMq Consume timeout 5
	Given I depend on a valid RabbitMQ server
	And I have a workflow "RabbitMqConsume5mintimeout"
	And "RabbitMqConsume5mintimeout" contains DsfRabbitMQPublish and Queue1 "DsfPublishRabbitMQActivity" into "[[result1]]"
	And "RabbitMqConsume5mintimeout" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" with timeout 5 seconds into "[[result]]"
	When "RabbitMqConsume5mintimeout" is executed
    Then the workflow execution has "No" error
	And the "RabbitMqConsume5mintimeout" has a start and end duration
	And "RabbitMqConsume5mintimeout" Duration is greater or equal to 5 seconds

@WorkflowExecution
Scenario:WF with RabbitMq Consume timeout 5
	Given I depend on a valid RabbitMQ server
	And I have a workflow "RabbitMqConsume5mintimeout"
	And "RabbitMqConsume5mintimeout" contains RabbitMQPublish and Queue1 - CorrelationID "PublishRabbitMQActivity" into "[[result1]]"
	And "RabbitMqConsume5mintimeout" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" with timeout 5 seconds into "[[result]]"
	When "RabbitMqConsume5mintimeout" is executed
    Then the workflow execution has "No" error
	And the "RabbitMqConsume5mintimeout" has a start and end duration
	And "RabbitMqConsume5mintimeout" Duration is greater or equal to 5 seconds
	
@WorkflowExecution
Scenario:WF with RabbitMq Consume with no timeout 
	Given I have a workflow "RabbitMqConsumeNotimeout"
	And "RabbitMqConsumeNotimeout" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" with timeout -1 seconds into "[[result]]"
	When "RabbitMqConsumeNotimeout" is executed
    Then the workflow execution has "No" error
	And the "RabbitMqConsumeNotimeout" has a start and end duration
	And "RabbitMqConsumeNotimeout" Duration is less or equal to 60 seconds

@WorkflowExecution
Scenario: COM DLL service execute
	Given I have a server at "localhost" with workflow "Testing COM DLL Activity Execute"
	When "localhost" is the active environment used to execute "Testing COM DLL Activity Execute"
    Then the workflow execution has "No" error
	And the "Com DLL" in Workflow "Testing COM DLL Activity Execute" debug outputs is
	|                                |
	| [[PrimitiveReturnValue]] = 0   |

@WorkflowExecution
Scenario: Workflow with ForEach and Manual Loop
      Given I have a workflow "WFWithForEachWithManualLoop"
	  And "WFWithForEachWithManualLoop" contains an Assign "Setup Counter" as
	    | variable    | value |
	    | [[counter]] | 0     |	
	  And "WFWithForEachWithManualLoop" contains an Assign "Increment Counter" as
	    | variable    | value          |
	    | [[counter]] | =[[counter]]+1 |
	  And "WFWithForEachWithManualLoop" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2"
	  And "ForEachTest" contains an Assign "MyAssign" as
	    | variable    | value |
	    | [[rec().a]] | Test  |
	  And "WFWithForEachWithManualLoop" contains a Decision "Check Counter" as
		| ItemToCheck | Condition | ValueToCompareTo | TrueArmToolName | FalseArmToolName  |
		| [[counter]] | =         | 3                | End Result      | Increment Counter |	  	 	  
	  And "WFWithForEachWithManualLoop" contains an Assign "End Result" as
	    | variable   | value |
	    | [[result]] | DONE  |	 
      When "WFWithForEachWithManualLoop" is executed
	  Then the workflow execution has "NO" error
	  And the "ForEachTest" number '1' in WorkFlow "WFWithForEachWithManualLoop" debug inputs as 
	    |                 | Number |
	    | No. of Executes | 2      |
      And the "ForEachTest" number '1' in WorkFlow "WFWithForEachWithManualLoop" has "2" nested children 
	  And the "MyAssign" in step 1 for "ForEachTest" number '1' debug inputs as
	    | # | Variable      | New Value |
	    | 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 1 for "ForEachTest" number '1' debug outputs as
		| # |                     |
		| 1 | [[rec(1).a]] = Test |
	  And the "MyAssign" in step 2 for "ForEachTest" number '1' debug inputs as
		| # | Variable      | New Value |
		| 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 2 for "ForEachTest" number '1' debug outputs as
		| # |                     |
		| 1 | [[rec(2).a]] = Test |
	  And the "ForEachTest" number '2' in WorkFlow "WFWithForEachWithManualLoop" debug inputs as 
	    |                 | Number |
	    | No. of Executes | 2      |
      And the "ForEachTest" number '2' in WorkFlow "WFWithForEachWithManualLoop" has "2" nested children 
	  And the "MyAssign" in step 1 for "ForEachTest" number '2' debug inputs as
	    | # | Variable      | New Value |
	    | 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 1 for "ForEachTest" number '2' debug outputs as
		| # |                     |
		| 1 | [[rec(3).a]] = Test |
	  And the "MyAssign" in step 2 for "ForEachTest" number '2' debug inputs as
		| # | Variable      | New Value |
		| 1 | [[rec().a]] = | Test      |
	  And the "MyAssign" in step 2 for "ForEachTest" number '2' debug outputs as
		| # |                     |
		| 1 | [[rec(4).a]] = Test |
