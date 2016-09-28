Feature: Sql Server
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sql Server Tool onto a new workflow creates Sql Server tool with large view on the design surface
	When I "Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface"
	Then I "Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface"

#@NeedsSQLServerToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking SQL Server Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_Sql_Server_Tool_small_View"
	Then I "Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface"
