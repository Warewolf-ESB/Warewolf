@Utility
Feature: WebRequest
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document


Scenario: Enter a URL to download html  
	Given I have the url "http://opswolf.com:8080/api/Values" without timeout
	When the web request tool is executed
	Then the result should contain the string "value1"
	And the result should contain the string "value2"
	And the execution has "NO" error
	And the debug inputs as  
	| URL                                | Header |
	| http://opswolf.com:8080/api/Values |        |
	And the debug output as 
	|                                  |
	| [[result]] = ["value1","value2"] |

Scenario: Enter a badly formed URL
	Given I have the url "www.google.comx" without timeout
	When the web request tool is executed 
	Then the result should contain the string ""
	And the execution has "AN" error
	And the debug inputs as  
	| URL             | Header |
	| www.google.comx |        |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Enter a URL made up of text and variables with no header
    Given I have the url "http://[[site]][[file]]" without timeout
	And I have a web request variable "[[site]]" equal to "opswolf.com:8080/api/"	
	And I have a web request variable "[[file]]" equal to "values"
	When the web request tool is executed
	Then the result should contain the string "value1"
	And the result should contain the string "value2"
	And the execution has "NO" error
	And the debug inputs as  
	| URL                                                                      | Header |
	| http://[[site]][[file]] = http://TFSBLD.premier.local:9810/api/products/Get |        |
	And the debug output as 
	|                     |
	| [[result]] = String |


Scenario: Enter a URL and 2 variables each with a header parameter (json)
	Given I have the url "http://opswolf.com:8080/api/Values" without timeout
	And I have a web request variable "[[ContentType]]" equal to "Content-Type"	
	And I have a web request variable "[[Type]]" equal to "application/json"	
	And I have the Header "[[ContentType]]: [[Type]]"
	When the web request tool is executed 
	Then the result should contain the string "value1"
	And the result should contain the string "value2"
	And the execution has "NO" error
	And the debug inputs as  
	| URL                                | Header                                                      |
	| http://opswolf.com:8080/api/Values | [[ContentType]]: [[Type]] = Content-Type: application/json" |
	And the debug output as 
	|                                  |
	| [[result]] = ["value1","value2"] |

Scenario: Enter a URL and 2 variables each with a header parameter (xml)
	Given I have the url "http://opswolf.com:8080/api/Values" without timeout
	And I have a web request variable "[[ContentType]]" equal to "Content-Type"	
	And I have a web request variable "[[Type]]" equal to "application/xml"	
	And I have the Header "[[ContentType]]: [[Type]]"
	When the web request tool is executed 
	Then the result should contain the string "<string>value1</string><string>value2</string>"
	And the execution has "NO" error
	And the debug inputs as  
	| URL                                | Header                                                     |
	| http://opswolf.com:8080/api/Values | [[ContentType]]: [[Type]] = Content-Type: application/xml" |
	And the debug output as 
	|                                                                                                                   |
	| [[result]] = <ArrayOfstring xmlns:i= http://www.w3.org/2001/XMLSchema-instance  xmlns= http://schemas.microsoft.c |

Scenario: Enter a URL that returns json
	Given I have the url "http://opswolf.com:8080/api/values" without timeout
	When the web request tool is executed
	Then the result should contain the string "value1"
	And the result should contain the string "value2"
	And the execution has "NO" error
	And the debug inputs as  
	| URL                                               | Header |
	| http://TFSBLD.premier.local:9810/api/products/Get |        |
	And the debug output as 
	|                                   |
	| [[result]] = ["value1","value2"]  |

Scenario: Enter a URL that returns xml
	Given I have the url "http://opswolf.com:8080/api/values" without timeout
	And I have the Header "Content-Type: application/xml"
	When the web request tool is executed	
	Then the result should contain the string "<string>value1</string><string>value2</string>"
	And the execution has "NO" error
	And the debug inputs as  
	| URL                                               | Header |
	| http://TFSBLD.premier.local:9810/api/products/Get |        |
	And the debug output as 
	|                                                                                                                   |
	| [[result]] = <ArrayOfstring xmlns:i= http://www.w3.org/2001/XMLSchema-instance  xmlns= http://schemas.microsoft.c |

