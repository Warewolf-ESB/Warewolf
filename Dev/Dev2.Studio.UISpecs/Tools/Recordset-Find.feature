Feature: Recordset-Find
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@RecordsetFind
Scenario: FindRecordsLargeViewTabAndValidate
	Given I have Warewolf running
	And all tabs are closed
	Given I click "EXPLORERFILTERCLEARBUTTON"  
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	And I drag "TOOLFIND" onto "WORKSURFACE,StartSymbol"
	And I double click point "5,5" on "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner)"
	When I click "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),DoneButton"
	Then "WORKSURFACE,UI_Error0_AutoID" is visible within "1" seconds
	When I click point "-40,35" on "WORKSURFACE,UI_CloseHelp_AutoID"
	And I send "var{TAB}{TAB}{TAB}{TAB}{TAB}res" to ""
	And I click "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),DoneButton"
	Then "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds