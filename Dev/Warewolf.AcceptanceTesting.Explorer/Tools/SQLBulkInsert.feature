Feature: SQLBulkInsert
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@SQLBulkInsert
Scenario: Sql Bulk Insert small view
	Given I have Sql Bulk Insert small view
	And Db selected is "Select a Database..."  
	And Select DB edit is "Disabled"
	And table selected is "Select a Table..."
	And Select table  edit is "Disabled"
	And data grid is "Disabled"
	And result is ""


Scenario: Sql Bulk Insert Large view
	Given I have Sql Bulk Insert Large view
	And Db selected is "Select a Database..."  
	And Select DB edit is "Disabled"
	And table selected is "Select a Table..."
	And Select table  edit is "Disabled"
	And data grid is "Disabled"
	And Batch Size is "0"
	And Timeout is "0"
	And Skip blank rows is "Selected"
	And Check Constraints is "UnSelected"
	And Keep Table Lock is "UnSelected"
	And Fire Triggers is "UnSelected"
	And Keep Identity is "UnSelected"
	And Use Internal Transaction is "UnSelected"
	And result is ""
	And On Error box consists
       | Put error in this variable | Call this web service |
       |                            |                       |
    And End this workflow is "Unselected"
    And Done button is "Visible"




Scenario: Sql Bulk Insert Large view water marks
	Given I have Sql Bulk Insert Large view
	And Db selected is "Select a Database..."  
	And Select DB edit is "Disabled"
	And table selected is "Select a Table..."
	And Select table  edit is "Disabled"
	And data grid is "Disabled"
	And Batch Size is "0"
	And Timeout is "0"
	And Skip blank rows is "Selected"
	And Check Constraints is "UnSelected"
	And Keep Table Lock is "UnSelected"
	And Fire Triggers is "UnSelected"
	And Keep Identity is "UnSelected"
	And Use Internal Transaction is "UnSelected"
	And result is "[[InsertSuccess]]"
	And On Error box consists
       | Put error in this variable | Call this web service       |
       | [[Errors().Message         | http://lcl:3142/service/err |
    And End this workflow is "Unselected"
    And Done button is "Visible"



Scenario: Select New Database source is opening new db source tab
	Given I have Sql Bulk Insert Large view
	When Db selected is "New Database Source"  
	Then "New Database Service" tab is opened
	And focus is "Select a source"


Scenario: Selecting a saved DB as DB in large view 
	Given I have Sql Bulk Insert Large view
	And Db selected is "testingDBSrc"  
	And Select DB edit is "Enabled"
	And table selected is "dbo.[City]"
	And Select table refresh is "Enabled"
	And Input Data or [[Variable]] is "Disabled"
	| Input Data or [[Variable]] | To Field    | Type         |
	| [[City(*).CityID]]         | CityID      | int          |
	| [[City(*).Description]]    | Description | varchar (50) |
	| [[City(*).CountryID]       | CountryID   | int          |
	And Scroll bar is "Enaabled"
	And Batch Size is "0"
	And Timeout is "0"
	And Skip blank rows is "Selected"
	And Check Constraints is "UnSelected"
	And Keep Table Lock is "UnSelected"
	And Fire Triggers is "UnSelected"
	And Keep Identity is "UnSelected"
	And Use Internal Transaction is "UnSelected"
	And result is "[[Result]]"
    And Done button is "Visible"
	When I click on Done
	Then Validation message is not thrown
	And Sql small view is "Visible"
	And Db selected is "testingDBSrc"
	And Select DB edit is "Enabled"
	And table selected is "dbo.[City]"
	And Select table refresh is "Enabled"
	And Sql small view is
	| [[City(*).CityID]]      | CityID      |
	| [[City(*).Description]] | Description |
	| [[City(*).CountryID]    | CountryID   |
	And Scroll bar is "Enaabled"




Scenario Outline: Large view done button is validating incorrect variables in result
	Given I have Sql Bulk Insert Large view
	And Db selected is "testingDBSrc"  
	And Select DB edit is "Enabled"
	And table selected is "dbo.[City]"
	And Select table refresh is "Enabled"
	And Input Data or [[Variable]] is "Disabled"
	| Input Data or [[Variable]] | To Field    | Type         |
	| [[City(*).CityID]]         | CityID      | int          |
	| [[City(*).Description]]    | Description | varchar (50) |
	| [[City(*).CountryID]       | CountryID   | int          |
	And Scroll bar is "Enaabled"
	And Batch Size is "0"
	And Timeout is "0"
	And Skip blank rows is "Selected"
	And Check Constraints is "UnSelected"
	And Keep Table Lock is "UnSelected"
	And Fire Triggers is "UnSelected"
	And Keep Identity is "UnSelected"
	And Use Internal Transaction is "UnSelected"
	And result is '<Result>'
	When I click on Done
	Then Validation message is thrown '<Vali>'
Examples: 
    | No | Result           | Vali  |
    | 1  | [[a]]            | False |
    | 2  | [[rec().a]]      | False |
    | 3  | [[a@]]           | True  |
    | 4  | [[rec().a@]]     | True  |
    | 5  | [[a]][[b]]       | True  |
    | 6  | [[rec([[a]]).a]] | True  |
    | 7  |                  | True  |
    


Scenario Outline: Large view is validating incorrect variables in Input fields
	Given I have Sql Bulk Insert Large view
	And Db selected is "testingDBSrc"  
	And Select DB edit is "Enabled"
	And table selected is "dbo.[City]"
	And Select table refresh is "Enabled"
	And Input Data or [[Variable]] is "Disabled"
	| Input Data or [[Variable]] | To Field    | Type         |
	| <Variable>                 | CityID      | int          |
	| [[City(*).Description]]    | Description | varchar (50) |
	| [[City(*).CountryID]       | CountryID   | int          |
	And Scroll bar is "Enaabled"
	And Batch Size is "0"
	And Timeout is "0"
	And Skip blank rows is "Selected"
	And result is "[[a]]
	When I click on Done
	Then Validation message is thrown '<Vali>'
Examples: 
    | No | Variable         | Vali  |
    | 1  | [[a]]            | False |
    | 2  | [[rec().a]]      | False |
    | 3  | [[a@]]           | True  |
    | 4  | [[rec().a@]]     | True  |
    | 5  | [[a]][[b]]       | True  |
    | 6  | [[rec([[a]]).a]] | True  |
    | 7  | [[rec(*).a]]     | False |
    




Scenario: Collapse largeview is closing large view
	Given I have Sql Bulk Insert Large view
	And Db selected is "testingDBSrc"  
	And Select DB edit is "Enabled"
	And table selected is "dbo.[City]"
	And Select table refresh is "Enabled"
	And Input Data or [[Variable]] is "Disabled"
	And I edit  Sql Bulk Insert Large view as
	| Input Data or [[Variable]] | To Field    | Type         |
	| [[rec().a@]]               | CityID      | int          |
	| [[City(*).Description]]    | Description | varchar (50) |
	| [[City(*).CountryID]       | CountryID   | int          |
	And Scroll bar is "Enaabled"
	And Batch Size is "0"
	And Timeout is "0"
	And Skip blank rows is "Selected"
	And result is "[[a]]
	When I collapse large view
	Then Validation message is not thrown
	Then Sql Bulk Insert Small View is "Visible"


Scenario: Opening Sql Bulk Insert Quick Variable Input
	Given I have Sql Bulk Insert Small View on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And Split List On selected as "Chars" with ""
	And Prefix as ""
	And Suffix as ""
	And Append is "Selected"
	And Replace is "Unselected"
	And Preview as
	||
	And Preview button is "Disabled"
	And Add button is "Dsiabled"


