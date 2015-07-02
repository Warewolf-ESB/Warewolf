Feature: FindIndex
	In order to find where characters or values are in sentences or words
	As a Warewolf user
	I want a tool that finds indexes

Scenario: Find the first Occurrence of a character in a sentence
	Given I have a findindex variable "[[a]]" equal to "I have managed to spend time in real innovation since I started using Warewolf"
	And the sentence "[[a]]"
	And I selected Index "First Occurrence"
	And I search for characters "since"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "49"
	And the execution has "NO" error
	And the debug inputs as
	| In Field                                                                               | Index            | Characters | Direction     |
	| [[a]] = I have managed to spend time in real innovation since I started using Warewolf | First Occurrence | since      | Left to Right |
	And the debug output as
	|                  |
	| [[result]] = 49 |

Scenario: Find all Occurrences of a word in a sentence and output to scalar going left to right
	Given I have a findindex variable "[[a]]" equal to "I have managed to spend time in real innovation since I started using Warewolf"
	And the sentence "[[a]]"
	And I selected Index "All Occurrences"
	And I search for characters "a"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "4,9,11,35,43,59,72"
	And the execution has "NO" error
	And the debug inputs as
	| In Field                                                                               | Index           | Characters | Direction     |
	| [[a]] = I have managed to spend time in real innovation since I started using Warewolf | All Occurrences | a          | Left to Right |
	And the debug output as
	|                                  |
	| [[result]] = 4,9,11,35,43,59,72 |

Scenario: Find all Occurrences of a word in a sentence and output to recordset going right to left 
	Given I have a findindex variable "[[a]]" equal to "I have managed to spend time in real innovation since I started using Warewolf"
	And the sentence "[[a]]"
	And I selected Index "All Occurrences"
	And I search for characters "a"
	And I selected direction as "Right to Left"
	When the data find index tool is executed
	Then the find index result is 
	|         |
	| 7      |
	| 20     |
	| 36     |
	| 44     |
	| 68     |
	| 70     |
	| 75     |
	And the execution has "NO" error
	And the debug inputs as
	| In Field                                                                               | Index           | Characters | Direction     |
	| [[a]] = I have managed to spend time in real innovation since I started using Warewolf | All Occurrences | a          | Right to Left |
	And the debug output as
	|                                   |
	| [[result]] = 7,20,36,44,68,70,75 |

Scenario: Find last Occurrence of a bracket in a sentence
	Given I have a findindex variable "[[a]]" equal to "!@#$%)@#$%)"
	And the sentence "[[a]]"
	And I selected Index "Last Occurrence"
	And I search for characters ")"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "11"
	And the execution has "NO" error
	And the debug inputs as
	| In Field            | Index           | Characters | Direction     |
	| [[a]] = !@#$%)@#$%) | Last Occurrence | )          | Left to Right |
	And the debug output as
	|                  |
	| [[result]] = 11 |

Scenario: Find first Occurrence of a character in a blank string
	Given I have a findindex variable "[[a]]" equal to ""
	And the sentence "[[a]]"
	And I selected Index "First Occurrence"
	And I search for characters "a"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "-1"
	And the execution has "NO" error
	And the debug inputs as
	| In Field | Index            | Characters | Direction     |
	| [[a]] =  | First Occurrence | a          | Left to Right |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find first Occurrence of a character in a string where it doesnt exist
	Given I have a findindex variable "[[a]]" equal to "fff"
	And the sentence "[[a]]"
	And I selected Index "First Occurrence"
	And I search for characters "a"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "-1"
	And the execution has "NO" error
	And the debug inputs as
	| In Field    | Index            | Characters | Direction     |
	| [[a]] = fff | First Occurrence | a          | Left to Right |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find all Occurrences of a character in a string where it doesnt exist
	Given I have a findindex variable "[[a]]" equal to ""
	And the sentence "[[a]]"
	And I selected Index "All Occurrence"
	And I search for characters "a"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "-1"
	And the execution has "NO" error
	And the debug inputs as
	| In Field | Index          | Characters | Direction     |
	| [[a]] =  | All Occurrence | a          | Left to Right |
	And the debug output as
	|                 |
	| [[result]] = -1 |

