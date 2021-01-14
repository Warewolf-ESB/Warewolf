@StudioTestFrameworkWithUtilityTools
Feature: StudioTestFrameworkWithUtilityTools
	In order to test workflows that contain utility tools in warewolf 
	As a user
	I want to create, edit, delete and update tests in a test window


Background: Setup for workflows for tests
		Given test folder is cleaned	
		And I have "Workflow 1" with inputs as
			| Input Var Name |
			| [[a]]          |
		And "Workflow 1" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |		
		Given I have "Workflow 2" with inputs as
			| Input Var Name |
			| [[rec().a]]    |
			| [[rec().b]]    |
		And "Workflow 2" has outputs as
			| Ouput Var Name |
			| [[returnVal]]  |
		Given I have "Workflow 3" with inputs as
			| Input Var Name |
			| [[A]]              |
			| [[B]]              |
			| [[C]]              |
		And "Workflow 3" has outputs as
			| Ouput Var Name |
			| [[message]]    |
		Given I have "WorkflowWithTests" with inputs as 
			| Input Var Name |
			| [[input]]      |
		And "WorkflowWithTests" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |			
		And "WorkflowWithTests" Tests as 
			| TestName | AuthenticationType | Error | TestFailing | TestPending | TestInvalid | TestPassed |
			| Test1    | Windows            | false | false       | false       | false       | true       |
			| Test2    | Windows            | false | true        | false       | false       | false      |
			| Test3    | Windows            | false | false       | false       | true        | false      |
			| Test4    | Windows            | false | false       | true        | false       | false      |

Scenario: Test WF with Random
	Given I have a workflow "RandomTestWF"
	And "RandomTestWF" contains Random "TestRandoms" as
	  | Type    | From | To | Result     |
	  | Numbers | 1    | 10 | [[result]] |
	And I save workflow "RandomTestWF"
	Then the test builder is open with "RandomTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestRandoms" as TestStep	
	And I add StepOutputs as 
	  	 | Variable Name | Condition  | Value |
	  	 | [[result]]    | Is Numeric |       |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RandomTestWF" is deleted as cleanup

Scenario: Test WF with Aggregate Calculate
	Given I have a workflow "AggrCalculateTestWF"
	And "AggrCalculateTestWF" contains an Assign "values1" as
      | variable | value |
      | [[a]]    | 31     |
      | [[b]]    | 15     |
      | [[c]]    | 8     |
      | [[d]]    | 24     |
	And "AggrCalculateTestWF" contains Calculate "TestAgrCalculate" with formula "Min([[a]],[[b]],[[c]],[[d]])" into "[[result]]"
	And I save workflow "AggrCalculateTestWF"
	Then the test builder is open with "AggrCalculateTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestAgrCalculate" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 8     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "AggrCalculateTestWF" is deleted as cleanup

Scenario: Test WF with WebRequest
	Given I depend on a valid HTTP web server
	And I have a workflow "WebRequestTestWF"
	And "WebRequestTestWF" contains WebRequest "TestWebRequest" as
	| Result       | Url                                            |
	| "[[Result]]" | http://TFSBLD.premier.local:9810/api/products/Get |
	And I save workflow "WebRequestTestWF"
	Then the test builder is open with "WebRequestTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestWebRequest" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value                                                                                                                                                                                                                                                                                                                                                                                                            |
	  	 | [[Result]]    | Contains  | [{"Id":1,"Name":"Television","Category":"Electronic","Price":82000.0},{"Id":2,"Name":"Refrigerator","Category":"Electronic","Price":23000.0},{"Id":3,"Name":"Mobiles","Category":"Electronic","Price":20000.0},{"Id":4,"Name":"Laptops","Category":"Electronic","Price":45000.0},{"Id":5,"Name":"iPads","Category":"Electronic","Price":67000.0},{"Id":6,"Name":"Toys","Category":"Gift Items","Price":15000.0}] |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "WebRequestTestWF" is deleted as cleanup

Scenario: Test WF with RabbitMq Publish
	Given I have a workflow "RabbitMqPubTestWF"
	And "RabbitMqPubTestWF" contains RabbitMQPublish "DsfPublishRabbitMQActivity" into "[[result]]"
	And I save workflow "RabbitMqPubTestWF"
	Then the test builder is open with "RabbitMqPubTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "DsfPublishRabbitMQActivity" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value                                         |
	  	 | [[result]]    | =         | Failure: Queue Name and Message are required. |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RabbitMqPubTestWF" is deleted as cleanup
	
