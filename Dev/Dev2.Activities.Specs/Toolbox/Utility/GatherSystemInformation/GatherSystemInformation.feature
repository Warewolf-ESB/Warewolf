Feature: SystemInformation
	In order to use system information
	As a warewolf user
	I want a tool that I retrieve system info


Scenario: Assign a system date time into a scalar
	Given I have a variable "[[aa]]" and I selected "Date & Time"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[aa]]" is a valid "DateTime"

