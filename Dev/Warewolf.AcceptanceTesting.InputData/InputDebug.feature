Feature:Ensuring the Input Debug window is fully functional
# Open Debug window to add inputs
# Working with tabs in Debug window
# Message when there are no inputs
# Incorrect Xml syntax entered
# Incorrect Json syntax entered

Scenario: Open Debug window to add inputs
	Given I a new workflow
	And I have variable "[[a]]" set as "Input"
	And I have variable "[[b]]" set as "Output"
	When I press "F5"
	And variable "[[a]]" is visible in the Input Debug window
	And variable "[[b]]" is not visible
	When I give variable "[[a]]" the value "Test"
	And I press "F6"
	Then the Input Data window is closed
	And the execution has "No" error
	And the debug output window appears as
	| Output |
	| [[b]]  |

	
Scenario Outline: Working with tabs in Debug window
	Given I have a new workflow
	And I have variable '<variable1>' set as '<type>'
	And I have variable '<variable2>' set as '<type2>'
	And variable '<variable2>' equals "Success <variable1>"
	And I press '<Debug>'
	Then the Input Data window is open
	And "Input Data" tab is visible
	And "Xml" tab header is visible
	And "Json" tab header is visible
	And I assign '<variable1>' the value '<value>'
	When I switch to the '<Mode>' tab
	And '<value>' is visible in the '<Mode>' tab
	And remember debug input equals "Checked"
	When I press '<launch>'
	And the execution has '<error>' error
	Then '<response>'
	Examples: 
	| variable1     | Value | type  | variable2 | type2  | Debug | Mode       | launch | Response                                     |
	| [[a]]         | Test  | input | [[b]]     | output | F5    | Input Data | F6     | [[b]] = Test                                 |
	| [[rec().set]] | Test  |       | [[b]]     | output | F6    | Input Data |        | [[b]] = Test                                 |
	| [[a]]         | Test  | input | [[b]]     | output | F5    | Input Data | F7     | <DataList><b>Successful Test.</b></DataList> |
	| [[var]]       | Test  | input | [[b]]     | output | F5    | Xml        | F6     | [[b]] = Test                                 |
	| [[rec().set]] | Test  |       | [[b]]     | output | F6    | Xml        |        | [[b]] = Test                                 |
	| [[a]]         | Test  | input | [[b]]     | output | F5    | Xml        | F7     | <DataList><b>Successful Test.</b></DataList> |
	| [[var]]       | Test  | input | [[b]]     | output | F5    | Json       | F6     | [[b]] = Test                                 |
	| [[rec().set]] | Test  |       | [[b]]     | output | F6    | Json       |        | [[b]] = Test                                 |
	| [[a]]         | Test  | input | [[b]]     | output | F5    | Json       | F7     | <DataList><b>Successful Test.</b></DataList> |	


Scenario: Message when there are no inputs
	Given I have a new workflow
	And I press 'F5'
	Then the Input Debug window is opened
	And "Mark the Input checkbox in the variable window to set workflow inputs" message is visible

Scenario: Incorrect Xml syntax entered
	Given I a new workflow
	And I have variable "[[variable1]]" set as Input
	And I press 'F5'
	When I switch to the "Xml" tab
	And the xml "<DataList><a></a></DataList>" is visible
	And I insert "<DataList><a>Test</a><DataList"
	And I execute the input
	And the execution has "An" error
	Then error message "Incorrectly formatted Xml has been entered" is visible

Scenario: Incorrect Json syntax entered
	Given I a new workflow
	And I have variable "[[variable1]]" set as Input
	And I press 'F5'
	When I switch to the "Xml" tab
	And the json "{"variable": ""}" is visible
	And I insert "{"variable": "test"variable2}"
	And I execute the input "F6"
	And the execution has "An" error
	Then error message "Incorrectly formatted Json has been entered" is visible
	And the "Input Data" window remains opened

Scenario: Shortcut Keys now working after error
	Given I a new workflow
	And I have variable "[[variable1]]" set as Input
	And I press 'F5'
	When I switch to the "Xml" tab
	And the json "{"variable": ""}" is visible
	And I insert "{"variable": "test"variable2}"
	And I execute the input using "F6"
	And the execution has "An" error
	Then error message "Incorrectly formatted Json has been entered" is visible
	And the "Input Data" window remains opened
	And I added {"variable": "test"}"
	And I execute the input using "F6" 
	And the execution has "No" error