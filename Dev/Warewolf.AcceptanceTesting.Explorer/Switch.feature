Feature: Switch
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Switch
Scenario: Switch On Design Surface
	Given I have Switch large view opened
	And Variable to switch on "" is "Visible"
	And Display text "" is "Visible" 
	And Done button is "Visible"

	
Scenario: Adding Switch Variable in Large View
	Given I have Switch large view opened
	When I enter Variable to switch on "[[a]]"  
	Then Display text is updated as "[[a]]"
	When I click on Done 
	Then Switch large view is closed
	And Switch small view is visible with display text as "[[a]]"

Scenario: Change Display Text is updating in small view
	Given Switch small view is visible with display text as "[[a]]"
	When I open largeview 
	And Variable to switch on "[[a]]" is "Visible"
	And Display text "[[a]]" is "Visible" 
	When I edit Display text as "This is Switch" 
	And I click on Done
	And Switch sma ll view is visible with with display text as "This is Switch"