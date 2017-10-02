@Data
Feature: CreateJSON
	In order to create a json payload
	As a warewolf user
	I want to be given the JSON representation of my variables


Scenario Outline: Single Scalar Variable
	Given I have a variable "[[a]]" with value "<value>"
	And I select variable "[[a]]" with name "a"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                 |
	| 1 | [[a]] = <value> |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	| type          | value | result        |
	| Character     | c     | {"a":"c"}     |
	| Integer       | 2     | {"a":2}       |
	| Decimal       | 5.6   | {"a":5.6}     |
	| String        | Hello | {"a":"Hello"} |
	| Boolean_True  | true  | {"a":true}    |
	| Boolean_False | false | {"a":false}   |
	| Null          |       | {"a":null}    |


Scenario Outline: Single Scalar VariableMultipleSelections
	Given I have a variable "[[a]]" with value "<value>"
	Given I have a variable "[[b]]" with value "x"
	And I select variable "[[a]]" with name "a"
	And I select variable "[[b]]" with name "c"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                 |
	| 1 | [[a]] = <value> |
	| 2 | [[b]] = x |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	| type | value | result        |
	| Character     | c     | {"a":"c","c":"x"}     |
	| Integer       | 2     | {"a":2,"c":"x"}       |
	| Decimal       | 5.6   | {"a":5.6,"c":"x"}     |
	| String        | Hello | {"a":"Hello","c":"x"} |
	| Boolean_True  | true  | {"a":true,"c":"x"}    |
	| Boolean_False | false | {"a":false,"c":"x"}   |
	| Null          |       | {"a":null,"c":"x"}    |

Scenario Outline: Single Scalar Variable with changed name
	Given I have a variable "[[a]]" with value "<value>"
	And I select variable "[[a]]" with name "myVar"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                 |
	| 1 | [[a]] = <value> |
	And the debug output as
	|                     |
	| [[json]] = <result> |
Examples: 
	|  type | value | result        |
	| Character     | c     | {"myVar":"c"}     |
	| Integer       | 2     | {"myVar":2}       |
	| Decimal       | 5.6   | {"myVar":5.6}     |
	| String        | Hello | {"myVar":"Hello"} |
	| Boolean_True  | true  | {"myVar":true}    |
	| Boolean_False | false | {"myVar":false}   |
	| Null          |       | {"myVar":null}    |

Scenario Outline: Simple Recordset single field
	Given I have a variable "[[rec().a]]" with value "<value>"
	And I select variable "[[rec().a]]" with name "a"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                        |
	| 1 | [[rec(1).a]] = <value> |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	|    type | value | result        |
	| Character     | c     | {"a":["c"]}     |
	| Integer       | 2     | {"a":[2]}       |
	| Decimal       | 5.6   | {"a":[5.6]}     |
	| String        | Hello | {"a":["Hello"]} |
	| Boolean_True  | true  | {"a":[true]}    |
	| Boolean_False | false | {"a":[false]}   |

Scenario Outline: Simple Recordset single field Null
	Given I have a variable "[[rec().a]]" with value "<value>"
	And I select variable "[[rec().a]]" with name "a"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                        |
	| 1 | [[rec().a]] = <value> |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	|    type | value | result        |
	| Null          |       | {"a":[null]}    |

Scenario Outline: Multiple Scalars Variable
	Given I have a variable "[[a]]" with value "<valueA>"
	And I have a variable "[[b]]" with value "<valueB>"
	And I select variable "[[a]]" with name "a"
	And I select variable "[[b]]" with name "b"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[a]] = <valueA> |
	| 2 | [[b]] = <valueB> |
	And the debug output as
	|                     |
	| [[json]] = <result> |
Examples: 
	| type | valueA | valueB | result                 |
	| Character     | c      | 3      | {"a":"c","b":3}        |
	| Integer       | 2      | a      | {"a":2,"b":"a"}        |
	| Decimal       | 5.6    | World  | {"a":5.6,"b":"World"}  |
	| String        | Hello  | 10.1   | {"a":"Hello","b":10.1} |
	| Boolean_True  | true   |        | {"a":true,"b":null}    |
	| Boolean_False | false  | true   | {"a":false,"b":true}   |
	| Null          |        | false  | {"a":null,"b":false}   |

Scenario Outline: Multiple Recordset Variable
	Given I have a variable "[[rec(1).a]]" with value "<valueA>"
	And I have a variable "[[rec(1).b]]" with value "<valueB>"
	And I select variable "[[rec().a]]" with name "a"
	And I select variable "[[rec().b]]" with name "b"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[rec(1).a]] = <valueA> |
	| 2 | [[rec(1).b]] = <valueB> |
	And the debug output as
	|                     |
	| [[json]] = <result> |
