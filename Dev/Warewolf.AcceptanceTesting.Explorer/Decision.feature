Feature: Decision
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Decision
Scenario: Opening Decision Large View
	Given I have Decision tool on the design surface
	When I double click on "Decision"
	Then the decision large view is opened
	And  "statement1 of "TextBox1" is "Visible"
	And  "statement1 of "TextBox2" is "NotVisible"
	And "TextBox1" is focussed
	And Evaluates a statement to True or False
	| TextBox1 | ComboBox  | TextBox2 | Delete |
	| ""       | Choose... |          | NO     |
	And Diplay text as "If Choose.."
	And True arm text as "True"
	And False arm text as "False"
	And Require All decision to be True selected as "Yes"
	And Done button is "Visible"


Scenario: Adding Statements in Decision Tool
	Given I have Decision tool on the design surface
	When I double click on "Decision"
	Then the large view is opened
	And  "statement1 of "TextBox1" is "Visible"
	And  "statement1 of "TextBox2" is "NotVisible"
	And "TextBox1" is focussed
	And Evaluates a statement to True or False
	| TextBox1 | ComboBox  | TextBox2 | Delete |
	| ""       | Choose... |          | NO     |
	When I "Add Statement"
	Then Statements are
	| TextBox1 | ComboBox  | TextBox2 | Delete |
	| ""       | Choose... |          | Yes    |
	| ""       | Choose... |          | Yes    |
	And Diplay text as "If Choose.."
	And True arm text as "True"
	And False arm text as "False"
	And Require All decision to be True selected as "Yes"
	And Done button is "Visible"

Scenario: Adding Statements more then five appears scroll bar
	Given Decision large view is opened 
	When I click on "Add Statement" 
	When I click on "Add Statement" 
	When I click on "Add Statement" 
	When I click on "Add Statement" 
	When I click on "Add Statement" 
	When I click on "Add Statement" 
	Then Evaluates a statement to True or False
	| TextBox1 | ComboBox  | TextBox2 | Delete |
	| ""       | Choose... |          | Yes    |
	| ""       | Choose... |          | Yes    |
	| ""       | Choose... |          | Yes    |
	| ""       | Choose... |          | Yes    |
	| ""       | Choose... |          | Yes    |
	| ""       | Choose... |          | Yes    |
	Then scroll bar is "visible" on "Decion large view"
	And Diplay text as "If Choose.."
	And True arm text as "True"
	And False arm text as "False"
	And Require All decision to be True selected as "Yes"
	And Done button is "Visible"


Scenario: Deleting Statements in Decision Tool
	Given Decision large view is opened 
	Then Statements are
	| TextBox1 | ComboBox  | TextBox2 | Delete |
	| ""       | Choose... |          | Yes    |
	| ""       | Choose... |          | Yes    |
	When I delete "statement1"
	Then Evaluates a statement to True or False
	| TextBox1 | ComboBox  | TextBox2 | Delete |
	| ""       | Choose... |          | NO     |



Scenario: Selecting Statement in combobox1
	Given Decision large view is opened 
	And  "statement1 of "TextBox1" is "Visible"
	And  "statement1 of "TextBox2" is "NotVisible"
	#Select There Is An Error
	When I select "statement1" of combobox as "There Is An Error"
	Then "statement1" of "TextBox1" is "NotVisible"
	And  "statement1" of "TextBox2" is "NotVisible"
	#Select There Is No Error
	And I "Add Statement"
	When I select "statement2" of combobox as "There Is No Error"
	Then "statement2" of "TextBox1" is "NotVisible"
	And  "statement2" of "TextBox2" is "NotVisible"
	#Select =
	And I "Add Statement"
	When I select "statement3" of combobox as "="
	Then "statement3" of "TextBox1" is "Visible"
	And  "statement3" of "TextBox2" is "Visible"
	#Select >
	When I select "statement4" of combobox as ">"
	Then "statement4" of "TextBox1" is "Visible"
	And  "statement4" of "TextBox2" is "Visible"
	#Select <
	And I "Add Statement"
	When I select "statement5" of combobox as "<"
	Then "statement5" of "TextBox1" is "Visible"
	And  "statement5" of "TextBox2" is "Visible"
	Then Evaluates a statement to True or False
	| TextBox1 | ComboBox           | TextBox2 | Delete |
	|          | There Is An Error  |          | Yes    |
	|          | There Is No Error  |          | Yes    |
	| ""       | =                  | ""       | Yes    |
	| ""       | >                  | ""       | Yes    |
	| ""       | <                  | ""       | Yes    |
	
	
	
	
