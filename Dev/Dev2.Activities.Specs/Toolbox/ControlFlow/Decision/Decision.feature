Feature: Decision
	In order to branch based on the data
	As Warewolf user
	I want tool that be makes a true or false (yes/no) decision based on the data

Scenario: decide if variable [[A]] is alphanumeric (True)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsAlphanumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] is alphanumeric (False)
	Given a decision variable "[[A]]" value "@"	
	And decide if "[[A]]" "IsAlphanumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|           | Statement | Require All decisions to be True |
	| [[A]] = @ | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] is Base64 (True)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsBase64" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                  | Statement | Require All decisions to be True |
	| [[A]] = dHNoZXBv | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] is Base64 (False)
	Given a decision variable "[[A]]" value "011110"	
	And decide if "[[A]]" "IsBase64"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                | Statement | Require All decisions to be True |
	| [[A]] = 011110 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] is Binary (True)
	Given a decision variable "[[A]]" value "011110"	
	And decide if "[[A]]" "IsBinary" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                | Statement | Require All decisions to be True |
	| [[A]] = 011110 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] is Binary (False)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsBinary"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                  | Statement | Require All decisions to be True |
	| [[A]] = dHNoZXBv | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] is a Date (True)
	Given a decision variable "[[A]]" value "2010-01-10"	
	And decide if "[[A]]" "IsDate" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                    | Statement | Require All decisions to be True |
	| [[A]] = 2010-01-10 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] is a Date (False)
	Given a decision variable "[[A]]" value "Hello World"	
	And decide if "[[A]]" "IsDate"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                     | Statement | Require All decisions to be True |
	| [[A]] = Hello World | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] is an Email (True)
	Given a decision variable "[[A]]" value "testmail@freemail.net"	
	And decide if "[[A]]" "IsEmail" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                               | Statement | Require All decisions to be True |
	| [[A]] = testmail@freemail.net | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] is an Email (False)
	Given a decision variable "[[A]]" value "Hello World"	
	And decide if "[[A]]" "IsEmail"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                     | Statement | Require All decisions to be True |
	| [[A]] = Hello World | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] is Hex (True)
	Given a decision variable "[[A]]" value "1E"	
	And decide if "[[A]]" "IsHex" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 1E | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] is Hex (False)
	Given a decision variable "[[A]]" value "KLM"	
	And decide if "[[A]]" "IsHex"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
    And the debug inputs as  
	|             | Statement | Require All decisions to be True |
	| [[A]] = KLM | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] is Numeric (True)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsNumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] is Numeric (False)
	Given a decision variable "[[A]]" value "3R"	
	And decide if "[[A]]" "IsNumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
   And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 3R | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

#Note that the debug comes out incorrectly beacuse the regex requires to be escaped but it fails the evaluation wehn escaped
Scenario: decide if variable [[A]] is Regex (True)
	Given a decision variable "[[A]]" value "tshepo.ntlhokoa@dev2.co.za"		
	And is "[[A]]" "IsRegEx" "^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                                    | Statement | Require All decisions to be True |
	| [[A]] = tshepo.ntlhokoa@dev2.co.za | String    | YES                              |
	And the debug output as 
	|         |
	| String |

#Note that the debug comes out incorrectly beacuse the regex requires to be escaped but it fails the evaluation wehn escaped
Scenario: decide if variable [[A]] is Regex (False)
	Given a decision variable "[[A]]" value "787877787"		
	And is "[[A]]" "IsRegEx" "^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                   | Statement | Require All decisions to be True |
	| [[A]] = 787877787 | String    | YES                              |
	And the debug output as 
	|         |
	| String |