Scenario: Find an xml fragment in a bigger xml document
	Given I have a findindex variable "[[a]]" equal to "<x><b id="1">One</b></x>"
	And the sentence "[[a]]"
	And I selected Index "First Occurrence"
	And I have a findindex variable "[[id]]" equal to "1"
	And I search for characters "<b id="[[id]]">"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "4"
	And the execution has "NO" error
	And the debug inputs as
	| In Field                         | Index            | Characters                   | Direction     |
	| [[a]] = <x><b id="1">One</b></x> | First Occurrence | <b id="[[id]]"> = <b id="1"> | Left to Right |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find a negative recordset index in a string
	Given I have a findindex variable "[[a]]" equal to "<x><b id="1">One</b></x>"
	And the sentence "[[a]]"
	And I selected Index "First Occurrence"
	And I search for characters "[[my(-1).data]]"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| In Field                         | Index            | Characters        | Direction     |
	| [[a]] = <x><b id="1">One</b></x> | First Occurrence | [[my(-1).data]] = | Left to Right |
	And the debug output as
	|                   |
	| [[result]] = |

Scenario: Find something with a negative recordset index as Input
	Given the sentence "[[a(-1).b]]"
	And I selected Index "First Occurrence"
	And I search for characters "12"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| In Field      | Index            | Characters | Direction     |
	| [[a(-1).b]] = | First Occurrence | 12         | Left to Right |
	And the debug output as
	|                  |
	|    [[result]] =              |

Scenario: Output values in recordset with star notation
    Given the sentence "abc3cde3fgh3"
	And I selected Index "All Occurrences"
	And I search for characters "3"
	And I selected direction as "Left to Right"
	And result variable as "[[rs(*).a]]"
	When the data find index tool is executed
	Then the execution has "NO" error
	And the debug inputs as
	| In Field     | Index           | Characters | Direction     |
	| abc3cde3fgh3 | All Occurrences | 3          | Left to Right |
	And the debug output as
	|                   |
	| [[rs(1).a]] = 4  |
	| [[rs(2).a]] = 8  |
	| [[rs(3).a]] = 12 | 

Scenario: Output values in recordset with numeric notation
    Given the sentence "abc3cde3fgh3"
	And I selected Index "All Occurrences"
	And I search for characters "3"
	And I selected direction as "Left to Right"
	And result variable as "[[rs(1).a]]"
	When the data find index tool is executed
	Then the execution has "NO" error
	And the debug inputs as
	| In Field     | Index           | Characters | Direction     |
	| abc3cde3fgh3 | All Occurrences | 3          | Left to Right |
	And the debug output as
	|                       |
	| [[rs(1).a]] = 4,8,12 |

Scenario: Output values in multiple result variables
    Given the sentence "abc3cde3fgh3"
	And I selected Index "All Occurrences"
	And I search for characters "3"
	And I selected direction as "Left to Right"
	And result variable as "[[res]],[[rs(*).a]]"
	When the data find index tool is executed
	Then the execution has "An" error
	And the debug inputs as
	| In Field     | Index           | Characters | Direction     |
	| abc3cde3fgh3 | All Occurrences | 3          | Left to Right |
	And the debug output as
	|                       |
	| [[res]],[[rs(*).a]] = |
#---find out about rule as no error message will display
Scenario: Find all Occurrences of a character in a variable where it doesnt exist
	Given I have a findindex variable "[[a]]" equal to "[[b]]"
	And I selected Index "All Occurrence"
	And I search for characters "[[c]]"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "-1"
	And the execution has "NO" error
	And the debug inputs as
	| In Field | Index          | Characters | Direction     |
	| [[a]] =  | All Occurrence | [[c]]      | Left to Right |

	And the debug output as
	|                 |
	| [[result]] = -1 |

Scenario: Find all Occurrences of a numeric character in a string
	Given I have a findindex variable "[[a]]" equal to "2211"
	And I selected Index "All Occurrence"
	And I search for characters "2"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "1,2"
	And the execution has "NO" error
	And the debug inputs as
	| In Field | Index          | Characters | Direction     |
	| [[a]] =  | All Occurrence | 2          | Left to Right |
	And the debug output as
	|                  |
	| [[result]] = 1,2 |

Scenario Outline: Find all occurances of Characters in a string
	Given I have a findindex variable "[[a]]" equal to " Warewolf"
	And I have selected Index "<Index>"
	And I search for characters "<Characters>"
	And I selected direction as "<Direction>" 
	When the data find index tool is executed
	Then the find index result is "<Result>"
	And the execution has "NO" error
	And the debug inputs as
	| In Field | Index    | Characters  | Direction   |
	| [[a]] =  | <Index > | <Character> | <Direction> |
	And the debug inputs as
	|          |
	| <result> |

