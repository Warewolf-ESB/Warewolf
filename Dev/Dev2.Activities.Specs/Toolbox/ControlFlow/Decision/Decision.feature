Feature: Decision
	In order to branch based on the data
	As Warewolf user
	I want tool that be makes a true or false (yes/no) decision based on the data

Scenario: decide if variable [[A]] is alphanumeric (True)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsAlphanumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is alphanumeric (False)
	Given a decision variable "[[A]]" value "@"	
	And decide if "[[A]]" "IsAlphanumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Base64 (True)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsBase64" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Base64 (False)
	Given a decision variable "[[A]]" value "011110"	
	And decide if "[[A]]" "IsBase64"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Binary (True)
	Given a decision variable "[[A]]" value "011110"	
	And decide if "[[A]]" "IsBinary" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Binary (False)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsBinary"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is a Date (True)
	Given a decision variable "[[A]]" value "2010-01-10"	
	And decide if "[[A]]" "IsDate" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is a Date (False)
	Given a decision variable "[[A]]" value "Hello World"	
	And decide if "[[A]]" "IsDate"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is an Email (True)
	Given a decision variable "[[A]]" value "testmail@freemail.net"	
	And decide if "[[A]]" "IsEmail" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is an Email (False)
	Given a decision variable "[[A]]" value "Hello World"	
	And decide if "[[A]]" "IsEmail"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Hex (True)
	Given a decision variable "[[A]]" value "1E"	
	And decide if "[[A]]" "IsHex" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Hex (False)
	Given a decision variable "[[A]]" value "KLM"	
	And decide if "[[A]]" "IsHex"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Numeric (True)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsNumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Numeric (False)
	Given a decision variable "[[A]]" value "3R"	
	And decide if "[[A]]" "IsNumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Regex (True)
	Given a decision variable "[[A]]" value "?:[^?+*{}()[\]\\|]+"	
	And decide if "[[A]]" "IsRegEx" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Regex (False)
	Given a decision variable "[[A]]" value "787877787"	
	And decide if "[[A]]" "IsRegEx"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Text (True)
	Given a decision variable "[[A]]" value "Hello Africa"	
	And decide if "[[A]]" "IsText" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] is Text (False)
	Given a decision variable "[[A]]" value "3000"	
	And decide if "[[A]]" "IsText"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Is XML (True)
	Given a decision variable "[[A]]" value "<A></A>"	
	And decide if "[[A]]" "IsXML" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Is XML (False)
	Given a decision variable "[[A]]" value "@"	
	And decide if "[[A]]" "IsXML"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Alphanumeric (True)
	Given a decision variable "[[A]]" value "@#$"	
	And decide if "[[A]]" "IsNotAlphanumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Alphanumeric (False)
	Given a decision variable "[[A]]" value "Hello"	
	And decide if "[[A]]" "IsNotAlphanumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Base64 (True)
	Given a decision variable "[[A]]" value "011110"	
	And decide if "[[A]]" "IsNotBase64" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Base64 (False)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsNotBase64"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Binary (True)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsNotBinary" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Binary (False)
	Given a decision variable "[[A]]" value "0111100"	
	And decide if "[[A]]" "IsNotBinary"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Date (True)
	Given a decision variable "[[A]]" value "Gracious"	
	And decide if "[[A]]" "IsNotDate" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Date (False)
	Given a decision variable "[[A]]" value "2010-01-10"	
	And decide if "[[A]]" "IsNotDate"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Email (True)
	Given a decision variable "[[A]]" value "Graciuos"	
	And decide if "[[A]]" "IsNotEmail" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Email (False)
	Given a decision variable "[[A]]" value "testmail@freemail.com"	
	And decide if "[[A]]" "IsNotEmail"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Hex (True)
	Given a decision variable "[[A]]" value "0111000"	
	And decide if "[[A]]" "IsNotHex" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Hex (False)
	Given a decision variable "[[A]]" value "1E"	
	And decide if "[[A]]" "IsNotHex"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Numeric (True)
	Given a decision variable "[[A]]" value "Red sox"	
	And decide if "[[A]]" "IsNotNumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Numeric (False)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsNotNumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Regex (True)
	Given a decision variable "[[A]]" value "6"	
	And decide if "[[A]]" "NotRegEx" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Regex (False)
	Given a decision variable "[[A]]" value "?:[^?+*{}()[\]\\|]+"	
	And decide if "[[A]]" "NotRegEx"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Text (True)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsNotText" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not Text (False)
	Given a decision variable "[[A]]" value "Gracious"	
	And decide if "[[A]]" "IsNotText"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not XML (True)
	Given a decision variable "[[A]]" value "A A"	
	And decide if "[[A]]" "IsNotXML" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Not XML (False)
	Given a decision variable "[[A]]" value "<A></A>"	
	And decide if "[[A]]" "IsNotXML"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Is Between variable [[B]] and [[C]] (True)
	Given a decision variable "[[A]]" value "30"	
	And a decision variable "[[B]]" value "20"
	And a decision variable "[[C]]" value "40"	
	And check if "[[A]]" "IsBetween" "[[B]]" and "[[C]]"
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Is Not Between variable [[B]] and [[C]] (True)
	Given a decision variable "[[A]]" value "20"	
	And a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "40"		
	And check if "[[A]]" "NotBetween" "[[B]]" and "[[C]]"
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Is Between variable [[B]] and [[C]] (False)
	Given a decision variable "[[A]]" value "20"	
	And a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "40"		
	And check if "[[A]]" "IsBetween" "[[B]]" and "[[C]]"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Is Not Between variable [[B]] and [[C]] (False)
	Given a decision variable "[[A]]" value "30"	
	And a decision variable "[[B]]" value "20"
	And a decision variable "[[C]]" value "40"		
	And check if "[[A]]" "NotBetween" "[[B]]" and "[[C]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] equals variable [[B]] and [[B]] equals [[C]] Mode is AND
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "30"		
	And is "[[A]]" "IsEqual" "[[B]]"	
	And is "[[C]]" "IsEqual" "[[B]]"	
	And the decision mode is "AND"
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] equals variable [[B]] and [[B]] equals [[C]] Mode is OR
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "31"	
	And is "[[A]]" "IsEqual" "[[B]]"	
	And is "[[C]]" "IsEqual" "[[B]]"	
	And the decision mode is "OR"
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] equals variable [[B]] (True)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] equals variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] greater than variable [[B]] (True)
	Given a decision variable "[[A]]" value "40"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsGreaterThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] greater than variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsGreaterThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] less than variable [[B]] (True)
	Given a decision variable "[[A]]" value "20"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsLessThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] less than variable [[B]] (False)
	Given a decision variable "[[A]]" value "70"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsLessThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error
	