Examples: 
	|  type         | valueA | valueB | result                     |
	| Character     | c      | 3      | {"a":["c"],"b":[3]}        |
	| Integer       | 2      | a      | {"a":[2],"b":["a"]}        |
	| Decimal       | 5.6    | World  | {"a":[5.6],"b":["World"]}  |
	| String        | Hello  | 10.1   | {"a":["Hello"],"b":[10.1]} |
	| Boolean_True  | true   |        | {"a":[true],"b":[null]}    |
	| Boolean_False | false  | true   | {"a":[false],"b":[true]}   |
	| Null          |        | false  | {"a":[null],"b":[false]}   |
	
Scenario Outline: Multiple Scalars Variable comma seperated
	Given I have a variable "[[a]]" with value "<valueA>"
	And I have a variable "[[b]]" with value "<valueB>"
	And I select variable "[[a]],[[b]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[a]],[[b]] = <valueA>,<valueB> |
	And the debug output as
	|                     |
	| [[json]] = <result> |
Examples: 
	|   type | valueA | valueB | result                      |
	| Character     | c      | 3      | {"rec":{"a":"c","b":3}}        |
	| Integer       | 2      | a      | {"rec":{"a":2,"b":"a"}}        |
	| Decimal       | 5.6    | World  | {"rec":{"a":5.6,"b":"World"}}  |
	| String        | Hello  | 10.1   | {"rec":{"a":"Hello","b":10.1}} |
	| Boolean_True  | true   |  3      | {"rec":{"a":true,"b":3}}    |
	| Boolean_False | false  | true   | {"rec":{"a":false,"b":true}}   |

Scenario Outline: Multiple Scalars Variable comma seperated Null 
	Given I have a variable "[[a]]" with value "<valueA>"
	And I have a variable "[[b]]" with value "<valueB>"
	And I select variable "[[a]],[[b]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[a]],[[b]] =  |
	And the debug output as
	|                     |
	| [[json]] = <result> |
Examples: 
	|   type | valueA | valueB | result                      |
	| Null          |        | false  | {"rec":{"a":null,"b":false}}   |

Scenario Outline: Multiple Recordset Variable comma seperatedNull
	Given I have a variable "[[rec(1).a]]" with value "<valueA>"
	And I have a variable "[[rec(1).b]]" with value "<valueB>"
	And I select variable "[[rec().a]],[[rec().b]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[rec().a]],[[rec().b]] =  |
	And the debug output as
	|                     |
	| [[json]] = <result> |
Examples: 
	|   type | valueA | valueB | result                         |
	| Null          |        | false  | {"rec":[{"a":null,"b":false}]}   |

Scenario Outline: Multiple Recordset Variable comma seperated
	Given I have a variable "[[rec(1).a]]" with value "<valueA>"
	And I have a variable "[[rec(1).b]]" with value "<valueB>"
	And I select variable "[[rec().a]],[[rec().b]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[rec().a]],[[rec().b]] = <valueA>,<valueB> |
	And the debug output as
	|                     |
	| [[json]] = <result> |
Examples: 
	|   type | valueA | valueB | result                         |
	| Character     | c      | 3      | {"rec":[{"a":"c","b":3}]}        |
	| Integer       | 2      | a      | {"rec":[{"a":2,"b":"a"}]}        |
	| Decimal       | 5.6    | World  | {"rec":[{"a":5.6,"b":"World"}]}  |
	| String        | Hello  | 10.1   | {"rec":[{"a":"Hello","b":10.1}]} |
	| Boolean_True  | true   |   3     | {"rec":[{"a":true,"b":3}]}    |
	| Boolean_False | false  | true   | {"rec":[{"a":false,"b":true}]}   |

Scenario Outline: Simple Recordset with * single field Null
	Given I have a variable "[[rec(*).a]]" with value "<value>"
	And I select variable "[[rec(*).a]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                        |
	| 1 | [[rec(*).a]] =  |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	|  type | value | result                |
	| Null          |       | {"rec":[null]}    |

Scenario Outline: Simple Recordset with * single field
	Given I have a variable "[[rec(*).a]]" with value "<value>"
	And I select variable "[[rec(*).a]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                        |
	| 1 | [[rec(1).a]] = <value> |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	|  type | value | result                |
	| Character     | c     | {"rec":["c"]}     |
	| Integer       | 2     | {"rec":[2]}       |
	| Decimal       | 5.6   | {"rec":[5.6]}     |
	| String        | Hello | {"rec":["Hello"]} |
	| Boolean_True  | true  | {"rec":[true]}    |
	| Boolean_False | false | {"rec":[false]}   |


Scenario Outline: Recordset with * multiple fields and values Null
	Given I have a variable "[[rec(1).a]]" with value "<valueA1>"
	Given I have a variable "[[rec(2).a]]" with value "<valueA2>"
	Given I have a variable "[[rec(3).a]]" with value "<valueA3>"
	Given I have a variable "[[rec(1).b]]" with value "<valueB1>"
	Given I have a variable "[[rec(2).b]]" with value "<valueB2>"
	Given I have a variable "[[rec(3).b]]" with value "<valueB3>"
	And I select variable "[[rec(*)]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                          |
	|  1 | [[rec(1).a]] = <valueA1> |
	|   | [[rec(2).b]] = <valueB2> |
	|   | [[rec(3).b]] = <valueB3> |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	| type          | valueA1 | valueA2 | valueA3 | valueB1 | valueB2 | valueB3 | result                                                                       |
	| Null          |a         |         |         |     | false   |    true     | {"rec":[{"a":"a","b":null},{"a":null,"b":false},{"a":null,"b":true}]}       |

