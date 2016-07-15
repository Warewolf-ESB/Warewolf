Feature: Utility - Email
	In order to send an email
	As a warewolf user
	I want a tool that performs this action
	
@NeedsBlankWorkflow
Scenario: Drag toolbox SMTP Email onto a new workflow
	When I "Drag_Toolbox_SMTP_Email_Onto_DesignSurface"
	Then I "Assert_Email_Exists_OnDesignSurface"

#@NeedsEmailOnDesignSurface
#Scenario: Click Nested Workflow Name Opens Nested Workflow Edit Tab
	#Given I "Assert_Email_Exists_OnDesignSurface"
	When I "Open_Email_Tool_Large_View"
	Then I "Assert_Email_Large_View_Exists_OnDesignSurface"
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Exchange Email onto a new workflow
	When I "Drag_Toolbox_Exchange_Email_Onto_DesignSurface"
	Then I "Assert_Exchange_Email_Exists_OnDesignSurface"

#@NeedsEmailOnDesignSurface
#Scenario: Click Nested Workflow Name Opens Nested Workflow Edit Tab
	#Given I "Assert_Email_Exists_OnDesignSurface"
	When I "Open_Exchange_Email_Tool_Large_View"
	Then I "Assert_Exchange_Email_Large_View_Exists_OnDesignSurface"

@Email
@Ignore
Scenario: Email tool small view
	Given I have email tool small view in design surface
	And Mail source selected as "Select an Email Source..."
	And Mail source edit button is "Disabled"
	And to ""
	And subject is ""
	And body is "" 
	And result is ""

@Ignore
Scenario: Email tool small view water marks 
	Given I have email tool small view in design surface
	And Mail source selected as "Select an Email Source..."
	And To address Watermark is "Account or email address"
	And subject Watermark is "Message Subject"
	And body Watermark is "Email Content" 
	And result Watermark is "[[EmailSuccess]]"

@Ignore
Scenario: Email tool Large view
	Given I have email tool large view in design surface
	And Mail source selected as "Select an Email Source..."
	And Mail source edit button is "Disabled"
	And From Address is ""
	And Password is ""
	And Test button is "Visible"
	And To address is ""
	And Cc address is ""
	And Bcc address is ""
	And Priority selected "Normal"
	And Subject is ""
	And Attachement is ""
	And Edit Attachememts is "Enabled"
	And body is "" 
	And result is ""
	And On Error box consists
    | Put error in this variable | Call this web service |
    |                            |                       |
    And End this workflow is "Unselected"
    And Done button is "Visible"
	