Scenario: decide if variable [[A]] not equals variable [[B]] (True)
	Given a decision variable "[[A]]" value "38"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsNotEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] not equals variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsNotEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

 Scenario: decide if variable [[A]] equal or greater than variable [[B]] (True)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsGreaterThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] equal or greater than variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsGreaterThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] equal or less than variable [[B]] (True)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsLessThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] equal or less than variable [[B]] (False)
	Given a decision variable "[[A]]" value "60"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsLessThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Starts With variable [[B]] (True)
	Given a decision variable "[[A]]" value "Hello World"
	And  a decision variable "[[B]]" value "Hello"
	And is "[[A]]" "IsStartsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Starts With variable [[B]] (False)
	Given a decision variable "[[A]]" value "Hello Africa"
	And  a decision variable "[[B]]" value "World"
	And is "[[A]]" "IsStartsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Ends With variable [[B]] (True)
	Given a decision variable "[[A]]" value "Hello Africa"
	And  a decision variable "[[B]]" value "Africa"
	And is "[[A]]" "IsEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Ends With variable [[B]] (False)
	Given a decision variable "[[A]]" value "Hello World"
	And  a decision variable "[[B]]" value "Africa"
	And is "[[A]]" "IsEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Contains variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "fantastic"
	And is "[[A]]" "IsContains" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Contains variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "terrible"
	And is "[[A]]" "IsContains" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Doesn't Starts With variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "country"
	And is "[[A]]" "DoesntStartWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Doesn't Starts With variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "South"
	And is "[[A]]" "DoesntStartWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Doesn't Ends With variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "continent"
	And is "[[A]]" "NotEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Doesn't Ends With variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "country"
	And is "[[A]]" "NotEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Doesn't Contains variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "Nile"
	And is "[[A]]" "NotContains" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the decision execution has "NO" error

Scenario: decide if variable [[A]] Doesn't Contains variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "Africa"
	And is "[[A]]" "NotContains" "[[B]]"
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "NO" error
		
Scenario: decide if There Is An Error (True)
	Given "An" error occurred
	And I want to check "IsError"
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: decide if There Is An Error (False)
	Given "No" error occurred
	And I want to check "IsError"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: decide if There Is No Error (True)
	Given "No" error occurred
	And I want to check "IsNotError"
	When the decision tool is executed
	Then the decision result should be "True"

Scenario: decide if There Is No Error (False)
	Given "An" error occurred
	And I want to check "IsNotError"
	When the decision tool is executed
	Then the decision result should be "False"

Scenario: Decide using a negative recordset index
	Given is "[[my(-1).var]]" "IsContains" ""
	When the decision tool is executed
	Then the decision result should be "False"
	And the decision execution has "AN" error
