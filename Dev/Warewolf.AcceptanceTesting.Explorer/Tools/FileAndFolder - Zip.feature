Feature: FileAndFolder-Zip
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Zip
Scenario: Zip tool Small View
       Given I have Zip Small View on design surface
       Then Zip small view has
       | File or Folder | Destination | Result |
       |                |             |        |

Scenario: Zip tool Large View
       Given I have Zip Large View on design surface
       Then Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Archive Password | Compression      | Result |
       |                |                 |                 |             |               |               |                  | Normal (Default) |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"

Scenario: Zip tool Small View water marks
       Given I have Zip Small View on design surface
       Then Zip small view watermarks are
       | File or Folder | Destination     | Result      |
       | [[PathToZip]]  | [[ZipFileName]] | [[Success]] |

Scenario: Zip tool Large View Water marks
       Given I have Zip Large View on design surface
       Then Zip Large View watermarks are
       | File or Folder | Source Username | Source Password | Destination     | Dest Username | Dest Password | Archive Password | Compression      | Result |
       | [[PathToZip]]  | [[Username]]    |                 | [[ZipFileName]] | [[Username]]  |               |                  | Normal (Default) |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service        |
       | [[Error().Message]]        | http://lcl:3142/services/err |    
       And End this workflow is "Unselected"
       And Done button is "Visible"

Scenario: Zip Large View is validating when clicking on done with blank fields
       Given I have Zip Large View on design surface
       And "File or Folder" is focused
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       |                |                 |                 |             |               |               |                 | Normal (Default) |        |
       And If it exists Overwrite is "Unselected"      
       When I click "Done"
       Then Validation message is thrown
       And Zip Small View is "Not Visible"

Scenario: Zip tool Large View to small view persisting data correctly
       Given I have Zip Large View on design surface
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | C:\Test.zip    |                 |                 | D:\         |               |               |                 | Normal (Default) |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       When I click "Done"
       Then Validation message is not thrown
       And Zip Small View is "Visible"
       And Zip small view has
       | File or Folder | Destination | Result  |
       | C:\Test.zip    | D:\         | [[Zip]] |

Scenario: Zip  Compression is not changing by opening and closing large view
       Given I have Zip Large View on design surface
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Archive Password | Compression   | Result |
       | C:\Test.zip    |                 |                 | D:\         |               |               |                  | <Compression> |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Selected"
       And Done button is "Visible"
       When I click "Done"
       Then Validation message is not thrown
       And Zip Small View is "Visible"
       When I open Zip largeview
	   Then Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Archive Password | Compression   | Result |
       | C:\Test.zip    |                 |                 | D:\         |               |               |                  | <Compression> |        |
Examples: 
     | No | Compression            |
     | 1  | Normal (Default)       |
     | 2  | None (No Compression)  |
     | 3  | Partial (Best Speed)   |
     | 4  | Max (Best Compression) |


Scenario: After correcting incorrect variable done button is closing large view
       Given I have Zip Large View on design surface
       When Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Archive Password | Compression      | Result |
       | C:\[[a]        |                 |                 | D:\         |               |               |                  | Normal (Default) |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown
       And Zip Small View is "Not Visible"
       When I edit Zip Large View
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | C:\[[a]]       |                 |                 | D:\         |               |               |                 | Normal (Default) |        |
       When I click on "Done"
       Then Validation message is not thrown
       And Zip Small View is "Visible"
       And Zip small view as
       | File or Folder | Destination | Result    |
       | C:\[[a]] | D:\         | [[Zip]] |


Scenario: Close large view is closing large view without validating
       Given I have Zip Large View on design surface
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password | Archive Password | Compression      | Result |
       | C:\[[a]        |                 |                 | D:\         |               |               |                  | Normal (Default) |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown
       And Zip Small View is "Not Visible"
       When collapse "Zip" large view
       Then Zip Small View is "Visible"
       And Zip small view as
       | File or Folder | Destination | Result    |
       | C:\[[a]  | D:\         | [[Zip]] |


Scenario Outline: Zip Large View is validating incorrect source path
       Given I have Zip Large View on design surface
       And "File or Folder" is focused
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | '<SPath>'      |                 |                 | D:\         |               |               |                 | Normal (Default) |        |
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


Scenario Outline: Zip Large View is validating incorrect Destination path
       Given I have Zip Large View on design surface
       And "File or Folder" is focused
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | C:/Test.txt    |                 |                 | '<DPath>'   |               |               |                 | Normal (Default) |        |
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



Scenario Outline: Zip Large View is validating incorrect variable in source username field
       Given I have Zip Large View on design surface
       And "File or Folder" is focused
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | C:/Test.txt    | '<Variable>'    | sas             | D:/         |               |               |                 | Normal (Default) |        |
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




Scenario Outline: Zip Large View is validating incorrect variable in Destination username field
       Given I have Zip Large View on design surface
       And "File or Folder" is focused
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | C:/Test.txt    |                 |                 | D:/         | '<Username>'  | abc           |                 | Normal (Default) |        |
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



Scenario Outline: Zip Large View is validating incorrect variable in Result field
       Given I have Zip Large View on design surface
       And "File or Folder" is focused
       And Zip Large View has
       | File or Folder | Source Username | Source Password | Destination | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | C:/Test.txt    |                 |                 | D:/         |               |               |                 | Normal (Default) |        |
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


Scenario Outline: Zip On error fields incorrect variables are validating
       Given I have Zip Large View on design surface
       And Zip Large View with water marks has
       | File or Folder | Source Username | Source Password | Destination   | Dest Username | Dest Password |Archive Password | Compression      | Result |
       | [[PathToZip]]  | [[Username]]    |                 | [[ZipToPath]] | [[Username]]  |               |                 | Normal (Default) |        |
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





















