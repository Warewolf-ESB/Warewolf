Feature: DesignTests
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DesignTests
Scenario: Change mappings and Saving workflow which has dependencies is showing popup
	Given I have a workflow "MappingWF" is opened on design surface
	And the "Assign" is "Visible"
	And the mappings are "visible"
	And the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| Var           | NO             | Yes          | No               |       | Yes    |
	|               | NO             | NO           | NO               |       |        |
	And the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| rec()          | NO             | Yes          | Yes              |       |        |
	| rec().a        | NO             | Yes          | Yes              | Yes   |        |
	| mr()           | NO             | Yes          |                  |       |        |
	| mr().a         | NO             | Yes          |                  | Yes   |        |
	|                | No             | No           |                  |       |        |  
	When I edit the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| Var           | NO             | Yes          | No               | Yes   |        |
	|               | NO             | NO           | NO               |       |        |
	And I edit the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| rec()          | NO             | Yes          |                  |       |        |
	| rec().a        | NO             | Yes          | Yes              |       | Yes    |
	| mr()           | NO             | Yes          |                  |       |        |
	| mr().a         | NO             | Yes          |                  |       | Yes    |
	|                | No             | No           |                  |       |        |  
	When I click on save
	Then "Inputs/Outputs Changed" popup is "Displayed"



Scenario: Mappings out of date mark is visible on workflow service proc
	Given I have a workflow "MappingWF" is opened on design surface
    Given I have "Unsaved 1" is opened 
	And I have "MappingsWF" on "Unsaved 1" design surface
	And Input mappings of "MappingsWF" are
	| Inpuy Data or [[Varaible]] | To Service |
	|                            | rec(*).a   |
	|                            | mr(*).a    |
	And Output mappings of "MappingsWF" are
	| Output From Service | To [[Variable]] |
	| var                 |                 |
	And Edit button is "Visible" on "MappingsWF"
	And Done button is "Visible"
	When I have focus on tab "MappingWF"
	Then the "Assign" is "Visible"
	When I edit mappings 
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| Var           | NO             | Yes          | No               |       |        |
	|               | NO             | NO           | NO               |       |        |
	And I edit the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| rec()          | NO             | Yes          |                  |       |        |
	| rec().a        | NO             | Yes          | Yes              |       |        |
	| mr()           | NO             | Yes          |                  |       |        |
	| mr().a         | NO             | Yes          |                  |       |        |
	|                | No             | No           |                  |       |        |  
	And I click on save
	Then "Inputs/Outputs Changed" popup is "Displayed"
	And I have focus on tab "Unsaved 1"
	And I have "MappingsWF" on "Unsaved 1" design surface
	And "MappingsWF" proc Mappings out of date is "Visible"
	When I open "MappingsWF" large view 
	Then Input mappings of "MappingsWF" are "Invisible"
	And Output mappings of "MappingsWF" are "Invisible"
	And Fix button is "Visible"
	And Done button is "Invisible"
	When I close "MappingsWF" large view
	Then "MappingsWF" proc Mappings out of date is "Visible" 
	When I open "MappingsWF" large view
	And click on "Fix"
	Then Done button is "Visible"
	When I click on "Done"
	Then "MappingsWF" small view is "Visible"


Scenario: Edit button on service proc is opening workflow
	Given I have "Unsaved 1" is opened 
	And I have "MappingsWF" on "Unsaved 1" design surface
	And Edit button is "Visible" on "MappingsWF"
	When click on "Edit" on "MappingsWF"
	Then workflow "MappingWF" is opened on design surface	 
	And Focus is at "MappingWF" 

Scenario: Workflow hyper link in Debug output is opening service
	Given I have "Unsaved 1" is opened 
	And I have "MappingsWF" on "Unsaved 1" design surface
	When I Debug "Unsaved 1" 
	Then Workflow "MappingsWF" hyperlink is "Visible"
	When I click on hyperlink of "Workflow: MappingsWF"
	Then workflow "MappingWF" is opened on design surface 

Scenario: Service hyper link in Debug output is opening service
	Given I have "Unsaved 1" is opened 
	And I have "Service" on "Unsaved 1" design surface
	When I Debug "Unsaved 1" 
	Then Workflow "Service" hyperlink is "Visible"
	When I click on hyperlink of "Service: Service"
	Then workflow "Service" is opened on design surface 

