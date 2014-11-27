Feature: WebService
	In order to use data from a webservice in my workflow
	As a Warewolf user
	I want a service which can retrieve webservice data for use

@Webservice_MappingJsonWithPrimitiveArrays_10641
Scenario: A webservice that returns a primitive array
	Given I have a webservice
	And the webservice returns JSON with a primitive array
	When the mapping is generated
	Then the mapping should contain the primitive array

@ignore
@Webservice_ExecutingJsonWithPrimitiveArrays_10641
Scenario: Execute webservice which returns a primitive array
	Given I have a webservice calling http://maps.googleapis.com/maps/api/geocode/json?sensor=true&amp;address=address
	When the service is executed
	Then I have the following data
		| formatted_address            | types                                    | geometrylocation_type | geometrylocationlat | geometrylocationlng | geometryviewportnortheastlat | geometryviewportnortheastlng | geometryviewportsouthwestlat | geometryviewportsouthwestlng |
		| Address, 28039 Madrid, Spain | point_of_interest,hospital,establishment | APPROXIMATE           | 40.447337           | -3.7070179          | 40.4486859802915             | -3.7056689197085             | 40.4459880197085             | -3.7083668802915             |


Scenario Outline: Apply JsonPath to payload
	Given I have a webservice with '<response file>' as a response
	When I apply '<path>' to the response
	Then the mapping should be '<mapping>'
	Examples: 
		| response file              | path                | mapping                              |
		| simple json.txt            | $.store.book[*]     | UnnamedArrayData().category          |
		| LargeWebServicePayLoad.txt | $.return.markets[*] | UnnamedArrayData().recenttrades().id |