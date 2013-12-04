Feature: Decision
	In order to branch based on the data
	As Warewolf user
	I want tool that be makes a true or false (yes/no) decision based on the data

Scenario: Check if variable A equals variable B
	Given I need to take a decision on variable "[[A]]" with the value "30"
	And  I need to take a decision on variable "[[B]]" with the value "30"	
	And I want to find out if the [[A]] "IsEqual" [[B]]
	And the decision mode is "AND"
	When the decision tool is executed
	Then the decision result should be "True"

#DECISIONS TO TESTS
#Choose,
#IsError,
#IsNotError,
#IsNumeric,
#IsNotNumeric,
#IsText,
#IsNotText,
#IsAlphanumeric,
#IsNotAlphanumeric,
#IsXML,
#IsNotXML,
#IsDate,
#IsNotDate,
#IsEmail,
#IsNotEmail,
#IsRegEx,
#IsEqual,
#IsNotEqual,
#IsLessThan,
#IsLessThanOrEqual,
#IsGreaterThan,
#IsGreaterThanOrEqual,
#IsContains,
#IsEndsWith,
#IsStartsWith,
#IsBetween,
#IsBinary,
#IsNotBinary,
#IsHex,
#IsNotHex,
#IsBase64,
#IsNotBase64