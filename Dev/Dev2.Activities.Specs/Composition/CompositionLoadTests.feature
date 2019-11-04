@CompositionLoadTests
Feature: CompositionLoadTests
	In order to execute a workflow
	As a Warewolf user
	I want to be able to build workflows and execute them against the server
	 
Background: Setup for workflow execution
	Given Debug events are reset
	And Debug states are cleared

Scenario: Workflow with AsyncLogging and ForEach
    Given I have a workflow "WFWithAsyncLoggingForEach"
    And "WFWithAsyncLoggingForEach" contains a Foreach "ForEachTest" as "NumOfExecution" executions "2000"
	And "ForEachTest" contains an Assign "Rec To Convert" as
	| variable    | value |
	| [[Warewolf]] | bob   |
	When "WFWithAsyncLoggingForEach" is executed
	Then the workflow execution has "NO" error
	And I set logging to "Debug"
	When "WFWithAsyncLoggingForEach" is executed "first time"
	Then the workflow execution has "NO" error
	And I set logging to "OFF"
	When "WFWithAsyncLoggingForEach" is executed "second time"
	Then the workflow execution has "NO" error
	And the delta between "first time" and "second time" is less than "2600" milliseconds

Scenario: Simple workflow assigning to 100 records executing against the server
	 Given I have a workflow "WorkflowWithAssign"
	 And "WorkflowWithAssign" contains an Assign "Rec To Convert" as
	  | variable    | value     |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | monkey    |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  | [[rec().a]] | probably  |
	  | [[rec().a]] | certainly |
	  | [[rec().a]] | yes       |
	  | [[rec().a]] | no        |
	  | [[rec().a]] | maybe     |
	  | [[rec().a]] | possibly  |
	  When "WorkflowWithAssign" is executed
	  Then the workflow execution has "NO" error
	  And the "WorkflowWithAssign" has a start and end duration
	  And the "Rec To Convert" in WorkFlow "WorkflowWithAssign" debug inputs as
	  | #   | Variable      | New Value |
	  | 1   | [[rec().a]] = | yes       |
	  | 2   | [[rec().a]] = | no        |
	  | 3   | [[rec().a]] = | maybe     |
	  | 4   | [[rec().a]] = | possibly  |
	  | 5   | [[rec().a]] = | probably  |
	  | 6   | [[rec().a]] = | certainly |
	  | 7   | [[rec().a]] = | yes       |
	  | 8   | [[rec().a]] = | no        |
	  | 9   | [[rec().a]] = | maybe     |
	  | 10  | [[rec().a]] = | possibly  |
	  | 11  | [[rec().a]] = | probably  |
	  | 12  | [[rec().a]] = | certainly |
	  | 13  | [[rec().a]] = | yes       |
	  | 14  | [[rec().a]] = | no        |
	  | 15  | [[rec().a]] = | maybe     |
	  | 16  | [[rec().a]] = | possibly  |
	  | 17  | [[rec().a]] = | probably  |
	  | 18  | [[rec().a]] = | certainly |
	  | 19  | [[rec().a]] = | yes       |
	  | 20  | [[rec().a]] = | no        |
	  | 21  | [[rec().a]] = | maybe     |
	  | 22  | [[rec().a]] = | possibly  |
	  | 23  | [[rec().a]] = | probably  |
	  | 24  | [[rec().a]] = | certainly |
	  | 25  | [[rec().a]] = | yes       |
	  | 26  | [[rec().a]] = | no        |
	  | 27  | [[rec().a]] = | maybe     |
	  | 28  | [[rec().a]] = | possibly  |
	  | 29  | [[rec().a]] = | probably  |
	  | 30  | [[rec().a]] = | certainly |
	  | 31  | [[rec().a]] = | yes       |
	  | 32  | [[rec().a]] = | no        |
	  | 33  | [[rec().a]] = | maybe     |
	  | 34  | [[rec().a]] = | possibly  |
	  | 35  | [[rec().a]] = | probably  |
	  | 36  | [[rec().a]] = | certainly |
	  | 37  | [[rec().a]] = | yes       |
	  | 38  | [[rec().a]] = | no        |
	  | 39  | [[rec().a]] = | maybe     |
	  | 40  | [[rec().a]] = | possibly  |
	  | 41  | [[rec().a]] = | probably  |
	  | 42  | [[rec().a]] = | certainly |
	  | 43  | [[rec().a]] = | yes       |
	  | 44  | [[rec().a]] = | no        |
	  | 45  | [[rec().a]] = | monkey    |
	  | 46  | [[rec().a]] = | possibly  |
	  | 47  | [[rec().a]] = | probably  |
	  | 48  | [[rec().a]] = | certainly |
	  | 49  | [[rec().a]] = | yes       |
	  | 50  | [[rec().a]] = | no        |
	  | 51  | [[rec().a]] = | maybe     |
	  | 52  | [[rec().a]] = | possibly  |
	  | 53  | [[rec().a]] = | probably  |
	  | 54  | [[rec().a]] = | certainly |
	  | 55  | [[rec().a]] = | yes       |
	  | 56  | [[rec().a]] = | no        |
	  | 57  | [[rec().a]] = | maybe     |
	  | 58  | [[rec().a]] = | possibly  |
	  | 59  | [[rec().a]] = | probably  |
	  | 60  | [[rec().a]] = | certainly |
	  | 61  | [[rec().a]] = | yes       |
	  | 62  | [[rec().a]] = | no        |
	  | 63  | [[rec().a]] = | maybe     |
	  | 64  | [[rec().a]] = | possibly  |
	  | 65  | [[rec().a]] = | probably  |
	  | 66  | [[rec().a]] = | certainly |
	  | 67  | [[rec().a]] = | yes       |
	  | 68  | [[rec().a]] = | no        |
	  | 69  | [[rec().a]] = | maybe     |
	  | 70  | [[rec().a]] = | possibly  |
	  | 71  | [[rec().a]] = | probably  |
	  | 72  | [[rec().a]] = | certainly |
	  | 73  | [[rec().a]] = | yes       |
	  | 74  | [[rec().a]] = | no        |
	  | 75  | [[rec().a]] = | maybe     |
	  | 76  | [[rec().a]] = | possibly  |
	  | 77  | [[rec().a]] = | probably  |
	  | 78  | [[rec().a]] = | certainly |
	  | 79  | [[rec().a]] = | yes       |
	  | 80  | [[rec().a]] = | no        |
	  | 81  | [[rec().a]] = | maybe     |
	  | 82  | [[rec().a]] = | possibly  |
	  | 83  | [[rec().a]] = | probably  |
	  | 84  | [[rec().a]] = | certainly |
	  | 85  | [[rec().a]] = | yes       |
	  | 86  | [[rec().a]] = | no        |
	  | 87  | [[rec().a]] = | maybe     |
	  | 88  | [[rec().a]] = | possibly  |
	  | 89  | [[rec().a]] = | probably  |
	  | 90  | [[rec().a]] = | certainly |
	  | 91  | [[rec().a]] = | yes       |
	  | 92  | [[rec().a]] = | no        |
	  | 93  | [[rec().a]] = | maybe     |
	  | 94  | [[rec().a]] = | possibly  |
	  | 95  | [[rec().a]] = | probably  |
	  | 96  | [[rec().a]] = | certainly |
	  | 97  | [[rec().a]] = | yes       |
	  | 98  | [[rec().a]] = | no        |
	  | 99  | [[rec().a]] = | maybe     |
	  | 100 | [[rec().a]] = | possibly  |

	  And the "Rec To Convert" in Workflow "WorkflowWithAssign" debug outputs as    
	  | #   |                            |
	  | 1   | [[rec(1).a]] = yes         |
	  | 2   | [[rec(2).a]] = no          |
	  | 3   | [[rec(3).a]] = maybe       |
	  | 4   | [[rec(4).a]] = possibly    |
	  | 5   | [[rec(5).a]] = probably    |
	  | 6   | [[rec(6).a]] = certainly   |
	  | 7   | [[rec(7).a]] = yes         |
	  | 8   | [[rec(8).a]] = no          |
	  | 9   | [[rec(9).a]] = maybe       |
	  | 10  | [[rec(10).a]] = possibly   |
	  | 11  | [[rec(11).a]] = probably   |
	  | 12  | [[rec(12).a]] = certainly  |
	  | 13  | [[rec(13).a]] = yes        |
	  | 14  | [[rec(14).a]] = no         |
	  | 15  | [[rec(15).a]] = maybe      |
	  | 16  | [[rec(16).a]] = possibly   |
	  | 17  | [[rec(17).a]] = probably   |
	  | 18  | [[rec(18).a]] = certainly  |
	  | 19  | [[rec(19).a]] = yes        |
	  | 20  | [[rec(20).a]] = no         |
	  | 21  | [[rec(21).a]] = maybe      |
	  | 22  | [[rec(22).a]] = possibly   |
	  | 23  | [[rec(23).a]] = probably   |
	  | 24  | [[rec(24).a]] = certainly  |
	  | 25  | [[rec(25).a]] = yes        |
	  | 26  | [[rec(26).a]] = no         |
	  | 27  | [[rec(27).a]] = maybe      |
	  | 28  | [[rec(28).a]] = possibly   |
	  | 29  | [[rec(29).a]] = probably   |
	  | 30  | [[rec(30).a]] = certainly  |
	  | 31  | [[rec(31).a]] = yes        |
	  | 32  | [[rec(32).a]] = no         |
	  | 33  | [[rec(33).a]] = maybe      |
	  | 34  | [[rec(34).a]] = possibly   |
	  | 35  | [[rec(35).a]] = probably   |
	  | 36  | [[rec(36).a]] = certainly  |
	  | 37  | [[rec(37).a]] = yes        |
	  | 38  | [[rec(38).a]] = no         |
	  | 39  | [[rec(39).a]] = maybe      |
	  | 40  | [[rec(40).a]] = possibly   |
	  | 41  | [[rec(41).a]] = probably   |
	  | 42  | [[rec(42).a]] = certainly  |
	  | 43  | [[rec(43).a]] = yes        |
	  | 44  | [[rec(44).a]] = no         |
	  | 45  | [[rec(45).a]] = monkey     |
	  | 46  | [[rec(46).a]] = possibly   |
	  | 47  | [[rec(47).a]] = probably   |
	  | 48  | [[rec(48).a]] = certainly  |
	  | 49  | [[rec(49).a]] = yes        |
	  | 50  | [[rec(50).a]] = no         |
	  | 51  | [[rec(51).a]] = maybe      |
	  | 52  | [[rec(52).a]] = possibly   |
	  | 53  | [[rec(53).a]] = probably   |
	  | 54  | [[rec(54).a]] = certainly  |
	  | 55  | [[rec(55).a]] = yes        |
	  | 56  | [[rec(56).a]] = no         |
	  | 57  | [[rec(57).a]] = maybe      |
	  | 58  | [[rec(58).a]] = possibly   |
	  | 59  | [[rec(59).a]] = probably   |
	  | 60  | [[rec(60).a]] = certainly  |
	  | 61  | [[rec(61).a]] = yes        |
	  | 62  | [[rec(62).a]] = no         |
	  | 63  | [[rec(63).a]] = maybe      |
	  | 64  | [[rec(64).a]] = possibly   |
	  | 65  | [[rec(65).a]] = probably   |
	  | 66  | [[rec(66).a]] = certainly  |
	  | 67  | [[rec(67).a]] = yes        |
	  | 68  | [[rec(68).a]] = no         |
	  | 69  | [[rec(69).a]] = maybe      |
	  | 70  | [[rec(70).a]] = possibly   |
	  | 71  | [[rec(71).a]] = probably   |
	  | 72  | [[rec(72).a]] = certainly  |
	  | 73  | [[rec(73).a]] = yes        |
	  | 74  | [[rec(74).a]] = no         |
	  | 75  | [[rec(75).a]] = maybe      |
	  | 76  | [[rec(76).a]] = possibly   |
	  | 77  | [[rec(77).a]] = probably   |
	  | 78  | [[rec(78).a]] = certainly  |
	  | 79  | [[rec(79).a]] = yes        |
	  | 80  | [[rec(80).a]] = no         |
	  | 81  | [[rec(81).a]] = maybe      |
	  | 82  | [[rec(82).a]] = possibly   |
	  | 83  | [[rec(83).a]] = probably   |
	  | 84  | [[rec(84).a]] = certainly  |
	  | 85  | [[rec(85).a]] = yes        |
	  | 86  | [[rec(86).a]] = no         |
	  | 87  | [[rec(87).a]] = maybe      |
	  | 88  | [[rec(88).a]] = possibly   |
	  | 89  | [[rec(89).a]] = probably   |
	  | 90  | [[rec(90).a]] = certainly  |
	  | 91  | [[rec(91).a]] = yes        |
	  | 92  | [[rec(92).a]] = no         |
	  | 93  | [[rec(93).a]] = maybe      |
	  | 94  | [[rec(94).a]] = possibly   |
	  | 95  | [[rec(95).a]] = probably   |
	  | 96  | [[rec(96).a]] = certainly  |
	  | 97  | [[rec(97).a]] = yes        |
	  | 98  | [[rec(98).a]] = no         |
	  | 99  | [[rec(99).a]] = maybe      |
	  | 100 | [[rec(100).a]] = possibly  |
 