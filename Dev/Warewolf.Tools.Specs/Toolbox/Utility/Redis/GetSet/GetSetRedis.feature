Feature: GetSetRedis
	In order to avoid rerunning the work-flow every time we need generated data
	As a user
	I want to be to set and get cached data while the Time To Live has not elapsed 

@RedisGetSet
Scenario: No data in cache
	Given Redis source "localhost" with password "pass123" and port "6379"
	And I have a key "MyData" and ttl of "3000" milliseconds
	And No data in the cache
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the get/set tool
	Then the cache will contain
		| Key    | Data             |
		| MyData | "[[Var1]],Test1" |
	And output variables have the following values
		| var      | value   |
		| [[Var1]] | "Test1" |

@RedisGetSet
Scenario: Data exists for given TTL not hit
	Given Redis source "localhost" with password "pass123" and port "6379"
	And I have a key "MyData" and ttl of "3000" milliseconds
	And data exists (TTL not hit) for key "MyData" as
		| Key    | Data                     |
		| MyData | "[[Var1]],Data in cache" |
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the get/set tool
	Then the assign "dataToStore" is not executed
	And output variables have the following values
		| var      | value                    |
		| [[Var1]] | "[[Var1]],Data in cache" |

@RedisGetSet
Scenario: Data Not Exist For Given Key (TTL exceeded) Spec
	Given Redis source "localhost" with password "pass123" and port "6379"
	And I have a key "MyData" and ttl of "3000" milliseconds
	And data does not exist (TTL exceeded) for key "MyData" as
		| | |
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the get/set tool
	Then the assign "dataToStore" is executed
	Then the cache will contain
		| Key    | Data             |
		| MyData | "[[Var1]],Test1" |
	And output variables have the following values
		| var      | value   |
		| [[Var1]] | "Test1" |

