@DecDialog
Feature: DecDialog
	In order to create a decision
	As a Warewolf User
	I want to be shown the decision window setup

@DecDialog
Scenario Outline: Ensure Inputs are enabled on decision window load
	Given I have a workflow New Workflow
	And drop a Decision tool onto the design surface
	Then the Decision window is opened
	And "<Inputs>" fields are "Enabled"
	And the decision match variables "<Variable>"and match "<Variable2>" and to match"<Variable3>"
	And MatchType  is "<MatchType>"
	Then the inputs are "<Inputs>"
		Examples: 
		| No | Inputs                             | Variable | Variable2 | Value2 | Variable3 | Value3 | MatchType       |
		| 1  | Visible, Visible, Visible          | [[a]]    | [[b]]     | 12     |           |        | <               |
		| 2  | Visible, Visible                   | [[a]]    |           |        |           |        | is Alphanumeric |
		| 3  | Visible, Visible, Visible, Visible | [[a]]    | [[b]]     | 5      | [[c]]     | 15     | Is Between      |

@Ignore
Scenario: Ensuring decision text is visible under tool
	Given I have a workflow New Workflow
	And drop a Decision tool onto the design surface
	Then the Decision window is opened
	And a decision variable "[[A]]" operation "=" right  "123 234" position "0"
	Then the Decision tool window is closed

@Ignore
Scenario: Ensure Decision window caches correctly
	Given I have a workflow New Workflow
	And drop a Decision tool onto the design surface
	Then the Decision window is opened
	And a decision variable "[[A]]" operation "=" right  "a123 234" position "0"
	And a decision variable "[[B]]" operation "=" right  "1a23" position "1"
	And "Require all decisions to be true" is "True"
	And the Decision window is opened
	When I change decision variable position "0" to "3a111"
	And "Done" is selected
	Then I open the Decision tool window
	And decision variable "[[B]]" is not visible
	And "3a111" is visible in Match field
	And "Require all decisions to be true" has a value of "True"
@Ignore
Scenario: Validation on incorrectly formatted variables
	Given I have a workflow New Workflow
	And drop a Decision tool onto the design surface
	And a decision variable "[[A]]}" value ""
	And a decision variable "[[rec().a]]" value "28/08/2015"	
	And Match Type equals "="
	And "Done" is selected
@Ignore
Scenario Outline: Ensure Match Type droplist is populated correctly
	Given I have a workflow New Workflow
	And drop a Decision tool onto the design surface
	And the Decision window is opened
	And I select the "Match Type" menu
	And Match Type has "<options>" visible
	Examples: 
	| options            |
	| There is An Error  |
	| There is No Error  |
	| =                  |
	| >                  |
	| <                  |
	| <> (Not Equal)     |
	| >=                 |
	| <=                 |
	| Starts With        |
	| Ends With          |
	| Contains           |
	| Doesn"t Start With |
	| Doesn"t End With   |
	| Doesn"t Contain    |
	| Is Alphanumeric    |
	| Is Base64          |
	| Is Between         |
	| Is Binary          |
	| Is Date            |
	| Is Email           |
	| Is Hex             |
	| Is Numeric         |
	| Is RegEx           |
	| Is Text            |
	| Is Xml             |
	| Not Alphanumeric   |
	| Not Base64         |
	| Not Between        |
	| Not Binary         |
	| Not Date           |
	| Not Email          |
	| Not Hex            |
	| Not Numeric        |
	| Not RegEx          |
	| Not Text           |
	| Not Xml            |