Scenario: Opening remote wflw From design surface 
	Given I have "Unsaved 1" is opened 
	And I have connected to remote "Sandbox-1"
	And I have "RemoteWf" on "Unsaved 1" design surface
	When I Debug "Unsaved 1" 
	Then Workflow "Service" hyperlink is "Visible"
	When I click on hyperlink of "Workflow: RemoteWf"
	Then workflow "Service" is opened on design surface 
	And "Create connection" popup is "Not Visible"

Scenario: Opening remote wflw on design surface prompts user to make connection
	Given I have "Unsaved 1" is opened 
	And I have not connected to remote "Sandbox-1"
	And I have "RemoteWf" on "Unsaved 1" design surface
	When I Debug "Unsaved 1" 
	Then Workflow "Service" hyperlink is "Visible"
	When I click on hyperlink of "Workflow: RemoteWf"
	Then workflow "Service" is not opened on design surface 
	And "Create connection" popup is "Visible"

Scenario: Editing Services which has dependencies is throwing popup
    Given DB Services "Testsrv" is opened on design surface 
	When I edit "Testsrv" mappings
	| Input       | Default Value | Required Field | Empty is Null |
	| CountryName |               | Yes            |               |
	| Prefix      |               | Yes            |               |
	Then "Inputs/Outputs Changed" popup is "Displayed" 


Scenario: DBService edit option opens dbservice
    Given I have "Unsaved 1" is opened 
	And I have "Testsrv" on "Unsaved 1" design surface
	And Edit button is "Visible" on "MappingsWF"
	When click on "Edit" on "Testsrv"
	Then DB Services "Testsrv" is opened on design surface	 
	And Focus is at "Testsrv" 

Scenario: Service update option is visible on the proc
    Given I have "Unsaved 1" is opened 
	And I have "Testsrv" on "Unsaved 1" design surface
	When I open "Testsrv" large view
	Then Input mappings of "Testsrv" are
	| Inpuy Data or [[Varaible]] | To Service  |
	| [[CountryName]]            | CountryName |
	| [[Prefix]]                 | Prefix      |
	And Output mappings of "Testsrv" are
	| Output From Service                    | To [[Variable]]                        |
	| [[dbo_Pr_CitiesGetByCountry().CityID]] | [[dbo_Pr_CitiesGetByCountry().CityID]] |
	| [[dbo_Pr_CitiesGetByCountry().City]]   | [[dbo_Pr_CitiesGetByCountry().City]]   |
	When I edit "Testsrv" mappings
	| Input       | Default Value | Required Field | Empty is Null |
	| CountryName | Test          | Yes            |               |
	| Prefix      | Warewolf      | Yes            |               |
	And I click on save
	Then "Inputs/Outputs Changed" popup is "Displayed" 
	When I have focus on tab "Unsaved 1"
	Then I have "Testsrv" on "Unsaved 1" design surface
	And "Testsrv" proc Mappings out of date is "Visible"
	When I open "Testsrv" large view 
	Then Input mappings of "MappingsWF" are "Invisible"
	And Output mappings of "MappingsWF" are "Invisible"
	And Fix button is "Visible"
	And Done button is "Invisible"
	When I close "MappingsWF" large view
	Then "MappingsWF" proc Mappings out of date is "Visible" 
	When I open "MappingsWF" large view
	And click on "Fix"
	Then Done button is "Visible"
	Then Input mappings of "Testsrv" are
	| Inpuy Data or [[Varaible]] | To Service  |
	| Test                       | CountryName |
	| Warewolf                   | Prefix      |
	And Output mappings of "Testsrv" are
	| Output From Service                    | To [[Variable]]                        |
	| [[dbo_Pr_CitiesGetByCountry().CityID]] | [[dbo_Pr_CitiesGetByCountry().CityID]] |
	| [[dbo_Pr_CitiesGetByCountry().City]]   | [[dbo_Pr_CitiesGetByCountry().City]]   |
	When I click on "Done"
	Then "MappingsWF" small view is "Visible"



Scenario: Saving a workflow is updaing name of design surface
    Given I have "Unsaved 1" is opened 
	And tab is opened as "Unsaved 1" with star
	And Design surface name is "Unsaved 1"
	And I save Unsaved 1 as "Workflow"
	Then tab is opened as "Workflow" with star
	And Design surface name is "Workflow"


Scenario: Renaming a saved workflow is updaing name on design surface
    Given I have "Workflow" is opened 
	And tab is opened as "Workflow" with star
	And Design surface name is "Workflow"
	And I Rename "Workflow" as "RenamedWF"
	Then tab is opened as "RenamedWF" with star
	And Design surface name is "RenamedWF"

