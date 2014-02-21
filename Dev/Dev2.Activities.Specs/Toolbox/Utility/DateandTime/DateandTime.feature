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
	And the execution has "NO" error
	And the debug inputs as  
	| Input      | Input Format | Add Time |    | Output Format |
	| 2013-11-29 | yyyy-mm-dd   | Weeks    | 52 | yyyy-mm-dd    |	
	And the debug output as 
	| Result                  |  
	| [[result]] = 2014-11-28 |


Scenario: Date and Time with Everything blank
	Given I have a date ""
	And the input format as ""
	And I selected Add time as "None" with a value of 0
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be a "System.DateTime"
	And the execution has "NO" error
	And the debug inputs as  
	| Input            | =        | Input Format            | =                      | Add Time |   | Output Format           | =                      |
	| System Date Time | DateTime | System Date Time Format | yyyy/MM/dd hh:mm:ss tt | None     | 0 | System Date Time Format | yyyy/MM/dd hh:mm:ss tt |
	And the debug output as 
	| Result                |
	| [[result]] = DateTime |

Scenario: Date and Time with input blank and incorrect format
	Given I have a date ""
	And the input format as "asdf"
	And I selected Add time as "None" with a value of 0
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be ""
	And the execution has "AN" error
	And the debug inputs as  
	| Input            | =        | Input Format | Add Time |   | Output Format           | =                      |
	| System Date Time | DateTime | asdf         | None     | 0 | System Date Time Format | yyyy/MM/dd hh:mm:ss tt |
	And the debug output as 
	| Result       |
	| [[result]] = |

Scenario: Date and Time with no seconds and add 1 second
	Given I have a date "12:30"
	And the input format as "24h:min"
	And I selected Add time as "Seconds" with a value of 61
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be "12:31"
	And the execution has "NO" error
	And the debug inputs as  
	| Input | Input Format | Add Time |    | Output Format           | =                      |
	| 12:30 | 24h:min      | Seconds  | 61 | System Date Time Format | yyyy/MM/dd hh:mm:ss tt |	
	And the debug output as 
	| Result             |
	| [[result]] = 12:31 |

Scenario: Date and Time with badly formed inputs
	Given I have a date "asdf"
	And the input format as "asdf"
	And I selected Add time as "None" with a value of 0
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be ""
	And the execution has "AN" error
	And the debug inputs as  
	| Input | Input Format | Add Time |   | Output Format           | =                      |
	| asdf  | asdf         | None     | 0 | System Date Time Format | yyyy/MM/dd hh:mm:ss tt |
	And the debug output as 
	| Result       |
	| [[result]] = |

Scenario: Date and Time with badly formed output format
	Given I have a date "12:30"
	And the input format as "24h:min"
	And I selected Add time as "Seconds" with a value of 61
	And the output format as "asdf"
	When the datetime tool is executed
	Then the datetime result should be "as1f"
	And the execution has "NO" error
	And the debug inputs as  
	| Input | Input Format | Add Time |    | Output Format |
	| 12:30 | 24h:min      | Seconds  | 61 | asdf          |
	And the debug output as 
	| Result            |
	| [[result]] = as1f |

Scenario: Date and Time with characters for time to add
	Given I have a date "12:30"
	And the input format as "24h:min"
	And I selected Add time as "Seconds" with a value of "asdf"
	And the output format as ""
	When the datetime tool is executed
	Then the datetime result should be ""
   And the execution has "AN" error
   And the debug inputs as  
	| Input | Input Format | Add Time |      | Output Format           | =                      |
	| 12:30 | 24h:min      | Seconds  | asdf | System Date Time Format | yyyy/MM/dd hh:mm:ss tt |
	And the debug output as 
	| Result       |
	| [[result]] = |

Scenario: Date and Time with output format - 12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ
	Given I have a date "2013/12/05 04:18:51 PM"
	And the input format as ""
	And I selected Add time as "None" with a value of 0
	And the output format as "12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ"
	When the datetime tool is executed
	Then the datetime result should be "04:05:Thursday:A.D.:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12:Dec:49:South Africa Standard Time:49:(UTC+02:00) Harare, Pretoria"
	And the execution has "NO" error
	 And the debug inputs as  
	| Input                  | Input Format            | =                      | Add Time |   | Output Format                                                               |
	| 2013/12/05 04:18:51 PM | System Date Time Format | yyyy/MM/dd hh:mm:ss tt | None     | 0 | 12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ |
	And the debug output as 
	| Result                                                                                                            |
	| [[result]] = 04:05:Thursday:A.D.:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12 |

Scenario: Date and Time with input format - 12h:dd:DW:ERA:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ with A.D.
	Given I have a date "04:05:Thursday:A.D.:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12:Dec:49:South Africa Standard Time:49:(UTC+02:00) Harare, Pretoria"
	And the input format as "12h:dd:DW:ERA:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ"
	And I selected Add time as "None" with a value of 0
	And the output format as "yyyy/mm/dd 12h:min:ss am/pm"
	When the datetime tool is executed
	Then the datetime result should be "2013/12/05 04:18:51 PM"
	And the execution has "NO" error
	And the debug inputs as  
	| Input                                                                                                | Input Format                                                                | Add Time |   | Output Format               |
	| 04:05:Thursday:A.D.:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12 | 12h:dd:DW:ERA:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ | None     | 0 | yyyy/mm/dd 12h:min:ss am/pm |	
	And the debug output as 
	| Result                              |
	| [[result]] = 2013/12/05 04:18:51 PM |

