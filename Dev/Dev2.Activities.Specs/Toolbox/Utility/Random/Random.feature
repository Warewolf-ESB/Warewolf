Feature: Random
	In order to generate random values
	As a Warewolf user
	I want a tool that can generate, numbers, guids and letters


Scenario: Generate Letters
	Given I have a type as "Letters"
	And I have a length as "10"
	When the random tool is executed 
	Then the result from the random tool should be of type "System.String" with a length of "10"
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | Length |
	| Letters | 10     |
	And the debug output as 
	|                     |
	| [[result]] = String |

Scenario: Generate Letters and Numbers
	Given I have a type as "LetterAndNumbers"
	And I have a length as "10"
	When the random tool is executed 
	Then the result from the random tool should be of type "System.String" with a length of "10"	
	And the execution has "NO" error
	And the debug inputs as  
	| Random            | Length |
	| Letters & Numbers | 10     |
	And the debug output as 
	|                      |
	| [[result]] = String |
	
Scenario: Generate Numbers one digit
	Given I have a type as "Numbers"
	And I have a range from "0" to "9" 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Int32" with a length of "1"
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | 0    | 9  |
	And the debug output as 
	|                     |
	| [[result]] = Int32 |	

Scenario: Generate Numbers two digits
	Given I have a type as "Numbers"
	And I have a range from "10" to "99" 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Int32" with a length of "2"
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | 10   | 99 |
	And the debug output as 
	|                     |
	| [[result]] = Int32 | 

Scenario: Generate Guid
	Given I have a type as "Guid"
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Guid" with a length of "36"
	And the execution has "NO" error
	And the debug inputs as  
	| Random |
	| GUID   |
	And the debug output as 
	|                    |
	| [[result]] = Guid |
	

Scenario: Generate Numbers with blank range
	Given I have a type as "Numbers"
	And I have a range from "" to "" 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.String" with a length of "0"
	And the execution has "AN" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | ""   | "" |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Generate Numbers with one blank range
	Given I have a type as "Numbers"
	And I have a range from "1" to "" 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.String" with a length of "0"
	And the execution has "AN" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | 1    | "" |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Generate Numbers with a negative range
	Given I have a type as "Numbers"
	And I have a range from "-1" to "-9" 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Int32" with a length of "2"
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | -1   | -9 |
	And the debug output as 
	|                     |
	| [[result]] = Int32 |

Scenario: Generate Letters with blank length
	Given I have a type as "Numbers"
	And I have a range from "" to ""  
	When the random tool is executed 
	Then the execution has "AN" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | ""   | "" |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Generate Letters with a negative length
	Given I have a type as "Letters"
	And I have a length as "-1"
	When the random tool is executed 
	Then the result from the random tool should be of type "System.String" with a length of "0"
	And the execution has "AN" error
	And the debug inputs as  
	| Random  | Length |
	| Letters | -1     |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Generate Letters and Numbers with blank length
	Given I have a type as "LetterAndNumbers"
	And I have a length as ""
	When the random tool is executed 
	Then the result from the random tool should be of type "System.String" with a length of "0"
	And the execution has "AN" error
	And the debug inputs as  
	| Random            | Length |
	| Letters & Numbers | ""     |
	And the debug output as 
	|               |
	| [[result]] = |

Scenario: Generate Letters and Numbers with a negative length
	Given I have a type as "LetterAndNumbers"
	And I have a length as ""
	When the random tool is executed 
	Then the result from the random tool should be of type "System.String" with a length of "0"
	And the execution has "AN" error
	And the debug inputs as  
	| Random            | Length |
	| Letters & Numbers | ""     |
	And the debug output as 
	|               |
	| [[result]] = |
	

Scenario: Generate a Number between 5 and 5
	Given I have a type as "Numbers"
	And I have a range from "5" to "5" 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Int32" with a length of "1"
	And the random value will be "5"
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | 5    | 5  |
	And the debug output as 
	|                     |
	| [[result]] = Int32 |

Scenario: Generate a Number between a negative index in a recordset and 5
	Given I have a type as "Numbers"
	And I have a range from "[[rec(-1).set]]" to "5" 
	When the random tool is executed 
	Then the execution has "AN" error
	And the debug inputs as  
	| Random  | From              | To |
	| Numbers | [[rec(-1).set]] = | 5  |


