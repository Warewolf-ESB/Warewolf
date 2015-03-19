Feature: FileAndFolder-Read Folder
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Read Folder
Scenario: Read Folder tool Small View
       Given I have Read Folder Small View on design surface
       Then Read Folder small view has
       | Directory | Result |
       |           |        |

Scenario: Read Folder tool Large View
       Given I have Read Folder Large View on design surface
       Then Read Folder Large View has
       | Directory | Username | Password | Result |
       |           |          |          |        |
	   And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |      
       And Done button is "Visible"

Scenario: Read Folder tool Small View water marks
       Given I have Read Folder Small View on design surface
       Then Read Folder small view watermarks are
       | Directory        | Result     |
       | [[FolderToRead]] | [[Result]] |

Scenario: Read Folder tool Large View Water marks
       Given I have Read Folder Large View on design surface
       Then Read Folder Large View watermarks are
       | Directory        | Username     | Password | Result     |
       | [[FolderToRead]] | [[Username]] |          | [[Result]] |
       And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"       
       And On Error box consists
       | Put error in this variable | Call this web service        |
       | [[Error().Message]]        | http://lcl:3142/services/err |     
       And Done button is "Visible"

Scenario: Removing Data in the field brings back water marks
       Given I have Read Folder Large View on design surface
       And Read Folder Large View has
       | Directory   | Username | Password | Result     |
       | C:\Test.txt |          |          | [[Result]] |
	   And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders" 
	   And On Error box consists
       | Put error in this variable | Call this web service |
       | [[a]]                      | dsf                   |      
	   When I remove data in fields
	   Then Read Folder Large View watermarks are
       | Directory        | Username     | Password | Result     |
       | [[FolderToRead]] | [[Username]] |          | [[Result]] |   
       And On Error box consists
       | Put error in this variable | Call this web service        |
       | [[Error().Message]]        | http://lcl:3142/services/err |     
       And Done button is "Visible"


Scenario: Read selected is not changing when I open and close large view
       Given I have Read Folder Large View on design surface
       And Read Folder Large View has
       | Directory   | Username | Password | Result     |
       | C:\Test.txt |          |          | [[Result]] |
	   And read selected '<File>'
       | Put error in this variable | Call this web service |
       |                            |                       |      
	   When I click "Done"
       Then Validation message is not thrown
       And Read Folder Small View is "Visible"
	   When I open Read Folder large view
	   Then Read Folder Large View has
       | Directory   | Username | Password | Result     |
       | C:\Test.txt |          |          | [[Result]] |
	   And read selected '<File>'
       | Put error in this variable | Call this web service |
       |                            |                       |      
Examples: 
       | No | File            |
       | 1  | Files           |
       | 2  | Folders         |
       | 3  | Files & Folders |


Scenario: Read Folder Large View is validating when clicking on done with blank fields
       Given I have Read Folder Large View on design surface
       And "Directory" is focused
       And Read Folder Large View has
       | Directory | Username | Password | Result |
       |           |          |          |        |
       And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"     
       When I click "Done"
       Then Validation message is thrown
       And Read Folder Small View is "Not Visible"


Scenario: After correcting incorrect variable done button is closing large view
       Given I have Read Folder Large View on design surface
       When Read Folder Large View has
        | Directory  | Username | Password | Result     |
        | C:\[[a2@]] |          |          | [[Result]] |
       And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"  
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |      
       And Done button is "Visible"
       Then Validation message is thrown
       And Read Folder Small View is "Not Visible"
       When I edit Read Folder Large View
        | Directory | Username | Password | Result     |
        | C:\[[a]]  |          |          | [[Result]] |
       When I click on "Done"
       Then Validation message is not thrown
       And Read Folder Small View is "Visible"
       And Read Folder small view as
       | Directory | Result     |
       | C:\[[a]]  | [[Result]] |


