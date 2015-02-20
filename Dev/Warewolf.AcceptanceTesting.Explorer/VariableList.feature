Feature: VariableList
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


#Ensure variables in the tools are adding on to variable list
#Ensure user is able search for variable in variable list
#Ensure search clear button is clearing text in variable list searchbox
#Ensure user is able to Delete all the unused variables in variable list
#Ensure user is able to select variables as input and output
#Ensure variable list has seperate group for recordset  and scalar variable
#Ensure variable textbox has delete and note button disabled 
#Ensure delete button in textbox is highlighted when variable is unused
#Ensure delete button in the textbox is deleting variable in the variable textbox 
#







@VariableList
Scenario: Adding Variables 
	Given I have entered 50 into the calculator
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen
