Feature: Design Surface
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DesignSurface
Scenario: Opening New Design surface
	Given I open new "localhost\Unsaved 1" design surface
	And design surface "localhost\Unsaved 1" is opened   
	And the data list hyper link "http://rsaklfmurali:3142/services/Unassigned/Unsaved 1.json?<DataList></DataList>" is "Visible"
	And the name of the flow chart design surface is as "Unsaved 1"
	And close button is "Visible"
	And position button is "Visible"

Scenario: After saving new design surface is updated
	Given I open new "localhost\Unsaved 1" design surface
	And design surface "localhost\Unsaved 1" is opened
	When I save "Unsaved 1" as "Workflow"
	Then design surface "localhost\Workflow" is opened 
	And the name of the flow chart design surface is as "Workflow"

Scenario: Closing the design surface
	Given design surface "localhost\SavedWf" is opened as "SavedWf" 
	When I close design surface 
	And design surface "SavedWf" is closed


Scenario: Attempting to close unsaved workflow is throwing validation message
	Given I open "localhost\SavedWf"  
	Given design surface "localhost\SavedWf" is opened with star
	When I close design surface 
	Then "Workflow not saved" pop up is thrown
	And the validation message contains "The workflow '2' that you are closing is not saved. Would you like to save the workflow?"
	And "close" button is visible in "Workflow not saved"
	And "Yes" button is visible in "Workflow not saved"
	And "No" button is visible in "Workflow not saved"
	And "Cancel" button is visible in "Workflow not saved"


Scenario: Saving a Workflow from validation message
	Given I open "localhost\SavedWf" 
	Given design surface "localhost\SavedWf" is opened with star
	When I close design surface 
	Then "Workflow not saved" pop up is thrown
	When I click "Yes" on "Workflow not saved" dialog
	Then workflow is saved
    And design surface "SavedWf" is closed


Scenario: Saving a new unsaved Workflow from validation message
	Given I open new "localhost\Unsaved 1" design surface
	And design surface "localhost\Unsaved 1" is opened  
	When I close design surface 
	Then "Workflow not saved" pop up is thrown
	When I click "Yes" on "Workflow not saved" dialog
	Then Save Dialog is opened


Scenario: Discarding changes made on design surface
	Given design surface "localhost\SavedWf" is opened with star
	When I close design surface 
	Then "Workflow not saved" pop up is thrown
	When I click "No" on "Workflow not saved" dialog
	Then workflow is not saved
    And design surface "SavedWf" is closed


Scenario: Canceling Validation message thrown on desing surface
	Given design surface "localhost\SavedWf" is opened with star
	Given design surface "localhost\SavedWf1" is opened with star
	When I close design surface 
	Then "Workflow not saved" pop up is thrown
	When I click "Cancel" on "Workflow not saved" dialog
	Then workflow is not saved
    And design surface is opened as "SavedWf" with star


Scenario: Copying and pasting items on design surface  
	Given I open new "localhost\Unsaved 1" design surface
	And design surface "localhost\Unsaved 1" is opened 
	And design surface "localhost\Unsaved 2" is opened 
	And "Unsaved 2" has "Assign" on it
	When I copy "Assign" on "Unsaved 2"
	Then tool "Assign" is visible on "Unsaved 2"
	And I swap the tab to "Unsaved 1"
	When I paste "Assign" on "Unsaved 1" 
	Then tool "Assign" is visible on "Unsaved 1"

Scenario: Cut and paste items on design surface  
	Given I open new "localhost\Unsaved 1" design surface
	And design surface "localhost\Unsaved 1" is opened  
	And design surface "localhost\Unsaved 2" is opened 
	And "Unsaved 2" has "Assign" on it
	When I cut "Assign" on "Unsaved 2"
	Then tool "Assign" is not visible on "Unsaved 2"
	And I swap the tab to "Unsaved 1"
	When I paste "Assign" on "Unsaved 1" 
	And design surface "localhost\Unsaved 1" is opened with star
	Then tool "Assign" is visible on "Unsaved 1"


##Shortcut Keys

Scenario: Shortcut key to Open New Design Surface
   Given I selected "localhost"
   When I type "Ctrl+W" 
   Then design surface "localhost\Unsaved 1" is opened 
   When I type "Ctrl+S" 
   Then "save" dialogbox is opened
   When I type "Ctrl+D" 
   Then "Deploy tab" is opened
   When I type "Ctrl+Shift+D" 
   Then "New Database Service" tab is opened
   When I type "Ctrl+Shift+P" 
   Then "New Plugin Service" tab is opened
   When I type "Ctrl+Shift+W" 
   Then "New Web Service" tab is opened
   
   When I type "" 
   Then "Scheduler" is opened

   When I type "F5" 
   Then "Debug input data" dialogbox is opened

   
   When I type "" 
   Then "Manage Settings" is opened





