Scenario: Selecting Statement in combobox2
	Given Decision large view is opened 
	And  "statement1 of "TextBox1" is "Visible"
	And  "statement1 of "TextBox2" is "NotVisible"	
	#Select <> (Not Equal)
	When I select "statement1" of combobox as "<> (Not Equal)"
	Then "statement1" of "TextBox1" is "Visible"
	And  "statement1" of "TextBox2" is "Visible"
	#Select >=
	And I "Add Statement"
	When I select "statement2" of combobox as ">="
	Then "statement2" of "TextBox1" is "Visible"
	And  "statement2" of "TextBox2" is "Visible"
	#Select <=
	And I "Add Statement"
	When I select "statement3" of combobox as "<="
	Then "statement3" of "TextBox1" is "Visible"
	And  "statement3 of "TextBox2" is "Visible"
	#Select Starts With
	And I "Add Statement"
	When I select "statement4" of combobox as "Starts With"
	Then "statement4" of "TextBox1" is "Visible"
	And  "statement4 of "TextBox2" is "Visible"
	#Select Ends With"
	And I "Add Statement"
	When I select "statement5" of combobox as "Ends With"
	Then "statement5" of "TextBox1" is "Visible"
	And  "statement5" of "TextBox2" is "Visible"
	#Select Contains
	And I "Add Statement"
	When I select "statement6" of combobox as "Contains"
	Then "statement6" of "TextBox1" is "Visible"
	And  "statement6" of "TextBox2" is "Visible"
	Then Evaluates a statement to True or False
	| TextBox1 | ComboBox           | TextBox2 | Delete |
	| ""       | <> (Not Equal)     | ""       | Yes    |
	| ""       | >=                 | ""       | Yes    |
	| ""       | <=                 | ""       | Yes    |
	| ""       | Starts With        | ""       | Yes    |
	| ""       | Ends With          | ""       | Yes    |
	| ""       | Contains           | ""       | Yes    |
	
	
	

	
Scenario: Selecting Statement in combobox3
	Given Decision large view is opened 
	And  "statement1" of "TextBox1" is "Visible"
	And  "statement1" of "TextBox2" is "NotVisible"	
	#Select Doesn't Start With
	When I select "statement1" of combobox as "Doesn't Start With"
	Then "statement1" of "TextBox1" is "Visible"
	And  "statement1" of "TextBox2" is "Visible"
	#Select Doesn't Contain
	And I "Add Statement"
	When I select "statement2" of combobox as "Doesn't Contain"
	Then "statement2" of "TextBox1" is "Visible"
	And  "statement2" of "TextBox2" is "Visible"
	#Select Is Alphanumeric
	When I select "statement3" of combobox as "Is Alphanumeric"
	Then "statement3" of "TextBox1" is "Visible"
	And  "statement3" of "TextBox2" is "NotVisible"
	#Select Is Base64
	And I "Add Statement"
	When I select "statement4" of combobox as "Is Base64"
	Then "statement4" of "TextBox1" is "Visible"
	And  "statement4" of "TextBox2" is "NotVisible"
	#Select Is Between
	And I "Add Statement"
	When I select "statement5" of combobox as "Is Between"
	Then "statement5" of "TextBox1" is "Visible"
	And  "statement5" of "TextBox2" is "Visible"
	And  "statement5" of "TextBox3" is "Visible"
	#Select Is Binary
	And I "Add Statement"
	When I select "statement6" of combobox as "Is Binary"
	Then "statement6" of "TextBox1" is "Visible"
	And  "statement6 of "TextBox2" is "NotVisible"
	#Select Is Date
	And I "Add Statement"
	When I select "statement7" of combobox as "Is Date"
	Then "statement7" of "TextBox1" is "Visible"
	And  "statement7 of "TextBox2" is "NotVisible"
	Then Evaluates a statement to True or False
	| TextBox1 | ComboBox           | TextBox2 | Delete |
	| ""       | Doesn't Start With | ""       | Yes    |
	| ""       | Doesn't Contain    | ""       | Yes    |
	| ""       | Is Alphanumeric    |          | Yes    |
	| ""       | Is Base64          |          | Yes    |
	| ""       | Is Between         | ""       | Yes    |
	| ""       | Is Binary          |          | Yes    |
	| ""       | Is Date            |          | Yes    |











