Feature: Email
	In order to automate sending emails
	As Warewolf user
	I want tool that I can use to send emails

##Send email to multiple receipients
##Send email with multiple from accounts
##Send email with badly formed multiple To Accounts
##Send email with no To Accounts
##Send email with Subject as both text and variable as xml 
##Send email with no body
##Send email with Body as both text and variable 
##Send email with variable as Body that is xml
##Send email with everything blank
##Send email with a blank from account
##Send email with a negative index recordset for From Accounts
##Send email with a negative index recordset for Recipients
##Send email with a negative index recordset for Subject
##Send email with a negative index recordset for Body
##Send Email with an attachment
##Sending an email 

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
	|                       |
	| [[result]] = Success |

Scenario: Send email with multiple from accounts
	Given the from account is "me@freemail.com;me2@freemail.com"  
	And to address is "test1@freemail.com"
	And the subject is "Just testing"
	And the email is html
	And the sever name is "pop3@freemail.com" with password as "3LittleP6"
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account                     | To                 | Subject      | Body                                 |
	| me@freemail.com;me2@freemail.com | test1@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                      |
	| [[result]] =  |

Scenario: Send email with badly formed multiple To Accounts
	Given the from account is "me@freemail.com" 
	And to address is "test1@freemail.com==test2@freemail.com"
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To                                     | Subject      | Body                                 |
	| me@freemail.com | test1@freemail.com==test2@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Send email with no To Accounts
	Given the from account is "me@freemail.com" 
	And  to address is ""
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To | Subject      | Body                                 |
	| me@freemail.com | "" | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] =  |

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
	|               |
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
	|                       |
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
	|                       |
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
	|                       |
	| [[result]] = Success |

Scenario: Send email with everything blank
	Given the from account is "me@freemail.com" 
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To | Subject | Body |
	| me@freemail.com | "" | ""      | ""   |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send email with a blank from account
	Given the from account is "" 
	And to address is "test1@freemail.com"	
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account | To                 | Subject | Body |
	|              | test1@freemail.com | ""      | ""   |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send email with a negative index recordset for From Accounts
	Given the from account is "[[me(-1).from]]"  
	And to address is "me@freemail.com"
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account      | To              | Subject      | Body                                 |
	| [[me(-1).from]] = | me@freemail.com | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send email with a negative index recordset for Recipients
	Given the from account is "me@freemail.com"
	And to address is "[[me(-1).to]]"
	And the subject is "Just testing"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To              | Subject      | Body                                 |
	| me@freemail.com | [[me(-1).to]] = | Just testing | testing email from the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send email with a negative index recordset for Subject
	Given the from account is "me@freemail.com" 
	And to address is "test1@freemail.com"
	And the subject is "[[my(-1).subject]]"	
	And body is "testing email from the cool specflow"
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To                 | Subject              | Body                                 |
	| me@freemail.com | test1@freemail.com | [[my(-1).subject]] = | testing email from the cool specflow |
	And the debug output as 
	|                       |
	| [[result]] =  |

Scenario: Send email with a negative index recordset for Body
	Given the from account is "me@freemail.com" 
	And to address is "test1@freemail.com" 	
	And body is "[[my(-1).body]]"
	When the email tool is executed
	Then the email result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| From Account    | To                 | Subject | Body              |
	| me@freemail.com | test1@freemail.com |  ""       | [[my(-1).body]] = |
	And the debug output as 
	|                       |
	| [[result]] =  |


	#wolf - 991
Scenario: Send Email with an attachment
	Given the from account is "warewolf@dev2.co.za"
	And to address is "test1@freemail.com" 	
	And I  want to attach an item
	When I expand the Email tool
	And I click "Attachments"
	Then the webs file chooser dialog opens 

