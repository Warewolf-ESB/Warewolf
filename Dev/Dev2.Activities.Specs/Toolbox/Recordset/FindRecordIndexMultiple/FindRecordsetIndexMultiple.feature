Feature: FindRecordsetIndexMultiple
	In order to search for pieces of data in a recordset
	As a Warewolf user
	I want a tool I can use to find an index 

Scenario: Find an index of data in a recordset with Is Between numeric
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 1     |
	| rs().field | 15    |
	| rs().field | 20    |
	| rs().field | 34    |
	And field to search is "[[rs().field]]"	
	And  is between search the recordset with type "Is Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 3
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                      | # |            |  |    | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 1  |   |            |  |    |     |                             |                                |
	|           | [[rs(2).field]] = 15 |   |            |  |    |     |                             |                                |
	|           | [[rs(3).field]] = 20 |   |            |  |    |     |                             |                                |
	|           | [[rs(4).field]] = 34 | 1 | Is Between |  | 16 | 33  | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 3 |

Scenario: Find an index of data in an empty recordset
	Given I have the following recordset to search for multiple criteria
	| rs | value |
	|    |       |
	And field to search is "[[rs().value]]"
	And  is between search the recordset with type "Is Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be ""
	And the execution has "AN" error
	And the debug inputs as
	| #           |                   | # |            |  |    | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).value]] = | 1 | Is Between |  | 16 | 33  | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] =  |	

Scenario: Find an index of data in a recordset with a blank from
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 1     |
	| rs().field | 15    |
	| rs().field | 20    |
	| rs().field | 34    |
	And field to search is "[[rs().field]]"	
	And  is between search the recordset with type "Is Between" and criteria is "" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be ""
	And the execution has "AN" error
	And the debug inputs as
	|  #         |                      | # |            |  |     | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 1  |   |            |  |     |     |                             |                                |
	|           | [[rs(2).field]] = 15 |   |            |  |     |     |                             |                                |
	|           | [[rs(3).field]] = 20 |   |            |  |     |     |                             |                                |
	|           | [[rs(4).field]] = 34 | 1 | Is Between |  | " " | 33  | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = -1 |

	Scenario: Find an index of data in a recordset with blank to
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 1     |
	| rs().field | 15    |
	| rs().field | 20    |
	| rs().field | 34    |	
	And field to search is "[[rs().field]]"
	And  is between search the recordset with type "Is Between" and criteria is "16" and ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be ""
	And the execution has "AN" error
	And the debug inputs as
	|  #         |                      | # |            |  |    | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 1  |   |            |  |    |     |                             |                                |
	|           | [[rs(2).field]] = 15 |   |            |  |    |     |                             |                                |
	|           | [[rs(3).field]] = 20 |   |            |  |    |     |                             |                                |
	|           | [[rs(4).field]] = 34 | 1 | Is Between |  | 16 | " " | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |
	
Scenario: Find an index of data in a recordset with Is Between DateTime
	Given I have the following recordset to search for multiple criteria
	| rs       | value      |
	| rs().field | 5/3/2013   |
	| rs().field | 2/3/2013   |
	| rs().field | 7/4/2013   |
	| rs().field | 11/11/2012 |	
	And field to search is "[[rs().field]]"
	And  is between search the recordset with type "Is Between" and criteria is "1/3/2013" and "3/3/2013"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 2
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                              | # |            |  |          | And      | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 5/3/2013   |   |            |  |          |          |                             |                                |
	|           | [[rs(2).field]] = 2/3/2013   |   |            |  |          |          |                             |                                |
	|           | [[rs(3).field]] = 7/4/2013   |   |            |  |          |          |                             |                                |
	|           | [[rs(4).field]] = 11/11/2012 | 1 | Is Between |  | 1/3/2013 | 3/3/2013 | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 2 |



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
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	|     #      |                                | # |           |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You          |   |           |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are          |   |           |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the          |   |           |  |  |     |                             |                                |
	|           | [[rs(4).field]] = d2FyZXdvbGY= | 1 | Is Base64 |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset with Is Binary
	Given I have the following recordset to search for multiple criteria
	| rs         | value        |
	| rs().field | You          |
	| rs().field | are          |
	| rs().field | the          |
	| rs().field | 101011110010 |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Binary" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	| #          |                                | # |           |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You          |   |           |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are          |   |           |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the          |   |           |  |  |     |                             |                                |
	|           | [[rs(4).field]] = 101011110010 | 1 | Is Binary |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset with Is Hex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |	
	| rs().field | 77617265776f6c66 |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Hex" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                                    | # |        |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You              |   |        |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are              |   |        |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the              |   |        |  |  |     |                             |                                |
	|           | [[rs(4).field]] = 77617265776f6c66 | 1 | Is Hex |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset with Not Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | d2FyZXdvbGY=      |
	| rs().field | d2FyZXdvbGY=      |
	| rs().field | d2FyZXdvbGY=      |	
	| rs().field | You |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                                | # |            |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = d2FyZXdvbGY= |   |            |  |  |     |                             |                                |
	|           | [[rs(2).field]] = d2FyZXdvbGY= |   |            |  |  |     |                             |                                |
	|           | [[rs(3).field]] = d2FyZXdvbGY= |   |            |  |  |     |                             |                                |
	|           | [[rs(4).field]] = You          | 1 | Not Base64 |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset with Not Between DateTime
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 2/3/2013|
	| rs().field | 7/3/2013|
	| rs().field | 2/3/2013|	
	| rs().field | 2/3/2013|	
	And field to search is "[[rs().field]]"
	And  is between search the recordset with type "Not Between" and criteria is "1/3/2013" and "3/3/2013"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 2
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |             |  |          | And      | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 2/3/2013 |   |             |  |          |          |                             |                                |
	|           | [[rs(2).field]] = 7/3/2013 |   |             |  |          |          |                             |                                |
	|           | [[rs(3).field]] = 2/3/2013 |   |             |  |          |          |                             |                                |
	|           | [[rs(4).field]] = 2/3/2013 | 1 | Not Between |  | 1/3/2013 | 3/3/2013 | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 2 |

Scenario: Find an index of data in a recordset with Not Between numeric
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 17    |
	| rs().field | 22    |
	| rs().field | 400   |
	| rs().field | 31    |	
	And field to search is "[[rs().field]]"
	And  is between search the recordset with type "Not Between" and criteria is "16" and "33"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 3
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                       | # |             |  |    | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 17  |   |             |  |    |     |                             |                                |
	|           | [[rs(2).field]] = 22  |   |             |  |    |     |                             |                                |
	|           | [[rs(3).field]] = 400 |   |             |  |    |     |                             |                                |
	|           | [[rs(4).field]] = 31  | 1 | Not Between |  | 16 | 33  | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 3 |

