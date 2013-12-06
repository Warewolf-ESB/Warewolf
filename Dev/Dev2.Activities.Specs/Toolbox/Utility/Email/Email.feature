Feature: Email
	In order to automate sending emails
	As Warewolf user
	I want tool that I can use to send emails

Scenario: Send email to multiple receipients
	Given I have an email variable "[[firstMail]]" equal to "test1@freemail.com"
	And I have an email variable "[[secondMail]]" equal to "test2@freemail.com"	
	And I have an email address input "[[firstMail]]; [[secondMail]]; test1@freemail.com"
	And the from account is "me@freemail.com" 
	And the subject is "Just testing"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And email execution has "NO" error

Scenario: Send email with multiple from accounts
	Given I have an email address input "test1@freemail.com"
	And the from account is "me@freemail.com;me2@freemail.com" 
	And the subject is "Just testing"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And email execution has "NO" error

Scenario: Send email with badly formed multiple To Accounts
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And the subject is "Just testing"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And email execution has "NO" error

Scenario: Send email with no To Accounts
	Given I have an email address input ""
	And the from account is "me@freemail.com" 
	And the subject is "Just testing"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And email execution has "NO" error

Scenario: Send email with Subject as both text and variable as xml 
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And I have an email variable "[[subject]]" equal to "<Wow>400%</Wow>"
	And the subject is "News: [[subject]]"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And email execution has "NO" error

Scenario: Send email with no subject and no body
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	When the email tool is executed
	Then the email result will be "Success"
	And email execution has "NO" error

Scenario: Send email with Body as both text and variable 
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And I have an email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the subject is "News"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from [[body]] the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And email execution has "NO" error

Scenario: Send email with variable as Body that is xml
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And I have an email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the subject is "News"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "[[body]]"
	When the email tool is executed
	Then the email result will be "Success"
	And email execution has "NO" error

Scenario: Send email with everything blank
	Given the from account is "me@freemail.com" 
	When the email tool is executed
	Then the email result will be "Failure"
	And email execution has "NO" error
