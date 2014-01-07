Feature: FindRecordsetIndexMultiple
	In order to search for pieces of data in a recordset
	As a Warewolf user
	I want a tool I can use to find an index 

Scenario: Find an index of data in a recordset with Is Between numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value |
	| rs().field | 1     |
	| rs().field | 15    |
	| rs().field | 20    |
	| rs().field | 34    |	
	And  is between search the recordset with type "Is Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 3
	And the execution has "NO" error

Scenario: Find an index of data in an empty recordset
	Given I have the following recordset to search for multiple criteria
	| rs       | value |
	And  is between search the recordset with type "Is Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be ""
	And the execution has "AN" error

Scenario: Find an index of data in a recordset with a blank from
	Given I have the following recordset to search for multiple criteria
	| rs       | value |
	| rs().field | 1     |
	| rs().field | 15    |
	| rs().field | 20    |
	| rs().field | 34    |	
	And  is between search the recordset with type "Is Between" and criteria is "" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be "-1"
	And the execution has "AN" error

	Scenario: Find an index of data in a recordset with blank to
	Given I have the following recordset to search for multiple criteria
	| rs       | value |
	| rs().field | 1     |
	| rs().field | 15    |
	| rs().field | 20    |
	| rs().field | 34    |	
	And  is between search the recordset with type "Is Between" and criteria is "16" and ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be ""
	And the execution has "AN" error


Scenario: Find an index of data in a recordset with Is Between DateTime
	Given I have the following recordset to search for multiple criteria
	| rs       | value      |
	| rs().field | 5/3/2013   |
	| rs().field | 2/3/2013   |
	| rs().field | 7/4/2013   |
	| rs().field | 11/11/2012 |	
	And  is between search the recordset with type "Is Between" and criteria is "1/3/2013" and "3/3/2013"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 2
	And the execution has "NO" error

#Scenario: Find an index of data in a recordset with Is Between Numbers and recordsets with stars
#	Given I have the following recordset in my datalist
#	| rs | value |
#	| BetweenFrom().from | 15|	
#	| BetweenFrom().from | 0|
#	Given I have the following recordset in my datalist
#	| rs | value |
#	| BetweenTo().to | 25|	
#	| BetweenTo().to | 26|
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs().field | 15|
#	| rs().field | 20|
#	| rs().field | 25|				
#	And  is between search the recordset with type "Is Between" and criteria is "[[BetweenFrom(*).from]]" and "[[BetweenTo(*).to]]"
#	And when all row true is "true"
#	When the find records index multiple tool is executed
#	Then the find records index multiple result should be 2
#	And the execution has "NO" error
	
Scenario: Find an index of data in a recordset with Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |	
	| rs().field | d2FyZXdvbGY= |	
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Is Binary
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |	
	| rs().field | 101011110010 |	
	And search the recordset with type "Is Binary" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Is Hex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |	
	| rs().field | 77617265776f6c66 |	
	And search the recordset with type "Is Hex" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Not Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | d2FyZXdvbGY=      |
	| rs().field | d2FyZXdvbGY=      |
	| rs().field | d2FyZXdvbGY=      |	
	| rs().field | You |	
	And search the recordset with type "Not Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Not Between DateTime
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 2/3/2013|
	| rs().field | 7/3/2013|
	| rs().field | 2/3/2013|	
	| rs().field | 2/3/2013|	
	And  is between search the recordset with type "Not Between" and criteria is "1/3/2013" and "3/3/2013"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 2
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Not Between numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 17|
	| rs().field | 22|
	| rs().field | 400|	
	| rs().field | 31|	
	And  is between search the recordset with type "Not Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 3
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Not Binary
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 101011110010 |
	| rs().field | 101011110010 |
	| rs().field | 101011110010 |	
	| rs().field | warewolf |	
	And search the recordset with type "Not Binary" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Not Hex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 77617265776f6c66      |
	| rs().field | 77617265776f6c66      |
	| rs().field | 77617265776f6c66      |	
	| rs().field | warewolf |	
	And search the recordset with type "Not Hex" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Not Regex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 999.999.999.999      |
	| rs().field | 999.999.999.999      |
	| rs().field | 999.999.999.999      |	
	| rs().field | warewolf      |	
	And search the recordset with type "Not Regex" and criteria is "\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Doesn't Start With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | what      |
	| rs().field | where      |
	| rs().field | why      |	
	| rs().field | yay      |	
	And search the recordset with type "Doesn't Start With" and criteria is "w"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset with Doesn't End With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | arev     |
	| rs().field | wherev      |
	| rs().field | modev      |	
	| rs().field | yay      |	
	And search the recordset with type "Doesn't End With" and criteria is "v"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Greater Than
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 4      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | 2     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type ">" and criteria is "3"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Greater Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 4 |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 8 |
	| rs().field | user     |
	And search the recordset with type ">" and criteria is "3"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Greater Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 4      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 8 |
	| rs().field | user     |
	And search the recordset with type ">" and criteria is "50"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	
