Feature: DateandTimeDifference
	In order to work with date and time
	As a Warewolf user
	I want a tool that will allow me to compare two dates

Scenario: Calculate the number of days between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-28" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Weeks" 	
	When the datetime difference tool is executed
	Then the difference should be "52"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In |
#	| 2013-11-29 | 2014-11-28 | yyyy-mm-dd   | Weeks     |
#	And the debug output as 
#	|                  |
#	| [[result]] = 52 |

Scenario: Calculate the number of years with mulitpart text and variable inputs to both input fields
	Given I have a DateAndTimeDifference variable "[[years]]" equal to 13
	And I have a first date "20[[years]]-11-29" 
	And I have a DateAndTimeDifference variable "[[years2]]" equal to 14
	And I have a second date "20[[years2]]-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Years" 	
	When the datetime difference tool is executed
	Then the difference should be "0"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                        | Input 2                        | Input Format | Output In |
#	| 20[[years]]-11-29 = 2013-11-29 | 20[[years2]]-11-01 = 2014-11-01 | yyyy-mm-dd   | Years     |
#	And the debug output as 
#	|                 |
#	| [[result]] = 0 |

Scenario: Calculate the number of Months between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Months" 	
	When the datetime difference tool is executed
	Then the difference should be "12"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In |
#	| 2013-11-29 | 2014-11-01 | yyyy-mm-dd   | Months    |
#	And the debug output as 
#	|                  |
#	| [[result]] = 12 |

Scenario: Calculate the number of hours between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be "8088"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In |
#	| 2013-11-29 | 2014-11-01 | yyyy-mm-dd   | Hours     |
#	And the debug output as 
#	|                   |
#	| [[result]] = 8088 |

Scenario: Calculate the number of minutes between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the difference should be "485280"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In |
#	| 2013-11-29 | 2014-11-01 | yyyy-mm-dd   | Minutes     |
#	And the debug output as 
#	|                     |
#	| [[result]] = 485280 |

Scenario: Calculate the number of seconds between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Seconds" 	
	When the datetime difference tool is executed
	Then the difference should be "29116800"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In |
#	| 2013-11-29 | 2014-11-01 | yyyy-mm-dd   | Seconds   |
#	And the debug output as 
#	|                        |
#	| [[result]] = 29116800 |

Scenario: Calculate the number of split seconds between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Split Secs" 	
	When the datetime difference tool is executed
	Then the difference should be "29116800000"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In  |
#	| 2013-11-29 | 2014-11-01 | yyyy-mm-dd   | Split Secs |
#	And the debug output as 
#	|                           |
#	| [[result]] = 29116800000 |

Scenario: Calculate the number of weeks between two given dates
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be "8088"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In |
#	| 2013-11-29 | 2014-11-01 | yyyy-mm-dd   | Hours     |
#	And the debug output as 
#	|                    |
#	| [[result]] = 8088 |

Scenario: Calculate the number of minutes between two blank inputs
	Given I have a first date "" 
	And I have a second date "" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1          | Input 2          | Input Format | Output In |
#	| now() = DateTime | now() = DateTime | yyyy-mm-dd   | Minutes   |
#	And the debug output as 
#	|               |
#	| [[result]] = |

Scenario: Calculate the number of minutes first date is blank
	Given I have a first date "" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1          | Input 2    | Input Format | Output In |
#	| now() = DateTime | 2014-11-01 | yyyy-mm-dd   | Minutes   |
#	And the debug output as 
#	|               |
#	| [[result]] = |

Scenario: Calculate the number of minutes second date is blank
	Given I have a first date "2014-11-01" 
	And I have a second date "" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1    | Input 2          | Input Format | Output In |
#	| 2014-11-01 | now() = DateTime | yyyy-mm-dd   | Minutes   |
#	And the debug output as 
#	|               |
#	| [[result]] = |

Scenario: Calculate the number of seconds with badly formed input format
	Given I have a first date "2013-11-29" 
	And I have a second date "2014-11-01" 
	And the date format as "yyy-mm-dd"
	And I selected output in "Seconds" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format | Output In |
#	| 2013-11-29 | 2014-11-01 | yyy-mm-dd    | Seconds   |
#	And the debug output as 
#	|               |
#	| [[result]] = |

Scenario: Leave input dates blank
	Given I selected output in "Years"
	When the datetime difference tool is executed
	Then the difference should be "0"
	And the execution has "NO" error
	And the debug inputs as  
#	| Input 1          | Input 2          | Input Format | Output In |
#	| now() = DateTime | now() = DateTime | ""           | Years     |
#	And the debug output as 
#	|                 |
#	| [[result]] = 0 |

