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
