@Utility
Feature: DeleteRedis
	In order to Delete a cache key on a Redis Cache Server
	As a Warewolf user
	I want to be able to manage Redis delete easily

Scenario: DeleteRedis Add two numbers again
	Given I have entered 50 into the calculators
	And I have entered 70 into the calculators
	When I press adds
	Then the result should be 120 on the screens
