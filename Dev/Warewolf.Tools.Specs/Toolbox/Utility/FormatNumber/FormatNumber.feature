Feature: FormatNumber
	In order to round off numbers
	As a Warewolf user
	I want a tool that will aid me to do so 

@Utility
Scenario: Format number rounding normal with zero rounding and zero decimals to show 
	Given I have a number 788.894564545645
	And I selected rounding "Normal" to 0 
	And I want to show 0 decimals with value "0"
	When the format number is executed
	Then the result 789 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Normal   | 0              | 0                |
	And the debug output as 
	|                  |
	| [[result]] = 789 |
	
@Utility
Scenario: Format number rounding down 
	Given I have a number 788.894564545645
	And I selected rounding "Down" to 3 
	And I want to show 3 decimals decimals with value "3"
	When the format number is executed
	Then the result 788.894 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Down     | 3              | 3                |
	And the debug output as 
	|                      |
	| [[result]] = 788.894 |
	
@Utility
Scenario: Format number rounding up
	Given I have a number 788.894564545645
	And I selected rounding "Up" to 3 
	And I want to show 3 decimals decimals with value "3"
	When the format number is executed
	Then the result 788.895 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Up       | 3              | 3                |
	And the debug output as 
	|                      |
	| [[result]] = 788.895 |
	
@Utility
Scenario: Format number rounding normal
	Given I have a number 788.894564545645
	And I selected rounding "Normal" to 2 
	And I want to show 3 decimals decimals with value "3"
	When the format number is executed
	Then the result 788.890 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Normal   | 2              | 3                |
	And the debug output as 
	|                      |
	| [[result]] = 788.890 |
	
@Utility
Scenario: Format number rounding none
	Given I have a number 788.894564545645
	And I selected rounding "None" to 0 
	And I want to show 4 decimals decimals with value "4"
	When the format number is executed
	Then the result 788.8945 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | None     | 0              | 4                |
	And the debug output as 
	|                       |
	| [[result]] = 788.8945 |
	
@Utility
Scenario: Format number rounding down to negative number
	Given I have a number 788.894564545645
	And I selected rounding "Down" to -2
	And I want to show 0 decimals decimals with value "0"
	When the format number is executed
	Then the result 700 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Down     | -2             | 0                |
	And the debug output as 
	|                  |
	| [[result]] = 700 |
	
@Utility
Scenario: Format number large number to negative decimals
	Given I have a number 788.894564545645
	And I selected rounding "None" to 0
	And I want to show -2 decimals decimals with value "-2"
	When the format number is executed
	Then the result 7 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | None     | 0              | -2               |
	And the debug output as 
	|                |
	| [[result]] = 7 |
	
@Utility
Scenario: Format number single digit to negative decimals
	Given I have a number 7
	And I selected rounding "None" to 0
	And I want to show -2 decimals decimals with value "-2"
	When the format number is executed
	Then the result 0 will be returned
	And the debug inputs as  
	| Number | Rounding | Rounding Value | Decimals to show |
	| 7      | None     | 0              | -2               |
	And the debug output as 
	|                |
	| [[result]] = 0 |
	And the execution has "NO" error
	
@Utility
Scenario: Format number rounding up to a character
	Given I have a number 34.2
	And I selected rounding "Up" to "c"
	And I want to show 2 decimals decimals with value "2"
	When the format number is executed
	Then the result "" will be returned
	And the execution has "AN" error
	And the debug inputs as  
	| Number | Rounding | Rounding Value | Decimals to show |
	| 34.2   | Up       | c              | 2                |
	And the debug output as 
	|              |
	| [[result]] = |
	
@Utility
Scenario: Format number that is blank
	Given I have a number ""
	When the format number is executed
	Then the result "" will be returned
   And the execution has "AN" error
   
@Utility
Scenario: Format non numeric
	Given I have a number "asdf"
	And I selected rounding "None" to 0
	And I want to show -2 decimals decimals with value "-2"
	When the format number is executed
	Then the result "" will be returned
   And the execution has "AN" error
   
@Utility
Scenario: Format number to charater decimals
	Given I have a number 34.2
	And I selected rounding "Up" to "1"
	And I want to show "asdf" decimals decimals with value "asdf"
	When the format number is executed
	Then the result "" will be returned
   And the execution has "AN" error
   
@Utility
Scenario: Format number with multipart variables and numbers for number rounding and decimals to show
	Given I have a formatnumber variable "[[int]]" equal to 788
	And I have a number "[[int]].894564545645"
	And I have a formatnumber variable "[[rounding]]" equal to 2
	And I selected rounding "Up" to "-[[rounding]]"
	And I have a formatnumber variable "[[decimals]]" equal to "-"
	And I want to show "[[decimals]]1" decimals with value "[[decimals]]1" 
	When the format number is executed
	Then the result 80 will be returned
    And the execution has "NO" error
	And the debug inputs as  
	| Number                                  | Rounding | Rounding Value     | Decimals to show   |
	| [[int]].894564545645 = 788.894564545645 | Up       | -[[rounding]] = -2 | [[decimals]]1 = -1 |
	And the debug output as 
	|                 |
	| [[result]] = 80 |
	
@Utility
Scenario: Format number with negative recordset index for number
	Given I have a number "[[my(-1).int]]"
	When the format number is executed
	Then the execution has "AN" error
	
@Utility
Scenario: Format number with negative recordset index for rounding
	Given I have a formatnumber variable "[[int]]" equal to 788
	And I have a number "[[int]].894564545645"
	And I selected rounding "Up" to "[[my(-1).rounding]]"
	When the format number is executed
	Then the execution has "AN" error
	