Scenario: Find an index of data in a recordset search type is Less Than
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 4      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 8 |
	| rs().field | user     |
	And search the recordset with type "<" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Less Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 4 |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 2 |
	| rs().field | user     |
	And search the recordset with type "<" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Less Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 2      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 5 |
	| rs().field | user     |
	And search the recordset with type "<" and criteria is "1"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	
Scenario: Find an index of data in a recordset search type is Not Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | Warewolf      |
	| rs().field | Warewolf      |
	| rs().field | Warewolf     |
	| rs().field | Warewolf |
	| rs().field | Warewolf     |
	And search the recordset with type "<> (Not Equal)" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "<> (Not Equal)" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,2,3,4,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Mars      |
	| rs().field | Mars      |
	| rs().field | Mars      |
	| rs().field | Mars      |
	| rs().field | Mars	   |
	| rs().field | Mars      |
	And search the recordset with type "<> (Not Equal)" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Greater Or Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 2      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 4 |
	| rs().field | user     |
	And search the recordset with type ">=" and criteria is "4"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Greater Or Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 50 |	
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | 4     |
	And search the recordset with type ">=" and criteria is "4"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Greater Or Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 1      |
	| rs().field | 2      |
	| rs().field | 3      |
	| rs().field | 1     |
	| rs().field | 2|
	| rs().field | 3     |
	And search the recordset with type ">=" and criteria is "4"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Less Or Equal
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 5 |
	| rs().field | user     |
	And search the recordset with type "<=" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Less Or Equal multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 1      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | 5     |
	And search the recordset with type "<=" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Less Or Equal result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 2      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 5 |
	| rs().field | user     |
	And search the recordset with type "<=" and criteria is "1"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Starts With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Starts With multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Starts With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Starts With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Ends With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Ends With multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Ends With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Ends With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Contains
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Contains multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Contains result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Contains" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Doesn't Contain
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf      |
	| rs().field | Warewolf      |
	| rs().field | Warewolf      |
	| rs().field | Warewolf     |
	| rs().field | user |
	| rs().field | Warewolf     |
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Doesn't Contain multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | Warewolf      |
	| rs().field | Warewolf      |
	| rs().field | Warewolf     |
	| rs().field | user |
	| rs().field | Warewolf     |
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Doesn't Contain result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Mars      |
	| rs().field | Mars      |
	| rs().field | Mars      |
	| rs().field | Mars     |
	| rs().field | Mars |
	| rs().field | Mars     |
	And search the recordset with type "Doesn't Contain" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | $$      |
	| rs().field | $$      |
	| rs().field | $$      |
	| rs().field | $$     |
	| rs().field | Warewolf |
	| rs().field | $$     |
	And search the recordset with type "Is Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | $$      |
	| rs().field | $$      |
	| rs().field | $$      |
	| rs().field | $$     |
	| rs().field | Warewolf |
	| rs().field | $$     |
	And search the recordset with type "Is Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | $$      |
	| rs().field | $$      |
	| rs().field | $$      |
	| rs().field | $$     |
	| rs().field | $$ |
	| rs().field | $$     |
	And search the recordset with type "Is Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | d2FyZXdvbGY=     |
	| rs().field | You |	
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Base64 multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | d2FyZXdvbGY= |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | You     |
	| rs().field | d2FyZXdvbGY= |
	| rs().field | You     |	
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Base64 result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | You     |
	| rs().field | You |
	| rs().field | You     |
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

	Scenario: Find an index of data in a recordset search type is Is Date
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 12/10/2013 |
	| rs().field | user     |
	And search the recordset with type "Is Date" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 12/10/2013 |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 12/10/2013 |
	| rs().field | user     |
	And search the recordset with type "Is Date" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Is Date" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Email
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | test@testEmail.co.za |
	| rs().field | user     |
	And search the recordset with type "Is Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | test@testEmail.co.za  |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | test@testEmail.co.za  |
	| rs().field | user     |
	And search the recordset with type "Is Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Is Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 45 |
	| rs().field | user     |
	And search the recordset with type "Is Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 41 |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 54 |
	| rs().field | user     |
	And search the recordset with type "Is Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Is Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Regex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Regex multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Regex result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Is Regex" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Text
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 15      |
	| rs().field | 56      |
	| rs().field | 45      |
	| rs().field | 7     |
	| rs().field | Warewolf |
	| rs().field | 16     |
	And search the recordset with type "Is Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | 45      |
	| rs().field | 54      |
	| rs().field | 51      |
	| rs().field | 86     |
	| rs().field | Warewolf |
	| rs().field | 8     |
	And search the recordset with type "Is Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 1      |
	| rs().field | 2      |
	| rs().field | 3      |
	| rs().field | 4     |
	| rs().field | 6 |
	| rs().field | 5     |
	And search the recordset with type "Is Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is XML
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | <test></test> |
	| rs().field | user     |
	And search the recordset with type "Is XML" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | <test></test> |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | <test></test> |
	| rs().field | user     |
	And search the recordset with type "Is XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Is XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Is XML" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error


