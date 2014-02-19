Feature: Email
	In order to automate sending emails
	As Warewolf user
	I want tool that I can use to send emails

Scenario: Send email to multiple receipients
	Given I have an email variable "[[firstMail]]" equal to "test1@freemail.com"
	And I have an email variable "[[secondMail]]" equal to "test2@freemail.com"	
	And the from account is "me@freemail.com"  
	And to address is "[[firstMail]];[[secondMail]]"
	And the subject is "Just testing"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error
	And the debug inputs as  
	| From Account    | To                                                                   | Subject      | Body                                 |
	| me@freemail.com | [[firstMail]];[[secondMail]] = test1@freemail.com;test2@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	| Result               |
	| [[result]] = Success |

Scenario: Send email with multiple from accounts
	Given the from account is "me@freemail.com;me2@freemail.com"  
	And to address is "test1@freemail.com"
	And the subject is "Just testing"
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account                     | To                 | Subject      | Body                                 |
	| me@freemail.com;me2@freemail.com | test1@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

Scenario: Send email with badly formed multiple To Accounts
	Given the from account is "me@freemail.com" 
	And to address is "test1@freemail.com==test2@freemail.com"
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To                                     | Subject      | Body                                 |
	| me@freemail.com | test1@freemail.com==test2@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	| Result       |
	| [[result]] = Failure|

Scenario: Send email with no To Accounts
	Given the from account is "me@freemail.com" 
	And  to address is ""
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To | Subject      | Body                                 |
	| me@freemail.com | "" | Just testing | testing email from the cool specflow |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

Scenario: Send email with Subject as both text and variable as xml 
	Given the from account is "me@freemail.com" 
	And  to address is "test1@freemail.com"
	And I have an email variable "[[subject]]" equal to "<Wow>400%</Wow>"
	And the subject is "News: [[subject]]"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error
	And the debug inputs as  
	| From Account    | To                 | Subject                                   | Body                                 |
	| me@freemail.com | test1@freemail.com | News: [[subject]] = News: <Wow>400%</Wow> | testing email from the cool specflow |
	And the debug output as 
	| Result       |
	| [[result]] = Success|

Scenario: Send email with no body
	Given the from account is "me@freemail.com"
	And to address is "test1@freemail.com"	
	And the subject is "Testing this cool framework"	
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error
	And the debug inputs as  
	| From Account    | To                 | Subject                     | Body |
	| me@freemail.com | test1@freemail.com | Testing this cool framework | ""   |
	And the debug output as 
	| Result               |
	| [[result]] = Success |

Scenario: Send email with Body as both text and variable 
	Given the from account is "me@freemail.com" 
	And to address is "test1@freemail.com"
	And I have an email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the subject is "News"	
	And body is "testing email from [[body]] the cool specflow"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error
	And the debug inputs as  
	| From Account    | To                 | Subject | Body                                                                                                                    |
	| me@freemail.com | test1@freemail.com | News    | testing email from [[body]] the cool specflow = testing email from <body><inner>inside</inner></body> the cool specflow |
	And the debug output as 
	| Result               |
	| [[result]] = Success |

Scenario: Send email with variable as Body that is xml
	Given the from account is "me@freemail.com"
	And to address is "test1@freemail.com" 
	And I have an email variable "[[body]]" equal to "<body><inner>inside</inner></body>"
	And the subject is "News"	
	And body is "[[body]]"
	When the email tool is executed
	Then the email result will be "Success"
	And the execution has "NO" error
	And the debug inputs as  
	| From Account    | To                 | Subject | Body                                           |
	| me@freemail.com | test1@freemail.com | News    | [[body]] =  <body><inner>inside</inner></body> |
	And the debug output as 
	| Result               |
	| [[result]] = Success |

Scenario: Send email with everything blank
	Given the from account is "me@freemail.com" 
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To | Subject | Body |
	| me@freemail.com | "" | ""      | ""   |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

Scenario: Send email with a blank from account
	Given the from account is "" 
	And to address is "test1@freemail.com"	
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account | To                 | Subject | Body |
	|              | test1@freemail.com | ""      | ""   |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

Scenario: Send email with a negative index recordset for From Accounts
	Given the from account is "[[me(-1).from]]"  
	And to address is "me@freemail.com"
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account      | To              | Subject      | Body                                 |
	| [[me(-1).from]] = | me@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

Scenario: Send email with a negative index recordset for Recipients
	Given the from account is "me@freemail.com"
	And to address is "[[me(-1).to]]"
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To              | Subject      | Body                                 |
	| me@freemail.com | [[me(-1).to]] = | Just testing | testing email from the cool specflow |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

Scenario: Send email with a negative index recordset for Subject
	Given the from account is "me@freemail.com" 
	And to address is "test1@freemail.com"
	And the subject is "[[my(-1).subject]]"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To                 | Subject              | Body                                 |
	| me@freemail.com | test1@freemail.com | [[my(-1).subject]] = | testing email from the cool specflow |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

Scenario: Send email with a negative index recordset for Body
	Given the from account is "me@freemail.com" 
	And to address is "test1@freemail.com" 	
	And body is "[[my(-1).body]]"
	When the email tool is executed
	Then the email result will be "Failure"
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To                 | Subject | Body              |
	| me@freemail.com | test1@freemail.com |         | [[my(-1).body]] = |
	And the debug output as 
	| Result               |
	| [[result]] = Failure |