Scenario: Find an index of data in a recordset with Not Binary
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | 101011110010 |
	| rs().field | 101011110010 |
	| rs().field | 101011110010 |	
	| rs().field | warewolf |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Binary" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                                | # |            |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 101011110010 |   |            |  |  |     |                             |                                |
	|           | [[rs(2).field]] = 101011110010 |   |            |  |  |     |                             |                                |
	|           | [[rs(3).field]] = 101011110010 |   |            |  |  |     |                             |                                |
	|           | [[rs(4).field]] = warewolf     | 1 | Not Binary |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset with Not Hex
	Given I have the following recordset to search for multiple criteria
	| rs         | value            |
	| rs().field | 77617265776f6c66 |
	| rs().field | 77617265776f6c66 |
	| rs().field | 77617265776f6c66 |
	| rs().field | warewolf         |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Hex" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                                    | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 77617265776f6c66 |   |         |  |  |     |                             |                                |
	|           | [[rs(2).field]] = 77617265776f6c66 |   |         |  |  |     |                             |                                |
	|           | [[rs(3).field]] = 77617265776f6c66 |   |         |  |  |     |                             |                                |
	|           | [[rs(4).field]] = warewolf         | 1 | Not Hex |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset with Not Regex
	Given I have the following recordset to search for multiple criteria
	| rs         | value           |
	| rs().field | 999.999.999.999 |
	| rs().field | 999.999.999.999 |
	| rs().field | 999.999.999.999 |
	| rs().field | warewolf        |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Regex" and criteria is "\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                                   | # |           |                                        |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 999.999.999.999 |   |           |                                        |  |     |                             |                                |
	|           | [[rs(2).field]] = 999.999.999.999 |   |           |                                        |  |     |                             |                                |
	|           | [[rs(3).field]] = 999.999.999.999 |   |           |                                        |  |     |                             |                                |
	|           | [[rs(4).field]] = warewolf        | 1 | Not Regex | \b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset with Doesn't Start With
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | what  |	
	| rs().field | why   |
	| rs().field | yay   |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Doesn't Start With" and criteria is "w"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 3
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                        | # |                    |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = what |   |                    |   |  |     |                             |                                |
	|           | [[rs(2).field]] = why  |   |                    |   |  |     |                             |                                |
	|           | [[rs(3).field]] = yay  | 1 | Doesn't Start With | w |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 3 |

Scenario: Find an index of data in a recordset with Doesn't End With
	Given I have the following recordset to search for multiple criteria
	| rs         | value  |
	| rs().field | arev   |
	| rs().field |      v |
	| rs().field | modev  |
	| rs().field | yay    |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Doesn't End With" and criteria is "v"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                         | # |                  |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = arev  |   |                  |   |  |     |                             |                                |
	|           | [[rs(2).field]] = v     |   |                  |   |  |     |                             |                                |
	|           | [[rs(3).field]] = modev |   |                  |   |  |     |                             |                                |
	|           | [[rs(4).field]] = yay   | 1 | Doesn't End With | v |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 4 |

Scenario: Find an index of data in a recordset search type is Equal To
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #          |                            | # |       |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |       |          |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |       |          |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |       |          |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |       |          |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |       |          |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | =     | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | Warewolf |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "=" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |   |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Warewolf |   |   |          |  |     |                             |                                |
	|           | [[rs(2).field]] = You      |   |   |          |  |     |                             |                                |
	|           | [[rs(3).field]] = are      |   |   |          |  |     |                             |                                |
	|           | [[rs(4).field]] = the      |   |   |          |  |     |                             |                                |
	|           | [[rs(5).field]] = best     |   |   |          |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf |   |   |          |  |     |                             |                                |
	|           | [[rs(7).field]] = user     | 1 | = | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "=" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |       |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |       |      |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |       |      |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |       |      |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |       |      |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |       |      |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | =     | Mars |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Greater Than
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | 4        |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | 2        |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type ">" and criteria is "3"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1
	And the execution has "NO" error
	And the debug inputs as
	|     #      |                            | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 4        |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = 2        |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | >     | 3 |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 1 |

Scenario: Find an index of data in a recordset search type is Greater Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 4     |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 8     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type ">" and criteria is "3"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #          |                        | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 4    |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = You  |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = are  |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = the  |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = best |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = 8    |   |       |   |  |     |                             |                                |
	|           | [[rs(7).field]] = user | 1 | >     | 3 |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Greater Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 4     |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 8     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type ">" and criteria is "50"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                        | # |       |    |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 4    |   |       |    |  |     |                             |                                |
	|           | [[rs(2).field]] = are  |   |       |    |  |     |                             |                                |
	|           | [[rs(3).field]] = the  |   |       |    |  |     |                             |                                |
	|           | [[rs(4).field]] = best |   |       |    |  |     |                             |                                |
	|           | [[rs(5).field]] = 8    |   |       |    |  |     |                             |                                |
	|           | [[rs(6).field]] = user | 1 | >     | 50 |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |
	
Scenario: Find an index of data in a recordset search type is Less Than
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 4     |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 8     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                        | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 4    |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are  |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the  |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = best |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = 8    |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = user | 1 | <     | 5 |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 1 |

Scenario: Find an index of data in a recordset search type is Less Than multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 4     |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 2     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                        | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 4    |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = You  |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = are  |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = the  |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = best |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = 2    |   |       |   |  |     |                             |                                |
	|           | [[rs(7).field]] = user | 1 | <     | 5 |  |     | NO                          | NO                             |
	And the debug output as
	|                    |
	| [[result]] = 1,6  |

Scenario: Find an index of data in a recordset search type is Less Than result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 2     |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 5     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<" and criteria is "1"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                        | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 2    |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are  |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the  |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = best |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = 5    |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = user | 1 | <     | 1 |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |
	
Scenario: Find an index of data in a recordset search type is Not Equal To
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<> (Not Equal)" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |                |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |                |          |  |     |                             |                                |
	|           | [[rs(2).field]] = Warewolf |   |                |          |  |     |                             |                                |
	|           | [[rs(3).field]] = Warewolf |   |                |          |  |     |                             |                                |
	|           | [[rs(4).field]] = Warewolf |   |                |          |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |                |          |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf | 1 | <> (Not Equal) | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] =  1 |

Scenario: Find an index of data in a recordset search type is Not Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<> (Not Equal)" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,2,3,4,6
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |                |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |                |          |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |                |          |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |                |          |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |                |          |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |                |          |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | <> (Not Equal) | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                         |
	| [[result]] = 1,2,3,4,6 |

