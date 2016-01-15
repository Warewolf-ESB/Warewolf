Feature: Apis
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

#Ensure all relevant information is displayed
#Ensure all information is displayed per work flow for the Apis
#Ensure that the .json and .api url contain workflow permissions
#Ensure the baseUrl permissions are valid
#Ensure that if permissions are not granted that the relevant information appears
#Ensure Access will be denied if permissions changed
#Ensure api returns correctly
#Ensure all relevant information is displayed
#Ensure swagger returns correctly
#Display formals based on http or https


@Apis
@ignore
Scenario: Ensure all relevant information is displayed
	Given I execute "http://localhost:3142/apis.json" 
	When the request returns
	Then the response will be valid apis.json format
	And will have properties as 
	| workflowDescription  | Value                             |
	| Name                 | RSAKLFLEROY                       |
	| Description          | ""                                |
	| Image                | null                              |
	| Url                  | http://RSAKLFLEROY:3142/apis.json |
	| Tags                 | null                              |
	| created              | 2015-07-22T00:00:00+02:00         |
	| Modified             | 2015-07-22T00:00:00+02:00         |
	| SpecificationVersion | 0.15                              |
	




Scenario: Ensure all information is displayed per work flow for the Apis
	Given I execute "http://localhost:3142/apis.json" 
	When the request returns
	Then the response will be valid apis.json format
	And will have Apis properties as 
	| workflowDescription | propertyvalue                                                                                   |
	| Name                | InOut                                                                                           |
	| Description         | null                                                                                            |
	| Image               | null                                                                                            |
	| humanUrl            | null                                                                                            |
	| baseUrl             | "http://RSAKLFLEROY:3142/secure/Acceptance Testing Resources/InOut.json"                        |
	| version             | null                                                                                            |
	| Tags                | null                                                                                            |
	| properties          | type = swagger; value = "http://RSAKLFLEROY:3142/secure/Acceptance Testing Resources/InOut.api" |
	| contact             | null                                                                                            |



Scenario: Ensure that the .json and .api url contain workflow permissions
	Given I execute "http://localhost:3142/apis.json" 
	When the request returns
	Then the response will be valid apis.json format
	And the work flow "Hello World" is "visible"
	And "Hello World" has the access permission as 
	| Permissions|
	| public     |
	| secure     |
	Then the permissions appear independantly in the baseUrl property as
	| BaseUrl    |
	| "http://RSAKLFLEROY:3142/secure/Hello World.json" |
	| "http://RSAKLFLEROY:3142/public/Hello World.json" |
	And the permissions appear independantly in the Properties value  as
	| Properties |
	| "http://RSAKLFLEROY:3142/secure/Hello World.api" |
	| "http://RSAKLFLEROY:3142/public/Hello World.api" |


Scenario: Ensure the baseUrl permissions are valid
	Given I execute "http://rsaklfleroy:3142/public/Hello%20World.json"
	And public permissions are "View,Execute,Contribute"
	When the request returns
	Then "http://rsaklfleroy:3142/public/Hello%20World.json" output appear as
	| Output |
	| {"rec" : [{"a":"name"},{"a":"dfsdfsdfsd"}],"Message":"Hello World."} |


Scenario Outline: Ensure that if permissions are not granted that the relevant information appears
	Given I execute ""
	And public permissions are '<permissions>'
	When the request returns
	Then "http://rsaklfleroy:3142/public/Hello%20World.json" output appear as
	| Output                                                               |
	|  |
Examples: 
| Execute                                           | permissions             | output                                                               |
| http://rsaklfleroy:3142/secure/Hello%20World.json | View,Execute,Contribute | {"rec" : [{"a":"name"},{"a":"dfsdfsdfsd"}],"Message":"Hello World."} |
| http://rsaklfleroy:3142/public/Hello%20World.json | View                    | Access Denied for this request                                       |
| http://rsaklfleroy:3142/public/Hello%20World.json | Execute                 | {"rec" : [{"a":"","b":""}],}                                         |


Scenario: Ensure Access will be denied if permissions changed
	Given I have "http://rsaklfleroy:3142/secure/Acceptance%20Tests/TestAssignWithRemote.json"
	And permissions are "View,Execute,Contribute"
	And I execute "http://rsaklfleroy:3142/public/Acceptance%20Tests/TestAssignWithRemote.json"
	When the request returns
	Then "http://rsaklfleroy:3142/public/Acceptance%20Tests/TestAssignWithRemote.json" properties appear as
	| Output                                   |
	| Access has been denied for this request. |

#wolf-1084
Scenario: Ensure all relevant information is displayed based on access type
	Given I execute "http://server:3142/public/apis.json" 
	When the request returns
	Then only publically available services should be visible

#wolf-1085
Scenario: Ensure swagger returns correctly
	Given I execute "http://rsaklfleroy:3142/public/Hello%20World.api"
	And public permissions are "View,Execute,Contribute"
	When the request returns
	Then "Https" is not visible in Swagger definition

#wolf-1085
Scenario: Display formals based on http or https
	Given I execute "http://rsaklfleroy:3142/public/Hello%20World.api"
	When the request returns
	Then the swagger definition should contain "produces" with the values "["application/json","application/xml"]" 

#wolf-1085
Scenario: Display full path on service name
	Given I execute "http://rsaklfleroy:3142/public/Hello%20World.api"
	When the request returns
	Then the swagger definition should contain "paths" with the values "{'serviceName':'Public/Hello World" 