Scenario: Find an index of data in a recordset search type is Not Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | $$ |
	| rs().field | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | $$ |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | $$ |
	| rs().field | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Date
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013     |
	| rs().field | Warewolf |
	| rs().field | 12/11/2013     |
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013     |
	| rs().field | Warewolf |
	| rs().field | 12/11/2013     |
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013      |
	| rs().field | 12/11/2013     |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013     |
	And search the recordset with type "Not Date" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Email
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za     |
	| rs().field | Warewolf |
	| rs().field | test@testEmail.co.za     |
	And search the recordset with type "Not Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za     |
	| rs().field | Warewolf |
	| rs().field | test@testEmail.co.za     |
	And search the recordset with type "Not Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za      |
	| rs().field | test@testEmail.co.za     |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za     |
	And search the recordset with type "Not Email" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 152      |
	| rs().field | 5      |
	| rs().field | 6      |
	| rs().field | 7     |
	| rs().field | Warewolf |
	| rs().field | 5     |
	And search the recordset with type "Not Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | 45      |
	| rs().field | 2      |
	| rs().field | 4      |
	| rs().field | 5     |
	| rs().field | Warewolf |
	| rs().field | 5     |
	And search the recordset with type "Not Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 1      |
	| rs().field | 1      |
	| rs().field | 2      |
	| rs().field | 2     |
	| rs().field | 3 |
	| rs().field | 3     |
	And search the recordset with type "Not Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Text
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 52 |
	| rs().field | user     |
	And search the recordset with type "Not Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 45 |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | 741 |
	| rs().field | user     |
	And search the recordset with type "Not Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And search the recordset with type "Not Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not XML
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | <test></test>      |
	| rs().field | <test></test>      |
	| rs().field | <test></test>      |
	| rs().field | <test></test>     |
	| rs().field | Warewolf |
	| rs().field | <test></test>     |
	And search the recordset with type "Not XML" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | Warewolf |
	| rs().field | <test></test>      |
	| rs().field | <test></test>      |
	| rs().field | <test></test>      |
	| rs().field | <test></test>     |
	| rs().field | Warewolf |
	| rs().field | <test></test>     |
	And search the recordset with type "Not XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type is Not XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	And search the recordset with type "Not XML" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error

