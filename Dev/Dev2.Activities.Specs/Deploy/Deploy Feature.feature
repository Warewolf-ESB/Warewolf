@Deploy
@RemoteServer
Feature: Deploy Feature
In order to deploy workflows
	As a Warewolf user
	I want to setup deployments

Scenario: Deploy a renamed resource
	Given localhost and destination server are connected
	And I have a workflow "OriginalName"
	And the workflow contains an Assign "Rec To Convert" as
	| variable    | value |
	| [[rec().a]] | yes   |
	| [[rec().a]] | no    |
	And the workflow contains Count Record "CountRec" on "[[rec()]]" into "[[count]]"
	When the workflow is Saved
	And I deploy the workflow
	And I rename the workflow to "RenamedResource" and re deploy
	Then Remote server has updated name