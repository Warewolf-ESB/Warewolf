Feature: FileAndFolder-Read
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Read
Scenario: Read tool Small View
       Given I have Read Small View on design surface
       Then Read small view has
       | File Name | Result |
       |           |        |

Scenario: Read tool Large View
       Given I have Read Large View on design surface
       Then Read Large View has
       | File Name | Username | Password | Result |
       |           |          |          |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |      
       And Done button is "Visible"

Scenario: Read tool Small View water marks
       Given I have Read Small View on design surface
       Then Read small view watermarks are
       | File Name      | Result      |
       | [[PathToRead]] | [[Success]] |

Scenario: Read tool Large View Water marks
       Given I have Read Large View on design surface
       Then Read Large View watermarks are
       | File Name      | Username     | Password | Result      |
       | [[PathToRead]] | [[Username]] |          | [[Success]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service        |
       | [[Error().Message]]        | http://lcl:3142/services/err |     
       And Done button is "Visible"

Scenario: Removing Data in the field brings back water marks
       Given I have Read Large View on design surface
       And Read Large View has
       | File Name   | Username | Password | Result          |
       | C:\Test.txt |          |          | [[FolderReadd]] |
	   And On Error box consists
       | Put error in this variable | Call this web service |
       | [[a]]                      | dsf                   |      
	   When I remove data in fields
	   Then Read Large View watermarks are
       | File Name   | Username     | Password |  Result      |
       | [[PathToRead]] | [[Username]] |          |  [[Success]] |   
       And On Error box consists
       | Put error in this variable | Call this web service        |
       | [[Error().Message]]        | http://lcl:3142/services/err |     
       And Done button is "Visible"


Scenario: Read Large View is validating when clicking on done with blank fields
       Given I have Read Large View on design surface
       And "File Name" is focused
       And Read Large View has
       | File Name | Username | Password | Result |
       |           |          |          |        |
       And If it exists Overwrite is "Unselected"      
       When I click "Done"
       Then Validation message is thrown
       And Read Small View is "Not Visible"

Scenario: Read tool Large View to small view persisting data correctly
       Given I have Read Large View on design surface
       And Read Large View has
       | File Name   | Username | Password | Result          |
       | C:\Test.txt |          |          | [[FolderReadd]] | 
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |      
       And Done button is "Visible"
       When I click "Done"
       Then Validation message is not thrown
       And Read Small View is "Visible"
       And Read small view has
       | File Name | Result          |
       | C:\Test   | [[FolderReadd]] |


Scenario: After correcting incorrect variable done button is closing large view
       Given I have Read Large View on design surface
       When Read Large View has
        | File Name  | Username | Password | Result          |
        | C:\[[a2@]] |          |          | [[FolderReadd]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |      
       And Done button is "Visible"
       Then Validation message is thrown
       And Read Small View is "Not Visible"
       When I edit Read Large View
        | File Name | Username | Password | Result          |
        | C:\[[a]]  |          |          | [[FolderReadd]] |
       When I click on "Done"
       Then Validation message is not thrown
       And Read Small View is "Visible"
       And Read small view as
       | File Name | Result          |
       | C:\[[a]]  | [[FolderReadd]] |


Scenario: Close large view is closing large view without validating
       Given I have Read Large View on design surface
       And Read Large View has
       | File Name  | Username | Password | Result          |
       | C:\[[a2@]] |          |          | [[FolderReadd]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |      
       And Done button is "Visible"
       Then Validation message is thrown
       And Read Small View is "Not Visible"
       When collapse "Read" large view
       Then Read Small View is "Visible"
       And Read small view as
        | File Name  | Result          |
        | C:\[[a2@]] | [[FolderReadd]] |


Scenario Outline: Read Large View is validating incorrect path
       Given I have Read Large View on design surface
       And "File Name" is focused
       And Read Large View has
       | File Name | Username | Password | Result          |
       | <SPath>   |          |          | [[FolderReadd]] |
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



Scenario Outline: Read Large View is validating incorrect variable in  username field
       Given I have Read Large View on design surface
       And "File Name" is focused
       And Read Large View has
        | File Name   | Username   | Password | Result          |
        | D:\Test.txt | <Variable> |          | [[FolderReadd]] |    
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



Scenario Outline: Read Large View is validating incorrect variable in Result field
       Given I have Read Large View on design surface
       And "File Name" is focused
       And Read Large View has
       | File Name   | Username | Password | Result   |
       | D:\Test.txt |          |          | <Result> | 
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


Scenario Outline: Read On error fields incorrect variables are validating
       Given I have Read Large View on design surface
       And Read Large View with water marks has
        And Read Large View has
       | File Name | Username | Password | Result     |
       | D:\Test.txt    |          |          | [[Result]] |
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





















