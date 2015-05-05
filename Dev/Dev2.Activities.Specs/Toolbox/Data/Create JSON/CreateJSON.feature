Feature: CreateJSON
	In order to create a json payload
	As a warewolf user
	I want to be given the JSON representation of my variables


Scenario Outline: Single Scalar Variable
	Given I have a variable "[[a]]" with value <value>
	And I select variable "[[a]]"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                 |
	| 1 | [[a]] = <value> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | value | result        |
	| Character     | c     | {"a":"c"}     |
	| Integer       | 2     | {"a":2}       |
	| Decimal       | 5.6   | {"a":5.6}     |
	| String        | Hello | {"a":"Hello"} |
	| Boolean_True  | true  | {"a":true}    |
	| Boolean_False | false | {"a":false}   |
	| Null          |       | {"a":null}    |

Scenario Outline: Single Scalar Variable with changed name
	Given I have a variable "[[a]]" with value <value>
	And I select variable "[[a]]" with name "myVar"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                 |
	| 1 | [[a]] = <value> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | value | result        |
	| Character     | c     | {"myVar":"c"}     |
	| Integer       | 2     | {"myVar":2}       |
	| Decimal       | 5.6   | {"myVar":5.6}     |
	| String        | Hello | {"myVar":"Hello"} |
	| Boolean_True  | true  | {"myVar":true}    |
	| Boolean_False | false | {"myVar":false}   |
	| Null          |       | {"myVar":null}    |

Scenario Outline: Simple Recordset single field
	Given I have a variable "[[rec().a]]" with value <value>
	And I select variable "[[rec().a]]"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                        |
	| 1 | [[rec(1).a]] = <value> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | value | result        |
	| Character     | c     | {"a":"c"}     |
	| Integer       | 2     | {"a":2}       |
	| Decimal       | 5.6   | {"a":5.6}     |
	| String        | Hello | {"a":"Hello"} |
	| Boolean_True  | true  | {"a":true}    |
	| Boolean_False | false | {"a":false}   |
	| Null          |       | {"a":null}    |

Scenario Outline: Multiple Scalars Variable
	Given I have a variable "[[a]]" with value <valueA>
	And I have a variable "[[b]]" with value <valueB>
	And I select variable "[[a]]"
	And I select variable "[[b]]"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[a]] = <valueA> |
	| 2 | [[b]] = <valueB> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | valueA | valueB | result                 |
	| Character     | c      | 3      | {"a":"c","b":3}        |
	| Integer       | 2      | a      | {"a":2,"b":"a"}        |
	| Decimal       | 5.6    | World  | {"a":5.6,"b":"World"}  |
	| String        | Hello  | 10.1   | {"a":"Hello","b":10.1} |
	| Boolean_True  | true   |        | {"a":true,"b":null}    |
	| Boolean_False | false  | true   | {"a":false,"b":true}   |
	| Null          |        | false  | {"a":null,"b":false}   |

Scenario Outline: Multiple Recordset Variable
	Given I have a variable "[[rec().a]]" with value <valueA>
	And I have a variable "[[rec().b]]" with value <valueB>
	And I select variable "[[rec().a]]"
	And I select variable "[[rec().b]]"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[a]] = <valueA> |
	| 2 | [[b]] = <valueB> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | valueA | valueB | result                 |
	| Character     | c      | 3      | {"a":"c","b":3}        |
	| Integer       | 2      | a      | {"a":2,"b":"a"}        |
	| Decimal       | 5.6    | World  | {"a":5.6,"b":"World"}  |
	| String        | Hello  | 10.1   | {"a":"Hello","b":10.1} |
	| Boolean_True  | true   |        | {"a":true,"b":null}    |
	| Boolean_False | false  | true   | {"a":false,"b":true}   |
	| Null          |        | false  | {"a":null,"b":false}   |
	
Scenario Outline: Multiple Scalars Variable comma seperated
	Given I have a variable "[[a]]" with value <valueA>
	And I have a variable "[[b]]" with value <valueB>
	And I select variable "[[a]],[[b]]"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[a]] = <valueA> |
	| 2 | [[b]] = <valueB> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | valueA | valueB | result                      |
	| Character     | c      | 3      | {"":{"a":"c","b":3}}        |
	| Integer       | 2      | a      | {"":{"a":2,"b":"a"}}        |
	| Decimal       | 5.6    | World  | {"":{"a":5.6,"b":"World"}}  |
	| String        | Hello  | 10.1   | {"":{"a":"Hello","b":10.1}} |
	| Boolean_True  | true   |        | {"":{"a":true,"b":null}}    |
	| Boolean_False | false  | true   | {"":{"a":false,"b":true}}   |
	| Null          |        | false  | {"":{"a":null,"b":false}}   |

Scenario Outline: Multiple Recordset Variable comma seperated
	Given I have a variable "[[rec().a]]" with value <valueA>
	And I have a variable "[[rec().b]]" with value <valueB>
	And I select variable "[[rec().a]],[[rec().b]]"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[a]] = <valueA> |
	| 2 | [[b]] = <valueB> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | valueA | valueB | result                         |
	| Character     | c      | 3      | {"rec":{"a":"c","b":3}}        |
	| Integer       | 2      | a      | {"rec":{"a":2,"b":"a"}}        |
	| Decimal       | 5.6    | World  | {"rec":{"a":5.6,"b":"World"}}  |
	| String        | Hello  | 10.1   | {"rec":{"a":"Hello","b":10.1}} |
	| Boolean_True  | true   |        | {"rec":{"a":true,"b":null}}    |
	| Boolean_False | false  | true   | {"rec":{"a":false,"b":true}}   |
	| Null          |        | false  | {"rec":{"a":null,"b":false}}   |

Scenario Outline: Simple Recordset with * single field
	Given I have a variable "[[rec(*).a]]" with value <value>
	And I select variable "[[rec(*).a]]"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be <result>
	And the execution has "NO" error
	And the debug inputs as
	| # |                        |
	| 1 | [[rec(1).a]] = <value> |
	And debug output as
	|  |                     |
	|  | [[json]] = <result> |
Examples: 
	| #             | value | result                |
	| Character     | c     | {"rec":["a":"c"]}     |
	| Integer       | 2     | {"rec":["a":2]}       |
	| Decimal       | 5.6   | {"rec":["a":5.6]}     |
	| String        | Hello | {"rec":["a":"Hello"]} |
	| Boolean_True  | true  | {"rec":["a":true]}    |
	| Boolean_False | false | {"rec":["a":false]}   |
	| Null          |       | {"rec":["a":null]}    |