Scenario Outline: Recordset with * multiple fields and values
	Given I have a variable "[[rec(1).a]]" with value "<valueA1>"
	Given I have a variable "[[rec(2).a]]" with value "<valueA2>"
	Given I have a variable "[[rec(3).a]]" with value "<valueA3>"
	Given I have a variable "[[rec(1).b]]" with value "<valueB1>"
	Given I have a variable "[[rec(2).b]]" with value "<valueB2>"
	Given I have a variable "[[rec(3).b]]" with value "<valueB3>"
	And I select variable "[[rec(*)]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "<result>"
	And the execution has "NO" error
	And the debug inputs as
	| # |                          |
	|  1 | [[rec(1).a]] = <valueA1> |
	|   | [[rec(2).a]] = <valueA2> |
	|   | [[rec(3).a]] = <valueA3> |
	|   | [[rec(1).b]] = <valueB1> |
	|   | [[rec(2).b]] = <valueB2> |
	|   | [[rec(3).b]] = <valueB3> |
	And the debug output as
	|                       |
	|   [[json]] = <result> |
Examples: 
	| type          | valueA1 | valueA2 | valueA3 | valueB1 | valueB2 | valueB3 | result                                                                       |
	| Character     | c       | b       | g       | 1       | 2       | 3       | {"rec":[{"a":"c","b":1},{"a":"b","b":2},{"a":"g","b":3}]}                    |
	| Integer       | 2       | 56      | 100     | g       | h       | i       | {"rec":[{"a":2,"b":"g"},{"a":56,"b":"h"},{"a":100,"b":"i"}]}                 |
	| Decimal       | 5.6     | 7.1     | 100.34  | Hello   | World   | bob     | {"rec":[{"a":5.6,"b":"Hello"},{"a":7.1,"b":"World"},{"a":100.34,"b":"bob"}]} |
	| String        | Hello   | name    | dora    | 34      |    st     | 56      | {"rec":[{"a":"Hello","b":34},{"a":"name","b":"st"},{"a":"dora","b":56}]}     |
	| Boolean_True  | true    | false   |   a      | 78.1    | 145.25  | 90.2    | {"rec":[{"a":true,"b":78.1},{"a":false,"b":145.25},{"a":"a","b":90.2}]}     |
	| Boolean_False | false   | bob     |   9      |1         |   8      |   7      | {"rec":[{"a":false,"b":1},{"a":"bob","b":8},{"a":9,"b":7}]}    |


Scenario: Recordset with * multiple fields and values different length for columns
	Given I have a variable "[[rec(1).a]]" with value "c"
	Given I have a variable "[[rec(2).a]]" with value "b"
	Given I have a variable "[[rec(3).a]]" with value "g"
	Given I have a variable "[[rec(1).b]]" with value "1"
	Given I have a variable "[[rec(2).b]]" with value "2"
	And I select variable "[[rec(*)]]" with name "rec"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "{"rec":[{"a":"c","b":1},{"a":"b","b":2},{"a":"g","b":null}]}"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[rec(1).a]] = c |
	|   | [[rec(2).a]] = b |
	|   | [[rec(3).a]] = g |
	|   | [[rec(1).b]] = 1 |
	|   | [[rec(2).b]] = 2 |
	And the debug output as
	|                                                                           |
	|   [[json]] = {"rec":[{"a":"c","b":1},{"a":"b","b":2},{"a":"g","b":null}]} |



Scenario: Recordset with * multiple fields and  scalar values different length for columnMultiple Columns 
	Given I have a variable "[[rec(1).a]]" with value "c"
	Given I have a variable "[[rec(2).a]]" with value "b"
	Given I have a variable "[[rec(3).a]]" with value "g"
	Given I have a variable "[[rec(1).b]]" with value "1"
	Given I have a variable "[[rec(2).b]]" with value "2"
	Given I have a variable "[[a]]" with value "the builder"
	And I select variable "[[rec(*).a]]" with name "rec"
	And I select variable "[[a]]" with name "bob"
	And a result variable "[[json]]"
	When the create json tool is executed
	Then the value of "[[json]]" should be "{"rec":["c","b","g"],"bob":"the builder"}"
	And the execution has "NO" error
	And the debug inputs as
	| # |                  |
	| 1 | [[rec(1).a]] = c |
	|   | [[rec(2).a]] = b |
	|   | [[rec(3).a]] = g |
	| 2 | [[a]] = the builder |
	And the debug output as
	|                                                                                             |
	| [[json]] = { rec :[ c , b , g ], bob : the builder } |