Scenario: decide if variable [[A]] is Text (True)
	Given a decision variable "[[A]]" value "Hello Africa"	
	And decide if "[[A]]" "IsText" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                      | Statement | Require All decisions to be True |
	| [[A]] = Hello Africa | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] is Text (False)
	Given a decision variable "[[A]]" value "3000"	
	And decide if "[[A]]" "IsText"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|              | Statement | Require All decisions to be True |
	| [[A]] = 3000 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Is XML (True)
	Given a decision variable "[[A]]" value "<A></A>"	
	And decide if "[[A]]" "IsXML" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                 | Statement | Require All decisions to be True |
	| [[A]] = <A></A> | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Is XML (False)
	Given a decision variable "[[A]]" value "@"	
	And decide if "[[A]]" "IsXML"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|           | Statement | Require All decisions to be True |
	| [[A]] = @ | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Not Alphanumeric (True)
	Given a decision variable "[[A]]" value "@#$"	
	And decide if "[[A]]" "IsNotAlphanumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|             | Statement | Require All decisions to be True |
	| [[A]] = @#$ | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Not Alphanumeric (False)
	Given a decision variable "[[A]]" value "Hello"	
	And decide if "[[A]]" "IsNotAlphanumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|               | Statement | Require All decisions to be True |
	| [[A]] = Hello | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Not Base64 (True)
	Given a decision variable "[[A]]" value "011110"	
	And decide if "[[A]]" "IsNotBase64" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                | Statement | Require All decisions to be True |
	| [[A]] = 011110 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] Not Base64 (False)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsNotBase64"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                  | Statement | Require All decisions to be True |
	| [[A]] = dHNoZXBv | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Not Binary (True)
	Given a decision variable "[[A]]" value "dHNoZXBv"	
	And decide if "[[A]]" "IsNotBinary" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                  | Statement | Require All decisions to be True |
	| [[A]] = dHNoZXBv | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Not Binary (False)
	Given a decision variable "[[A]]" value "0111100"	
	And decide if "[[A]]" "IsNotBinary"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                 | Statement | Require All decisions to be True |
	| [[A]] = 0111100 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Not Date (True)
	Given a decision variable "[[A]]" value "Gracious"	
	And decide if "[[A]]" "IsNotDate" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                  | Statement | Require All decisions to be True |
	| [[A]] = Gracious | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Not Date (False)
	Given a decision variable "[[A]]" value "2010-01-10"	
	And decide if "[[A]]" "IsNotDate"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                    | Statement | Require All decisions to be True |
	| [[A]] = 2010-01-10 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] Not Email (True)
	Given a decision variable "[[A]]" value "Graciuos"	
	And decide if "[[A]]" "IsNotEmail" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                  | Statement | Require All decisions to be True |
	| [[A]] = Graciuos | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] Not Email (False)
	Given a decision variable "[[A]]" value "testmail@freemail.com"	
	And decide if "[[A]]" "IsNotEmail"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                               | Statement | Require All decisions to be True |
	| [[A]] = testmail@freemail.com | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] Not Hex (True)
	Given a decision variable "[[A]]" value "0111000"	
	And decide if "[[A]]" "IsNotHex" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                 | Statement | Require All decisions to be True |
	| [[A]] = 0111000 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] Not Hex (False)
	Given a decision variable "[[A]]" value "1E"	
	And decide if "[[A]]" "IsNotHex"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 1E | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] Not Numeric (True)
	Given a decision variable "[[A]]" value "Red sox"	
	And decide if "[[A]]" "IsNotNumeric" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                 | Statement | Require All decisions to be True |
	| [[A]] = Red sox | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Not Numeric (False)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsNotNumeric"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Not Regex (True)
	Given a decision variable "[[A]]" value "6"		
	And is "[[A]]" "NotRegEx" "^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|           | Statement | Require All decisions to be True |
	| [[A]] = 6 | String    | YES                              |
	And the debug output as 
	|         |
	| String |

Scenario: decide if variable [[A]] Not Regex (False)
	Given a decision variable "[[A]]" value "tshepo.ntlhokoa@dev2.co.za"		
	And is "[[A]]" "NotRegEx" "^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                                    | Statement | Require All decisions to be True |
	| [[A]] = tshepo.ntlhokoa@dev2.co.za | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Not Text (True)
	Given a decision variable "[[A]]" value "30"	
	And decide if "[[A]]" "IsNotText" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|           | Statement | Require All decisions to be True |
	| [[A]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] Not Text (False)
	Given a decision variable "[[A]]" value "Gracious"	
	And decide if "[[A]]" "IsNotText"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                  | Statement | Require All decisions to be True |
	| [[A]] = Gracious | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] Not XML (True)
	Given a decision variable "[[A]]" value "A A"	
	And decide if "[[A]]" "IsNotXML" 
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|             | Statement | Require All decisions to be True |
	| [[A]] = A A | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] Not XML (False)
	Given a decision variable "[[A]]" value "<A></A>"	
	And decide if "[[A]]" "IsNotXML"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                 | Statement | Require All decisions to be True |
	| [[A]] = <A></A> | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Is Between variable [[B]] and [[C]] (True)
	Given a decision variable "[[A]]" value "30"	
	And a decision variable "[[B]]" value "20"
	And a decision variable "[[C]]" value "40"	
	And check if "[[A]]" "IsBetween" "[[B]]" and "[[C]]"
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 20 |           |                                  |
	| [[C]] = 40 |           |                                  |
	|            | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Is Not Between variable [[B]] and [[C]] (True)
	Given a decision variable "[[A]]" value "20"	
	And a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "40"		
	And check if "[[A]]" "NotBetween" "[[B]]" and "[[C]]"
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 20 |           |                                  |
	| [[B]] = 30 |           |                                  |
	| [[C]] = 40 |           |                                  |
	|            | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Is Between variable [[B]] and [[C]] (False)
	Given a decision variable "[[A]]" value "20"	
	And a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "40"		
	And check if "[[A]]" "IsBetween" "[[B]]" and "[[C]]"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 20 |           |                                  |
	| [[B]] = 30 |           |                                  |
	| [[C]] = 40 |           |                                  |
	|            | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] Is Not Between variable [[B]] and [[C]] (False)
	Given a decision variable "[[A]]" value "30"	
	And a decision variable "[[B]]" value "20"
	And a decision variable "[[C]]" value "40"		
	And check if "[[A]]" "NotBetween" "[[B]]" and "[[C]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 20 |           |                                  |
	| [[C]] = 40 |           |                                  |
	|            | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] equals variable [[B]] and [[B]] equals [[C]] Mode is AND
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "30"		
	And is "[[A]]" "IsEqual" "[[B]]"	
	And is "[[C]]" "IsEqual" "[[B]]"	
	And the decision mode is "AND"
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 30 |           |                                  |
	| [[C]] = 30 |           |                                  |
	|            | String    | YES                              |
	And the debug output as 
	|         |
	| YES     |

