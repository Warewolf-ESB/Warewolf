Feature: FindRecordsetIndexMultiple
	In order to search for pieces of data in a recordset
	As a Warewolf user
	I want a tool I can use to find an index 

Scenario: Find an index of data in a recordset with Is Between numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 1|
	| rs().row | 15|
	| rs().row | 20|	
	| rs().row | 34|	
	And  is between search the recordset with type "Is Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 3

Scenario: Find an index of data in a recordset with Is Between DateTime
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 5/3/2013|
	| rs().row | 2/3/2013|
	| rs().row | 7/4/2013|	
	| rs().row | 11/11/2012|	
	And  is between search the recordset with type "Is Between" and criteria is "1/3/2013" and "3/3/2013"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 2

Scenario: Find an index of data in a recordset with Is Between Numbers and recordsets with stars
	Given I have the following recordset in my datalist
	| rs | value |
	| BetweenFrom().from | 15|	
	| BetweenFrom().from | 0|
	Given I have the following recordset in my datalist
	| rs | value |
	| BetweenTo().to | 25|	
	| BetweenTo().to | 26|
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 15|
	| rs().row | 20|
	| rs().row | 25|				
	And  is between search the recordset with type "Is Between" and criteria is "[[BetweenFrom(*).from]]" and "[[BetweenTo(*).to]]"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 2
	
Scenario: Find an index of data in a recordset with Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |	
	| rs().row | d2FyZXdvbGY= |	
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Is Binary
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |	
	| rs().row | 101011110010 |	
	And search the recordset with type "Is Binary" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Is Hex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |	
	| rs().row | 77617265776f6c66 |	
	And search the recordset with type "Is Hex" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Not Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | d2FyZXdvbGY=      |
	| rs().row | d2FyZXdvbGY=      |
	| rs().row | d2FyZXdvbGY=      |	
	| rs().row | You |	
	And search the recordset with type "Not Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Not Between DateTime
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 2/3/2013|
	| rs().row | 7/3/2013|
	| rs().row | 2/3/2013|	
	| rs().row | 2/3/2013|	
	And  is between search the recordset with type "Not Between" and criteria is "1/3/2013" and "3/3/2013"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 2

Scenario: Find an index of data in a recordset with Not Between numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 17|
	| rs().row | 22|
	| rs().row | 400|	
	| rs().row | 31|	
	And  is between search the recordset with type "Not Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 3

Scenario: Find an index of data in a recordset with Not Binary
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 101011110010 |
	| rs().row | 101011110010 |
	| rs().row | 101011110010 |	
	| rs().row | warewolf |	
	And search the recordset with type "Not Binary" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Not Hex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 77617265776f6c66      |
	| rs().row | 77617265776f6c66      |
	| rs().row | 77617265776f6c66      |	
	| rs().row | warewolf |	
	And search the recordset with type "Not Hex" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Not Regex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | 999.999.999.999      |
	| rs().row | 999.999.999.999      |
	| rs().row | 999.999.999.999      |	
	| rs().row | warewolf      |	
	And search the recordset with type "Not Regex" and criteria is "\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Doesn't Start With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | what      |
	| rs().row | where      |
	| rs().row | why      |	
	| rs().row | yay      |	
	And search the recordset with type "Doesn't Start With" and criteria is "w"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset with Doesn't End With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | arev     |
	| rs().row | wherev      |
	| rs().row | modev      |	
	| rs().row | yay      |	
	And search the recordset with type "Doesn't End With" and criteria is "v"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

Scenario: Find an index of data in a recordset search type is Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Greater Than
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Greater Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Greater Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0
	
Scenario: Find an index of data in a recordset search type is Less Than
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Less Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Less Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0
	
Scenario: Find an index of data in a recordset search type is Not Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<>" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<>" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<>" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Greater Or Equal To
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Greater Or Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Greater Or Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type ">=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Less Or Equal
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Less Or Equal multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Less Or Equal result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "<=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Starts With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Starts With multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Starts With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Ends With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Ends With multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Ends With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Ends With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Contains
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Contains multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Contains result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Contains" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Doesn't Contain
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Doesn't Contain multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Doesn't Contain result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Doesn't Contain" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Alphanumeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Base64" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Base64 multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Base64" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Base64 result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Base64" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

	Scenario: Find an index of data in a recordset search type is Is Date
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Date" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Email
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Email" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Numeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Regex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Regex multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Regex result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Regex" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is Text
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is Text" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Is XML
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Is XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Is XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Is XML" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Alphanumeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Date
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Date" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Email
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Email" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Email" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Numeric
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Numeric" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Numeric" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not Text
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Text" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not Text" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0

Scenario: Find an index of data in a recordset search type is Not XML
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

Scenario: Find an index of data in a recordset search type is Not XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6

Scenario: Find an index of data in a recordset search type is Not XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Not XML" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 0