@Ignore
Scenario: Email tool water marks Large view
	Given I have email tool large view in design surface
	And Mail source selected as "Select an Email Source..."
	And Mail source edit button is "Disabled"
	And From Address Watermark is ""
	And Password Watermark is ""
	And Test button is "Visible"
	And To address Watermark is ""
	And Cc address Watermark is ""
	And Bcc address Watermark is ""
	And Priority selected "Normal"
	And Subject Watermark is ""
	And Attachement Watermark is ""
	And Edit Attachememts is "Enabled"
	And body Watermark is "" 
	And result Watermark is ""
	And On Error box Watermark
    | Put error in this variable | Call this web service       |
    | [[Errors().Message         | http://lcl:3142/service/err |
    And End this workflow is "Unselected"
    And Done button is "Visible"

	
@Ignore
Scenario: Email tool small view to large view persiting data
	Given I have email tool small view in design surface
	And Mail source selected as "Select an Email Source..."
	And Mail source edit button is "Disabled"
	And I enter to  address "Testwarewolf@dev2.co.za"
	And I enter subject is "Test Email Tool For Warewolf"
	And I enter body is "Warewolf Rocking" 
	And I enter result is "[[Result]]"
	When I open largeview
	Then "Email" large view is opened
	And Mail source selected as "Select an Email Source..."
	And Mail source edit button is "Disabled"
	And From Address is ""
	And Password is ""
	And Test button is "Visible"
	And To address is "Testwarewolf@dev2.co.za"
	And Cc address is ""
	And Bcc address is ""
	And Priority selected "Normal"
	And Subject is "Warewolf Rocking"
	And Attachement is ""
	And Edit Attachememts is "Enabled"
	And body is "" 
	And result is "[[Result]]"
	 And Done button is "Visible"
	 
@Ignore
Scenario: Email tool Done is validating when no source selected
	Given I have email tool large view in design surface
	And Mail source selected as "Select an Email Source..."
	And Mail source edit button is "Disabled"
	And From Address is ""
	And Password is ""
	And Test button is "Visible"
	And I enter To address is "Testwarewolf@dev2.co.za"
	And Cc address is ""
	And Bcc address is ""
	And Priority selected "Normal"
	And I enter Subject is "Warewolf Rocking"
	And Attachement is ""
	And Edit Attachememts is "Enabled"
	And I enter body is "" 
	And I enter result is "[[Result]]"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown
	
@Ignore
Scenario: Email tool New email source opens New Email Source tab
	Given I have email tool large view in design surface
	When Mail source selected as "New Email Source..."
	Then new Email source tab is opened
	When I save "Test Email" email source
	Then focus is "Mail Source"
	Then Mail source selected as "Test Email"
	
@Ignore
Scenario: From address field is updating when I select source
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And From Address is "Testwarewolf@gmail.com"
	And Password is ""
	And Test button is "Visible"
	
	
@ignore	
Scenario: Validation is thrown on done button if there is no To address
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And To address is ""
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is thrown
	
@ignore	
Scenario: Validation is thrown on done button if To address is incorrect
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "testwarewolf"
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is thrown
		
@Ignore
Scenario: Validation is thrown for incorrect email in CC field 
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "testwarewolf@gmail.com"
	And Cc address is ""
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is not thrown
	And I open large view
	When I enter Cc is "testgmail.com"
	And I click on "Done"	
	Then Validation message is thrown
	When I edit Cc is "test@gmail.com"
	And I click on "Done"	
	Then Validation message is not thrown
	
@Ignore
Scenario: Validation is thrown for incorrect email in Bcc field 
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "testwarewolf@gmail.com"
	And Bcc address is ""
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is not thrown
	And I open large view
	When I enter Cc is "testgmail.com"
	And I click on "Done"	
	Then Validation message is thrown
	When I edit Cc is "test@gmail.com"
	And I click on "Done"	
	Then Validation message is not thrown

@ignore	
Scenario: Attachemets fiels is showing attaced file paths by seperating them with semicolon
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "testwarewolf@gmail.com"
	And Bcc address is ""
	And I enter Subject is "Test"
	And I attach button is "Visible"
	And I select "c/test.txt"
	And I select "d/Warewolf.txt"
	Then attachement is "c/test.txt;d/Warewolf.txt"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is not thrown
	
@Ignore
Scenario Outline: Throwing validation error for incorrect variables
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "<To Address>"
	And Bcc address is ""
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is thrown "<Validation>"
Examples: 
   | No | To Address       | Validation |
   | 1  | [[rec().a]]      | False      |
   | 2  | [[a]]            | False      |
   | 3  | [[a]][[b]]       | False      |
   | 4  | [[rec().a]][[a]] | False      |
   | 5  | [[rec(*).a]]     | False      |
   | 6  | 123              | True       |
   | 7  | [[a!1]]          | True       |
   | 8  | [[rec().a!]]     | True       |
   | 9  | [[rec([[a]]).a]] | False      |
   | 10 | [[rec().[[a]]]]  | Falsse     |
   | 11 | [[[[a]]]]        | False      |
   | 12 | [[a]             | True       |
   | 13 | [[rec()]]        | True       |
   | 14 | [[rec().a]       | True       |
   | 15 | [[a]].com        | False      |
   | 16 | test@[[a]].com   | False      |
   | 17 |                  | True       |
   
@Ignore
Scenario Outline: Throwing validation error for Bcc incorrect variables
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "test@gmail.com
	And I enter Bcc address is "<Bcc Address>"
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is thrown "<Validation>"
Examples: 
   | No | Bcc Address      | Validation |
   | 1  | [[rec().a]]      | False      |
   | 2  | [[a]]            | False      |
   | 3  | [[a]][[b]]       | False      |
   | 4  | [[rec().a]][[a]] | False      |
   | 5  | [[rec(*).a]]     | False      |
   | 6  | 123              | True       |
   | 7  | [[a!1]]          | True       |
   | 8  | [[rec().a!]]     | True       |
   | 9  | [[rec([[a]]).a]] | False      |
   | 10 | [[rec().[[a]]]]  | Falsse     |
   | 11 | [[[[a]]]]        | False      |
   | 12 | [[a]             | True       |
   | 13 | [[rec()]]        | True       |
   | 14 | [[rec().a]       | True       |
   | 15 | [[a]].com        | False      |
   | 16 | test@[[a]].com   | False      |
   | 17 |                  | False      |

@ignore   
Scenario Outline: Throwing validation error for Cc incorrect variables
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "test@gmail.com
	And I enter Cc address is "<Cc Address>"
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is thrown "<Validation>"
Examples: 
   | No | Cc Address       | Validation |
   | 1  | [[rec().a]]      | False      |
   | 2  | [[a]]            | False      |
   | 3  | [[a]][[b]]       | False      |
   | 4  | [[rec().a]][[a]] | False      |
   | 5  | [[rec(*).a]]     | False      |
   | 6  | 123              | True       |
   | 7  | [[a!1]]          | True       |
   | 8  | [[rec().a!]]     | True       |
   | 9  | [[rec([[a]]).a]] | False      |
   | 10 | [[rec().[[a]]]]  | Falsse     |
   | 11 | [[[[a]]]]        | False      |
   | 12 | [[a]             | True       |
   | 13 | [[rec()]]        | True       |
   | 14 | [[rec().a]       | True       |
   | 15 | [[a]].com        | False      |
   | 16 | test@[[a]].com   | False      |
   | 17 |                  | False      |
   
@Ignore
Scenario Outline: Throwing validation error for Subject incorrect variables
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "test@gmail.com
	And I enter Subject is "<Subject>"
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result]]"
	When I click on "Done"
	Then Validation message is thrown "<Validation>"
Examples: 
   | No | Subject          | Validation |
   | 1  | [[rec().a]]      | False      |
   | 2  | [[a]]            | False      |
   | 3  | [[a]][[b]]       | False      |
   | 4  | [[rec().a]][[a]] | False      |
   | 5  | [[rec(*).a]]     | False      |
   | 6  | 123              | False      |
   | 7  | [[a!1]]          | True       |
   | 8  | [[rec().a!]]     | True       |
   | 9  | [[rec([[a]]).a]] | False      |
   | 10 | [[rec().[[a]]]]  | Falsse     |
   | 11 | [[[[a]]]]        | False      |
   | 12 | [[a]             | True       |
   | 13 | [[rec()]]        | True       |
   | 14 | [[rec().a]       | True       |
   | 15 | [[a]].com        | False      |
   | 16 | test@[[a]].com   | False      |
   | 17 |                  | False      |

   
@Ignore
Scenario Outline: Throwing validation error for Result incorrect variables
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "test@gmail.com
	And I enter Subject is "Test"
	And I enter Subject is "Test"
	And I enter Body is "Test Warewolf"
	And I enter result is "<Result>"
	When I click on "Done"
	Then Validation message is thrown "<Validation>"
Examples: 
   | No | Result           | Validation |
   | 1  | result           | False      |
   | 2  | [[result]]       | False      |
   | 3  | [[a]][[b]]       | True       |
   | 4  | [[rec([[a]]).a]] | True       |
   | 5  | [[[[a]]]]        | True       |
   | 6  | [[rec(*).a]]     | False      |
   | 7  | [[rec().a@]]     | True       |   
   
   
@ignore   
Scenario: Validation is not thrown when I close large view
	Given I have email tool large view in design surface
	When Mail source selected as "Test Email"
	Then Mail source edit button is "Enabled"
	And I enter To address is "testwarewolf"
	And I enter Subject is "Te[t"
	And I enter Body is "Test Warewolf"
	And I enter result is "[[Result#2#]]"
	When I clode large view
	Then Validation message is not thrown  
    And "Email" Small view is opened	
	
@Ignore
Scenario: Create new Email source from tool
	Given I have a new Workflow
	And I drag Email Tool onto the design surface
	Then "Mail Source" is Enabled
	And "Edit" is Disabled
	And "To" input is ""
	And "Subject" input is ""
	And "Body" input is ""
	When I select "New Email Source"
	Then the new Email Source Tab is opened
	
@Ignore
Scenario: Edit Email source from tool
	Given I have a new Workflow
	And I drag Email Tool onto the design surface
	Then "Mail Source" is Enabled
	And "Edit" is Disabled
	And "To" input is ""
	And "Subject" input is ""
	And "Body" input is ""
	When I select "Email Source"
	Then "Edit" is Enabled
	And I Click "Edit"
	Then the "Email Source" Tab is opened
	
@Ignore
Scenario: Change Email source
	Given I have a new Workflow
	And I drag Email Tool onto the design surface
	Then "Mail Source" is Enabled
	And "Edit" is Disabled
	When I select "Email Source"
	Then "Edit" is Enabled
	And "To" input is "warewolfworks@gmail.com"
	And "Subject" input is "Sample Text"
	And "Body" input is "Text should not clear"
	When "Mail Source" is changed from "Email Source" to "NewEmailSource"
	Then the "Email Source" Tab is opened
	And "To" input is "warewolfworks@gmail.com"
	And "Subject" input is "Sample Text"
	And "Body" input is "Text should not clear"