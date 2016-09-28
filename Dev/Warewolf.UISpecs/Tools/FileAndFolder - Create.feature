Feature: FileAndFolder-Create
	In order to be able to create files
	as a Warewolf user
	I want a tool that creates a file at a given location
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Create_Path onto a new workflow
	When I "Drag_Toolbox_Create_Onto_DesignSurface"
	Then I "Assert_Create_Exists_OnDesignSurface"

#@NeedsCreate_PathToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Create_Path Tool Small View on the Design Surface Opens Large View
	When I "Open_Create_Tool_Large_View"
	Then I "Assert_Create_Path_Large_View_Exists_OnDesignSurface"

@ignore
@Create
# Coded UI TESTS
Scenario: Create tool Small View
       Given I have Create Small View on design surface
       Then Create small view has
       | File or Folder |  Result |
       |                |         |
@ignore
Scenario: Create tool Large View
       Given I have Create Large View on design surface
       Then Create Large View has
       | File or Folder | Username | Password | Result |
       |                |          |          |        |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
@ignore
Scenario: Create tool Small View water marks
       Given I have Create Small View on design surface
       Then Create small view watermarks are
       | File or Folder   |Result      |
       | [[PathToCreate]] |[[Success]] |
@ignore
Scenario: Create tool Large View Water marks
       Given I have Create Large View on design surface
       Then Create Large View watermarks are
       | File or Folder   | Username     | Password |  Result      |
       | [[PathToCreate]] | [[Username]] |          |  [[Success]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
@ignore
Scenario: Create Large View is validating when clicking on done with blank fields
       Given I have Create Large View on design surface
       And "File or Folder" is focused
       And Create Large View has
       | File or Folder | Username | Password |Result |
       |                |          |          |       |
       And If it exists Overwrite is "Unselected"      
       When I click "Done"
       Then Validation message is thrown
       And Create Small View is "Not Visible"
@ignore
Scenario: Create tool Large View to small view persisting data correctly
       Given I have Create Large View on design surface
       And Create Large View has
       | File or Folder | Username | Password | Result            |
       | C:\Test        |          |          | [[FolderCreated]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       When I click "Done"
       Then Validation message is not thrown
       And Create Small View is "Visible"
       And Create small view has
       | File or Folder |  Result            |
       | C:\Test        |  [[FolderCreated]] |

@ignore
Scenario: After correcting incorrect variable done button is closing large view
       Given I have Create Large View on design surface
       When Create Large View has
        | File or Folder | Username | Password | Result            |
        | C:\[[a2@]]     |          |          | [[FolderCreated]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown
       And Create Small View is "Not Visible"
       When I edit Create Large View
        | File or Folder | Username | Password | Result            |
        | C:\[[a]]       |          |          | [[FolderCreated]] |
       When I click on "Done"
       Then Validation message is not thrown
       And Create Small View is "Visible"
       And Create small view as
       | File or Folder | Result            |
       | C:\[[a]]       | [[FolderCreated]] |

@ignore
Scenario: Close large view is closing large view without validating
       Given I have Create Large View on design surface
       And Create Large View has
       | File or Folder | Username | Password | Result            |
       | C:\[[a2@]]     |          |          | [[FolderCreated]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       Then Validation message is thrown
       And Create Small View is "Not Visible"
       When collapse "Create" large view
       Then Create Small View is "Visible"
       And Create small view as
        | File or Folder | Result            |
        | C:\[[a2@]]     | [[FolderCreated]] |

@ignore
Scenario Outline: Create Large View is validating incorrect path
       Given I have Create Large View on design surface
       And "File or Folder" is focused
       And Create Large View has
       | File or Folder | Username | Password | Result            |
       | <SPath>        |          |          | [[FolderCreated]] |
       And If it exists Overwrite is "Unselected"      
       When I click on "Done"
       Then Validation message is thrown "<Validation>"
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


@ignore
Scenario Outline: Create Large View is validating incorrect variable in  username field
       Given I have Create Large View on design surface
       And "File or Folder" is focused
       And Create Large View has
        | File or Folder | Username   | Password | Result            |
        | D:\Test.txt    | <Variable> |          | [[FolderCreated]] |
       And If it exists Overwrite is "Unselected"      
       When I click on "Done"
       Then Validation message is thrown "<Validation>"
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


@ignore
Scenario Outline: Create Large View is validating incorrect variable in Result field
       Given I have Create Large View on design surface
       And "File or Folder" is focused
       And Create Large View has
       | File or Folder | Username | Password | Result   |
       | D:\Test.txt    |          |          | <Result> |
       And If it exists Overwrite is "Unselected"      
       When I click on "Done"
       Then Validation message is thrown "<Validation>"
Examples: 
    | No | Result           | Validation |
    | 1  | result           | False      |
    | 2  | [[result]]       | False      |
    | 3  | [[a]][[b]]       | True       |
    | 4  | [[rec([[a]]).a]] | True       |
    | 5  | [[[[a]]]]        | True       |
    | 6  | [[rec(*).a]]     | False      |
    | 7  | [[rec().a@]]     | True       |

@ignore
Scenario Outline: Create On error fields incorrect variables are validating
       Given I have Create Large View on design surface
       And Create Large View with water marks has
        And Create Large View has
       | File or Folder | Username | Password | Result     |
       | D:\Test.txt    |          |          | [[Result]] |
       And If it exists Overwrite is "Unselected"      
       And On Error box consists
       | Put error in this variable | Call this web service |
       | "<Variable>"               | "<Variable>"          |
       And End this workflow is "Unselected"
       And Done button is "Visible"
       When I click on "Done"
       Then Validation message is thrown "<Validation>"
Examples: 
    | No | Variable      | Validation |
    | 1  | [[a]]         | False      |
    | 2  | [[a]][[b]]    | False      |
    | 3  | ""            | False      |
    | 4  | [[rec().a]]   | False      |
    | 5  | [[a]]]]       | True       |
    | 6  | [[rec(**).a]] | True       |
    | 7  | [[rec()]]     | True       |





