Scenario: Find an index of data in a recordset search type is Not Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<> (Not Equal)" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|    #       |                        | # |                |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Mars |   |                |      |  |     |                             |                                |
	|           | [[rs(2).field]] = Mars |   |                |      |  |     |                             |                                |
	|           | [[rs(3).field]] = Mars |   |                |      |  |     |                             |                                |
	|           | [[rs(4).field]] = Mars |   |                |      |  |     |                             |                                |
	|           | [[rs(5).field]] = Mars |   |                |      |  |     |                             |                                |
	|           | [[rs(6).field]] = Mars | 1 | <> (Not Equal) | Mars |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Greater Or Equal To
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 2     |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 4     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type ">=" and criteria is "4"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                        | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 2    |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are  |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the  |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = best |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = 4    |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = user | 1 | >=    | 4 |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = 5  |

Scenario: Find an index of data in a recordset search type is Greater Or Equal To multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | 50       |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | 4        |
	And field to search is "[[rs().field]]"
	And search the recordset with type ">=" and criteria is "4"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #          |                            | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 50       |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = 4        | 1 | >=    | 4 |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Greater Or Equal To result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 1     |
	| rs().field | 2     |
	| rs().field | 3     |
	| rs().field | 1     |
	| rs().field | 2     |
	| rs().field | 3     |
	And field to search is "[[rs().field]]"
	And search the recordset with type ">=" and criteria is "4"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                     | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 1 |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = 2 |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = 3 |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = 1 |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = 2 |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = 3 | 1 | >=    | 4 |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Less Or Equal
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 5     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<=" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                        | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You  |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are  |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the  |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = best |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = 5    |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = user | 1 | <=    | 5 |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Less Or Equal multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | 1        |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | 5        |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<=" and criteria is "5"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 1        |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = 5        | 1 | <=    | 5 |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |  

Scenario: Find an index of data in a recordset search type is Less Or Equal result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 2     |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 5     |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "<=" and criteria is "1"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                        | # |       |   |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 2    |   |       |   |  |     |                             |                                |
	|           | [[rs(2).field]] = are  |   |       |   |  |     |                             |                                |
	|           | [[rs(3).field]] = the  |   |       |   |  |     |                             |                                |
	|           | [[rs(4).field]] = best |   |       |   |  |     |                             |                                |
	|           | [[rs(5).field]] = 5    |   |       |   |  |     |                             |                                |
	|           | [[rs(6).field]] = user | 1 | <=    | 1 |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |  

Scenario: Find an index of data in a recordset search type is Starts With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |             |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |             |          |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |             |          |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |             |          |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |             |          |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |             |          |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Starts With | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |  

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
	And field to search is "[[rs().field]]"
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |             |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Warewolf |   |             |          |  |     |                             |                                |
	|           | [[rs(2).field]] = You      |   |             |          |  |     |                             |                                |
	|           | [[rs(3).field]] = are      |   |             |          |  |     |                             |                                |
	|           | [[rs(4).field]] = the      |   |             |          |  |     |                             |                                |
	|           | [[rs(5).field]] = best     |   |             |          |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf |   |             |          |  |     |                             |                                |
	|           | [[rs(7).field]] = user     | 1 | Starts With | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                    |
	| [[result]] = 1,6  |  

Scenario: Find an index of data in a recordset search type is Starts With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Starts With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |             |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |             |      |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |             |      |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |             |      |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |             |      |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |             |      |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Starts With | Mars |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Ends With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |           |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |           |          |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |           |          |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |           |          |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |           |          |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |           |          |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Ends With | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

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
	And field to search is "[[rs().field]]"
	And search the recordset with type "Ends With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |           |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Warewolf |   |           |          |  |     |                             |                                |
	|           | [[rs(2).field]] = You      |   |           |          |  |     |                             |                                |
	|           | [[rs(3).field]] = are      |   |           |          |  |     |                             |                                |
	|           | [[rs(4).field]] = the      |   |           |          |  |     |                             |                                |
	|           | [[rs(5).field]] = best     |   |           |          |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf |   |           |          |  |     |                             |                                |
	|           | [[rs(7).field]] = user     | 1 | Ends With | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |  

Scenario: Find an index of data in a recordset search type is Ends With result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Ends With" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |           |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |           |      |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |           |      |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |           |      |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |           |      |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |           |      |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Ends With | Mars |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Contains
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |          |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |          |          |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |          |          |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |          |          |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |          |          |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |          |          |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Contains | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

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
	And field to search is "[[rs().field]]"
	And search the recordset with type "Contains" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |          |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Warewolf |   |          |          |  |     |                             |                                |
	|           | [[rs(2).field]] = You      |   |          |          |  |     |                             |                                |
	|           | [[rs(3).field]] = are      |   |          |          |  |     |                             |                                |
	|           | [[rs(4).field]] = the      |   |          |          |  |     |                             |                                |
	|           | [[rs(5).field]] = best     |   |          |          |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf |   |          |          |  |     |                             |                                |
	|           | [[rs(7).field]] = user     | 1 | Contains | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |  

Scenario: Find an index of data in a recordset search type is Contains result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Contains" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |          |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |          |      |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |          |      |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |          |      |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |          |      |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |          |      |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Contains | Mars |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |


Scenario: Find an index of data in a recordset search type is Doesn't Contain
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | user     |
	| rs().field | Warewolf |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |                 |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Warewolf |   |                 |          |  |     |                             |                                |
	|           | [[rs(2).field]] = Warewolf |   |                 |          |  |     |                             |                                |
	|           | [[rs(3).field]] = Warewolf |   |                 |          |  |     |                             |                                |
	|           | [[rs(4).field]] = Warewolf |   |                 |          |  |     |                             |                                |
	|           | [[rs(5).field]] = user     |   |                 |          |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf | 1 | Doesn't Contain | Warewolf |  |     | NO                          | NO                             |      
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Doesn't Contain multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | Warewolf |
	| rs().field | user     |
	| rs().field | Warewolf |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Doesn't Contain" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,5
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |                 |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |                 |          |  |     |                             |                                |
	|           | [[rs(2).field]] = Warewolf |   |                 |          |  |     |                             |                                |
	|           | [[rs(3).field]] = Warewolf |   |                 |          |  |     |                             |                                |
	|           | [[rs(4).field]] = Warewolf |   |                 |          |  |     |                             |                                |
	|           | [[rs(5).field]] = user     |   |                 |          |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf | 1 | Doesn't Contain | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,5 |


