﻿Feature: XPath
	In order to run a query against xml
	As a Warewolf user
	I want a tool that I can use to execute xpath queries

Scenario: Use XPath to get data off XML - Id = 1
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[firstNum]]" output with xpath "//root/number[@id='1']/text()"
	When the xpath tool is executed
	Then the variable "[[firstNum]]" should have a value "One"
	And the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                              | # |                                              |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[firstNum]] = //root/number[@id='1']/text() |
	And the debug output as 
	| # |                    |
	| 1 | [[firstNum]] = One |       

Scenario: Use XPath to get data off XML - Id = 2
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[firstNum]]" output with xpath "//root/number[@id='2']/text()"
	When the xpath tool is executed
	Then the variable "[[firstNum]]" should have a value "Two"
	And the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                              | # |                                              |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[firstNum]] = //root/number[@id='2']/text() |
	And the debug output as 
	| # |                    |
	| 1 | [[firstNum]] = Two |

Scenario: Use XPath to build a recordset with 2 fields
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[rec(*).id]]" output with xpath "//root/number/@id"
	And I have a variable "[[rec2(*).text]]" output with xpath "//root/number/text()"
	When the xpath tool is executed
	Then the xpath result for this varibale "[[rec(*).id]]" will be
	| rec().id |
	| 1        |
	| 2        |
	| 3        |
	Then the xpath result for this varibale "[[rec2(*).text]]" will be
	| rec().text |
	| One        |
	| Two        |
	| Three      |
	And the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                              | # |                                         |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[rec(*).id]] = //root/number/@id       |
	|                                                                                                  | 2 | [[rec2(*).text]] = //root/number/text() |
	And the debug output as 
	| # |                          |
	| 1 | [[rec(1).id]] = 1        |
	|   | [[rec(2).id]] = 2        |
	|   | [[rec(3).id]] = 3        |
	| 2 | [[rec2(1).text]] = One   |
	|   | [[rec2(2).text]] = Two   |
	|   | [[rec2(3).text]] = Three |  

Scenario: Use XPath that does not exist
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'	
	And I have a variable "[[ids]]" output with xpath "//root/num/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                              | # |                          |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[ids]] = //root/num/@id |
	And the debug output as 
	| # |           |
	| 1 | [[ids]] = |

Scenario: Use invalid xpath query
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>' in a variable "[[myxml]]"
	And I assign the variable "[[myxml]]" as xml input
	And I have a variable "[[ids]]" output with xpath "@@#$"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error
	And the debug inputs as  
	| XML                                                                                                          | # |                |
	| [[myxml]] = <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[ids]] = @@#$ |
	And the debug output as 
	| # |           |
	| 1 | [[ids]] = | 

Scenario: Use XPath with append notation should add
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>' in a variable "[[rec().set]]"
	And I assign the variable "[[rec().set]]" as xml input
	And I have a variable "[[rec().set]]" output with xpath "//root/number/@id"
	When the xpath tool is executed
	Then the xpath result for this varibale "[[rec().set]]" will be
	| rec().set                                                                                        |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> |
	| 1                                                                                                |
	| 2                                                                                                |
	| 3                                                                                                |
	And the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                                               | # |                                    |
	| [[rec(1).set]] = <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[rec().set]] = //root/number/@id |
	And the debug output as 
	| # |                                                                                                                   |
	| 1 | [[rec(1).set]] = <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> |
	|   | [[rec(2).set]] = 1                                                                                                |
	|   | [[rec(3).set]] = 2                                                                                                |
	|   | [[rec(4).set]] = 3                                                                                                |  

Scenario: Use XPath with invalid XML as input inside a 
	Given I have this xml '<start></end>' in a variable "[[myxml]]"
	And I assign the variable "[[myxml]]" as xml input
	And I have a variable "[[ids]]" output with xpath "//root"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error
	And the debug inputs as  
	| XML                      | # |                  |
	| [[myxml]] = <start></end> | 1 | [[ids]] = //root |
	And the debug output as 
	| # |           |
	| 1 | [[ids]] = |
	
Scenario: Use XPath with no  result but valid xpath
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "" output with xpath "//root/number/@id"
	When the xpath tool is executed	
	Then the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                              | # |  |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> |   |  |

Scenario: Use XPath to get multiple results into a scalar in CSV
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[ids]]" output with xpath "//root/number/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value "1,2,3"
	And the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                              | # |                             |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[ids]] = //root/number/@id |
	And the debug output as 
	| # |                 |
	| 1 | [[ids]] = 1,2,3 | 

Scenario: Use the XPath to process blank XML
	Given I have this xml ''
	And I have a variable "[[ids]]" output with xpath "//root/number/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error
	And the debug inputs as  
	| XML | # |                              |
	| ""  | 1 | [[ids]] =  //root/number/@id |
	And the debug output as 
	| # |           |
	| 1 | [[ids]] = |  

