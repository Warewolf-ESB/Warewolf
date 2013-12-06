Feature: Decision
	In order to branch based on the data
	As Warewolf user
	I want tool that be makes a true or false (yes/no) decision based on the data
#
Scenario: Check if variable A equals variable B
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And a decision  variable "[[C]]" value "30"	
	And check if [[A]] "IsEqual" [[B]]	
	And check if [[C]] "IsEqual" [[B]]	
	And the decision mode is "AND"
	When the decision tool is executed
	Then the decision result should be "True"
#
#Scenario: Check if variable A equals variable B
#	Given a decision  variable "[[A]]" value "30"
#	And  a decision  variable "[[B]]" value "30"
#	And a decision  variable "[[C]]" value "30"	
#	And check if [[A]] "IsEqual" [[B]]	
#	And check if [[C]] "IsEqual" [[B]]	
#	And the decision mode is "OR"
#	When the decision tool is executed
#	Then the decision result should be "True"

Scenario: Check If There Is An Error (True)
	Given An error "occurred"
	And I want to check "There Is An Error"
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check If There Is An Error (False)
	Given An error "did not occur"
	And I want to check "There Is An Error"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check If There Is No Error (True)
	Given An error "did not occur"
	And I want to check "There Is An Error"
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check If There Is No Error (False)
	Given An error "occurred"
	And I want to check "There Is An Error"
	When the decision tool is executed
	Then the decision result should be "False"


Scenario: Check if variable A equals variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "IsEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A equals variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "IsEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"


Scenario: Check if variable A greater than variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "IsGreaterThan" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A greater than variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "IsGreaterThan" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A less than variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "IsLessThan" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A less than variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "IsLessThan" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"
	
Scenario: Check if variable A not equals variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "IsNotEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A not equals variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "IsNotEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

 Scenario: Check if variable A equal or greater than variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "IsGreaterThanOrEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A equal or greater than variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "IsGreaterThanOrEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A equal or less than variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "IsLessThanOrEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A equal or less than variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "IsLessThanOrEqual" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Starts With variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "Starts With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Starts With variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "Starts With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Ends With variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "Ends With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Ends With variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "Ends With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Contains variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "Contains" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Contains variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "Contains" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Doesn't Starts With variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "Doesn't Starts With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Doesn't Starts With variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "Doesn't Starts With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Doesn't Ends With variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "Doesn't Ends With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Doesn't Ends With variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "Doesn't Ends With" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Doesn't Contains variable B (True)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "30"
	And check if [[A]] "Doesn't Contains" [[B]]	
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Doesn't Contains variable B (False)
	Given a decision  variable "[[A]]" value "30"
	And  a decision  variable "[[B]]" value "40"
	And check if [[A]] "Doesn't Contains" [[B]]	
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Alphanumeric (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Alphanumeric" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Alphanumeric (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Alphanumeric"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Base64 (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Base64" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Base64 (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Base64"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Binary (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Binary" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Binary (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Binary"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Date (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Date" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Date (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Date"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Email (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Email" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Email (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Email"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Hex (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Hex" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Hex (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Hex"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Numeric (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Numeric" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Numeric (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Numeric"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Regex (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Regex" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Regex (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Regex"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Text (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Text" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Text (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is Text"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is XML (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is XML" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is XML (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Is XML"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Alphanumeric (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Alphanumeric" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Alphanumeric (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Alphanumeric"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Base64 (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Base64" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Base64 (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Base64"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Binary (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Binary" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Binary (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Binary"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Date (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Date" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Date (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Date"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Email (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Email" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Email (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Email"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Hex (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Hex" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Hex (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Hex"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Numeric (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Numeric" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Numeric (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Numeric"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Regex (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Regex" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Regex (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Regex"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not Text (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Text" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not Text (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not Text"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Not XML (True)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not XML" 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Not XML (False)
	Given a decision  variable "[[A]]" value "30"	
	And check if [[A]] "Not XML"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Between variable B and C (True)
	Given a decision  variable "[[A]]" value "30"	
	And a decision  variable "[[B]]" value "30"
	And a decision  variable "[[C]]" value "30"	
	And check if [[A]] "Is Between" [[B]] and [[C]] 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Not Between variable B and C (True)
	Given a decision  variable "[[A]]" value "30"	
	And a decision  variable "[[B]]" value "30"
	And a decision  variable "[[C]]" value "30"	
	And check if [[A]] "Is Not Between" [[B]] and [[C]] 
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: Check if variable A Is Between variable B and C (False)
	Given a decision  variable "[[A]]" value "30"	
	And a decision  variable "[[B]]" value "30"
	And a decision  variable "[[C]]" value "30"	
	And check if [[A]] "Is Between" [[B]] and [[C]] 
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Check if variable A Is Not Between variable B and C (False)
	Given a decision  variable "[[A]]" value "30"	
	And a decision  variable "[[B]]" value "30"
	And a decision  variable "[[C]]" value "30"	
	And check if [[A]] "Is Not Between" [[B]] and [[C]] 
	When the decision tool is executed
	Then the decision result should be "False"
	

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