Scenario: Calculate the number of weeks dates do not match date format
	Given I have a first date "20131212" 
	And I have a second date "20141212" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1  | Input 2  | Input Format | Output In |
#	| 20131212 | 20141212 | yyyy-mm-dd   | Hours     |
#	And the debug output as 
#	|               |
#	| [[result]] = |
	
Scenario: Calculate the number of weeks using an invalid date
	Given I have a first date "2" 
	And I have a second date "20141212" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1 | Input 2  | Input Format | Output In |
#	| 2       | 20141212 | yyyy-mm-dd   | Hours     |
#	And the debug output as 
#	|               |
#	| [[result]] = |

Scenario: Calculate with negative recordset index for Input 1
	Given I have a first date "[[my(-1).date]]" 
	And I have a second date "2014-11-01" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the execution has "AN" error
#	And the debug inputs as  
#	| Input 1           | Input 2    | Input Format | Output In |
#	| [[my(-1).date]] = | 2014-11-01 | yyyy-mm-dd   | Minutes   |
#	And the debug output as 
#	|               |

Scenario: Calculate with negative recordset index for Input 2
	Given I have a first date "2014-11-01" 
	And I have a second date "[[my(-1).date]]" 
	And the date format as "yyyy-mm-dd"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the execution has "AN" error
#	And the debug inputs as  
#	| Input 1    | Input 2           | Input Format | Output In |
#	| 2014-11-01 | [[my(-1).date]] = | yyyy-mm-dd   | Minutes   |
#	And the debug output as 
#	|               |

Scenario: Calculate with negative recordset index for Format
	Given I have a first date "2014-11-01" 
	And I have a second date "2014-11-01" 
	And the date format as "[[my(-1).format]]"
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the execution has "AN" error
#	And the debug inputs as  
#	| Input 1    | Input 2    | Input Format        | Output In |
#	| 2014-11-01 | 2014-11-01 | [[my(-1).format]] = | Minutes   |
#	And the debug output as 
#	|              |

Scenario: Calculate the number of weeks between two given dates format has quoted strings
	Given I have a first date "2013-11-29 date" 
	And I have a second date "2014-11-01 date" 
	And the date format as "yyyy-mm-dd 'date'"
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be "8088"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1         | Input 2         | Input Format      | Output In |
#	| 2013-11-29 date | 2014-11-01 date | yyyy-mm-dd 'date' | Hours     |
#	And the debug output as 
#	|                   |
#	| [[result]] = 8088 |

Scenario: Calculate the number of years with incorrect inputs
	Given I have a DateAndTimeDifference variable "[[a]]" equal to 01.
	And I have a first date "2014/[[a]]/01" 
	And I have a second date "2030/01/01" 
	And the date format as "yyyy/mm/dd"
	And I selected output in "Years" 	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1                  | Input 2    | Input Format | Output In |
#	| 2014/[[a]]/01 = 2014/01./01 | 2030/01/01 | yyyy/mm/dd   | Years     |
#	And the debug output as 
#	|              |
#	| [[result]] = |

Scenario: Calculate the number of years with incorrect variable in input1
	Given I have a DateAndTimeDifference variable "[[a]]" equal to 01.
	And I have a first date "[[2014/01/01]]" 
	And I have a second date "2030/01/01" 
	And the date format as "yyyy/mm/dd"	
	When the datetime difference tool is executed
	Then the difference should be ""
	And the execution has "AN" error
#	And the debug inputs as  
#	| Input 1          | Input 2    | Input Format | Output In |
#	| [[2014/01/01]] = | 2030/01/01 | yyyy/mm/dd   | Years     |
#	And the debug output as 
#	|              |

Scenario: Calculate the number of split seconds
	Given I have a first date "06/01/2014 08:00:01.00" 
	And I have a second date "06/01/2014 08:00:01.68" 
	And the date format as "dd/mm/yyyy 12h:min:ss.sp"
	And I selected output in "Split Secs" 	
	When the datetime difference tool is executed
	Then the difference should be "68"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format             | Output In  |
#	| 06/01/2014 08:00:01.00 | 06/01/2014 08:00:01.68 | dd/mm/yyyy 12h:min:ss.sp | Split Secs |
#	And the debug output as 
#	|                 |
#	| [[result]] = 68 |

#Bug 12330
Scenario: Calculate the number of split seconds by using default date format
	Given I have a first date "2014/01/06 08:00:01.00" 
	And I have a second date "2014/01/06 08:00:01.68" 
	And the date format as ""
	And I selected output in "Split Secs" 	
	When the datetime difference tool is executed
	Then the difference should be "68"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In  |
