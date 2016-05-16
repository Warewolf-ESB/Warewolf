Feature: EmailDialog
	In order to attach files
	I want to attach items to me email to send
	So that I can reuse them

Scenario: Send Email with an attachment
	Given I open a "New Workflow"
	And I drag the "Email" tool onto the design surface
	And Mail Source is "EmaiSource" 
	And "From" account is "warewolf@dev2.co.za"
	And "To" address is "test1@freemail.com" 	
	And I  want to attach "C:\Test.txt"
	When I expand the Email tool to Large view
	And I click "..." for attachments
	Then the file chooser dialog opens 
	Then I navigate to "C:\Test.txt"
	And I click "Save"
	Then attachment should appear as "C:\Test.txt"

Scenario: Selecting multiple attachments
	Given the Email dialog is opened
	And I navigate to "C:\Temp\Testing" 
	And I attach "C:\Temp\Testing\test.txt"
	And I navigate to "E:\AppData\Le"
	And I attach "E:\AppData\Le\test.txt"
	Then attachment should appear as "C:\Temp\Testing\test.txt;E:\AppData\Le\test.txt"


Scenario: Ensure that dialog tree view is populated correctly
	Given the Email dialog is opened
	And all network drives are visible
	And I expand "C:\"
	Then all the folders in "C:\" are visible

Scenario: Ensure Email tool accepts HTML
	Given I have a workflow "New Workflow"
	And I drag the "Email" tool onto the design surface
	And Mail Source is "EmaiSource" 
	And "From" account is "warewolf@dev2.co.za"
	And "To" address is "test1@freemail.com" 	
	And "Html Body" is "Checked"
	And Body as "<h1 style="color:blue">This is a Blue Heading</h1>"