@Utility
Scenario: Format number with negative recordset index for decimals to show
	Given I have a formatnumber variable "[[int]]" equal to 788
	And I have a number "[[int]].894564545645"
	And I want to show "[[my(-1).decimals]]" decimals with value "[[my(-1).decimals]]"
	When the format number is executed
	Then the execution has "AN" error
	
@Utility
Scenario: Format number with unknown scalar for rounding
	Given I have a formatnumber variable "[[int]]" equal to ""
	And I have a number ""
	And I selected rounding "up" to "[[var]]" equal to ""
	And I want to show "[[decimal]]" decimals with value "2" 
	When the format number is executed
	Then the execution has "AN" error
	
@Utility
Scenario: Format number rounding with unknown scalar decimals value to show 
	Given I have a number 788.894564545645
	And I selected rounding "Normal" to 0 
	And I want to show "[[var]]" decimals with values ""
	When the format number is executed
	Then the result 789 will be returned
	And the execution has "NO" error
	And the debug inputs as  
	| Number           | Rounding | Rounding Value | Decimals to show |
	| 788.894564545645 | Normal   | 0              | ""               |
	And the debug output as 
	|                  |
	| [[result]] = 789 |
	
@Utility
Scenario: Format a variable with a null value
	Given I have a formatnumber variable "[[int]]" equal to NULL
	And I have a number "[[int]]"
	And I want to show 2 decimals decimals with value "2" 
	When the format number is executed
	Then the execution has "AN" error
	
@Utility
Scenario: Format a variable with a non existent value
	Given I have a number "[[int]]"
	And I want to show 2 decimals decimals with value "2" 
	When the format number is executed
	Then the execution has "AN" error
	
@Utility
Scenario: Format number with record star notation
	Given I have a formatnumber variable "[[format().num]]" equal to 788
	Given I have a formatnumber variable "[[format().num]]" equal to "9.894564545645"
	And I have a number "[[format(*).num]]"
	And I have a formatnumber variable "[[res().val]]" equal to 2
	And I selected rounding "Up" to "3"
	And I want to show "4" decimals with value "4" 
	And I have a formatnumber result is "[[res(*).val]]"
	When the format number is executed
    Then the execution has "NO" error
	And the debug inputs as  
	| Number                             | Rounding | Rounding Value | Decimals to show |
	| [[format(1).num]] = 788            |          |                |                  |
	| [[format(2).num]] = 9.894564545645 | Up       | 3              | 4                |
	And the debug output as 
	|                           |
	| [[res(1).val]] = 788.0000 |
	| [[res(2).val]] = 9.8950   |
	
@Utility
Scenario: Number Format tool with complext object multi array
	Given There is a complexobject in the datalist with this shape
	| rs                    | value |
	| [[@Person().Score()]]	| 0.3   |
	| [[@Person().Score()]]	| 0.45  |
	| [[@Person().Score()]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person(*).Score(*)]]"
	And I use a Number Format tool configured as
	| Number    | Rounding | Rounding Value | Decimals to show | Result    |
	| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person(1).Score(1)]]" has a value of "0.300"
	And "[[@Person(2).Score(1)]]" has a value of "0.450"
	And "[[@Person(3).Score(1)]]" has a value of "0.120"
	
@Utility
Scenario: Number Format tool with complex object multi array and field
	Given There is a complexobject in the datalist with this shape
	| rs                                | value |
	| [[@Person.Member().Team().Score]]	| 0.3   |
	| [[@Person.Member().Team().Score]]	| 0.45  |
	| [[@Person.Member().Team().Score]]	| 0.12  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Member().Team().Score]]"
	And I use a Number Format tool configured as
	| Number    | Rounding | Rounding Value | Decimals to show | Result    |
	| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person.Member(1).Team(1).Score]]" has a value of "0.300"
	And "[[@Person.Member(2).Team(1).Score]]" has a value of "0.450"
	And "[[@Person.Member(3).Team(1).Score]]" has a value of "0.120"
	
@Utility
Scenario: Number Format tool with complex object multi array and field and multi values
	Given There is a complexobject in the datalist with this shape
	| rs                                  | value |
	| [[@Person.Member(1).Team(1).Score]] | 0.3   |
	| [[@Person.Member(1).Team(2).Score]] | 0.45  |
	| [[@Person.Member(2).Team(1).Score]] | 0.12  |
	| [[@Person.Member(2).Team(2).Score]] | 0.11  |
	| [[@Person.Member(2).Team(3).Score]] | 0.13  |
	| [[@Person.Member(3).Team(1).Score]] | 0.14  |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Member().Team().Score]]"
	And I use a Number Format tool configured as
	| Number    | Rounding | Rounding Value | Decimals to show | Result    |
	| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person.Member(1).Team(1).Score]]" has a value of "0.300"
	And "[[@Person.Member(1).Team(2).Score]]" has a value of "0.450"
	And "[[@Person.Member(2).Team(1).Score]]" has a value of "0.120"
	And "[[@Person.Member(2).Team(2).Score]]" has a value of "0.110"
	And "[[@Person.Member(2).Team(3).Score]]" has a value of "0.130"
	And "[[@Person.Member(3).Team(1).Score]]" has a value of "0.140"

Scenario: Number Format tool with complex non array
	Given There is a complexobject in the datalist with this shape
	| rs                            | value |
	| [[@Person.Member.Team.Score]]	| 0.3   |
	And Alias is "[[Score]]"
	And Datasource is "[[@Person.Member.Team.Score]]"
	And I use a Number Format tool configured as
	| Number    | Rounding | Rounding Value | Decimals to show | Result    |
	| [[Score]] | Up       | 2              | 3                | [[Score]] |
	When the selectAndApply tool is executed
	Then the execution has "NO" error
	And "[[@Person.Member.Team.Score]]" has a value of "0.300"
