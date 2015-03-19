Feature: FileAndFolder-Unzip
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Unzip
Scenario: Unzip tool Small View
       Given I have Unzip Small View on design surface
       Then Unzip small view has
       | Zip Name | Destination | Result |
       |          |             |        |

Scenario: Unzip tool Large View
       Given I have Unzip Large View on design surface
       Then Unzip Large View has
       | Zip Name | Source Username | Source Password | Destination | Dest Username | Dest Password | Result | Archive Password |
       |          |                 |                 |             |               |               |        |                  |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"

Scenario: Unzip tool Small View water marks
       Given I have Unzip Small View on design surface
       Then Unzip small view watermarks are
       | Zip Name        | Destination     | Result      |
       | [[PathToUnzip]] | [[UnzipToPath]] | [[Success]] |

Scenario: Unzip tool Large View Water marks
       Given I have Unzip Large View on design surface
       Then Unzip Large View watermarks are
       | Zip Name        | Source Username | Source Password | Destination     | Dest Username | Dest Password | Result      |Archive Password |
       | [[PathToUnzip]] | [[Username]]    |                 | [[UnzipToPath]] | [[Username]]  |               | [[Success]] |                 |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service        |
       | [[Error().Message]]        | http://lcl:3142/services/err |    
       And End this workflow is "Unselected"
       And Done button is "Visible"

Scenario: Unzip Large View is validating when clicking on done with blank fields
       Given I have Unzip Large View on design surface
       And "Zip Name" is focused
       And Unzip Large View has
       | Zip Name | Source Username | Source Password | Destination | Dest Username | Dest Password | Result |Archive Password |
       |          |                 |                 |             |               |               |        |                 |
       And If it exists Overwrite is "Unselected"      
       When I click "Done"
       Then Validation message is thrown
       And Unzip Small View is "Not Visible"

Scenario: Unzip tool Large View to small view persisting data correctly
       Given I have Unzip Large View on design surface
       And Unzip Large View has
       | Zip Name    | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    | Archive Password |
       | C:\Test.zip |                 |                 | D:\         |               |               | [[Unzip]] |                 |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       When I click "Done"
       Then Validation message is not thrown
       And Unzip Small View is "Visible"
       And Unzip small view has
       | Zip Name    | Destination | Result    |
       | C:\Test.zip | D:\         | [[Unzip]] |


Scenario: After correcting incorrect variable done button is closing large view
       Given I have Unzip Large View on design surface
       When Unzip Large View has
       | Zip Name | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    |Archive Password |
       | C:\[[a]  |                 |                 | D:\         |               |               | [[Unzip]] |                 |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown
       And Unzip Small View is "Not Visible"
       When I edit Unzip Large View
       | Zip Name | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    |Archive Password |
       | C:\[[a]] |                 |                 | D:\         |               |               | [[Unzip]] |                 |
       When I click on "Done"
       Then Validation message is not thrown
       And Unzip Small View is "Visible"
       And Unzip small view as
       | Zip Name | Destination | Result    |
       | C:\[[a]] | D:\         | [[Unzip]] |


Scenario: Close large view is closing large view without validating
       Given I have Unzip Large View on design surface
       And Unzip Large View has
       | Zip Name | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    |Archive Password |
       | C:\[[a]  |                 |                 | D:\         |               |               | [[Unzip]] |                 |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown
       And Unzip Small View is "Not Visible"
       When collapse "Unzip" large view
       Then Unzip Small View is "Visible"
       And Unzip small view as
       | Zip Name | Destination | Result    |
       | C:\[[a]  | D:\         | [[Unzip]] |


Scenario Outline: Unzip Large View is validating incorrect source path
       Given I have Unzip Large View on design surface
       And "Zip Name" is focused
       And Unzip Large View has
       | Zip Name  | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    |Archive Password |
       | '<SPath>' |                 |                 | D:\         |               |               | [[Unzip]] |                 |
       And If it exists Overwrite is "Unselected"      
       When I click on "Done"
       Then Validation message is thrown '<Validation>'
Examples: 
    | No | SPath             | Validation |
    | 1  | D:\Test.txt       | False      |
    | 2  | [[a]]:\Test.txt   | False      |
    | 3  | D:[[rec().a]].zip | False      |
    | 4  | [[rec(1).a]].txt  | False      |
    | 5  | abc               | True       |
    | 6  | 123               | True       |
    | 7  | \\abc             | False      |
    | 8  | [[a]]             | False      |
    | 9  | [[a#]]            | True       |
    | 10 | [[rec(@).a]]      | True       |
    | 11 | [[[rec().a]]      | True       |


Scenario Outline: Unzip Large View is validating incorrect Destination path
       Given I have Unzip Large View on design surface
       And "Zip Name" is focused
       And Unzip Large View has
       | Zip Name    | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    |Archive Password |
       | C:/Test.txt |                 |                 | '<DPath>'   |               |               | [[Unzip]] |                 |
       And If it exists Overwrite is "Unselected"      
       When I click on "Done"
       Then Validation message is thrown '<Validation>'
Examples: 
    | No | DPath             | Validation |
    | 1  | D:\Test.txt       | False      |
    | 2  | [[a]]:\Test.zip   | False      |
    | 3  | D:[[rec().a]].txt | False      |
    | 4  | [[rec(1).a]].txt  | False      |
    | 5  | abc               | True       |
    | 6  | 123               | True       |
    | 7  | \\abc             | False      |
    | 8  | [[a]]             | False      |
    | 9  | [[a#]]            | True       |
    | 10 | [[rec(@).a]]      | True       |
    | 11 | [[[rec().a]]      | True       |



Scenario Outline: Unzip Large View is validating incorrect variable in source username field
       Given I have Unzip Large View on design surface
       And "Zip Name" is focused
       And Unzip Large View has
       | Zip Name    | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    |Archive Password |
       | C:/Test.txt | '<Variable>'    | sas             | D:/         |               |               | [[Unzip]] |                 |
       And If it exists Overwrite is "Unselected"      
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




Scenario Outline: Unzip Large View is validating incorrect variable in Destination username field
       Given I have Unzip Large View on design surface
       And "Zip Name" is focused
       And Unzip Large View has
       | Zip Name    | Source Username | Source Password | Destination | Dest Username | Dest Password | Result    |Archive Password |
       | C:/Test.txt |                 |                 | D:/         | '<Username>'  | abc           | [[Unzip]] |                 |
       And If it exists Overwrite is "Unselected"      
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



Scenario Outline: Unzip Large View is validating incorrect variable in Result field
       Given I have Unzip Large View on design surface
       And "Zip Name" is focused
       And Unzip Large View has
       | Zip Name    | Source Username | Source Password | Destination | Dest Username | Dest Password | Result     |Archive Password |
       | C:/Test.txt |                 |                 | D:/         |               |               | '<Result>' |                 |
       And If it exists Overwrite is "Unselected"      
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


Scenario Outline: Unzip On error fields incorrect variables are validating
       Given I have Unzip Large View on design surface
       And Unzip Large View with water marks has
       | Zip Name        | Source Username | Source Password | Destination     | Dest Username | Dest Password | Result      |Archive Password |
       | [[PathToUnzip]] | [[Username]]    |                 | [[UnzipToPath]] | [[Username]]  |               | [[Success]] |                 |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       | '<Variable>'               | '<Variable>'          |
       And End this workflow is "Unselected"
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





















