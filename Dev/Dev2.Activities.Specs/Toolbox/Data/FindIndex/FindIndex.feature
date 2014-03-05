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
	|                  |
	| [[result]] = -1 |

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
	| [[result]] = -1 |

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
	Then the execution has "NO" error
	And the debug inputs as
	| In Field     | Index           | Characters | Direction     |
	| abc3cde3fgh3 | All Occurrences | 3          | Left to Right |
	And the debug output as
	|                   |
	| [[res]] = 4,8,12 |
	| [[rs(1).a]] = 4  |
	| [[rs(2).a]] = 8  |
	| [[rs(3).a]] = 12 |