#Scenario: Find an index of data in a recordset search type Contains and requires all fields to match true and match all rows true
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs(1).field1 |123 |
#	| rs(2).field1 |2 |
#	| rs(3).field1 |5 |
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |	
#	| rs1(1).field2 |214 |
#	| rs1(2).field2 |51 |
#	| rs1(3).field2 |56 |
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs2(1).field3 |512 |
#	| rs2(2).field3 |84 |
#	| rs2(3).field3 |12 |
#	And the fields to search is
#	| field    |
#	| [[rs().field1]] |
#	| [[rs1().field2]] |
#	| [[rs2().field3]] |
#	And search the recordset with type "Contains" and criteria is "1"	
#	And search the recordset with type "Contains" and criteria is "2"	
#	And when match all search criteria is "true"
#	And when requires all fields to match is "true"
#	When the find records index multiple tool is executed
#	Then the find records index multiple result should be 1
#	And the execution has "NO" error

#Scenario: Find an index of data in a recordset search type Contains and requires all fields to match false and match all rows true
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs(1).field1 |123 |
#	| rs(2).field1 |2 |
#	| rs(3).field1 |5 |
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |	
#	| rs1(1).field2 |214 |
#	| rs1(2).field2 |51 |
#	| rs1(3).field2 |56 |
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs2(1).field3 |512 |
#	| rs2(2).field3 |84 |
#	| rs2(3).field3 |12 |
#	And the fields to search is
#	| field    |
#	| [[rs().field1]] |
#	| [[rs1().field2]] |
#	| [[rs2().field3]] |
#	And search the recordset with type "Contains" and criteria is "1"	
#	And search the recordset with type "Contains" and criteria is "2"	
#	#And when All Contains true is "true"
#	#And when requires All Fields to match is "false"
#	And when match all search criteria is "true"
#	And when requires all fields to match is "false"
#	When the find records index multiple tool is executed
#	Then the find records index multiple result should be 1,3
#	And the execution has "NO" error

#Scenario: Find an index of data in a recordset search type Contains and requires all fields to match false and match all rows false
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| AB(1).f1 |123 |
#	| AB(2).f1 |2 |
#	| AB(3).f1 |5 |
#	And I have the following recordset to search for multiple criteria
#	| rs       | value    |	
#	| CD(1).f2 |214 |
#	| CD(2).f2 |51 |
#	| CD(3).f2 |56 |
#	And I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| EF(1).f3 |512 |
#	| EF(2).f3 |84 |
#	| EF(3).f3 |12 |
#	And the fields to search is
#	| field    |
#	| [[AB().f1]] |
#	| [[CD().f2]] |
#	| [[EF().f3]] |
#	And search the recordset with type "Contains" and criteria is "1"	
#	And search the recordset with type "Contains" and criteria is "2"	
#	And when match all search criteria is "false"
#	And when requires all fields to match is "false"
#	When the find records index multiple tool is executed
#	Then the find records index multiple result should be 1,2,3
#	And the execution has "NO" error
#
#Scenario: Find an index of data in a recordset search type Contains and requires all fields to match true and match all rows false
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs(1).field1 |123 |
#	| rs(2).field1 |2 |
#	| rs(3).field1 |5 |
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |	
#	| rs1(1).field2 |214 |
#	| rs1(2).field2 |52 |
#	| rs1(3).field2 |56 |
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs2(1).field3 |512 |
#	| rs2(2).field3 |82 |
#	| rs2(3).field3 |12 |
#	And the fields to search is
#	| field    |
#	| [[rs().field1]] |
#	| [[rs1().field2]] |
#	| [[rs2().field3]] |
#	And search the recordset with type "Contains" and criteria is "1"	
#	And search the recordset with type "Contains" and criteria is "2"	
#	And when match all search criteria is "false"
#	And when requires all fields to match is "true"
#	When the find records index multiple tool is executed
#	Then the find records index multiple result should be 1,2
#	And the execution has "NO" error

Scenario: Search using a negative index recordset criteria
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | <test></test>      |
	And search the recordset with type "Not XML" and criteria is "[[my(-1).set]]"
	When the find records index multiple tool is executed
	Then the execution has "AN" error