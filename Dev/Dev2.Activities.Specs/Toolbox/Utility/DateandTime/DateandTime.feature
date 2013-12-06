Feature: DateandTime
	In order to work with date and time
	As a Warewolf user
	I want a tool that will allow me to do date time calcuations

Scenario: Add weeks to a given date
	Given I have a date "2013-11-29" 
	And the input format as "yyyy-mm-dd"
	And I selected Add time as "Weeks" with a value of 52
	And the output format as "yyyy-mm-dd"
	When the datetime tool is executed
	Then the datetime result should be "2014-11-28"
	And datetime execution has "NO" error

Scenario: Date and Time with Everything blank
	Given I have a first date ""
	And the input format as ""
	And I selected Add time as "None" with a value of 0
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be a "System.DateTime"
	And datetime execution has "NO" error

Scenario: Date and Time with input blank and incorrect format
	Given I have a first date ""
	And the input format as "asdf"
	And I selected Add time as "None" with a value of 0
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be ""
	And datetime execution has "AN" error

Scenario: Date and Time with no seconds and add 1 second
	Given I have a first date "12:30"
	And the input format as "24h:min"
	And I selected Add time as "Seconds" with a value of 61
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be "12:31"
	And datetime execution has "NO" error

Scenario: Date and Time with badly formed inputs
	Given I have a first date "asdf"
	And the input format as "asdf"
	And I selected Add time as "None" with a value of 0
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be ""
	And datetime execution has "AN" error

Scenario: Date and Time with badly formed output format
	Given I have a first date "12:30"
	And the input format as "24h:min"
	And I selected Add time as "Seconds" with a value of 61
	And the output format as "asdf"
	When the datetime tool is executed
	Then the datetime result should be "as1f"
	And datetime execution has "AN" error

Scenario: Date and Time with characters for time to add
	Given I have a first date "12:30"
	And the input format as "24h:min"
	And I selected Add time as "Seconds" with a value of "asdf"
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be ""
   And datetime execution has "AN" error

Scenario: Date and Time with output format - 12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ
	Given I have a first date "2013/12/05 04:18:51 PM"
	And the input format as ""
	And I selected Add time as "None" with a value of 0
	And the output format as "12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ"
	When the datetime tool is executed
	Then the datetime result should be "04:05:Thursday:A.D.:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12:Dec:49:South Africa Standard Time:49:(UTC+02:00) Harare, Pretoria"
	And datetime execution has "NO" error

Scenario: Date and Time with input format - 12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ
	Given I have a first date "04:05:Thursday:A.D.:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12:Dec:49:South Africa Standard Time:49:(UTC+02:00) Harare, Pretoria"
	And the input format as "12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ"
	And I selected Add time as "None" with a value of 0
	And the output format as "yyyy/mm/dd 12h:min:ss am/pm"
	When the datetime tool is executed
	Then the datetime result should be "2013/12/05 04:18:51 PM"
	And datetime execution has "NO" error