Scenario: decide if variable [[A]] equals variable [[B]] and [[B]] equals [[C]] Mode is OR
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And a decision variable "[[C]]" value "31"	
	And is "[[A]]" "IsEqual" "[[B]]"	
	And is "[[C]]" "IsEqual" "[[B]]"	
	And the decision mode is "OR"
	When the decision tool is executed
	Then the decision result should be "true"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 30 |           |                                  |
	| [[C]] = 31 |           |                                  |
	|            | String    | NO                               |
	And the debug output as 
	|         |
	| YES     |

Scenario: decide if variable [[A]] equals variable [[B]] (True)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] equals variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 40 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] greater than variable [[B]] (True)
	Given a decision variable "[[A]]" value "40"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsGreaterThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 40 |           |                                  |
	| [[B]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] greater than variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsGreaterThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 40 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] less than variable [[B]] (True)
	Given a decision variable "[[A]]" value "20"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsLessThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 20 |           |                                  |
	| [[B]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] less than variable [[B]] (False)
	Given a decision variable "[[A]]" value "70"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsLessThan" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 70 |           |                                  |
	| [[B]] = 40 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |
	
Scenario: decide if variable [[A]] not equals variable [[B]] (True)
	Given a decision variable "[[A]]" value "38"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsNotEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 38 |           |                                  |
	| [[B]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |
	

Scenario: decide if variable [[A]] not equals variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsNotEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

 Scenario: decide if variable [[A]] equal or greater than variable [[B]] (True)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsGreaterThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] equal or greater than variable [[B]] (False)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsGreaterThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error	
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 40 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |

Scenario: decide if variable [[A]] equal or less than variable [[B]] (True)
	Given a decision variable "[[A]]" value "30"
	And  a decision variable "[[B]]" value "30"
	And is "[[A]]" "IsLessThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 30 |           |                                  |
	| [[B]] = 30 | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] equal or less than variable [[B]] (False)
	Given a decision variable "[[A]]" value "60"
	And  a decision variable "[[B]]" value "40"
	And is "[[A]]" "IsLessThanOrEqual" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|            | Statement | Require All decisions to be True |
	| [[A]] = 60 |           |                                  |
	| [[B]] = 40 | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] Starts With variable [[B]] (True)
	Given a decision variable "[[A]]" value "Hello World"
	And  a decision variable "[[B]]" value "Hello"
	And is "[[A]]" "IsStartsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                     | Statement | Require All decisions to be True |
	| [[A]] = Hello World |           |                                  |
	| [[B]] = Hello       | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |


Scenario: decide if variable [[A]] Starts With variable [[B]] (False)
	Given a decision variable "[[A]]" value "Hello Africa"
	And  a decision variable "[[B]]" value "World"
	And is "[[A]]" "IsStartsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                      | Statement | Require All decisions to be True |
	| [[A]] = Hello Africa |           |                                  |
	| [[B]] = World        | String    | YES                              |
	And the debug output as 
	|         |
	| NO     |


