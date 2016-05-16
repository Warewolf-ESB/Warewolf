Feature: ExchangeEmail
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: Send Exchange Email to multiple receipients
	Given I have an exchange email variable "[[firstMail]]" equal to "testmail@freemail.com"
	And I have an exchange email variable "[[secondMail]]" equal to "test2@freemail.com"	
	And to exchange address is "[[firstMail]];[[secondMail]]"
	And the exchange subject is "Just testing"
	And exchange body is "testing email from the cool specflow"
	When the exchange email tool is executed
	Then the exchange email result will be "Success"
	And the exchange execution has "NO" error
	And the debug inputs as
	| To                                                                   | Subject      | Body                                 |
	| [[firstMail]];[[secondMail]] = testmail@freemail.com;test2@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                      |
	| [[result]] = Success |