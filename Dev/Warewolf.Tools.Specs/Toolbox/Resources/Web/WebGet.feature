Feature: WebGet
	In order to create New Web Get Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Get Request.

Scenario: Open new Get Web Tool
	And I drag Web Get Request Connector Tool onto the design surface
	And New Source is Enabled
	And Edit Source is Disabled
	When I Select "LocalhostSource" as a Get web Source
	Then Get Header is Enabled
	And  Get Header appears as
	| Header | Value |
	And Get Edit is Enabled
	And Get Body is Enabled 
	And Get Url is Visible
	And Get Query is Enabled
	And Get Generate Outputs is Enabled
	And Get mapped outputs are
	| Output | Output Alias |

 Scenario: Editing Get Web Service
	Given I drag Web Get Request Connector Tool onto the design surface
    And New Source is Enabled
	When I Select "LocalhostSource" as a Get web Source
	And New Source is Enabled
	And Get Edit is Enabled
	When I click Get Edit
	Then the "LocalhostSource" Get Source tab is opened
	
 Scenario: Generate Get Web Service Creates Output Mappings
	Given I drag Web Get Request Connector Tool onto the design surface
    And New Source is Enabled
	When I Select "LocalhostSource" as a Get web Source
	And New Source is Enabled
	And Get Edit is Enabled
	Then I click Get Generate Outputs
	Then Get the Generate Outputs window is shown
	When Get Test Inputs is Successful
	Then Get Response contains Data
	When I click Get Done
	Then Get Mapping is Enabled
	And Get mapped outputs are
	| Mapped From | Mapped To                       |
	| Id          | [[UnnamedArrayData().Id]]       |
	| Name        | [[UnnamedArrayData().Name]]     |
	| Category    | [[UnnamedArrayData().Category]] |
	| Price       | [[UnnamedArrayData().Price]]    |


 Scenario: Change Selected Source Clears mappings
	Given I drag Web Get Request Connector Tool onto the design surface
    And New Source is Enabled
	When I Select "LocalhostSource" as a Get web Source
	And New Source is Enabled
	And Get Edit is Enabled
	Then I click Get Generate Outputs
	Then Get the Generate Outputs window is shown
	When Get Test Inputs is Successful
	Then Get Response contains Data
	When I click Get Done
	Then Get Mapping is Enabled
	And Get mapped outputs are
	| Mapped From | Mapped To                       |
	| Id          | [[UnnamedArrayData().Id]]       |
	| Name        | [[UnnamedArrayData().Name]]     |
	| Category    | [[UnnamedArrayData().Category]] |
	| Price       | [[UnnamedArrayData().Price]]    |
	When I Select "OtherWebSource" as a Get web Source
	Then Get Header is Enabled
	And Get Body is Enabled 
	And Get Url is Visible
	And Get Query is Enabled
	And Get Generate Outputs is Enabled
	And Get mapped outputs are
	| Mapped From | Mapped To                       |

Scenario: Change Recordset Name to Nothing
	Given I drag Web Get Request Connector Tool onto the design surface
    And New Source is Enabled
	When I Select "LocalhostSource" as a Get web Source
	And New Source is Enabled
	And Get Edit is Enabled
	Then I click Get Generate Outputs
	Then Get the Generate Outputs window is shown
	When Get Test Inputs is Successful
	Then Get Response contains Data
	When I click Get Done
	Then Get Mapping is Enabled
	And Get mapped outputs are
	| Mapped From | Mapped To                       |
	| Id          | [[UnnamedArrayData().Id]]       |
	| Name        | [[UnnamedArrayData().Name]]     |
	| Category    | [[UnnamedArrayData().Category]] |
	| Price       | [[UnnamedArrayData().Price]]    |
	When I change Recordset Name to ""
	And Get mapped outputs are
	| Mapped From | Mapped To    |
	| Id          | [[Id]]       |
	| Name        | [[Name]]     |
	| Category    | [[Category]] |
	| Price       | [[Price]]    |

Scenario: Change Recordset Name to newRecordset
	Given I drag Web Get Request Connector Tool onto the design surface
    And New Source is Enabled
	When I Select "LocalhostSource" as a Get web Source
	And New Source is Enabled
	And Get Edit is Enabled
	Then I click Get Generate Outputs
	Then Get the Generate Outputs window is shown
	When Get Test Inputs is Successful
	Then Get Response contains Data
	When I click Get Done
	Then Get Mapping is Enabled
	And Get mapped outputs are
	| Mapped From | Mapped To                       |
	| Id          | [[UnnamedArrayData().Id]]       |
	| Name        | [[UnnamedArrayData().Name]]     |
	| Category    | [[UnnamedArrayData().Category]] |
	| Price       | [[UnnamedArrayData().Price]]    |
	When I change Recordset Name to "newRecordset"
	And Get mapped outputs are
	| Mapped From | Mapped To                   |
	| Id          | [[newRecordset().Id]]       |
	| Name        | [[newRecordset().Name]]     |
	| Category    | [[newRecordset().Category]] |
	| Price       | [[newRecordset().Price]]    |