Scenario: decide if variable [[A]] Ends With variable [[B]] (True)
	Given a decision variable "[[A]]" value "Hello Africa"
	And  a decision variable "[[B]]" value "Africa"
	And is "[[A]]" "IsEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                      | Statement | Require All decisions to be True |
	| [[A]] = Hello Africa |           |                                  |
	| [[B]] = Africa       | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if variable [[A]] Ends With variable [[B]] (False)
	Given a decision variable "[[A]]" value "Hello World"
	And  a decision variable "[[B]]" value "Africa"
	And is "[[A]]" "IsEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                     | Statement | Require All decisions to be True |
	| [[A]] = Hello World |           |                                  |
	| [[B]] = Africa      | String    | YES                              |
	And the debug output as 
	|    |
	| NO |


Scenario: decide if variable [[A]] Contains variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "fantastic"
	And is "[[A]]" "IsContains" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = fantastic                           | String    | YES                              |
	And the debug output as 
	|     |
	| YES |


Scenario: decide if variable [[A]] Contains variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "terrible"
	And is "[[A]]" "IsContains" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = terrible                            | String    | YES                              |
	And the debug output as 
	|    |
	| NO |


Scenario: decide if variable [[A]] Doesn't Starts With variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "country"
	And is "[[A]]" "NotStartsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = country                             | String    | YES                              |
	And the debug output as 
	|     |
	| YES |


Scenario: decide if variable [[A]] Doesn't Starts With variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "South"
	And is "[[A]]" "NotStartsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = South                               | String    | YES                              |
	And the debug output as 
	|    |
	| NO |

Scenario: decide if variable [[A]] Doesn't Ends With variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "continent"
	And is "[[A]]" "NotEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = continent                           | String    | YES                              |
	And the debug output as 
	|     |
	| YES |

Scenario: decide if variable [[A]] Doesn't Ends With variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "country"
	And is "[[A]]" "NotEndsWith" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = country                             | String    | YES                              |
	And the debug output as 
	|    |
	| NO |

Scenario: decide if variable [[A]] Doesn't Contains variable [[B]] (True)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "Nile"
	And is "[[A]]" "NotContain" "[[B]]"	
	When the decision tool is executed
	Then the decision result should be "True"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = Nile                                | String    | YES                              |
	And the debug output as 
	|     |
	| YES |

Scenario: decide if variable [[A]] Doesn't Contains variable [[B]] (False)
	Given a decision variable "[[A]]" value "South Africa is a fantastic country"
	And  a decision variable "[[B]]" value "Africa"
	And is "[[A]]" "NotContain" "[[B]]"
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                                             | Statement | Require All decisions to be True |
	| [[A]] = South Africa is a fantastic country |           |                                  |
	| [[B]] = Africa                              | String    | YES                              |
	And the debug output as 
	|    |
	| NO |

		
Scenario: decide if There Is An Error (True)	
	Given a decision variable "[[rec(-1).row]]" value "South Africa is a fantastic country"
	And I want to check "IsError"
	When the decision tool is executed
	Then the decision result should be "True"
	And the debug inputs as  
	|  | Statement | Require All decisions to be True |
	|  | String    | YES                              |
	And the debug output as 
	|     |
	| YES |

Scenario: decide if There Is An Error (False)
	Given a decision variable "[[rec().row]]" value "South Africa is a fantastic country"
	And I want to check "IsError"
	When the decision tool is executed
	Then the decision result should be "False"
	And the debug inputs as  
	|  | Statement | Require All decisions to be True |
	|  | String    | YES                              |
	And the debug output as 
	|    |
	| NO |

Scenario: decide if There Is No Error (True)
	Given a decision variable "[[rec().row]]" value "South Africa is a fantastic country"	
	And I want to check "IsNotError"
	When the decision tool is executed
	Then the decision result should be "True"
	And the debug inputs as  
	|  | Statement | Require All decisions to be True |
	|  | String    | YES                              |
	And the debug output as 
	|         |
	| YES    |

Scenario: decide if There Is No Error (False)
	Given a decision variable "[[rec(-1).row]]" value "South Africa is a fantastic country"	
	And I want to check "IsNotError"
	When the decision tool is executed
	Then the decision result should be "False"
	Then the execution has "AN" error
	And the debug inputs as  
	|  | Statement | Require All decisions to be True |
	|  | String    | YES                              |
	And the debug output as 
	|    |
	| NO |

Scenario: decide if text with space is equal to same text with extra space (False)
	Given a decision variable "[[A]]" value "123 234"		
	And is "[[A]]" "IsEqual" "123   234"	
	When the decision tool is executed
	Then the decision result should be "False"
	And the execution has "NO" error
	And the debug inputs as  
	|                 | Statement | Require All decisions to be True |
	| [[A]] = 123 234 | String    | YES                              |
	And the debug output as 
	|    |
	| NO |