Scenario: While workflow is debuging tools are highligting by showing the flow
   Given I have "Unsave 1" is opened on design surface
   And I have "Assign" on "Unsaved 1"
   And I have "Base Conversion" on "Unsaved 1"
   And I have "Case Conversion" on "Unsaved 1"
   And I have "Data Merge" on "Unsaved 1"
   And I have "Data Split" on "Unsaved 1"
   And I have "Find Index" on "Unsaved 1"
   And I have "Replace" on "Unsaved 1"
   And I have "Unique" on "Unsaved 1"
   And I have "Sort" on "Unsaved 1"
   When I Debug "Unsaved 1"
   Then design surface tools are highlighted as
   | Assign          |
   | Base Conversion |
   | Case Conversion |
   | Data Merge      |
   | Data Split      |
   | Find Index      |
   | Replace         |
   | Unique          |
   | Sort            |

 
Scenario Outline: By Selecting each Debug Output is highlighting related tool or service
   Given I have "Unsave 1" is opened on design surface
   And I have "Assign" on "Unsaved 1"
   And I have "Base Conversion" on "Unsaved 1"
   And I have "Case Conversion" on "Unsaved 1"
   And I have "Data Merge" on "Unsaved 1"
   And I have "Data Split" on "Unsaved 1"
   And I have "Find Index" on "Unsaved 1"
   And I have "Replace" on "Unsaved 1"
   And I have "Unique" on "Unsaved 1"
   And I have "Sort" on "Unsaved 1"
   And I have "Testsrv" on "Unsaved 1"
   And I have "Workflow" on "Unsaved 1"
   And I have "Webservice" on "Unsaved 1"
   And I have "RemoteWorkflow" on "Unsaved 1"
   When I Debug "Unsaved 1"
   Then debug output is "Visible"
   When I select '<Select>'
   Then "Assign" on design is '<Assign>'
   Then "Base Conversion" on design is '<Base Conversion>'
   Then "Case Conversion" on design is '<Case Conversion>'
   Then "Data Merge" on design is '<Data Merge>'
   Then "Data Split" on design is '<Data Split>'
   Then "Find Index" on design is '<Find Index>'
   Then "Replace" on design is '<Replace>'
   Then "Unique" on design is '<Unique>'
   Then "Sort" on design is '<Sort>'
   Then "Testsrv" on design is '<Testsrv>'
   Then "Workflow" on design is '<Workflow>'
   Then "Webservice" on design is '<Webservice>'
   Then "RemoteWorkflow" on design is '<RemoteWorkflow>'
Examples: 
   | Select                   | Assign        | Base Conversion | Case Conversion | Data Merge      | Data Split      | Find Index      | Replace         | Unique          | Sort            | Testsrv         | Workflow        | Webservice      | RemoteWorkflow  |
   | Step: Assign             | Highlight     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Base Conversion    | Not Highlight | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Case Conversion    | Not Highlight | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Data Merge         | Not Highlight | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Data Split         | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Find Index         | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Replace            | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Sort               | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted |
   | Step: Testsrv            | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted | Not Highlighted |
   | Workflow: Workflow       | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted | Not Highlighted |
   | Service: Webservice      | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     | Not Highlighted |
   | Workflow: RemoteWorkflow | Not Highlight | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Not Highlighted | Highlighted     |
 



Scenario: Input data hyper link has max 3 lines and for bigger data scroll bar
      Given I have "Unsave 1" is opened on design surface
	  And I have "Assign" on "Unsaved 1"
	  And the Variable Names are
	  | Variable Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	  | Var           | NO             | Yes          | No               | Yes   |        |
	  |               | NO             | NO           | NO               |       |        |
	  And Input hyper link contains "http://rsaklf"
	  And Input hyper link has No scroll bar
	  When I Debug "Unsaved 1"
	  Then "Debug input data" dialogbox is opened
	  And I enter "Var" value as "Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll barInput data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll barInput data hyper link has max 3 lines and for bigger data scroll bar"
	  And Input hyper link has No scroll bar
	  When I Debug "Unsaved 1"
	  Then I enter "Var" value as "Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll barInput data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll bar Input data hyper link has max 3 lines and for bigger data scroll barInput data hyper link has max 3 lines and for bigger data scroll bar lines and for bigger data scroll barInput data hyper link has max 3 lines and for bigger data scroll bar lines and for bigger data scroll barInput data hyper link has max 3 lines and for bigger data scroll bar"
	  And Input hyper link has scroll bar











