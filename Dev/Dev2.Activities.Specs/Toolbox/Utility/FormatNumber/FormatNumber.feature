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

Scenario: Format number rounding up
	Given I have a number 788.894564545645
	And I selected rounding "Down" to 3 
	And I want to show 3 decimals 
	When the format number is executed
	Then the result 788.895 will be returned

Scenario: Format number rounding normal
	Given I have a number 788.894564545645
	And I selected rounding "Normal" to 2 
	And I want to show 3 decimals 
	When the format number is executed
	Then the result 788.890 will be returned

Scenario: Format number rounding none
	Given I have a number 788.894564545645
	And I selected rounding "None" to 0 
	And I want to show 4 decimals 
	When the format number is executed
	Then the result 788.8945 will be returned

Scenario: Format number rounding down to negative number
	Given I have a number 788.894564545645
	And I selected rounding "Down" to 0
	And I want to show 0 decimals 
	When the format number is executed
	Then the result 700 will be returned

Scenario: Format number large number to negative decimals
	Given I have a number 788.894564545645
	And I selected rounding "None" to 0
	And I want to show -2 decimals 
	When the format number is executed
	Then the result 7 will be returned

Scenario: Format number single digit to negative decimals
	Given I have a number 7
	And I selected rounding "None" to 0
	And I want to show -2 decimals 
	When the format number is executed
	Then the result 0 will be returned

Scenario: Format number rounding up to a character
	Given I have a number 34.2
	And I selected rounding "Up" to 0
	And I want to show 2 decimals 
	When the format number is executed
	Then the result 35.00 will be returned

# Number is a decimal type and therefore won't allow for a string varibale
#Scenario: Format number that is blank
#	Given I have a number ""
#	When the format number is executed
#	Then the result "" will be returned

#
#Scenario: Format non numeric
#	Given I have a number "asdf"
#	And I selected rounding "None" to 0
#	And I want to show -2 decimals 
#	When the format number is executed
#	Then the result "" will be returned
#
#Scenario: Format number to charater decimals
#	Given I have a number 34.2
#	And I selected rounding "Up" to 0
#	And I want to show "asdf" decimals 
#	When the format number is executed
#	Then the result 35 will be returned

#Scenario: Format number with multipart variables and numbers for number rounding and decimals to show
#	Given I have a formatnumber variable "[[int]]" equal to 788
#	And I have a number "[[int]].894564545645"
#	And I have a formatnumber variable "[[rounding]]" equal to 2
#	And I selected rounding "Up" to "-[[rounding]]"
#	And I have a formatnumber variable "[[decimals]]" equal to "-"
#	And I want to show "[[decimals]]1" decimals 
#	When the format number is executed
#	Then the result 80 will be returned