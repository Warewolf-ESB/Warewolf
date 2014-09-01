@Explorer
Feature: Explorer
	In order to interact with resources on my machine
	As a Warewolf user
	I want an explorer that represents a tree of items on my disk

##ONLY FOLDERS
#Scenario Outline: Add folders to explorer
#	Given I have a path '<path>'	
#	And the folder '<folderName>' exists on the server '<exists>'
#	When I add a folder with a name  '<folderName>'	
#	Then the folder path will be '<resultPath>'
#	And an error message will be '<errorMessage>'
#	Examples:
#	| No | path                       | folderName  | exists | resultPath                       | errorMessage                               |
#	| 1  | SpecFlow                   | AddedFolder | false  | SpecFlow\AddedFolder             |                                            |
#	| 2  | SpecFlow                   | Exists      | true   | SpecFlow\Exists                  | Requested folder already exists on server. |
#	| 3  | Warewolf                   | Warewolf    | false  | Warewolf\Warewolf                |                                            |
#	| 4  | Testings\Test1             | Test2       | false  | Testings\Test1\Test2             |                                            |
#	| 5  | Testings                   | Test1       | true   | Testings\Test1                   | Requested folder already exists on server. |
#	| 6  | Nesting folder\Next1\Next2 | Next3       | false  | Nesting folder\Next1\Next2\Next3 |                                            |
#	| 7  | Nesting folder             | N ext1      | false  | Nesting folder\N ext1            |                                            |
#	| 8  | Nesting folder             | Next 1      | false  | Nesting folder\Next 1            |                                            |
#	| 9  | Nesting folder             | ?*:         | false  | Nesting folder\                  | Illegal characters in path.                |
#
#Scenario Outline: Rename resources on explorer
#	Given I have a path '<path>'	
#	And the resource '<newName>' exists on the server '<exists>'
#	When I rename the resource '<resourceToRename>' to '<newName>'
#	Then the folder path will be '<resultPath>'
#	And an error message will be '<errorMessage>'
#	Examples: 
#	| No | path                       | resourceToRename | exists | newName    | resultPath                            | errorMessage                                        |
#	| 1  | SpecFlow                   | OldName          | false  | NewName    | SpecFlow\NewName                      |                                                     |
#	| 2  | SpecFlow\Exists            | MyResource       | false  | Murali     | SpecFlow\Exists\Murali                |                                                     |
#	| 3  | SpecFlow                   | Exists           | true   | Zebra      | SpecFlow\Exists                       | Cannot create a file when that file already exists. |
#	| 4  | Testings\Test1             | Wolf             | true   | Test       | Testings\Test1\Test                   |                                                     |
#	| 5  | Nesting folder             | Pack             | false  | Wolfpack   | Nesting folder\Wolfpack               |                                                     |
#	| 6  | Testings\Test1\Test2\Test3 | Explorer         | false  | Explore    | Testings\Test1\Test2\Test3\Explore    |                                                     |
#	| 7  | Testings\Test1\Test2\Test3 | New              | true   | Explore    | Testings\Test1\Test2\Test3\New        | Cannot create a file when that file already exists. |
#	| 8  | Testings\Test1\Test2\Test3 | WolfPack         | false  | Wolf  Pack | Testings\Test1\Test2\Test3\Wolf  Pack |                                                     |
#		
##ONLY FOLDERS	
#Scenario Outline: Delete a folder
#	Given I have a path '<path>'	
#	And the resource '<resourceToDelete>' exists on the server '<exists>'
#	When I delete the resource '<resourceToDelete>'
#	Then the folder path '<resultPath>' will be deleted
#	And an error message will be '<errorMessage>'
#	Examples: 
#	| No | path                 | resourceToDelete | exists | resultPath               | errorMessage                    |
#	| 1  | SpecFlow             | Doddi            | true   | SpecFlow\Doddi           |                                 |
#	| 2  | SpecFlow             | Doddii           | false  | SpecFlow\Doddii          | Unknown 'Doddii' was not found. |
#	| 3  | Testings\Test1\Test2 | Pack             | true   | Tesings\Test1\Test2\Pack |                                 |
#	| 4  | Testings\Test1\Test2 | Pack             | false  | Tesings\Test1\Test2\Pack | Unknown 'Pack' was not found.   |
#
#Scenario Outline: Filtering on string
#	Given I have a path '<path>'
#	And I have string '<filter>'
#	When I filter
#	Then the filtered results will be '<filteredResults>'
#	Examples: 
#	| No | path                                 | filter | filteredResults         |
#	| 1  | SpecFlow\Flow\EasyFlow               | Flow   | SpecFlow,Flow,EasyFlow  |
#	| 2  | Testing\Warewolf\Count\Delete        | Delete | Delete                  |
#	| 3  | TEST\warewolf\ware                   | ware   | warewolf,ware           |
#	| 4  | Afgha\Amrit\Amith\Ajith              | A      | Afgha,Amrit,Amith,Ajith |
#	| 5  | Baskar\Bhanu\Badri                   | Bh     | Bhanu                   |
#	| 6  | Ajith\Bharat\Candy\Dany\Elien\Farukh | Dan    | Dany                    |
#	| 7  | Nightly\Swiftly\Heartly\Feroz        | tly    | Nightly,Swiftly,Heartly |
#	| 8  | Records\Recordset\Recor              | cor    | Records,Recordset,Recor |
#	| 9  | Records\Recordset\Recor              | Re     | Records,Recordset,Recor |
#
#
#
#
#
#
#
#