Scenario Outline: Sending an email 
	Given the from account is '<from>'
	And to address is '<To>'
	And the subject is '<subject>'
	And Password is '<password>' 
	And the Bcc is '<Bcc>'
	And the Cc is '<Cc>'
	And body is '<body>'
	And the attachment is '<attachments>'
	And the execution has '<error>' error
	And Result is '<result>'
	Examples: 
	| from                                        | To                                          | subject                           | password                 | Bcc                                       | Cc                                    | body                         | attachments                                | error | result                                                  |
	| 22                                          | [[va]] =""                                  | [[rec([[int]]).a]] = Numeric Test | 3                        | 100                                       | 50                                    | [[rs().a]] = hello           | [[a]] = ""                                 | An    | [[result]] = From address is not in the valid format:22 |
	| [[va]]      =""                             | 11                                          | [[rs().set]] = T                  | [[var]] = test123        | [[a]] = warewolf@dev2.co.za               | info@dev2.co.za                       | 45                           | [[at().set]] = E:\test.txt                 | An    | [[result]] = To address is not in the valid format:11   |
	| [[rec(1).set]] = warewolf@dev2.co.za        | [[rs(1).a]] =  info@dev2.co.za              | [[va]]     =""                    | [[q]]  = test123         | test@dev2.co.za                           | [[var]] = user@dev2.co.za             | [[b]]                        | [[at(1).set]] = E:\test.txt                | No    | [[rs(*).a]] = Success                                   |
	| [[email().rs]] = warewolf@dev2.co.za        | [[email([[int]]).rs]] = warewolf@dev2.co.za | [[rs(*).a]] = Test                | [[rs(1).a]] = test123    | [[e]]                                     | [[rec(1).set]] = info@dev2.co.za      | [[email([[int]]).rs]] = Test | [[at(*).set]] = E:\test.txt;E:\tr.txt;     | No    | [[rs([[int]]).a]] = Success, [[int]] = 1                |
	| [[email(*).rs]] =                           | [[email().rs]] = warewolf@dev2.co.za        | New Email Test                    | [[rs().b]] = test123     | [[rec(1).set]] = warewolf@dev2.co.za      | [[e]]                                 | [[rs(*).a]] =                | [[at([[int]]).set]] = E:\tr.txt, [[int]]=2 | An    | [[result]] = Invalid Email Source                       |
	| [[email([[int]]).rs]] = warewolf@dev2.co.za | [[email(*).rs]] =                           | New Email Test                    | [[rs(*).a]] = Test       | [[rec().set]] = warewolf@dev2.co.za       | [[rs(*).a]] =                         | This is a test               |                                            | An    | [[result]] = The recipient must be specified            |
	| warewolf@dev2.co.za                         | info@dev2.co.za                             | New Email Test                    | [[rs([[int]]).a]] = Test | [[rs(*).a]] =                             | [[rec().set]] = warewolf@dev2.co.za   | This is a test               | E:\test.txt                                | No    | [[rs().a]] = Success                                    |
	| warewolf@dev2.co.za                         | info@dev2.co.za                             | New Email Test                    | Test123                  | [[rs([[int]]).a]] =   warewolf@dev2.co.za | [[rs([[int]]).a]] =   info@dev2.co.za | This is a test               |                                            | No    | [[rs(1).a]] = Success                                   |
	| warewolf@dev2.co.za                         | info@dev2.co.za                             | New Email Test                    | Test123                  | [[rs([[int]]).a]] =   warewolf@dev2.co.za | [[rs([[int]]).a]] =   info@dev2.co.za | This is a test               | 121                                        | AN    | [[result]] = Attachment is not the valid format :121    |

@ignore
#Complex Types WOLF-1042
Scenario Outline: Sending an email using complex types
	Given the from account is '<from>' equals '<FromVal>'
	And to address is '<To>' equals '<ToVal>'
	And the subject is '<subject>'
	And Password is '<password>' 
	And the Bcc is '<Bcc>'
	And the Cc is '<Cc>'
	And body is '<body>'
	And the attachment is '<attachments>'
	And the execution has '<error>' error
	And Result is '<result>'
	Examples: 
	| from                             | FromVal             | To                      | ToVal           | subject      | password | Bcc             | Cc              | body        | attachments | error | result                                             |
	| [[client().set().value]]         | warewolf@dev2.co.za | [[rs(1).set().value()]] | ""              | Numeric Test | 3        | 100             | 50              | hello world |             | An    | [[result]] = To address is not in the valid format |
	| [[client(1).set(*).value]]       | warewolf@dev2.co.za | [[rs(1).set().value]]   | info@dev2.co.za | ""           | test123  | test@dev2.co.za | user@dev2.co.za | [[b]]       | E:\test.txt | No    | [[rs(*).a]] = Success                              |
	| [[client(1).set([[int]]).value]] | warewolf@dev2.co.za | [[rs(1).set().value]]   | info@dev2.co.za | ""           | test123  | test@dev2.co.za | user@dev2.co.za | [[b]]       | E:\test.txt | No    | [[rs(*).a]] = Success                              |
Scenario: Send email with a null variable in from account
	Given I have an email variable "[[a]]" equal to "NULL" 
	And the from account is "[[a]]" 
	And to address is "test1@freemail.com" 	
	And body is "this is a test"
	When the email tool is executed
	Then the execution has "AN" error


Scenario: Send email with a non existent variable in from account
	Given the from account is "[[a]]" 
	And to address is "test1@freemail.com" 	
	And body is "this is a test"
	When the email tool is executed
	Then the execution has "AN" error

