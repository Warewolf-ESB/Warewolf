Feature: BaseConversion
	In order to convert base encoding types
	As a Warewolf user
	I want a tool that converts data from one base econding to another

Scenario: Convert from text to text 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error

Scenario: Convert from text to binary 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error

Scenario: Convert from text to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"
	And the execution has "NO" error

Scenario: Convert from text to base64 
	Given I have a convert variable "[[var]]" with a value of "AA"
	And I convert a variable "[[var]]" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error

Scenario: Convert from binary to binary 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error

Scenario: Convert from binary to text 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error

Scenario: Convert from binary to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"
	And the execution has "NO" error

Scenario: Convert from binary to base64 
	Given I have a convert variable "[[var]]" with a value of "0100000101000001"
	And I convert a variable "[[var]]" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error

Scenario: Convert from hexadecimal to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"	
	And the execution has "NO" error

Scenario: Convert from hexadecimal to text 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error

Scenario: Convert from hexadecimal to binary 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error

Scenario: Convert from hexadecimal to base64 
	Given I have a convert variable "[[var]]" with a value of "0x4141"
	And I convert a variable "[[var]]" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error

Scenario: Convert from base64 to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"	
	And the execution has "NO" error

Scenario: Convert from base64 to text 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And the execution has "NO" error

Scenario: Convert from base64 to binary 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And the execution has "NO" error

Scenario: Convert from base64 to base64 
	Given I have a convert variable "[[var]]" with a value of "QUE="
	And I convert a variable "[[var]]" from type "Base 64" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And the execution has "NO" error

Scenario: Convert blank from text to binary 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "NO" error

Scenario: Convert blank from text to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from text to base64 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from binary to text 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from binary to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from binary to base64 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from hexadecimal to text 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from hexadecimal to binary 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from hexadecimal to base64 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from base64 to hexadecimal 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "NO" error

Scenario: Convert blank from base64 to text 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert blank from base64 to binary 
	Given I have a convert variable "[[var]]" with a value of ""
	And I convert a variable "[[var]]" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "NO" error

Scenario: Convert negative recordset index from text to binary 
	Given I have a convert variable "[[my().var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "AN" error

Scenario: Convert negative recordset index from text to hexadecimal 
	Given I have a convert variable "[[my().var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from text to base64 
	Given I have a convert variable "[[my().var]]" with a value of "AA"
	And I convert a variable "[[my(-1).var]]" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from binary to text 
	Given I have a convert variable "[[my().var]]" with a value of "0100000101000001"
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from binary to hexadecimal 
	Given I have a convert variable "[[my().var]]" with a value of "0100000101000001""
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from binary to base64 
	Given I have a convert variable "[[my().var]]" with a value of "0100000101000001"
	And I convert a variable "[[my(-1).var]]" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from hexadecimal to text 
	Given I have a convert variable "[[my().var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from hexadecimal to binary 
	Given I have a convert variable "[[my().var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from hexadecimal to base64 
	Given I have a convert variable "[[my().var]]" with a value of "0x4141"
	And I convert a variable "[[my(-1).var]]" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from base64 to hexadecimal 
	Given I have a convert variable "[[my().var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""	
	And the execution has "AN" error

Scenario: Convert negative recordset index from base64 to text 
	Given I have a convert variable "[[my().var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error

Scenario: Convert negative recordset index from base64 to binary 
	Given I have a convert variable "[[my().var]]" with a value of "QUE="
	And I convert a variable "[[my(-1).var]]" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And the execution has "AN" error
