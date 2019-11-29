@Utility
Feature: RedisDelete
	In order to avoid key conflicts
	As a user
	I want to be able to delete keys within the cache 

@RedisDelete
Scenario: Delete Key From Cache
	Given Redis source "192.168.104.19" with password "pass123" and port "6379"
	And I have a key "MyData" and ttl of "3000" milliseconds
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	Then I execute the get/set tool
	Then The "MyData" Cache exists
	Then I have an existing key to delete "MyData"
	When I execute the delete tool
	Then The "MyData" Cache has been deleted

@RedisDelete
Scenario: Delete Specific Key From Cache
	Given Redis source "192.168.104.19" with password "pass123" and port "6379"
	And I have a key "MyData" and ttl of "3000" milliseconds
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	Then I execute the get/set tool
	Then I add another key "MyData2"
	And another assign "dataToStore2" as
		| var      | value   |
		| [[Var3]] | "Test4" |
	Then I execute the get/set tool
	Then The "MyData" Cache exists
	Then The "MyData2" Cache exists
	Then I have an existing key to delete "MyData"
	When I execute the delete tool
	Then The "MyData" Cache has been deleted
	Then The "MyData2" Cache exists