Scenario: Find an index of data in a recordset search type is Doesn't Contain result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	| rs().field | Mars  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Doesn't Contain" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|    #       |                        | # |                 |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Mars |   |                 |      |  |     |                             |                                |
	|           | [[rs(2).field]] = Mars |   |                 |      |  |     |                             |                                |
	|           | [[rs(3).field]] = Mars |   |                 |      |  |     |                             |                                |
	|           | [[rs(4).field]] = Mars |   |                 |      |  |     |                             |                                |
	|           | [[rs(5).field]] = Mars |   |                 |      |  |     |                             |                                |
	|           | [[rs(6).field]] = Mars | 1 | Doesn't Contain | Mars |  |     | NO                          | NO                             |     
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Is Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | $$       |
	| rs().field | $$       |
	| rs().field | $$       |
	| rs().field | $$       |
	| rs().field | Warewolf |
	| rs().field | $$       |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |                 |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(2).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(3).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(4).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |                 |  |  |     |                             |                                |
	|           | [[rs(6).field]] = $$       | 1 | Is Alphanumeric |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Is Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | Warewolf |
	| rs().field | $$       |
	| rs().field | $$       |
	| rs().field | $$       |
	| rs().field | $$       |
	| rs().field | Warewolf |
	| rs().field | $$       |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |                 |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = Warewolf |   |                 |  |  |     |                             |                                |
	|           | [[rs(2).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(3).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(4).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(5).field]] = $$       |   |                 |  |  |     |                             |                                |
	|           | [[rs(6).field]] = Warewolf |   |                 |  |  |     |                             |                                |
	|           | [[rs(7).field]] = $$       | 1 | Is Alphanumeric |  |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | $$    |
	| rs().field | $$    |
	| rs().field | $$    |
	| rs().field | $$    |
	| rs().field | $$    |
	| rs().field | $$    |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                      | # |                 |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = $$ |   |                 |  |  |     |                             |                                |
	|           | [[rs(2).field]] = $$ |   |                 |  |  |     |                             |                                |
	|           | [[rs(3).field]] = $$ |   |                 |  |  |     |                             |                                |
	|           | [[rs(4).field]] = $$ |   |                 |  |  |     |                             |                                |
	|           | [[rs(5).field]] = $$ |   |                 |  |  |     |                             |                                |
	|           | [[rs(6).field]] = $$ | 1 | Is Alphanumeric |  |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs         | value        |
	| rs().field | You          |
	| rs().field | You          |
	| rs().field | are          |
	| rs().field | the          |
	| rs().field | d2FyZXdvbGY= |
	| rs().field | You          |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|    #       |                                | # |           |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You          |   |           |  |  |     |                             |                                |
	|           | [[rs(2).field]] = You          |   |           |  |  |     |                             |                                |
	|           | [[rs(3).field]] = are          |   |           |  |  |     |                             |                                |
	|           | [[rs(4).field]] = the          |   |           |  |  |     |                             |                                |
	|           | [[rs(5).field]] = d2FyZXdvbGY= |   |           |  |  |     |                             |                                |
	|           | [[rs(6).field]] = You          | 1 | Is Base64 |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Is Base64 multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value        |
	| rs().field | d2FyZXdvbGY= |
	| rs().field | You          |
	| rs().field | are          |
	| rs().field | the          |
	| rs().field | You          |
	| rs().field | d2FyZXdvbGY= |
	| rs().field | You          |	
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                                | # |           |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = d2FyZXdvbGY= |   |           |  |  |     |                             |                                |
	|           | [[rs(2).field]] = You          |   |           |  |  |     |                             |                                |
	|           | [[rs(3).field]] = are          |   |           |  |  |     |                             |                                |
	|           | [[rs(4).field]] = the          |   |           |  |  |     |                             |                                |
	|           | [[rs(5).field]] = You          |   |           |  |  |     |                             |                                |
	|           | [[rs(6).field]] = d2FyZXdvbGY= |   |           |  |  |     |                             |                                |
	|           | [[rs(7).field]] = You          | 1 | Is Base64 |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is Base64 result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | You   |
	| rs().field | You   |
	| rs().field | You   |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                       | # |           |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You |   |           |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are |   |           |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the |   |           |  |  |     |                             |                                |
	|           | [[rs(4).field]] = You |   |           |  |  |     |                             |                                |
	|           | [[rs(5).field]] = You |   |           |  |  |     |                             |                                |
	|           | [[rs(6).field]] = You | 1 | Is Base64 |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                  |
	| [[result]] = -1 |

	Scenario: Find an index of data in a recordset search type is Is Date
	Given I have the following recordset to search for multiple criteria
	| rs         | value      |
	| rs().field | You        |
	| rs().field | are        |
	| rs().field | the        |
	| rs().field | best       |
	| rs().field | 12/10/2013 |
	| rs().field | user       |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Date" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|     #      |                              | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You        |   |         |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are        |   |         |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the        |   |         |  |  |     |                             |                                |
	|           | [[rs(4).field]] = best       |   |         |  |  |     |                             |                                |
	|           | [[rs(5).field]] = 12/10/2013 |   |         |  |  |     |                             |                                |
	|           | [[rs(6).field]] = user       | 1 | Is Date |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Is Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value      |
	| rs().field | 12/10/2013 |
	| rs().field | You        |
	| rs().field | are        |
	| rs().field | the        |
	| rs().field | best       |
	| rs().field | 12/10/2013 |
	| rs().field | user       |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Date" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                              | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = 12/10/2013 |   |         |  |  |     |                             |                                |
	|           | [[rs(2).field]] = You        |   |         |  |  |     |                             |                                |
	|           | [[rs(3).field]] = are        |   |         |  |  |     |                             |                                |
	|           | [[rs(4).field]] = the        |   |         |  |  |     |                             |                                |
	|           | [[rs(5).field]] = best       |   |         |  |  |     |                             |                                |
	|           | [[rs(6).field]] = 12/10/2013 |   |         |  |  |     |                             |                                |
	|           | [[rs(7).field]] = user       | 1 | Is Date |  |  |     | NO                          | NO                             |    
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Date" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #          |                            | # |         |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |         |      |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |         |      |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |         |      |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |         |      |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |         |      |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Is Date | Mars |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Is Email
	Given I have the following recordset to search for multiple criteria
	| rs         | value                |
	| rs().field | You                  |
	| rs().field | are                  |
	| rs().field | the                  |
	| rs().field | best                 |
	| rs().field | test@testEmail.co.za |
	| rs().field | user                 |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                                        | # |          |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You                  |   |          |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are                  |   |          |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the                  |   |          |  |  |     |                             |                                |
	|           | [[rs(4).field]] = best                 |   |          |  |  |     |                             |                                |
	|           | [[rs(5).field]] = test@testEmail.co.za |   |          |  |  |     |                             |                                |
	|           | [[rs(6).field]] = user                 | 1 | Is Email |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Is Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value                |
	| rs().field | test@testEmail.co.za |
	| rs().field | You                  |
	| rs().field | are                  |
	| rs().field | the                  |
	| rs().field | best                 |
	| rs().field | test@testEmail.co.za |
	| rs().field | user                 |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	|    #       |                                        | # |          |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = test@testEmail.co.za |   |          |  |  |     |                             |                                |
	|           | [[rs(2).field]] = You                  |   |          |  |  |     |                             |                                |
	|           | [[rs(3).field]] = are                  |   |          |  |  |     |                             |                                |
	|           | [[rs(4).field]] = the                  |   |          |  |  |     |                             |                                |
	|           | [[rs(5).field]] = best                 |   |          |  |  |     |                             |                                |
	|           | [[rs(6).field]] = test@testEmail.co.za |   |          |  |  |     |                             |                                |
	|           | [[rs(7).field]] = user                 | 1 | Is Email |  |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                            | # |          |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |          |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |          |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |          |  |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |          |  |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |          |  |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 | Is Email |  |  |     | NO                          | NO                             |	    
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Is Numeric
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 45    |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	|   #        |                        | # |            |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You  |   |            |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are  |   |            |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the  |   |            |  |  |     |                             |                                |
	|           | [[rs(4).field]] = best |   |            |  |  |     |                             |                                |
	|           | [[rs(5).field]] = 45   |   |            |  |  |     |                             |                                |
	|           | [[rs(6).field]] = user | 1 | Is Numeric |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Is Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 41    |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 54    |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                        | # |            |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 41   |   |            |  |  |     |                             |                                |
	|             | [[rs(2).field]] = You  |   |            |  |  |     |                             |                                |
	|             | [[rs(3).field]] = are  |   |            |  |  |     |                             |                                |
	|             | [[rs(4).field]] = the  |   |            |  |  |     |                             |                                |
	|             | [[rs(5).field]] = best |   |            |  |  |     |                             |                                |
	|             | [[rs(6).field]] = 54   |   |            |  |  |     |                             |                                |
	|             | [[rs(7).field]] = user | 1 | Is Numeric |  |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	|  #         |                            | # |  |            |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s)| [[rs(1).field]] = You      |   |  |            |  |  |     |                             |                                |
	|           | [[rs(2).field]] = are      |   |  |            |  |  |     |                             |                                |
	|           | [[rs(3).field]] = the      |   |  |            |  |  |     |                             |                                |
	|           | [[rs(4).field]] = best     |   |  |            |  |  |     |                             |                                |
	|           | [[rs(5).field]] = Warewolf |   |  |            |  |  |     |                             |                                |
	|           | [[rs(6).field]] = user     | 1 |  | Is Numeric |  |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Is Regex
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |          |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You      |   |          |          |  |     |                             |                                |
	|             | [[rs(2).field]] = are      |   |          |          |  |     |                             |                                |
	|             | [[rs(3).field]] = the      |   |          |          |  |     |                             |                                |
	|             | [[rs(4).field]] = best     |   |          |          |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf |   |          |          |  |     |                             |                                |
	|             | [[rs(6).field]] = user     | 1 | Is Regex | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Is Regex multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | Warewolf |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Regex" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |          |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = Warewolf |   |          |          |  |     |                             |                                |
	|             | [[rs(2).field]] = You      |   |          |          |  |     |                             |                                |
	|             | [[rs(3).field]] = are      |   |          |          |  |     |                             |                                |
	|             | [[rs(4).field]] = the      |   |          |          |  |     |                             |                                |
	|             | [[rs(5).field]] = best     |   |          |          |  |     |                             |                                |
	|             | [[rs(6).field]] = Warewolf |   |          |          |  |     |                             |                                |
	|             | [[rs(7).field]] = user     | 1 | Is Regex | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is Regex result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Regex" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |          |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You      |   |          |      |  |     |                             |                                |
	|             | [[rs(2).field]] = are      |   |          |      |  |     |                             |                                |
	|             | [[rs(3).field]] = the      |   |          |      |  |     |                             |                                |
	|             | [[rs(4).field]] = best     |   |          |      |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf |   |          |      |  |     |                             |                                |
	|             | [[rs(6).field]] = user     | 1 | Is Regex | Mars |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Is Text
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | 15       |
	| rs().field | 56       |
	| rs().field | 45       |
	| rs().field | 7        |
	| rs().field | Warewolf |
	| rs().field | 16       |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 15       |   |         |  |  |     |                             |                                |
	|             | [[rs(2).field]] = 56       |   |         |  |  |     |                             |                                |
	|             | [[rs(3).field]] = 45       |   |         |  |  |     |                             |                                |
	|             | [[rs(4).field]] = 7        |   |         |  |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf |   |         |  |  |     |                             |                                |
	|             | [[rs(6).field]] = 16       | 1 | Is Text |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Is Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | Warewolf |
	| rs().field | 45       |
	| rs().field | 54       |
	| rs().field | 51       |
	| rs().field | 86       |
	| rs().field | Warewolf |
	| rs().field | 8        |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = Warewolf |   |         |  |  |     |                             |                                |
	|             | [[rs(2).field]] = 45       |   |         |  |  |     |                             |                                |
	|             | [[rs(3).field]] = 54       |   |         |  |  |     |                             |                                |
	|             | [[rs(4).field]] = 51       |   |         |  |  |     |                             |                                |
	|             | [[rs(5).field]] = 86       |   |         |  |  |     |                             |                                |
	|             | [[rs(6).field]] = Warewolf |   |         |  |  |     |                             |                                |
	|             | [[rs(7).field]] = 8        | 1 | Is Text |  |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 1     |
	| rs().field | 2     |
	| rs().field | 3     |
	| rs().field | 4     |
	| rs().field | 6     |
	| rs().field | 5     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                     | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 1 |   |         |  |  |     |                             |                                |
	|             | [[rs(2).field]] = 2 |   |         |  |  |     |                             |                                |
	|             | [[rs(3).field]] = 3 |   |         |  |  |     |                             |                                |
	|             | [[rs(4).field]] = 4 |   |         |  |  |     |                             |                                |
	|             | [[rs(5).field]] = 6 |   |         |  |  |     |                             |                                |
	|             | [[rs(6).field]] = 5 | 1 | Is Text |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Is XML
	Given I have the following recordset to search for multiple criteria
	| rs         | value         |
	| rs().field | You           |
	| rs().field | are           |
	| rs().field | the           |
	| rs().field | best          |
	| rs().field | <test></test> |
	| rs().field | user          |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is XML" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                 | # |        |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You           |   |        |  |  |     |                             |                                |
	|             | [[rs(2).field]] = are           |   |        |  |  |     |                             |                                |
	|             | [[rs(3).field]] = the           |   |        |  |  |     |                             |                                |
	|             | [[rs(4).field]] = best          |   |        |  |  |     |                             |                                |
	|             | [[rs(5).field]] = <test></test> |   |        |  |  |     |                             |                                |
	|             | [[rs(6).field]] = user          | 1 | Is XML |  |  |     | NO                          | NO                             |
	And the debug output as
	|                |
	| [[result]] = 5 |
	