#	| 2014/01/06 08:00:01.00 | 2014/01/06 08:00:01.68 | ""           | Split Secs |
#	And the debug output as 
#	|                 |
#	| [[result]] = 68 |

Scenario: Calculate the number of weeks
	Given I have a first date "06/01/2014 08:00:01.00" 
	And I have a second date "30/01/2014 08:00:01.06" 
	And the date format as "dd/mm/yyyy 12h:min:ss.sp"
	And I selected output in "Weeks" 	
	When the datetime difference tool is executed
	Then the difference should be "3"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format             | Output In |
#	| 06/01/2014 08:00:01.00 | 30/01/2014 08:00:01.06 | dd/mm/yyyy 12h:min:ss.sp | Weeks     |
#	And the debug output as 
#	|                |
#	| [[result]] = 3 |

Scenario: Calculate the number of weeks by using default format
	Given I have a first date "2014/01/06 08:00:01.00" 
	And I have a second date "2014/01/30 08:00:01.06" 
	And the date format as ""
	And I selected output in "Weeks" 	
	When the datetime difference tool is executed
	Then the difference should be "3"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In |
#	| 2014/01/06 08:00:01.00 | 2014/01/30 08:00:01.06 | ""           | Weeks     |
#	And the debug output as 
#	|                |
#	| [[result]] = 3 |

Scenario: Calculate the number of hours by using default format
	Given I have a first date "2014/01/06 08:00:01.00" 
	And I have a second date "2014/01/30 08:00:01.06" 
	And the date format as ""
	And I selected output in "Hours" 	
	When the datetime difference tool is executed
	Then the difference should be "576"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In |
#	| 2014/01/06 08:00:01.00 | 2014/01/30 08:00:01.06 |    ""          | Hours     |
#	And the debug output as 
#	|                |
#	| [[result]] = 576 |


Scenario: Calculate the number of Minutes by using default format
	Given I have a first date "2014/01/06 08:00:01.00" 
	And I have a second date "2014/01/30 08:00:01.68" 
	And the date format as ""
	And I selected output in "Minutes" 	
	When the datetime difference tool is executed
	Then the difference should be "34560"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In |
#	| 2014/01/06 08:00:01.00 | 2014/01/30 08:00:01.68 | ""           | Minutes   |
#	And the debug output as 
#	|                    |
#	| [[result]] = 34560 |

Scenario: Calculate the number of Seconds by using default format
	Given I have a first date "2014/01/06 08:00:01.00" 
	And I have a second date "2014/01/30 08:00:01.68" 
	And the date format as ""
	And I selected output in "Seconds" 	
	When the datetime difference tool is executed
	Then the difference should be "2073600"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In |
#	| 2014/01/06 08:00:01.00 | 2014/01/30 08:00:01.68 | ""           | Seconds   |
#	And the debug output as 
#	|                      |
#	| [[result]] = 2073600 |

Scenario: Calculate the number of Days by using default format
	Given I have a first date "2014/01/06 08:00:01.00" 
	And I have a second date "2014/01/30 08:00:01.68" 
	And the date format as ""
	And I selected output in "Days" 	
	When the datetime difference tool is executed
	Then the difference should be "24"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In |
#	| 2014/01/06 08:00:01.00 | 2014/01/30 08:00:01.68 | ""           | Days      |
#	And the debug output as 
#	|                 |
#	| [[result]] = 24 |

Scenario: Calculate the number of Months by using default format
	Given I have a first date "2011/08/06 08:00:01.00" 
	And I have a second date "2014/01/30 08:00:01.68" 
	And the date format as ""
	And I selected output in "Months" 	
	When the datetime difference tool is executed
	Then the difference should be "29"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In |
#	| 2011/08/06 08:00:01.00 | 2014/01/30 08:00:01.68 | ""           | Months    |
#	And the debug output as 
#	|                 |
#	| [[result]] = 29 |

Scenario: Calculate the number of Years by using default format
	Given I have a first date "2014/01/06 08:00:01.00" 
	And I have a second date "2016/01/06 08:00:01.68" 
	And the date format as ""
	And I selected output in "Years" 	
	When the datetime difference tool is executed
	Then the difference should be "2"
	And the execution has "NO" error
#	And the debug inputs as  
#	| Input 1                | Input 2                | Input Format | Output In |
#	| 2014/01/06 08:00:01.00 | 2016/01/06 08:00:01.68 | ""           | Years     |
#	And the debug output as 
#	|                |
#	| [[result]] = 2 |
