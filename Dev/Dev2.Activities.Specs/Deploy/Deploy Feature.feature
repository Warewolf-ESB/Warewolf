@Deploy
Feature: Deploy Feature
In order to schedule workflows
	As a Warewolf user
	I want to setup schedules

Scenario: Create and Deploy a renamed resource to localhost
	Given localhost and destination server are connected
	And I have a workflow "OriginalName"
	And "OriginalName" contains an Assign "Rec To Convert" as
	| variable    | value |
	| [[rec().a]] | yes   |
	| [[rec().a]] | no    |
	And "OriginalName" contains Count Record "CountRec" on "[[rec()]]" into "[[count]]"
	When "OriginalName" is Saved
	And I select and deploy resource from source server
	When I rename "OriginalName" to "RenamedResource" and re deploy
	Then I select and deploy resource from remote server
	Then Remote server has updated name