Scenario: Close large view is closing large view without validating
       Given I have Read Folder Large View on design surface
       And Read Folder Large View has
       | Directory  | Username | Password | Result     |
       | C:\[[a2@]] |          |          | [[Result]] |
       And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |      
       And Done button is "Visible"
       Then Validation message is thrown
       And Read Folder Small View is "Not Visible"
       When collapse "Read Folder" large view
       Then Read Folder Small View is "Visible"
       And Read Folder small view as
        | Directory  | Result     |
        | C:\[[a2@]] | [[Result]] |


Scenario Outline: Read Folder Large View is validating incorrect path
       Given I have Read Folder Large View on design surface
       And "Directory" is focused
       And Read Folder Large View has
       | Directory | Username | Password | Result     |
       | <SPath>   |          |          | [[Result]] |
	   And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"      
       And On Error box consists
       When I click on "Done"
       Then Validation message is thrown '<Validation>'
Examples: 
    | No | SPath             | Validation |
    | 1  | D:\Test.txt       | False      |
    | 2  | [[a]]:\Test.txt   | False      |
    | 3  | D:[[rec().a]].txt | False      |
    | 4  | [[rec(1).a]].txt  | False      |
    | 5  | abc               | True       |
    | 6  | 123               | True       |
    | 7  | \\abc             | False      |
    | 8  | [[a]]             | False      |
    | 9  | [[a#]]            | True       |
    | 10 | [[rec(@).a]]      | True       |
    | 11 | [[[rec().a]]      | True       |



Scenario Outline: Read Folder Large View is validating incorrect variable in  username field
       Given I have Read Folder Large View on design surface
       And "Directory" is focused
       And Read Folder Large View has
        | Directory   | Username   | Password | Result     |
        | D:\Test.txt | <Variable> |          | [[Result]] |
		 And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"      
       And On Error box consists    
       When I click on "Done"
       Then Validation message is thrown '<Validation>'
Examples: 
    | No | Variable          | Validation |
    | 1  | testing           | False      |
    | 2  | test@dev2         | False      |
    | 3  | test1234          | False      |
    | 4  | test12@dev2.co.za | False      |
    | 5  | test@@dev2.co.za  | True       |
    | 6  | test12@dev2,co,za | True       |
    | 7  | [[a]]@dev2        | False      |
    | 8  | [[Username]]      | False      |
    | 9  | [[User]][[name]]  | False      |
    | 10 | [[rec(@).a]]      | True       |
    | 11 | [[[rec().a]]      | True       |



Scenario Outline: Read Folder Large View is validating incorrect variable in Result field
       Given I have Read Folder Large View on design surface
       And "Directory" is focused
       And Read Folder Large View has
       | Directory   | Username | Password | Result   |
       | D:\Test.txt |          |          | <Result> | 
	    And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"      
       And On Error box consists
       When I click on "Done"
       Then Validation message is thrown '<Validation>'
Examples: 
    | No | Result           | Validation |
    | 1  | result           | False      |
    | 2  | [[result]]       | False      |
    | 3  | [[a]][[b]]       | True       |
    | 4  | [[rec([[a]]).a]] | True       |
    | 5  | [[[[a]]]]        | True       |
    | 6  | [[rec(*).a]]     | False      |
    | 7  | [[rec().a@]]     | True       |


Scenario Outline: Read Folder On error fields incorrect variables are validating
       Given I have Read Folder Large View on design surface
       And Read Folder Large View with water marks has
        And Read Folder Large View has
       | Directory   | Username | Password | Result     |
       | D:\Test.txt |          |          | [[Result]] |
	    And read selected "File"
	   And read UnSelected "Folder"
	   And read UnSelected "File & Folders"      
       And On Error box consists
       And On Error box consists
       | Put error in this variable | Call this web service |
       | '<Variable>'               | '<Variable>'          |
       
       And Done button is "Visible"
       When I click on "Done"
       Then Validation message is thrown '<Validation>'
Examples: 
    | No | Variable      | Validation |
    | 1  | [[a]]         | False      |
    | 2  | [[a]][[b]]    | False      |
    | 3  | ""            | False      |
    | 4  | [[rec().a]]   | False      |
    | 5  | [[a]]]]       | True       |
    | 6  | [[rec(**).a]] | True       |
    | 7  | [[rec()]]     | True       |





