Examples: 
| No | Index            | Characters                        | Directon      | Result                          |
| 1  | All Occurrences  | ""                                | Left to Right | No Character to search          |
| 2  | All Occurrences  | " "                               | Left to Right | [[result]] = 1                  |
| 3  | All Occurrences  | w                                 | Left to Right | [[rs(1).a]] = 1,[[rs(2).a]] = 5 |
| 4  | All Occurrences  | [[b]] = ""                        | Left to Right | [[result]] = -1                 |
| 5  | All Occurrences  | [[rs(1).a]] = a                   | Left to Right | [[result]] = 2                  |
| 6  | All Occurrences  | [[rs().a]] = w                    | Left to Right | [[rs(1).a]] = 1,[[rs(2).a]] = 5 |
| 7  | All Occurrences  | [[rs().a]] = w                    | Right to Left | [[rs(1).a]] = 4,[[rs(2).a]] = 8 |
| 8  | All Occurrences  | [[rs(*).a]] = w                   | Right to Left | [[rs(1).a]] = 4,[[rs(2).a]] = 8 |
| 9  | All Occurrences  | [[int]] = 1,[[rs([[int]]).a]] = a | Right to Left | [[rs(1).a]] = 7                 |
| 10 | First Occurrence | ""                                | Left to Right | No Character to search          |
| 11 | First Occurrence | " "                               | Left to Right | [[result]] = 1                  |
| 12 | First Occurrence | w                                 | Left to Right | [[result]] = 2                  |
| 13 | First Occurrence | [[b]] = ""                        | Left to Right | [[result]] = -1                 |
| 14 | First Occurrence | [[rs(1).a]] = a                   | Left to Right | [[result]] = 2                  |
| 15 | First Occurrence | [[rs().a]] = w                    | Left to Right | [[rs(1).a]] = 1                 |
| 16 | First Occurrence | [[rs().a]] = w                    | Right to Left | [[rs(1).a]] = 4                 |
| 17 | First Occurrence | [[rs(*).a]] = w                   | Right to Left | [[rs(1).a]] = 4                 |
| 18 | First Occurrence | [[int]] = 1,[[rs([[int]]).a]] = w | Right to Left | [[rs(1).a]] = 4                 |
| 19 | Last Occurrence  | ""                                | Left to Right | No Character to search          |
| 20 | Last Occurrence  | " "                               | Left to Right | [[result]] = 1                  |
| 21 | Last Occurrence  | w                                 | Left to Right | [[result]] = 2                  |
| 22 | Last Occurrence  | [[b]] = ""                        | Left to Right | [[result]] = -1                 |
| 23 | Last Occurrence  | [[rs(1).a]] = a                   | Left to Right | [[result]] = 2                  |
| 24 | Last Occurrence  | [[rs().a]] = w                    | Left to Right | [[rs(1).a]] = 1                 |
| 25 | Last Occurrence  | [[rs().a]] = w                    | Right to Left | [[rs(1).a]] = 4                 |
| 26 | Last Occurrence  | [[rs(*).a]] = w                   | Right to Left | [[rs(1).a]] = 4                 |
| 27 | Last Occurrence  | [[int]] = 1,[[rs([[int]]).a]] = w | Right to Left | [[rs(1).a]] = 4                 |

Scenario Outline: Find all Recordsets with invalid Indexes
	Given I have a findindex variable "<var>" equal to "<value>"
	And I selected Index "First Occurrence"
	And I search for characters "<Character>"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| In Field | Index            | Characters | Direction     |
	| [[a]] =  | First Occurrence | 12         | Left to Right |
	And the debug output as
	|          |
	| <result> |

Examples: 
| No | var     | value | Characters        | result          | Error                   |
| 1  | [[var]] | wolf  | [[rs([[var]]).a]] | [[result]] = -1 | Index is not an integer |
| 2  | [[var]] | w     | [[rs([[var]]).a]] | [[result]] = -1 | Index is not an integer |
| 3  | [[var]] | " "   | [[rs([[var]]).a]] | [[result]] =    | Index is not an integer |
| 4  | [[var]] | 1.2   | [[rs([[var]]).a]] | [[result]] =    | Index is not an integer |
| 5  | [[b]]   | ""    | [[rs([[b]]).a]]   | [[result]] = -1 | Index is not an integer |
| 6  | [[var]] | 123   | [[rs([[var]]).a]] | [[result]] = -1 | Index is not valid      |