Feature: BaseConversion
	In order to convert base encoding types
	As a Warewolf user
	I want a tool that converts data from one base econding to another

Scenario: Convert from text to text 
	Given I convert value "AA" from type "Text" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And there is NO error

Scenario: Convert from text to binary 
	Given I convert value "AA" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And there is NO error

Scenario: Convert from text to hexadecimal 
	Given I convert value "AA" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"
	And there is NO error

Scenario: Convert from text to base64 
	Given I convert value "AA" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And there is NO error

Scenario: Convert from binary to binary 
	Given I convert value "0100000101000001" from type "Binary" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And there is NO error

Scenario: Convert from binary to text 
	Given I convert value "0100000101000001" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And there is NO error

Scenario: Convert from binary to hexadecimal 
	Given I convert value "0100000101000001" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"
	And there is NO error

Scenario: Convert from binary to base64 
	Given I convert value "0100000101000001" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And there is NO error

Scenario: Convert from hexadecimal to hexadecimal 
	Given I convert value "0x4141" from type "Hex" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"	
	And there is NO error

Scenario: Convert from hexadecimal to text 
	Given I convert value "0x4141" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And there is NO error

Scenario: Convert from hexadecimal to binary 
	Given I convert value "0x4141" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And there is NO error

Scenario: Convert from hexadecimal to base64 
	Given I convert value "0x4141" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And there is NO error

Scenario: Convert from base64 to hexadecimal 
	Given I convert value "QUE=" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is "0x4141"	
	And there is NO error

Scenario: Convert from base64 to text 
	Given I convert value "QUE=" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is "AA"
	And there is NO error

Scenario: Convert from base64 to binary 
	Given I convert value "QUE=" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is "0100000101000001"
	And there is NO error

Scenario: Convert from base64 to base64 
	Given I convert value "QUE=" from type "Base 64" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is "QUE="
	And there is NO error

Scenario: Convert blank from text to binary 
	Given I convert value "" from type "Text" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from text to hexadecimal 
	Given I convert value "" from type "Text" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from text to base64 
	Given I convert value "" from type "Text" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from binary to text 
	Given I convert value "" from type "Binary" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from binary to hexadecimal 
	Given I convert value "" from type "Binary" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from binary to base64 
	Given I convert value "" from type "Binary" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from hexadecimal to text 
	Given I convert value "" from type "Hex" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from hexadecimal to binary 
	Given I convert value "" from type "Hex" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from hexadecimal to base64 
	Given I convert value "" from type "Hex" to type "Base 64" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from base64 to hexadecimal 
	Given I convert value "" from type "Base 64" to type "Hex" 
	When the base conversion tool is executed
	Then the result is ""	
	And there is NO error

Scenario: Convert blank from base64 to text 
	Given I convert value "" from type "Base 64" to type "Text" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error

Scenario: Convert blank from base64 to binary 
	Given I convert value "" from type "Base 64" to type "Binary" 
	When the base conversion tool is executed
	Then the result is ""
	And there is NO error