Scenario: Test WF with RabbitMq Consume
	Given I have a workflow "RabbitMqConsumeTestFailWF"
	And "RabbitMqConsumeTestFailWF" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" into "[[result]]"
	And I save workflow "RabbitMqConsumeTestFailWF"
	Then the test builder is open with "RabbitMqConsumeTestFailWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "DsfConsumeRabbitMQActivity" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value                                         |
	  	 | [[result]]    | =         | Failure: Queue Name is required. |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RabbitMqConsumeTestFailWF" is deleted as cleanup

Scenario: Test WF with RabbitMq Consume object result
	Given I have a workflow "RabbitMqConsumeObjectTestFailWF"
	And "RabbitMqConsumeObjectTestFailWF" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" into ""
	And "RabbitMqConsumeObjectTestFailWF" is object is set to "true"
	And "RabbitMqConsumeObjectTestFailWF" objectname as "[[@result]]"
	And I save workflow "RabbitMqConsumeObjectTestFailWF"
	Then the test builder is open with "RabbitMqConsumeObjectTestFailWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "DsfConsumeRabbitMQActivity" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value                                         |
	  	 | [[@result]]   | =         | Failure: Queue Name and Message are required. |
	When I save
	And I run the test
	Then test result is Failed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RabbitMqConsumeObjectTestFailWF" is deleted as cleanup

Scenario: Test WF with RabbitMq Consume object Array result 
	Given I have a workflow "RabbitMqConsumeObjectResultTestFailWF"
	And "RabbitMqConsumeObjectResultTestFailWF" contains a Foreach "ForEachTest" as "NumOfExecution" executions "3"		
    And "ForEachTest" contains a RabbitMQPublish "DsfPublishRabbitMQActivity" into "[[publishResult]]" 
	And "RabbitMqConsumeObjectResultTestFailWF" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" into ""
	And "RabbitMqConsumeObjectResultTestFailWF" is object is set to "true"
	And "RabbitMqConsumeObjectResultTestFailWF" objectname as "[[@result()]]"
	And I save workflow "RabbitMqConsumeObjectResultTestFailWF"
	Then the test builder is open with "RabbitMqConsumeObjectResultTestFailWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "DsfConsumeRabbitMQActivity" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value                                         |
	  	 | [[@result()]]   | =         | Failure: Queue Name and Message are required. |
	When I save
	And I run the test
	Then test result is Failed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RabbitMqConsumeObjectResultTestFailWF" is deleted as cleanup
	
Scenario: Test WF with RabbitMq Consume and count Recordset
	Given I have a workflow "RabbitMqConsumeAndCountTestFailWF"
	And "RabbitMqConsumeAndCountTestFailWF" contains RabbitMQConsume "DsfConsumeRabbitMQActivity" into "[[msgRec().msgs]]" 
	And Queue Name as "TestQuestForSpecsEmptyResults"
	And "RabbitMqConsumeAndCountTestFailWF" contains Count Record "CountRec" on "[[msgRec()]]" into "[[count]]"
	And I save workflow "RabbitMqConsumeAndCountTestFailWF"
	Then the test builder is open with "RabbitMqConsumeAndCountTestFailWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
   	And I Add "CountRec" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[count]]     | =         | 0     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RabbitMqConsumeAndCountTestFailWF" is deleted as cleanup

Scenario: Test WF with Calculate
	Given I have a workflow "CalculateTestWF"
	And "CalculateTestWF" contains an Assign "values1" as
      | variable | value |
      | [[a]]    | 1     |
      | [[b]]    | 5     |
	And "CalculateTestWF" contains Calculate "TestCalculate" with formula "Sum([[a]],[[b]])" into "[[result]]"
	And I save workflow "CalculateTestWF"
	Then the test builder is open with "CalculateTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestCalculate" as TestStep
And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 6     |		 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "CalculateTestWF" is deleted as cleanup

Scenario: Test WF with Calculate outputs with no variable
	Given I have a workflow "CalculateTestWF"
	And "CalculateTestWF" contains an Assign "values1" as
      | variable | value |
      | [[a]]    | 1     |
      | [[b]]    | 5     |
	And "CalculateTestWF" contains Calculate "TestCalculate" with formula "Sum([[a]],[[b]])" into "[[result]]"
	And I save workflow "CalculateTestWF"
	Then the test builder is open with "CalculateTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestCalculate" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 |               | =         |       |		 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "CalculateTestWF" is deleted as cleanup	

