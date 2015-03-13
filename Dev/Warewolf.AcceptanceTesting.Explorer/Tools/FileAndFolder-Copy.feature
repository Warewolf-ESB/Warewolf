Feature: FileAndFolder-Copy
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Copy
Scenario: Copy tool Small View
       Given I have Copy Small View on design surface
       Then Copy small view has
       | File or Folder | Destination | Result |
       |                |             |        |

Scenario: Copy tool Large View
       Given I have Copy Large View on design surface
       Then Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result |
       |                |                 |                 |             |               |               |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"

Scenario: Copy tool Small View water marks
       Given I have Copy Small View on design surface
       Then Copy small view watermarks are
       | File or Folder | Destination    | Result      |
       | [[PathToCopy]] | [[CopyToPath]] | [[Success]] |

Scenario: Copy tool Large View Water marks
       Given I have Copy Large View on design surface
       Then Copy Large View watermarks are
       | File or Folder | Source Username | Source Password | Destination    | Dest Username | Dest Password | Result      |
       | [[PathToCopy]] | [[Username]]    |                 | [[CopyToPath]] | [[Username]]  |               | [[Success]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"

Scenario: Copy Large View is validating when clicking on done with blank fields
       Given I have Copy Large View on design surface
       And "File or Folder" is focused
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result |
       |                |                 |                 |             |               |               |        |
       And If it exists Overwrite is "Unselected"      
       When I click "Done"
       Then Validation message is thrown
       And Copy Small View is "Not Visible"

Scenario: Copy tool Large View to small view persisting data correctly
       Given I have Copy Large View on design surface
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | C:\            |                 |                 | D:\         |               |               | [[copy]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       When I click "Done"
       Then Validation message is not thrown
       And Copy Small View is "Visible"
       And Copy small view has
       | File or Folder | Destination | Result   |
       | C:\            | D:\         | [[copy]] |


Scenario: After correcting incorrect variable done button is closing large view
       Given I have Copy Large View on design surface
       When Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | C:\[[a]        |                 |                 | D:\         |               |               | [[copy]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown "True"
       And Copy Small View is "Not Visible"
       When I edit Copy Large View
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | C:\[[a]]       |                 |                 | D:\         |               |               | [[copy]] |
       When I click on "Done"
       Then Validation message is thrown "False"
       And Copy Small View is "Visible"
       And Copy small view as
       | File or Folder | Destination | Result   |
       | C:\[[a]]       | D:\         | [[copy]] |


Scenario: Close large view is closing large view without validating
       Given I have Copy Large View on design surface
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | C:\[[a]        |                 |                 | D:\         |               |               | [[copy]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown "True"
       And Copy Small View is "Not Visible"
       When collapse "Copy" large view
       Then Copy Small View is "Visible"
       And Copy small view as
       | File or Folder | Destination | Result   |
       | C:\[[a]        | D:\         | [[copy]] |


Scenario Outline: Copy Large View is validating incorrect source path
       Given I have Copy Large View on design surface
       And "File or Folder" is focused
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | '<SPath>'      |                 |                 | D:\         |               |               | [[Copy]] |
       And If it exists Overwrite is "Unselected"      
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


Scenario Outline: Copy Large View is validating incorrect Destination path
       Given I have Copy Large View on design surface
       And "File or Folder" is focused
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | C:/Test.txt    |                 |                 | '<DPath>'   |               |               | [[Copy]] |
       And If it exists Overwrite is "Unselected"      
       When I click on "Done"
       Then Validation message is thrown '<Validation>'
Examples: 
    | No | DPath             | Validation |
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



Scenario Outline: Copy Large View is validating incorrect variable in source username field
       Given I have Copy Large View on design surface
       And "File or Folder" is focused
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | C:/Test.txt    | '<Variable>'    | sas             | D:/         |               |               | [[Copy]] |
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




Scenario Outline: Copy Large View is validating incorrect variable in Destination username field
       Given I have Copy Large View on design surface
       And "File or Folder" is focused
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result   |
       | C:/Test.txt    |                 |                 | D:/         | '<Username>'  | abc           | [[Copy]] |
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



Scenario Outline: Copy Large View is validating incorrect variable in Result field
       Given I have Copy Large View on design surface
       And "File or Folder" is focused
       And Copy Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Result       |
       | C:/Test.txt    |                 |                 | D:/         |               |               | '<Result>'   |
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


Scenario Outline: Copy On error fields incorrect variables are validating
       Given I have Copy Large View on design surface
       And Copy Large View with water marks has
       | File or Folder | Source Username | Source Password | Destination    | Dest Username | Dest Password | Result      |
       | [[PathToCopy]] | [[Username]]    |                 | [[CopyToPath]] | [[Username]]  |               | [[Success]] |
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





