Scenario: Generate a Number between 5 and a negative index in a recordset
	Given I have a type as "Numbers"
	And I have a range from "5" to "[[rec(-1).set]]" 
	When the random tool is executed 
	Then the execution has "AN" error
	And the debug inputs as  
	| Random  | From | To                 |
	| Numbers | 5    | [[rec(-1).set]]  = |

Scenario: Generate Letters with a negative recordset index for length
	Given I have a type as "Letters"
	And I have a length as "[[rec(-1).set]]"
	When the random tool is executed 
	Then the execution has "AN" error
	And the debug inputs as  
	| Random  | Length             |
	| Letters | [[rec(-1).set]]  = | 


Scenario: Generate Letters and Numbers with a negative recordset index for length
	Given I have a type as "LetterAndNumbers"
	And I have a length as "[[rec(-1).set]]"
	When the random tool is executed 
	Then the execution has "AN" error
	And the debug inputs as  
	| Random            | Length            |
	| Letters & Numbers | [[rec(-1).set]] = |


	
Scenario: Generate decimal Numbers one digit
	Given I have a type as "Numbers"
	And I have a range from "0.1" to "0.9" 
	When the random tool is executed 
	Then the result from the random tool should be of the same type as "System.Double" 
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | 0.1    | 0.9  |
	And the debug output as 
	|                     |
	| [[result]] = Double |	

	
Scenario: Generate decimal Numbers many digits
	Given I have a type as "Numbers"
	And I have a range from "0.000000001" to "0.9" 
	When the random tool is executed 
	Then the result from the random tool should be of the same type as "System.Double" 
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | 0.000000001    | 0.9  |
	And the debug output as 
	|                     |
	| [[result]] = Double |	


Scenario: Generate a Number between 5.5 and 5.5
	Given I have a type as "Numbers"
	And I have a range from "5.5" to "5.5" 
	When the random tool is executed 
	Then the result from the random tool should be of the same type as "System.Double" 
	And the random value will be between "5.5" and "5.5" inclusive 
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To |
	| Numbers | 5.5    | 5.5  |
	And the debug output as 
	|                     |
	| [[result]] = Double |

	
Scenario: Generate a Number between double min and double max
	Given I have a type as "Numbers"
	And I have a range from "-0.000000000000005" to "170000000000000" 
	When the random tool is executed 
	Then the result from the random tool should be of the same type as "System.Double" 
	And the random value will be between "-0.000000000000005" and "170000000000000" inclusive 
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From               | To              |
	| Numbers | -0.000000000000005 | 170000000000000 |
	And the debug output as 
	|                     |
	| [[result]] = Double |


##This test seems to crash or timeout...	
#Scenario: Generate a Number between double min and double max scientific
#	Given I have a type as "Numbers"
#	And I have a range from "5.0E-324" to "1.7E+308" 
#	When the random tool is executed 
#	Then the result from the random tool should be of the same type as "System.Double" 
#	And the random value will be between "5.0E-324" and "1.7E+308" inclusive 
#	And the execution has "NO" error
#	And the debug inputs as  
#	| Random  | From | To |
#	| Numbers | 5.E-324    | 1.7E+308  |
#	And the debug output as 
#	|                     |
#	| [[result]] = Double |

		
Scenario: Generate a Number between double min and double max with no decimals
	Given I have a type as "Numbers"
	And I have a range from "0" to "170000000000000" 
	When the random tool is executed 
	Then the result from the random tool should be of the same type as "System.Double" 
	And the random value will be between "0" and "170000000000000" inclusive 
	And the execution has "NO" error
	And the debug inputs as  
	| Random  | From | To              |
	| Numbers | 0    | 170000000000000 |
	And the debug output as 
	|                     |
	| [[result]] = Double |


@ignore
#Audit
Scenario Outline: Generate numbers using variables and recordsets
	Given I have a type as '<Type>'
	And I have a range from '<From>' to '<To>' 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Int32" with a length of '<length>'
	And the execution has "NO" error
	And the result variable '<res>' will be '<result>'
