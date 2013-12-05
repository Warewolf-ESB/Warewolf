Feature: Email
	In order to automate sending emails
	As Warewolf user
	I want tool that I can use to send emails

Scenario: Send email to multiple receipients
	Given I have a variable "[[firstMail]]" with this email address "test1@freemail.com"
	And I have a variable "[[secondMail]]" with this email address "test2@freemail.com"	
	And the from account is "me@freemail.com" with the subject "Just testing"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the number of emails sent will be ""

#Scenario: Send email with multiple from accounts
#Scenario: Send email with multiple To Accounts
#Scenario: Send email with badly formed multiple To Accounts
#Scenario: Send email with no To Accounts
#Scenario: Send email with both text and variable To Accounts
#Scenario: Send email with Subject as both text and variable 
#Scenario: Send email with no subject
#Scenario: Send email with variable as subject that is xml
#Scenario: Send email with Body as both text and variable 
#Scenario: Send email with no sBody
#Scenario: Send email with variable as Body that is xml
#Scenario: Send email with everything blank