Scenario: Find an index of data in a recordset search type is Is XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value         |
	| rs().field | <test></test> |
	| rs().field | You           |
	| rs().field | are           |
	| rs().field | the           |
	| rs().field | best          |
	| rs().field | <test></test> |
	| rs().field | user          |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                 | # |        |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = <test></test> |   |        |          |  |     |                             |                                |
	|             | [[rs(2).field]] = You           |   |        |          |  |     |                             |                                |
	|             | [[rs(3).field]] = are           |   |        |          |  |     |                             |                                |
	|             | [[rs(4).field]] = the           |   |        |          |  |     |                             |                                |
	|             | [[rs(5).field]] = best          |   |        |          |  |     |                             |                                |
	|             | [[rs(6).field]] = <test></test> |   |        |          |  |     |                             |                                |
	|             | [[rs(7).field]] = user          | 1 | Is XML | Warewolf |  |     | NO                          | NO                             |     
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Is XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Is XML" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |        |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You      |   |        |      |  |     |                             |                                |
	|             | [[rs(2).field]] = are      |   |        |      |  |     |                             |                                |
	|             | [[rs(3).field]] = the      |   |        |      |  |     |                             |                                |
	|             | [[rs(4).field]] = best     |   |        |      |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf |   |        |      |  |     |                             |                                |
	|             | [[rs(6).field]] = user     | 1 | Is XML | Mars |  |     | NO                          | NO                             |  
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Not Alphanumeric
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | $$    |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                        | # |                  |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You  |   |                  |  |  |     |                             |                                |
	|             | [[rs(2).field]] = are  |   |                  |  |  |     |                             |                                |
	|             | [[rs(3).field]] = the  |   |                  |  |  |     |                             |                                |
	|             | [[rs(4).field]] = best |   |                  |  |  |     |                             |                                |
	|             | [[rs(5).field]] = $$   |   |                  |  |  |     |                             |                                |
	|             | [[rs(6).field]] = user | 1 | Not Alphanumeric |  |  |     | NO                          | NO                             |
	And the debug output as
	|                 |
	| [[result]] = 5 |

