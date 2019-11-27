@Utility
Feature: GetSetRedis
	In order to Get and Set a cache on a Redis Cache Server
	As a Warewolf user
	I want to be able to manage Redis get/set easily

Scenario: GetSetRedis Add two numbers
	Given I have entered 50 into the calculator
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen