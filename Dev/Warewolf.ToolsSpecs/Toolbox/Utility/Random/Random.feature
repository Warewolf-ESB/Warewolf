@Utils
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


Scenario: Generate random using star notation
	Given I have a type as "Letters"
	And I have a a random variable "[[rand().num]]" equal to "5"
	And I have a a random variable "[[rand().num]]" equal to "10"
	And I have a a random variable "[[res().val]]" equal to "bob"
	And I have a length as "[[rand(*).num]]"
	And I have a random result variable as "[[res(*).val]]"
	When the random tool is executed 
	Then the execution has "NO" error
	And the debug inputs as  
	| Random  | Length               |
	| Letters | [[rand(1).num]] = 5  |
	|         | [[rand(2).num]] = 10 |
	And the debug output as 
	|                     |
	| [[res(1).val]] = String |
	| [[res(2).val]] = String |

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



