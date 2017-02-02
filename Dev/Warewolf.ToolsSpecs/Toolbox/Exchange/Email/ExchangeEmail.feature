@Utils
Feature: ExchangeEmail
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

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

Scenario: Send Exchange email with no To Accounts
	Given exchange to address is ""
	And the exchange subject is "Just testing"	
	And exchange body is "testing email from the cool specflow"
	When the exchange email tool is executed ""
	Then the exchange email result will be ""	
	And the debug inputs as  
	| To | Subject      | Body                                 |
	| "" | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send Exchange email with Subject as both text and variable as xml 
	Given exchange to address is "test1@freemail.com"
	And I have an exchange email variable "[[subject]]" equal to "<Wow>400%</Wow>"
	And the exchange subject is "News: [[subject]]"	
	And exchange body is "testing email from the cool specflow"
	When the exchange email tool is executed
	Then the exchange email result will be "Success"
	And the exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject                                   | Body                                 |
	| test1@freemail.com | News: [[subject]] = News: <Wow>400%</Wow> | testing email from the cool specflow |
	And the debug output as 
	|               |
	| [[result]] = Success|

Scenario: Send exchange email with no body
	Given exchange to address is "test1@freemail.com"	
	And the exchange subject is "Testing this cool framework"	
	When the exchange email tool is executed
	Then the exchange email result will be "Success"
	And the exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject                     | Body |
	| test1@freemail.com | Testing this cool framework | ""   |
	And the debug output as 
	|                       |
	| [[result]] = Success |

Scenario: Send exchange email with Body as both text and variable 
	Given exchange to address is "test1@freemail.com"
	And I have an exchange email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the exchange subject is "News"	
	And exchange body is "testing email from [[body]] the cool specflow"
	When the exchange email tool is executed
	Then the exchange email result will be "Success"
	And the exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject | Body                                                                                                                    |
	| test1@freemail.com | News    | testing email from [[body]] the cool specflow = testing email from <body><inner>inside</inner></body> the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] = Success |

Scenario: Send exchange email with variable as Body that is xml
	Given exchange to address is "test1@freemail.com" 
	And I have an exchange email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the exchange subject is "News"	
	And exchange body is "[[body]]"
	When the exchange email tool is executed
	Then the exchange email result will be "Success"
	And the exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject | Body                                           |
	| test1@freemail.com | News    | [[body]] =  <body><inner>inside</inner></body> |
	And the debug output as 
	|                       |
	| [[result]] = Success |

Scenario: Send exchange email with everything blank
	When the exchange email tool is executed ""
	Then the exchange email result will be ""	
	And the debug inputs as  
	| To | Subject | Body |
	| "" | ""      | ""   |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send exchange email with a negative index recordset for Recipients
	And exchange to address is "[[me(-1).to]]"
	And the exchange subject is "Just testing"	
	And exchange body is "testing email from the cool specflow"
	When the exchange email tool is executed
	Then the exchange email result will be ""
	And the exchange execution has "AN" error
	And the debug inputs as  
	| To              | Subject      | Body                                 |
	| [[me(-1).to]] = | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send exchange email with a negative index recordset for Subject
	And exchange to address is "test1@freemail.com"
	And the exchange subject is "[[my(-1).subject]]"	
	And exchange body is "testing email from the cool specflow"
	When the exchange email tool is executed
	Then the exchange email result will be ""
	And the exchange execution has "AN" error
	And the debug inputs as  
	| To                 | Subject              | Body                                 |
	| test1@freemail.com | [[my(-1).subject]] = | testing email from the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send exchange email with a negative index recordset for Body
	And to address is "test1@freemail.com" 	
	And exchange body is "[[my(-1).body]]"
	When the exchange email tool is executed
	Then the exchange email result will be ""
	And the exchange execution has "AN" error
	And the debug inputs as  
	| To                 | Subject | Body              |
	| test1@freemail.com |  ""       | [[my(-1).body]] = |
	And the debug output as 
	|                       |
	| [[result]] =  |
