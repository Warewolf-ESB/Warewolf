@Exchange
Feature: ExchangeEmail
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Send New Exchange Email to multiple recipients
	Given I have a new exchange email variable "[[firstMail]]" equal to "testmail@freemail.com"
	And I have a new exchange email variable "[[secondMail]]" equal to "test2@freemail.com"	
	And new to exchange address is "[[firstMail]];[[secondMail]]"
	And the new exchange subject is "Just testing"
	And new exchange body is "testing email from the cool specflow"
	When the new exchange email tool is executed
	Then the new exchange email result will be "Success"
	And the new exchange execution has "NO" error
	And the debug inputs as
	| To                                                                   | Subject      | Body                                 |
	| [[firstMail]];[[secondMail]] = testmail@freemail.com;test2@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                      |
	| [[result]] = Success |

Scenario: Send New Exchange email with no To Accounts
	Given new exchange to address is ""
	And the new exchange subject is "Just testing"	
	And new exchange body is "testing email from the cool specflow"
	When the new exchange email tool is executed ""
	Then the new exchange email result will be ""	
	And the debug inputs as  
	| To | Subject      | Body                                 |
	| "" | Just testing | testing email from the cool specflow |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Send New Exchange email with Subject as both text and variable as xml 
	Given new exchange to address is "test1@freemail.com"
	And I have a new exchange email variable "[[subject]]" equal to "<Wow>400%</Wow>"
	And the new exchange subject is "News: [[subject]]"	
	And new exchange body is "testing email from the cool specflow"
	When the new exchange email tool is executed
	Then the new exchange email result will be "Success"
	And the new exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject                                   | Body                                 |
	| test1@freemail.com | News: [[subject]] = News: <Wow>400%</Wow> | testing email from the cool specflow |
	And the debug output as 
	|                      |
	| [[result]] = Success |

Scenario: Send New Exchange email with no body
	Given new exchange to address is "test1@freemail.com"	
	And the new exchange subject is "Testing this cool framework"	
	When the new exchange email tool is executed
	Then the new exchange email result will be "Success"
	And the new exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject                     | Body |
	| test1@freemail.com | Testing this cool framework | ""   |
	And the debug output as 
	|                      |
	| [[result]] = Success |

Scenario: Send New Exchange email with Body as both text and variable 
	Given new exchange to address is "test1@freemail.com"
	And I have a new exchange email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the new exchange subject is "News"	
	And new exchange body is "testing email from [[body]] the cool specflow"
	When the new exchange email tool is executed
	Then the new exchange email result will be "Success"
	And the new exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject | Body                                                                                                                    |
	| test1@freemail.com | News    | testing email from [[body]] the cool specflow = testing email from <body><inner>inside</inner></body> the cool specflow |
	And the debug output as 
	|                      |
	| [[result]] = Success |

Scenario: Send New Exchange email with variable as Body that is xml
	Given new exchange to address is "test1@freemail.com" 
	And I have a new exchange email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the new exchange subject is "News"	
	And new exchange body is "[[body]]"
	When the new exchange email tool is executed
	Then the new exchange email result will be "Success"
	And the new exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject | Body                                           |
	| test1@freemail.com | News    | [[body]] =  <body><inner>inside</inner></body> |
	And the debug output as 
	|                      |
	| [[result]] = Success |

Scenario: Send New Exchange email with everything blank
	When the new exchange email tool is executed ""
	Then the new exchange email result will be ""	
	And the debug inputs as  
	| To | Subject | Body |
	| "" | ""      | ""   |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Send New Exchange email with a negative index recordset for Recipients
	And new exchange to address is "[[me(-1).to]]"
	And the new exchange subject is "Just testing"	
	And new exchange body is "testing email from the cool specflow"
	When the new exchange email tool is executed
	Then the new exchange email result will be ""
	And the new exchange execution has "AN" error
	And the debug inputs as  
	| To              | Subject      | Body                                 |
	| [[me(-1).to]] = | Just testing | testing email from the cool specflow |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Send New Exchange email with a negative index recordset for Subject
	And new exchange to address is "test1@freemail.com"
	And the new exchange subject is "[[my(-1).subject]]"	
	And new exchange body is "testing email from the cool specflow"
	When the new exchange email tool is executed
	Then the new exchange email result will be ""
	And the new exchange execution has "AN" error
	And the debug inputs as  
	| To                 | Subject              | Body                                 |
	| test1@freemail.com | [[my(-1).subject]] = | testing email from the cool specflow |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Send New Exchange email with a negative index recordset for Body
	And new to address is "test1@freemail.com" 	
	And new exchange body is "[[my(-1).body]]"
	When the new exchange email tool is executed
	Then the new exchange email result will be ""
	And the new exchange execution has "AN" error
	And the debug inputs as  
	| To                 | Subject | Body              |
	| test1@freemail.com | ""      | [[my(-1).body]] = |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Send New Exchange email with new line in body
	Given new exchange to address is "test1@freemail.com"	
	And the new exchange subject is "Testing this cool framework"	
	And new exchange body is "testing email with \r\n new line"
	When the new exchange email tool is executed
	Then the new exchange email result will be "Success"
	And the new exchange execution has "NO" error
	And the debug inputs as  
	| To                 | Subject                     | Body                             |
	| test1@freemail.com | Testing this cool framework | testing email with \r\n new line |
	And the debug output as 
	|                      |
	| [[result]] = Success |

Scenario: Send New Exchange email with Html false
	Given new exchange to address is ""
	And the new exchange subject is "Just testing"	
	And new exchange body is "testing email from the cool specflow"
	And new exchange IsHtml is "true"
	When the new exchange email tool is executed ""
	Then the new exchange email result will be ""	
	And the debug inputs as  
	| To | Subject      | Body                                 |
	| "" | Just testing | testing email from the cool specflow |
	And the debug output as 
	|              |
	| [[result]] = |