Examples: 
	| Type    | From                                | To                                  | length               | res                           | result            |
	| Numbers | [[a]] = 1                           | [[b]] = 9                           |                      | [[rec().a]]                   | [[rec(1).a]] = 4  |
	| Numbers | [[rec(1).a]] = 10                   | [[rec([[int]]).b]] = 70, [[int]] =1 |                      | [[rec([[int]]).a]],[[int]]= 3 | [[rec(3).a]] = 35 |
	| Numbers | [[rec([[int]]).a]] = 10, [[int]] =1 | [[rec(1).b]] = 70                   |                      | [[rs(*).b]]                   | [[rs(1).b]] = 55  |
	| Numbers | [[rec().a]] = 10                    | [[rec().b]] = 100                   |                      | [[b]]                         | [[b]] = 55        |
	| Numbers | [[rec(*).a]] = 1                    | [[rec(*).b]] = 700                  |                      | [[b]]                         | [[b]] = 423       |
	| Letters |                                     |                                     | [[f]] = 1            | [[rec().a]]                   | [[rec(1).a]] = D  |
	| Letters |                                     |                                     | [[rec([[f]]).b]] = 2 | [[rec([[int]]).a]],[[int]]= 3 | [[rec(3).a]] = zs |
	| Letters |                                     |                                     | [[rj(*).set]] = 2    | [[rs(*).b]]                   | [[rs(1).b]] = ht  |
	| Letters |                                     |                                     | [[rj(1).set]] = 2    | [[b]]                         | [[b]] = hy        |


Scenario Outline: Generate error using variables and recordsets
	Given I have a type as '<Type>'
	And I have a range from '<From>' to '<To>' 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Int32" with a length of '<length>'
	And the execution has "An" error
	And the execution has '<Error>' error
Examples: 
	| Type    | From      | To        | length | res                           | Error                                                         |
	| Numbers | dfsdf     | [[b]] = 9 |        | [[rec().a]]                   | Please ensure that the Start is an Integer or decimal from -1 |
	| Numbers | [[b]] = 9 | dfsdf     |        | [[rec([[int]]).a]],[[int]]= 3 | Please ensure that the End is an Integer or decimal from -1   |
	| Numbers | [[b]]     | 3         |        | [[rec([[int]]).a]],[[int]]= 3 | The expression [[b]] has no value assigned                    |
	| Numbers | 1         | [[u]]     |        | [[rec([[int]]).a]],[[int]]= 3 | The expression [[u]] has no value assigned                    |
	| Letters |           |           | sdf    | [[rec().a]]                   | Please ensure that the length is an integer value             |
	| Letters |           |           | [[u]]  | [[rec().a]]                   | The expression [[u]] has no value assigned                    |
	| Letters |           |           | [[q]]  | [[rec().a]]                   | The expression [[u]] has no value assigned                    |

@ignore
#Complex Types WOLF-1042
Scenario Outline: Generate numbers using complex types
	Given I have a type as '<Type>'
	And I have a range from '<From>' to '<To>' 
	When the random tool is executed 
	Then the result from the random tool should be of type "System.Int32" with a length of '<length>'
	And the execution has "<Error>" error
	And the execution has '<Message>' error
Examples: 
	| Type    | From                        | To                             | length | res                      | Error | Message                        |
	| Numbers | [[rec(1).count(3).val]] = 9 | [[rec(1).count(1).val]] = 1000 |        | [[rec().result().value]] | An    | [[rec().result().value]] = 557 |
	| Numbers | [[rec(1).count(*).val]] = 0 | [[rec(1).count(3).val]] = 9    |        | [[rec().result().value]] | No    | [[rec().result().value]] = 5   |

Scenario: Generate a Number using a null variable
	Given  I have a formatnumber variable "[[int]]" equal to NULL
	And I have a type as "Numbers"
	And I have a range from "[[int]]" to "170000000000000" 
	When the random tool is executed 
	Then the execution has "An" error


Scenario: Generate a Number using a null variable to
	Given  I have a formatnumber variable "[[int]]" equal to NULL
	And I have a type as "Numbers"
	And I have a range from "170000000000000" to "[[int]]" 
	When the random tool is executed 
	Then the execution has "An" error


Scenario: Generate a Number using a non existent variable
	Given  I have a type as "Numbers"
	And I have a range from "[[int]]" to "170000000000000" 
	When the random tool is executed 
	Then the execution has "An" error



