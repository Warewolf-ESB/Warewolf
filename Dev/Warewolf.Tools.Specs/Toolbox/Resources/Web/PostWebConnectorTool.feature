@Resources
Feature: Post Web Connector Tool
	In order to create New Web Post Service Tool in Warewolf
	As a Warewolf User
	I want to Create or Edit Warewolf Web Post Request.

# layout of tool not ready

Scenario: Open new Post Web Tool
	Given I drag Web Post Request Connector Tool onto the design surface
	And Post Edit is Disabled
	Then Post Header is Enabled
	And  Post Header appears as
	| Header | Value |
	And Post Query is Enabled
	And Post mapped outputs are
	| Output | Output Alias |

Scenario: Create Web Service with different methods
	Given I drag Web Post Request Connector Tool onto the design surface
	And Post Edit is Disabled
	Then Post Header is Enabled
	And  Post Header appears as
	| Header | Value |
	And Post Query is Enabled
	And Post the response is loaded
	And Post Mapping is Enabled
	And Post mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |
	
Scenario: Adding parameters in Post Post Web Connector Tool request headers is updating variables
	Given I drag Web Post Request Connector Tool onto the design surface
	Then Post Header is Enabled
	And Post Query is Enabled
	And I enter "?extension=[[extension]]&prefix=[[prefix]]" as Post Query String
	And I add Post Header as
         | Name  | Value |
         | [[a]] | T     |
	And Post Input variables are
	| Name          |
	| [[a]]         |
	| [[extension]] |
	| [[prefix]]    |
	And Post Mapping is Enabled
    And Post mapped outputs are
	| Output      | Output Alias    |
	| CountryID   | [[CountryID]]   |
	| Description | [[Description]] |

Scenario: Changing Post Post Web Connector Tool Sources
	Given I drag Web Post Request Connector Tool onto the design surface
	Then Post Header is Enabled
	And Post Query is Enabled
	Then Post Response appears as "{"rec" : [{"a":"1","b":"a"}]}"
	And Post Mapping is Enabled
	And Post mapped outputs are
	| Mapped From | Mapped To   |
	| a           | [[rec().a]] |
	| b           | [[rec().b]] |
	Then Post Header is Enabled
	And Post Query is Enabled
	And Post Mappings is Disabled

Scenario: Post Web Connector Tool returns text
	Given I drag Web Post Request Connector Tool onto the design surface
	Then Post Header is Enabled
	And Post Mapping is Enabled
	And Post mapped outputs are
	| Output   | Output Alias |
	| a | [[rec().a]]     |
