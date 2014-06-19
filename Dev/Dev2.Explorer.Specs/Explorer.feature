@explorer
Feature: Explorer
		In order to be able a manage my resources
		As a Warewolf user
		I want to a windows explorer like interface

Scenario: Load resources for local server
	Given I have a server "localhost"
	When resources are loaded for "localhost"
	Then the explorer for "localhost" will have
		| Parent        | Child                   | Type            |
		|               | localhost               | Server          |
		| localhost     | ExplorerSpecs           | Folder          |
		| ExplorerSpecs | Fetch                   | DbService       |
		| ExplorerSpecs | FetchCities             | WebService      |
		| ExplorerSpecs | Email Service           | PluginService   |
		| ExplorerSpecs | PrimitiveReturnTypeTest | WorkflowService |
		| ExplorerSpecs | TravsTestFlow           | WorkflowService |


Scenario: Rename Folder
	Given I have a server "localhost"
	When  I rename the folder "FolderToRename" to "Bob"
	Then the explorer tree for "localhost" will have
		| Parent    | Child                   | Type            |
		|           | localhost               | Server          |
		| localhost | Bob                     | Folder          |
		| Bob       | PrimitiveReturnTypeTest | WorkflowService |


Scenario: Rename Resource
	Given I have a server "localhost"
	When  I rename the resource "ItemToRename" to "BobAndDora"
	Then the explorer tree for "localhost" will have
		| Parent                  | Child                   | Type            |
		|                         | localhost               | Server          |
		| localhost               | ExplorerSpecsRenameItem | Folder          |
		| ExplorerSpecsRenameItem | BobAndDora              | WorkflowService |

Scenario: Delete Resource
	Given I have a server "localhost"
	When  I delete the resource "ResourceToDelete"
	Then the explorer tree for "localhost" will have
		| Parent                  | Child                   | Type            |
		|                         | localhost               | Server          |
		| localhost               | ExplorerSpecsDeleteItem		| Folder          |


Scenario: Delete Folder
Given I have a server "localhost"
When  I delete the Folder "ExplorerSpecsDeleteFolder"
Then the explorer tree for "localhost" will not have ""ExplorerSpecsDeleteFolder""
		| Parent                  | Child                   | Type            |
		|                         | localhost               | Server          |

Scenario: Non Recursive Delete Folder
Given I have a server "localhost"
When  I delete the Folder without recursive delete flag "FolderToDeleteRecursive"
Then the explorer tree for "localhost" will have
		| Parent    | Child               | Type   |
		|           | localhost           | Server |
		| localhost | ExplorerSpecsDelete | Folder |

Scenario: Create Folder
Given I have a server "localhost"
When  I create the Folder "FolderName"
Then the reloaded explorer tree for "localhost" will have the created folder as a child
		| Parent    | Child               | Type   |


#Scenario: Save resource with nested folder
#	Given I have a server "localhost"
#	And resources are saved as
#		| ResourceName   | ResourcePath                |
#		| SpecResource1  |                             |
#		| SpecResource2  | Level1                      |
#		| SpecResource3  | Level1\Deeper1              |
#		| SpecResource4  | Level2\Deeper2\InnerDeeper2 |
#		| SpecResource1 | Level3                      |
#	When resources are loaded for "localhost"
#	Then the explorer will have
#         | Parent       | Child         | Type     |
#         |              | localhost     | Server   |
#         | localhost    | SpecResource1 | Folder   |
#         | localhost    | Level1        | Folder   |
#         | Level1       | SpecResourc2  | Workflow |
#         | Level1       | Deeper1       | Folder   |
#         | Deeper1      | SpecResource3 | Workflow |
#         | localhost    | Level2        | Folder   |
#         | Level2       | Deeper2       | Folder   |
#         | Deeper2      | InnerDeeper2  | Folder   |
#         | InnerDeeper2 | SpecResource4 | Workflow |
#         | localhost    | Level3        | Folder   |
#         | Level3       | SpecResource1 | Workflow |