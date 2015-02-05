Feature: StudioStyling
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Styling
Scenario: Testing Studio Styling
	Given I have Warewolf running
	And all tabs are closed
	When I send "{Ctrl}{x}" to ""
	Then "ControlStyleTestingWindow" is visible
	And "Testing_Combobox" is visible
	When I mouse over to "Testing_Combobox"
	And "Testing_RadioButton" is visible
	When I mouse over to "Testing_RadioButton"
	Then "Testing_RadioButton" is highlighted with orange colour		
	And "Testing_PasswordBox" is visible
	When I mouse over to "Testing_PasswordBox"
	Then "Testing_PasswordBox" is highlighted with orange colour		
	And "Testing_NormalButton" is visible
	When I mouse over to "Testing_NormalButton"
	Then "Testing_NormalButton" is highlighted with orange colour			
	And "Testing_RadioButton" is visible
	When I mouse over to "Testing_PopupButton"
	Then "Testing_PopupButton" is highlighted with orange colour			
	And "Testing_PopupRoundedButton" is visible
	When I mouse over to "Testing_PopupRoundedButton"
	Then "Testing_PopupRoundedButton" is highlighted with orange colour
			
				
			
	 		
			
			