Scenario: Find an index of data in a recordset search type is Not Alphanumeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | $$    |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | $$    |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                        | # |                  |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = $$   |   |                  |  |  |     |                             |                                |
	|             | [[rs(2).field]] = You  |   |                  |  |  |     |                             |                                |
	|             | [[rs(3).field]] = are  |   |                  |  |  |     |                             |                                |
	|             | [[rs(4).field]] = the  |   |                  |  |  |     |                             |                                |
	|             | [[rs(5).field]] = best |   |                  |  |  |     |                             |                                |
	|             | [[rs(6).field]] = $$   |   |                  |  |  |     |                             |                                |
	|             | [[rs(7).field]] = user | 1 | Not Alphanumeric |  |  |     | NO                          | NO                             |
	And the debug output as
	|                   |
	| [[result]] = 1,6 |

Scenario: Find an index of data in a recordset search type is Not Alphanumeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Alphanumeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |                  |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You      |   |                  |  |  |     |                             |                                |
	|             | [[rs(2).field]] = are      |   |                  |  |  |     |                             |                                |
	|             | [[rs(3).field]] = the      |   |                  |  |  |     |                             |                                |
	|             | [[rs(4).field]] = best     |   |                  |  |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf |   |                  |  |  |     |                             |                                |
	|             | [[rs(6).field]] = user     | 1 | Not Alphanumeric |  |  |     | NO                          | NO                             |   
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Not Date
	Given I have the following recordset to search for multiple criteria
	| rs         | value      |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | Warewolf   |
	| rs().field | 12/11/2013 |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                              | # |          |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(2).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(3).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(4).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf   |   |          |          |  |     |                             |                                |
	|             | [[rs(6).field]] = 12/11/2013 | 1 | Not Date | Warewolf |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] = 5  |

Scenario: Find an index of data in a recordset search type is Not Date multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value      |
	| rs().field | Warewolf   |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | Warewolf   |
	| rs().field | 12/11/2013 |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Date" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                              | # |          |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = Warewolf   |   |          |          |  |     |                             |                                |
	|             | [[rs(2).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(3).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(4).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(5).field]] = 12/11/2013 |   |          |          |  |     |                             |                                |
	|             | [[rs(6).field]] = Warewolf   |   |          |          |  |     |                             |                                |
	|             | [[rs(7).field]] = 12/11/2013 | 1 | Not Date | Warewolf |  |     | NO                          | NO                             |    
	And the debug output as
	|                    |
	| [[result]] = 1,6  |

Scenario: Find an index of data in a recordset search type is Not Date result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value      |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	| rs().field | 12/11/2013 |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Date" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                              | # |          |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 12/11/2013 |   |          |  |  |     |                             |                                |
	|             | [[rs(2).field]] = 12/11/2013 |   |          |  |  |     |                             |                                |
	|             | [[rs(3).field]] = 12/11/2013 |   |          |  |  |     |                             |                                |
	|             | [[rs(4).field]] = 12/11/2013 |   |          |  |  |     |                             |                                |
	|             | [[rs(5).field]] = 12/11/2013 |   |          |  |  |     |                             |                                |
	|             | [[rs(6).field]] = 12/11/2013 | 1 | Not Date |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                  |
	| [[result]] = -1 |

Scenario: Find an index of data in a recordset search type is Not Email
	Given I have the following recordset to search for multiple criteria
	| rs         | value                |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | Warewolf             |
	| rs().field | test@testEmail.co.za |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                        | # |           |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(2).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(3).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(4).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf             |   |           |  |  |     |                             |                                |
	|             | [[rs(6).field]] = test@testEmail.co.za | 1 | Not Email |  |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] =  5 |