Scenario: Enter a blank URL
	Given I have the url "" without timeout
	When the web request tool is executed	
	Then the result should contain the string ""
	And the execution has "AN" error
	And the debug inputs as  
	| URL | Header |
	| ""  |        |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario: Enter a URL that is a negative index recordset
	Given I have the url "[[rec(-1).set]]" without timeout
	When the web request tool is executed	
	Then the result should contain the string ""
	And the execution has "AN" error
	And the debug inputs as  
	| URL               | Header |
	| [[rec(-1).set]] = |        |
	And the debug output as 
	|              |
	| [[result]] = |

Scenario Outline: Enter a number or variable that does not exist as URL
	Given I have the url "<url>" with timeoutSeconds "<timeoutSeconds>"
	And I have the Header "<Header>"
	When the web request tool is executed	
	Then the result should contain the string ""
	And the execution has "AN" error
	And the debug inputs as  
	| URL   | Header | Time Out Seconds |
	| <url> | <Header>       | <timeoutSeconds> |
	And the debug output as 
	|              | 
	| [[result]] = | 
Examples: 
	| url                                                  | timeoutSeconds | Header  | Error                                                                            |
	| 88                                                   |                |         | Unable to connect to the remote server                                           |
	| [[y]]                                                |                |         | Invalid URI: The hostname could not be parsed                                    |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15 | 10             |         | Value [[y]] for TimeoutSeconds Text could not be interpreted as a numeric value. |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15 | 10             |         | Value    for TimeoutSeconds Text could not be interpreted as a numeric value.    |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15 | 10             |         | Value sdf for TimeoutSeconds Text could not be interpreted as a numeric value.   |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15 | 10             | 21245   | Index was outside the bounds of the array                                        |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15 | 10             | [[var]] | Object reference not set to instance  of object                                  |


Scenario: Enter a URL that is a null variable
	Given I have a formatnumber variable "[[var]]" equal to NULL
	And I have the url "[[var]]" without timeout
	When the web request tool is executed	
	Then the execution has "AN" error
	And the debug inputs as  
	| URL       | Header |
	| [[var]] = |        |


Scenario: Enter a URL that is a non existent variable
	Given I have the url "[[var]]" without timeout
	When the web request tool is executed	
	Then the execution has "AN" error
	And the debug inputs as  
	| URL       | Header |
	| [[var]] = |        |
	
Scenario Outline: Enter a URL to download html with timeout specified too short 
	Given I have the url "<url>" with timeoutSeconds "<timeoutSeconds>"
	When the web request tool is executed 
	Then the execution has "AN" error
	And the debug inputs as  
	| URL   | Header | Time Out Seconds |
	| <url> |        | <timeoutSeconds> |
	And the debug output as 
	|                     |
	| [[result]] = String |
	Examples:
	| url                                                   | timeoutSeconds |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=150 | 10             |
	| http://tst-ci-remote:3142/Public/Wait?WaitSeconds=15  | 10             |

Scenario: Enter a recordset star input and output
	Given I have a web request variable "[[urls().url]]" equal to "http://TFSBLD.premier.local/IntegrationTestSite/Proxy.ashx"	
	And I have a web request variable "[[urls().url]]" equal to "http://tst-ci-remote:3142/secure/Wait?WaitSeconds=15"	
	And I have a web request variable "[[results().res]]" equal to "res1"	
	And I have the url "[[urls(*).url]]" without timeout
	And I have web request result as "[[results(*).res]]"
	When the web request tool is executed 
	Then the execution has "NO" error
	And the debug inputs as  
	| URL                                                                     | Header |
	| [[urls(1).url]] = http://TFSBLD.premier.local/IntegrationTestSite/Proxy.ashx |        |
	| [[urls(2).url]] = http://tst-ci-remote:3142/secure/Wait?WaitSeconds=15  |        |
