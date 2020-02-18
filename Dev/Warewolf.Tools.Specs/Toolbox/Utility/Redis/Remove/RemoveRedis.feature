@Utility
Feature: RedisRemove
	In order to avoid key conflicts
	As a user
	I want to be able to Remove keys within the cache 

@RedisRemove
Scenario: Remove Non Existing Key From Cache
	Given valid Redis source
	When I execute the Redis Remove "MyData" tool
	Then The Cache has been Removed with "Failure"

@RedisRemove
Scenario: Remove Existing Key From Cache
	Given valid Redis source
	And I have a key "MyData" with GUID and ttl of "3000" milliseconds
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	Then I execute the cache tool
	When I execute the Redis Remove "MyData" with GUID tool
	Then The Cache has been Removed with "Success"

@RedisRemove
Scenario: Remove Specific Key From Cache
	Given valid Redis source
	And I have a key "MyData" with GUID and ttl of "3000" milliseconds
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	Then I execute the cache tool 
	Then I add another key "MyData2"
	And another assign "dataToStore2" as
		| var      | value   |
		| [[Var3]] | "Test4" |
	Then I execute the cache tool
	Then The "MyData2" Cache exists
	When I execute the Redis Remove "MyData" with GUID tool
	Then The "MyData2" Cache exists