Scenario: Find an index of data in a recordset search type is Not Email multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value                |
	| rs().field | Warewolf             |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | Warewolf             |
	| rs().field | test@testEmail.co.za |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Email" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                        | # |           |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = Warewolf             |   |           |  |  |     |                             |                                |
	|             | [[rs(2).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(3).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(4).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(5).field]] = test@testEmail.co.za |   |           |  |  |     |                             |                                |
	|             | [[rs(6).field]] = Warewolf             |   |           |  |  |     |                             |                                |
	|             | [[rs(7).field]] = test@testEmail.co.za | 1 | Not Email |  |  |     | NO                          | NO                             |
	And the debug output as
	|                    |
	| [[result]] =  1,6 |

Scenario: Find an index of data in a recordset search type is Not Email result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value                |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	| rs().field | test@testEmail.co.za |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Email" and criteria is "Mars"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                        | # |           |      |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = test@testEmail.co.za |   |           |      |  |     |                             |                                |
	|             | [[rs(2).field]] = test@testEmail.co.za |   |           |      |  |     |                             |                                |
	|             | [[rs(3).field]] = test@testEmail.co.za |   |           |      |  |     |                             |                                |
	|             | [[rs(4).field]] = test@testEmail.co.za |   |           |      |  |     |                             |                                |
	|             | [[rs(5).field]] = test@testEmail.co.za |   |           |      |  |     |                             |                                |
	|             | [[rs(6).field]] = test@testEmail.co.za | 1 | Not Email | Mars |  |     | NO                          | NO                             |   
	And the debug output as
	|                   |
	| [[result]] =  -1 |

Scenario: Find an index of data in a recordset search type is Not Numeric
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | 152      |
	| rs().field | 5        |
	| rs().field | 6        |
	| rs().field | 7        |
	| rs().field | Warewolf |
	| rs().field | 5        |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |             |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 152      |   |             |  |  |     |                             |                                |
	|             | [[rs(2).field]] = 5        |   |             |  |  |     |                             |                                |
	|             | [[rs(3).field]] = 6        |   |             |  |  |     |                             |                                |
	|             | [[rs(4).field]] = 7        |   |             |  |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf |   |             |  |  |     |                             |                                |
	|             | [[rs(6).field]] = 5        | 1 | Not Numeric |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                   |
	| [[result]] =  5  |

Scenario: Find an index of data in a recordset search type is Not Numeric multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | Warewolf |
	| rs().field | 45       |
	| rs().field | 2        |
	| rs().field | 4        |
	| rs().field | 5        |
	| rs().field | Warewolf |
	| rs().field | 5        |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |             |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = Warewolf |   |             |  |  |     |                             |                                |
	|             | [[rs(2).field]] = 45       |   |             |  |  |     |                             |                                |
	|             | [[rs(3).field]] = 2        |   |             |  |  |     |                             |                                |
	|             | [[rs(4).field]] = 4        |   |             |  |  |     |                             |                                |
	|             | [[rs(5).field]] = 5        |   |             |  |  |     |                             |                                |
	|             | [[rs(6).field]] = Warewolf |   |             |  |  |     |                             |                                |
	|             | [[rs(7).field]] = 5        | 1 | Not Numeric |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                    |
	| [[result]] =  1,6 |


Scenario: Find an index of data in a recordset search type is Not Numeric result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 1     |
	| rs().field | 1     |
	| rs().field | 2     |
	| rs().field | 2     |
	| rs().field | 3     |
	| rs().field | 3     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Numeric" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                     | # |             |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 1 |   |             |  |  |     |                             |                                |
	|             | [[rs(2).field]] = 1 |   |             |  |  |     |                             |                                |
	|             | [[rs(3).field]] = 2 |   |             |  |  |     |                             |                                |
	|             | [[rs(4).field]] = 2 |   |             |  |  |     |                             |                                |
	|             | [[rs(5).field]] = 3 |   |             |  |  |     |                             |                                |
	|             | [[rs(6).field]] = 3 | 1 | Not Numeric |  |  |     | NO                          | NO                             |     
	 And the debug output as	   
	|                   |
	| [[result]] =  -1 |

Scenario: Find an index of data in a recordset search type is Not Text
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 52    |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                        | # |          |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You  |   |          |  |  |     |                             |                                |
	|             | [[rs(2).field]] = are  |   |          |  |  |     |                             |                                |
	|             | [[rs(3).field]] = the  |   |          |  |  |     |                             |                                |
	|             | [[rs(4).field]] = best |   |          |  |  |     |                             |                                |
	|             | [[rs(5).field]] = 52   |   |          |  |  |     |                             |                                |
	|             | [[rs(6).field]] = user | 1 | Not Text |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                   |
	| [[result]] =  5  |


Scenario: Find an index of data in a recordset search type is Not Text multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value |
	| rs().field | 45    |
	| rs().field | You   |
	| rs().field | are   |
	| rs().field | the   |
	| rs().field | best  |
	| rs().field | 741   |
	| rs().field | user  |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                        | # |          |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = 45   |   |          |  |  |     |                             |                                |
	|             | [[rs(2).field]] = You  |   |          |  |  |     |                             |                                |
	|             | [[rs(3).field]] = are  |   |          |  |  |     |                             |                                |
	|             | [[rs(4).field]] = the  |   |          |  |  |     |                             |                                |
	|             | [[rs(5).field]] = best |   |          |  |  |     |                             |                                |
	|             | [[rs(6).field]] = 741  |   |          |  |  |     |                             |                                |
	|             | [[rs(7).field]] = user | 1 | Not Text |  |  |     | NO                          | NO                             |     
	And the debug output as
	|                    |
	| [[result]] =  1,6 |

Scenario: Find an index of data in a recordset search type is Not Text result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs         | value    |
	| rs().field | You      |
	| rs().field | are      |
	| rs().field | the      |
	| rs().field | best     |
	| rs().field | Warewolf |
	| rs().field | user     |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not Text" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                            | # |          |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = You      |   |          |  |  |     |                             |                                |
	|             | [[rs(2).field]] = are      |   |          |  |  |     |                             |                                |
	|             | [[rs(3).field]] = the      |   |          |  |  |     |                             |                                |
	|             | [[rs(4).field]] = best     |   |          |  |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf |   |          |  |  |     |                             |                                |
	|             | [[rs(6).field]] = user     | 1 | Not Text |  |  |     | NO                          | NO                             |   
	 And the debug output as			  
	|                   |
	| [[result]] =  -1 |


Scenario: Find an index of data in a recordset search type is Not XML
	Given I have the following recordset to search for multiple criteria
	| rs         | value         |
	| rs().field | <test></test> |
	| rs().field | <test></test> |
	| rs().field | <test></test> |
	| rs().field | <test></test> |
	| rs().field | Warewolf      |
	| rs().field | <test></test> |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not XML" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                 | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(2).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(3).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(4).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(5).field]] = Warewolf      |   |         |  |  |     |                             |                                |
	|             | [[rs(6).field]] = <test></test> | 1 | Not XML |  |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] =  5 |