Scenario: Use the XPath without any xpath query
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[ids]]" output with xpath ""
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error
	And the debug inputs as  
	| XML                                                                                              | # |           |
	| <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[ids]] = |
	And the debug output as 
	| # |           |
	| 1 | [[ids]] = | 
	
Scenario: Use XPath with blank  as XML input	
	Given I have this xml '' in a variable "[[myxml]]"
	And I assign the variable "[[myxml]]" as xml input
	And I have a variable "[[ids]]" output with xpath "//root/num/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error
	And the debug inputs as  
	| XML         | # |                          |
	| [[myxml]] = | 1 | [[ids]] = //root/num/@id |
	And the debug output as 
	| # |           |
	| 1 | [[ids]] = |


Scenario: Use XPath with negative recordset index as output 
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>' in a variable "[[xml]]"
	And I assign the variable "[[xml]]" as xml input
	And I have a variable "[[rec(-1).ids]]" output with xpath "//root/number[@id='2']/text()"
	When the xpath tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| XML                                                                                                        | # |                                                 |
	| [[xml]] = <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | 1 | [[rec(-1).ids]] = //root/number[@id='2']/text() |
	And the debug output as 
	| # |                   |
	| 1 | [[rec(-1).ids]] = |

Scenario: Use XPath to with a comment in it
	Given I have this xml '<?xml version="1.0" encoding="utf-8"?><!-- Generated by Warewolf --><root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>' in a variable "[[xml]]"
	And I assign the variable "[[xml]]" as xml input
	And I have a variable "[[rec(*).id]]" output with xpath "//root/number/@id"
	When the xpath tool is executed
	Then the xpath result for this varibale "[[rec(*).id]]" will be
	| rec().id |
	| 1        |
	| 2        |
	| 3        |
	And the execution has "NO" error
	And the debug inputs as  
	| XML                                                                                                            | # |                                   |
	| [[xml]] = <?xml version="1.0" encoding="utf-8"?><!-- Generated by Warewolf --><root><number id="1">One</number | 1 | [[rec(*).id]] = //root/number/@id |
	And the debug output as 
	| # |                          |
	| 1 | [[rec(1).id]] = 1        |
	|   | [[rec(2).id]] = 2        |
	|   | [[rec(3).id]] = 3        |	


Scenario: Use XPath with a NULL variable as output 
	Given I have a formatnumber variable "[[var]]" equal to NULL
	And I assign the variable "[[var]]" as xml input
	And I have a variable "[[rec(1).ids]]" output with xpath "//root/number[@id='2']/text()"
	When the xpath tool is executed
	Then the execution has "AN" error

Scenario: Use XPath with a non existent variable as output 
	Given I assign the variable "[[var]]" as xml input
	And I have a variable "[[rec(1).ids]]" output with xpath "//root/number[@id='2']/text()"
	When the xpath tool is executed
	Then the execution has "AN" error

Scenario Outline: Use XPath to get data off XML using recordsets
	Given I have this xml '<Xmlvalue>'
	And The result variable is '<Xml>' 
	And The path variable is '<Xpath>' equals '<path>'
	When the xpath tool is executed
	Then the variable '<Recordset>' should have a value '<value>'
	And the execution has '<Error>' error
	And the debug inputs as  
	| XML        | value   | Variable    | Xpath  |
	| <Xmlvalue> | <value> | <Recordset> | <path> |
	And the debug output as 
	| # |                   |
	| 1 | <First> = <value> |  
Examples: 
| Xml                            | Xmlvalue                                                                                         | Recordset                        | Xpath                           | path                          | value                                                                | Error |
| [[rec().a]]                    | <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | [[firstNum]]                     | [[path().set]]                  | //root/number[@id='1']/text() | One                                                                  | No    |
| [[rec(1).a]]                   | <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | [[firstNum]]                     | [[path(1).set]]                 | //root/number[@id='1']/text() | One                                                                  | No    |
| [[rec(*).a]]                   | <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | [[F([[int]]).number]],[[int]] =1 | [[path(*).set]]                 | //root/number[@id='1']/text() | One                                                                  | No    |
| [[rec([[int]]).a]], [[int]] =1 | <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | [[firstNum]]                     | [[path([[int]]).set]],[[int]]=1 | //root/number[@id='1']/text() | One                                                                  | No    |
| [[var]]                        |                                                                                                  | [[F().number]]                   | [[var]]                         | //root/number[@id='1']/text() | <InnerError>Value cannt be null. Parameter name:xmlData</InnerError> | An    |
| [[rec().a]]                    | <root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root> | [[firstNum]]                     | [[variable]]                    |                               | <InnerError>Value cannt be null. Parameter name:xPath</InnerError>   | An    |