Scenario: Date and Time with input format - 12h:dd:DW:era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ with AD
	Given I have a date "04:05:Thursday:AD:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12:Dec:49:South Africa Standard Time:49:(UTC+02:00) Harare, Pretoria"
	And the input format as "12h:dd:DW:era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ"
	And I selected Add time as "None" with a value of 0
	And the output format as "yyyy/mm/dd 12h:min:ss am/pm"
	When the datetime tool is executed
	Then the datetime result should be "2013/12/05 04:18:51 PM"
	And the execution has "NO" error
	 And the debug inputs as  
	| Input                                                                                                | Input Format                                                                | Add Time |   | Output Format               |
	| 04:05:Thursday:AD:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12:D | 12h:dd:DW:era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ | None     | 0 | yyyy/mm/dd 12h:min:ss am/pm |
	And the debug output as 
	| Result                              |
	| [[result]] = 2013/12/05 04:18:51 PM |


Scenario: Date and Time with input format - 12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ with A.D
	Given I have a date "04:05:Thursday:A.D:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12:Dec:49:South Africa Standard Time:49:(UTC+02:00) Harare, Pretoria"
	And the input format as "12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ"
	And I selected Add time as "None" with a value of 0
	And the output format as "yyyy/mm/dd 12h:min:ss am/pm"
	When the datetime tool is executed
	Then the datetime result should be "2013/12/05 04:18:51 PM"
	And the execution has "NO" error
	 And the debug inputs as  
	| Input                                                                                                | Input Format                                                                | Add Time |   | Output Format               |
	| 04:05:Thursday:A.D:12:December:18:51:0:2013:13:South Africa Standard Time:PM:16:5:4:Thursday:339:12: | 12h:dd:DW:Era:mm:MM:min:ss:sp:yyyy:yy:Z:am/pm:24h:d:dw:DW:dy:m:M:w:ZZ:w:ZZZ | None     | 0 | yyyy/mm/dd 12h:min:ss am/pm |
	And the debug output as 
	| Result                              |
	| [[result]] = 2013/12/05 04:18:51 PM |

Scenario: Calculate for a negative recordset index for Date 1
	Given I have a date "[[my(-1).date]]" 
	And the input format as "yyyy-mm-dd"
	And I selected Add time as "Weeks" with a value of 52
	And the output format as "yyyy-mm-dd"
	When the datetime tool is executed
	Then the execution has "AN" error
	 And the debug inputs as  
	| Input             | Input Format | Add Time |    | Output Format |
	| [[my(-1).date]] = | yyyy-mm-dd   | Weeks    | 52 | yyyy-mm-dd    |
	And the debug output as 
	| Result       |
	| [[result]] = |

Scenario: Calculate for a negative recordset index for Format
	Given I have a date "2013-11-29" 
	And the input format as "[[my(-1).format]]"
	And I selected Add time as "Weeks" with a value of 52
	And the output format as "yyyy-mm-dd"
	When the datetime tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Input      | Input Format        | Add Time |    | Output Format |
	| 2013-11-29 | [[my(-1).format]] = | Weeks    | 52 | yyyy-mm-dd    |
	And the debug output as 
	| Result       |
	| [[result]] = |

Scenario: Calculate for a negative recordset index for Time Value
	Given I have a date "2013-11-29" 
	And the input format as "yyyy-mm-dd"
	And I selected Add time as "Weeks" with a value of "[[my(-1).int]]"
	And the output format as "yyyy-mm-dd"
	When the datetime tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Input      | Input Format | Add Time |                  | Output Format |
	| 2013-11-29 | yyyy-mm-dd   | Weeks    | [[my(-1).int]] = | yyyy-mm-dd    |
	And the debug output as 
	| Result       |
	| [[result]] = |
	
Scenario: Calculate for a negative recordset index for Output Format
	Given I have a date "2013-11-29" 
	And the input format as "yyyy-mm-dd"
	And I selected Add time as "Weeks" with a value of 52
	And the output format as "[[my(-1).format]]"
	When the datetime tool is executed
	Then the execution has "AN" error
	And the debug inputs as  
	| Input      | Input Format | Add Time |    | Output Format       |
	| 2013-11-29 | yyyy-mm-dd   | Weeks    | 52 | [[my(-1).format]] = |
	And the debug output as 
	| Result       |
	| [[result]] = |
	
Scenario: Default outputs for dateparts not present
	Given I have a date "0" 
	And the input format as "sp"
	And I selected Add time as "None" with a value of 0
	And the output format as "yyyy-mm-dd 24hr:min:ss am/pm Era"
	When the datetime tool is executed
	Then the datetime result should be "0001-01-01 00r:00:00 AM A.D."
	And the execution has "NO" error
	And the debug inputs as  
	| Input | Input Format | Add Time |   | Output Format                    |
	| 0     | sp           | None     | 0 | yyyy-mm-dd 24hr:min:ss am/pm Era |
	And the debug output as 
	| Result                                    |
	| [[result]] = 0001-01-01 00r:00:00 AM A.D. |
