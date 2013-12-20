Feature: Email
	In order to automate sending emails
	As Warewolf user
	I want tool that I can use to send emails

Scenario: Send email to multiple receipients
	Given I have an email variable "[[firstMail]]" equal to "test1@freemail.com"
	And I have an email variable "[[secondMail]]" equal to "test2@freemail.com"	
	And I have an email address input "test1@freemail.com"
	And the from account is "me@freemail.com" 
	And the subject is "Just testing"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error

Scenario: Send email with multiple from accounts
	Given I have an email address input "test1@freemail.com"
	And the from account is "me@freemail.com;me2@freemail.com" 
	And the subject is "Just testing"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with badly formed multiple To Accounts
	Given I have an email address input "test1@freemail.com==test2@freemail.com"
	And the from account is "me@freemail.com" 
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with no To Accounts
	Given I have an email address input ""
	And the from account is "me@freemail.com" 
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with Subject as both text and variable as xml 
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And I have an email variable "[[subject]]" equal to "<Wow>400%</Wow>"
	And the subject is "News: [[subject]]"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error

Scenario: Send email with no body
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 	
	And the subject is "Testing this cool framework"	
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with Body as both text and variable 
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And I have an email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the subject is "News"	
	And body is "testing email from [[body]] the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error

Scenario: Send email with variable as Body that is xml
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And I have an email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the subject is "News"	
	And body is "[[body]]"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error

Scenario: Send email with everything blank
	Given the from account is "me@freemail.com" 
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with a blank from account
	Given I have an email address input "test1@freemail"
	And the from account is "" 	
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with a negative index recordset for From Accounts
	Given I have an email address input "me@freemail.com"
	And the from account is "[[me(-1).from]]" 
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with a negative index recordset for Recipients
	Given I have an email address input "[[me(-1).to]]"
	And the from account is "me@freemail.com" 
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with a negative index recordset for Subject
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 
	And the subject is "[[my(-1).subject]]"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

Scenario: Send email with a negative index recordset for Body
	Given I have an email address input "test1@freemail"
	And the from account is "me@freemail.com" 	
	And body is "[[my(-1).body]]"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error

