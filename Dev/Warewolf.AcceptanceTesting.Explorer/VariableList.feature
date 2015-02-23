Feature: VariableList
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


## System Requirements for Variable List
#Ensure variables used in the tools are adding automatically to variable list.
#Ensure user is able search for variable in variable list.
#Ensure search clear button is clearing text in variable list search box.
#Ensure user is able to Delete all the unused variables in variable list.
#Ensure sort alphabetically button is available in variable list box.
#Ensure scalar variables are Sorting alphabetically when user clicks on sort button.
#Ensure Recordset variables are Sorting alphabetically when user clicks on sort button.
#Ensure user is able to select variables as input.
#Ensure user is able to select variables as output.
#Ensure input and output checkboxes must be disabled if variable is unused.
#Ensure input and Output checkbox is disabled when variable in the variable box is unused.
#Ensure variable list has separate groups for recordset and scalar variable.
#Ensure variable textbox has delete and note button disabled.
#Ensure delete button in textbox is highlighted when variable is unused.
#Ensure delete button in the textbox is deleting variable in the variable textbox.
#Ensure variable Notes button is available in the variable box.
#Ensure Notes button is disabled if the variable box is empty.
#Ensure note textbox is opened when user click on button.
#Ensure user is able to edit notes of the variable in variable note box.
#Ensure one scrollbar is made for variable list to move up and down.
#Ensure next variable textboxe appears only when user has a variable in previous box.





@VariableList
Scenario: Adding Variables 
	Given I have entered 50 into the calculator
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen
