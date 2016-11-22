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