#WOLF-2280
Scenario: Test WF with Calculate No outPuts
	Given I have a workflow "CalculateTestNoOutputsWF"
	And "CalculateTestNoOutputsWF" contains an Assign "values1" as
      | variable | value |
      | [[a]]    | 1     |
      | [[b]]    | 5     |
	And "CalculateTestNoOutputsWF" contains Calculate "TestCalculate" with formula "Sum([[a]],[[b]])" into ""
	And I save workflow "CalculateTestNoOutputsWF"
	Then the test builder is open with "CalculateTestNoOutputsWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "TestCalculate" as TestStep	 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "CalculateTestNoOutputsWF" is deleted as cleanup

Scenario: Test WF with Xpath
	Given I have a workflow "XPathTestWF"
	And "XPathTestWF" contains XPath \"XPathTest" with source "//XPATH-EXAMPLE/CUSTOMER[@id='2' or @type='C']/text()"
	And I save workflow "XPathTestWF"
	Then the test builder is open with "XPathTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "XPathTest" as TestStep
And I add StepOutputs as 
	  	 | Variable Name   | Condition | Value        |
	  	 | [[singleValue]] | =         | Mr.  Johnson |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "XPathTestWF" is deleted as cleanup

Scenario: Test WF with SysInfo
	Given I have a workflow "SysInfoTestWF"
	And "SysInfoTestWF" contains Gather System Info "System info" as
	| Variable | Selected    |
	| [[a]]    | Date & Time |
	And I save workflow "SysInfoTestWF"
	Then the test builder is open with "SysInfoTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "System info" as TestStep
And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[a]]         | Is Date   |       |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "SysInfoTestWF" is deleted as cleanup

Scenario: Test WF with FormatNumber
	Given I have a workflow "FormatNumberTestWF"
	And "FormatNumberTestWF" contains Format Number "Fnumber" as 
	| Number  | Rounding Selected | Rounding To | Decimal to show | Result     |
	| 12.3412 | Up                | 3           | 2               | [[result]] |
	And I save workflow "FormatNumberTestWF"
	Then the test builder is open with "FormatNumberTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "Fnumber" as TestStep
And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 12.34 |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "FormatNumberTestWF" is deleted as cleanup

Scenario: Test WF with Count Record
	Given I have a workflow "CountRecTestWF"
	And "CountRecTestWF&2Delete" contains an Assign "countrecordval1" as
	  | variable    | value |
	  | [[rec().a]] | 21    |
	  | [[rec().a]] | 22    |
	  | [[rec().a]] |       |
	  And "CountRecTestWF&2Delete" contains Count Record "Cnt1" on "[[rec()]]" into "[[result]]"
	And I save workflow "CountRecTestWF"
	Then the test builder is open with "CountRecTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "Cnt1" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 3     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "CountRecTestWF" is deleted as cleanup

Scenario: Test WF with Lenght
	Given I have a workflow "LenghtTestWF"
	And "LenghtTestWF" contains an Assign "Rec To Convert" as
	  | variable    | value |
	  | [[rec().a]] | 1213  |
	  | [[rec().a]] | 4561  |
	  And "LenghtTestWF" contains Length "Len" on "[[rec(*)]]" into "[[result]]"
	And I save workflow "LenghtTestWF"
	Then the test builder is open with "LenghtTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "Len" as TestStep
And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 2     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "LenghtTestWF" is deleted as cleanup

Scenario: Test WF with Find Records
	Given I have a workflow "FindRecTestWF"
	 And "FindRecTestWF" contains an Assign "Record" as
      | variable     | value |
      | [[rec(1).a]] | 23    |
      | [[rec(2).a]] | 34    |
      | [[rec(3).a]]  | 10    |
	  And "FindRecTestWF" contains Find Record Index "FindRecord0" into result as "[[result]]"
	  | # | In Field    | # | Match Type | Match | Require All Matches To Be True | Require All Fields To Match |
	  | # | [[rec().a]] | 1 | =          | 34    | YES                            | NO                          |
	And I save workflow "FindRecTestWF"
	Then the test builder is open with "FindRecTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "FindRecord0" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 2     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "FindRecTestWF" is deleted as cleanup

Scenario: Test WF with Delete Records
	Given I have a workflow "DeleteRecTestWF"
	And "DeleteRecTestWF" contains an Assign "Assign to delete" as
	  | variable    | value |
	  | [[rec().a]] | 50    |
	  And "DeleteRecTestWF" contains Delete "Delet1" as
	  | Variable   | result     |
	  | [[rec(1)]] | [[result]] |
      And I save workflow "DeleteRecTestWF"
	Then the test builder is open with "DeleteRecTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "Delet1" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value   |
	  	 | [[result]]    | =         | Success |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "DeleteRecTestWF" is deleted as cleanup