Scenario: Find an index of data in a recordset search type is Not XML multiple results
	Given I have the following recordset to search for multiple criteria
	| rs         | value         |
	| rs().field | Warewolf      |
	| rs().field | <test></test> |
	| rs().field | <test></test> |
	| rs().field | <test></test> |
	| rs().field | <test></test> |
	| rs().field | Warewolf      |
	| rs().field | <test></test> |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not XML" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,6
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                 | # |         |          |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = Warewolf      |   |         |          |  |     |                             |                                |
	|             | [[rs(2).field]] = <test></test> |   |         |          |  |     |                             |                                |
	|             | [[rs(3).field]] = <test></test> |   |         |          |  |     |                             |                                |
	|             | [[rs(4).field]] = <test></test> |   |         |          |  |     |                             |                                |
	|             | [[rs(5).field]] = <test></test> |   |         |          |  |     |                             |                                |
	|             | [[rs(6).field]] = Warewolf      |   |         |          |  |     |                             |                                |
	|             | [[rs(7).field]] = <test></test> | 1 | Not XML | Warewolf |  |     | NO                          | NO                             |  
	And the debug output as
	|                    |
	| [[result]] =  1,6 |


Scenario: Find an index of data in a recordset search type is Not XML result doesnt exist
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	| rs().field |<test></test> |
	And field to search is "[[rs().field]]"
	And search the recordset with type "Not XML" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be -1
	And the execution has "NO" error
	And the debug inputs as
	| #           |                                 | # |         |  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(2).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(3).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(4).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(5).field]] = <test></test> |   |         |  |  |     |                             |                                |
	|             | [[rs(6).field]] = <test></test> | 1 | Not XML |  |  |     | NO                          | NO                             |
	And the debug output as
	|                    |
	| [[result]] =  -1  |

Scenario: Find an index of data in a recordset search type Contains and requires all fields to match true and match all rows true
	Given I have the following recordset to search for multiple criteria
	| rs           | value |
	| rs(1).field1 | 123   |
	| rs(2).field1 | 2     |
	| rs(3).field1 | 5     |
	Given I have the following recordset to search for multiple criteria
	| rs            | value |
	| rs1(1).field2 | 214   |
	| rs1(2).field2 | 51    |
	| rs1(3).field2 | 56    |
	Given I have the following recordset to search for multiple criteria
	| rs            | value |
	| rs2(1).field3 | 512   |
	| rs2(2).field3 | 84    |
	| rs2(3).field3 | 12    |
	And the fields to search is
	| field            |
	| [[rs().field1]]  |
	| [[rs1().field2]] |
	| [[rs2().field3]] |
	And search the recordset with type "Contains" and criteria is "1"	
	And search the recordset with type "Contains" and criteria is "2"	
	And when match all search criteria is "true"
	And when requires all fields to match is "true"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type Contains and requires all fields to match false and match all rows true
	Given I have the following recordset to search for multiple criteria
	| rs           | value |
	| rs(1).field1 | 123   |
	| rs(2).field1 | 2     |
	| rs(3).field1 | 5     |
	Given I have the following recordset to search for multiple criteria
	| rs            | value |
	| rs1(1).field2 | 214   |
	| rs1(2).field2 | 51    |
	| rs1(3).field2 | 56    |
	Given I have the following recordset to search for multiple criteria
	| rs            | value |
	| rs2(1).field3 | 512   |
	| rs2(2).field3 | 84    |
	| rs2(3).field3 | 12    |
	And the fields to search is
	| field            |
	| [[rs().field1]]  |
	| [[rs1().field2]] |
	| [[rs2().field3]] |
	And search the recordset with type "Contains" and criteria is "1"	
	And search the recordset with type "Contains" and criteria is "2"	
	And when match all search criteria is "true"
	And when requires all fields to match is "false"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,3
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type Contains and requires all fields to match false and match all rows false
	Given I have the following recordset to search for multiple criteria
	| rs       | value |
	| AB(1).f1 | 123   |
	| AB(2).f1 | 2     |
	| AB(3).f1 | 5     |
	And I have the following recordset to search for multiple criteria
	| rs       | value |
	| CD(1).f2 | 214   |
	| CD(2).f2 | 51    |
	| CD(3).f2 | 56    |
	And I have the following recordset to search for multiple criteria
	| rs       | value |
	| EF(1).f3 | 512   |
	| EF(2).f3 | 84    |
	| EF(3).f3 | 12    |
	And the fields to search is
	| field       |
	| [[AB().f1]] |
	| [[CD().f2]] |
	| [[EF().f3]] |
	And search the recordset with type "Contains" and criteria is "1"	
	And search the recordset with type "Contains" and criteria is "2"	
	And when match all search criteria is "false"
	And when requires all fields to match is "false"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,2,3
	And the execution has "NO" error

Scenario: Find an index of data in a recordset search type Contains and requires all fields to match true and match all rows false
	Given I have the following recordset to search for multiple criteria
	| rs           | value |
	| rs(1).field1 | 123   |
	| rs(2).field1 | 2     |
	| rs(3).field1 | 5     |
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |	
	| rs1(1).field2 |214 |
	| rs1(2).field2 |52 |
	| rs1(3).field2 |56 |
	Given I have the following recordset to search for multiple criteria
	| rs            | value |
	| rs2(1).field3 | 512   |
	| rs2(2).field3 | 82    |
	| rs2(3).field3 | 12    |
	And the fields to search is
	| field            |
	| [[rs().field1]]  |
	| [[rs1().field2]] |
	| [[rs2().field3]] |
	And search the recordset with type "Contains" and criteria is "1"	
	And search the recordset with type "Contains" and criteria is "2"	
	And when match all search criteria is "false"
	And when requires all fields to match is "true"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 1,2
	And the execution has "NO" error

Scenario: Search using a negative index recordset criteria
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | Warewolf |
	| rs().row | User     |
	And field to search is "[[rs().row]]"
	And search the recordset with type "Not XML" and criteria is "[[my(-1).set]]"
	When the find records index multiple tool is executed
	Then the execution has "AN" error
	And the debug inputs as
	| #           |                          | # |         |                  |  | And | Require All Fields To Match | Require All Matches To Be True |
	| In Field(s) | [[rs(1).row]] = Warewolf |   |         |                  |  |     |                             |                                |
	|             | [[rs(2).row]] = User     | 1 | Not XML | [[my(-1).set]] = |  |     | NO                          | NO                             |
	And the debug output as
	|                  |
	| [[result]] =  -1 |
