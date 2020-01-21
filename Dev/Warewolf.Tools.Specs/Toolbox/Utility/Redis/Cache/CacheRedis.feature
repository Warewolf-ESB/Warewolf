Feature: RedisCache
	In order to avoid rerunning the work-flow every time we need generated data
	As a user
	I want to be to cached data while the Time To Live has not elapsed 

@RedisCache
Scenario: No data in cache
	Given valid Redis source
	And I have a key "MyData" with GUID and ttl of "3000" milliseconds
	And No data in the cache
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the Redis Cache tool
	Then the cache will contain
		| Key    | Data             |
		| MyData | "[[Var1]],Test1" |
	And output variables have the following values
		| var      | value   |
		| [[Var1]] | "Test1" |

@RedisCache
Scenario: Data exists for given TTL not hit
	Given valid Redis source
	And I have a key "MyData" with GUID and ttl of "9000" milliseconds
	And data exists (TTL not hit) for key "MyData" with GUID as
		| Key    | Data                     |
		| MyData | "[[Var1]],Data in cache" |
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the Redis Cache tool
	Then the assign "dataToStore" is not executed
	And output variables have the following values
		| var      | value                    |
		| [[Var1]] | "[[Var1]],Data in cache" |

@RedisCache
Scenario: Data Not Exist For Given Key (TTL exceeded) Spec
	Given valid Redis source
	And I have a key "MyData" with GUID and ttl of "3000" milliseconds
	And data does not exist (TTL exceeded) for key "MyData" as
		| | |
	And an assign "dataToStore" as
		| var      | value   |
		| [[Var1]] | "Test1" |
	When I execute the Redis Cache tool
	Then the assign "dataToStore" is executed 
	Then the cache will contain
		| Key    | Data             |
		| MyData | "[[Var1]],Test1" |
	And output variables have the following values
		| var      | value   |
		| [[Var1]] | "Test1" |


@RedisCache
Scenario: Input Variable Keys Are Less Then Cached Data Variable Keys
	Given valid Redis source
	And I have "key1" of "MyData" with GUID and "ttl1" of "15" seconds
	And I have "key2" of "MyData" with GUID and "ttl2" of "3" seconds
	And an assign "dataToStore1" into "DsfMultiAssignActivity1" with
		| name      | value  |
		| [[Var1]] | "Test1" |
		| [[Var2]] | "Test2" |
	And an assign "dataToStore2" into "DsfMultiAssignActivity2" with
		| name      | value   |
		| [[Var1]] | "Test21" |
	Then the assigned "key1", "ttl1" and innerActivity "DsfMultiAssignActivity1" is executed by "RedisActivity1"
	And the Redis Cache under "key1" with GUID will contain
		| name     | value   |
		| [[Var1]] | "Test1" |
		| [[Var2]] | "Test2" |
	Then the assigned "key2", "ttl2" and innerActivity "DsfMultiAssignActivity2" is executed by "RedisActivity2"
	Then "RedisActivity2" output variables have the following values
		| label							| variable		| operator	| value		|
		|	Redis key { MyData } found	|  null			|			|			|
		|			null				| [[Var1]]		|	 =		| "Test21"	|
		|			null				| [[Var2]]		|	 =		| "Test22"	|
		|			null				| [[Var3]]		|	 =		| "Test23"	|
		|			null				| [[Var4]]		|	 =		| "Test24"	|


Scenario: Input Variable Keys Are Greater Then Cached Data Variable Keys
	Given valid Redis source
	And I have "key1" of "MyData" with GUID and "ttl1" of "15" seconds
	And I have "key2" of "MyData" with GUID and "ttl2" of "3" seconds
	And an assign "dataToStore1" into "DsfMultiAssignActivity1" with
		| name      | value   |
		| [[Var1]] | "Test1" |
		| [[Var2]] | "Test2" |
	And an assign "dataToStore2" into "DsfMultiAssignActivity2" with
		| name      | value   |
		| [[Var1]] | "Test21" |
		| [[Var2]] | "Test22" |
		| [[Var3]] | "Test23" |
		| [[Var4]] | "Test24" |
	Then the assigned "key1", "ttl1" and innerActivity "DsfMultiAssignActivity1" is executed by "RedisActivity1"
	And the Redis Cache under "key1" with GUID will contain
		| name     | value   |
		| [[Var1]] | "Test1" |
		| [[Var2]] | "Test2" |
	Then the assigned "key2", "ttl2" and innerActivity "DsfMultiAssignActivity2" is executed by "RedisActivity2"
	Then "RedisActivity2" output variables have the following values
		| label							| variable		| operator	| value		|
		|	Redis key { MyData } found	|  null			|			|			|
		|			null				| [[Var1]]		|	 =		| "Test21"	|
		|			null				| [[Var2]]		|	 =		| "Test22"	|
		|			null				| [[Var3]]		|	 =		| "Test23"	|
		|			null				| [[Var4]]		|	 =		| "Test24"	|