Scenario: Test WF with Unique Record
	Given I have a workflow "UniqueTestWF"
	 And "UniqueTestWF" contains an Assign "Records" as
	  | variable      | value |
	  | [[rs().row]]  | 10    |
	  | [[rs().data]] | 10    |
	  | [[rs().row]]  | 40    |
	  | [[rs().data]] | 20    |
	  | [[rs().row]]  | 20    |
	  | [[rs().data]] | 20    |
	  | [[rs().row]]  | 30    |
	  | [[rs().data]] | 40    |
	  And "UniqueTestWF" contains an Unique "Unique rec" as
	  | In Field(s)                  | Return Fields | Result           |
	  | [[rs(*).row]],[[rs(*).data]] | [[rs().row]]  | [[rec().unique]] |
	 And I save workflow "UniqueTestWF"
	Then the test builder is open with "UniqueTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "Unique rec" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name     | Condition | Value |
	  	 | [[rec(1).unique]] | =         | 10    |
	  	 | [[rec(2).unique]] | =         | 40    |
	  	 | [[rec(3).unique]] | =         | 20    |
	  	 | [[rec(4).unique]] | =         | 30    |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "UniqueTestWF" is deleted as cleanup

Scenario: Test WF with Sort
	Given I have a workflow "SortTestWF"
	And "SortTestWF" contains an Assign "sortval5" as
	  | variable    | value |
	  | [[rs(1).a]] | 10    |	  	  
	  | [[rs(2).a]] | 20    |
	  And "SortTestWF" contains an Sort "sortRec1" as
	  | Sort Field  | Sort Order |
	  | [[rs(*).a]] | Backwards  |
	 And I save workflow "SortTestWF"
	Then the test builder is open with "SortTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "sortRec1" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value |
	  	 | [[rs(1).a]]   | =         | 20    |
	  	 | [[rs(2).a]]   | =         | 10    |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "SortTestWF" is deleted as cleanup

Scenario: Test WF with DateTime
	Given I have a workflow "DateTimeTestWF"
	And "DateTimeTestWF" contains Date and Time "AddDate" as
	| Input      | Input Format | Add Time | Output Format | Result     |
	| 12 03 2016 | dd mm yyyy   |          | yy mm dd      | [[result]] |
	And I save workflow "DateTimeTestWF"
	Then the test builder is open with "DateTimeTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "AddDate" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value    |
	  	 | [[result]]    | =         | 16 03 12 |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "DateTimeTestWF" is deleted as cleanup

Scenario: Test WF with DateTimeDiff
	Given I have a workflow "DateTimeDiffTestWF"	 	  
	And "DateTimeDiffTestWF" contains Date and Time Difference "DateTimedif" as
       | Input1     | Input2     | Input Format | Output In | Result     |
       | 02 03 2016 | 16 11 2016 | dd mm yyyy   | Days      | [[result]] |  
	And I save workflow "DateTimeDiffTestWF"
	Then the test builder is open with "DateTimeDiffTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "DateTimedif" as TestStep
	And I add StepOutputs as 
	  	 | Variable Name | Condition | Value         |
		 | [[result]]      | =         | 259 |		 
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "DateTimeDiffTestWF" is deleted as cleanup

Scenario: Run a passing Test with RabbitMq Object return
	Given the test builder is open with existing service "RabbitTestWf"	
	And Tab Header is "RabbitTestWf - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "RabbitTestWf - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank	
	And I Add "RabbitMQ Consume" as TestStep
	And I Clear existing StepOutputs
	And I add StepOutputs item as 
	| Variable Name      | Condition | Value |
	| [[@AllMessages()]] | Contains  | A0003 |
    And I add StepOutputs item as 
    | Variable Name      | Condition | Value |
    | [[@AllMessages()]] | Contains  | Bob   |
	And I add StepOutputs item as 
	| Variable Name      | Condition | Value |
	| [[@AllMessages()]] | Contains  | 32    |
	And I Add outputs as
	| Variable Name  | Condition | Value |
	| @AllMessages() | Contains  | 32    |
	And save is enabled
	And test status is pending	
	And test is enabled	
	And I save
	When I run the test
	Then test result is Passed		
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
