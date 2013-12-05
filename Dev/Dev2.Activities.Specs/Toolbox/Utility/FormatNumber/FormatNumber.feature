Feature: FormatNumber
	In order to round off numbers
	As a Warewolf user
	I want a tool that will aid me to do so 

Scenario: Format number rounding down 
	Given I have a number 788.894564545645
	And I selected rounding "Down" to 3 
	And I want to show 3 decimals 
	When the format number is executed
	Then the result 788.894 will be returned

#Scenario: Format number rounding up
#Scenario: Format number rounding normal
#Scenario: Format number rounding none
#Scenario: Format number rounding down to negative number
#Scenario: Format number large number to negative decimals
#Scenario: Format number single digit to negative decimals
#Scenario: Format non numeric
#Scenario: Format number rounding up to a character
#Scenario: Format number to charater decimals
#Scenario: Format number rounding up to negative number and showing negative decimals
#Scenario: Format number with multipart variables and numbers for number rounding and decimals to show