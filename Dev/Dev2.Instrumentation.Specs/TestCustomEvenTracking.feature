Feature: TestCustomEvenTracking
	This test is to be done for tracking event in custom category
 of revulytics logging

@TestCustomEvenTracking
Scenario:TestCustomApplicationEvent

	Given I have application Tracker instance 
	And I will enable application tracking by calling method
	When I will call track custom event method	
	Then I will disable application tracking by calling method
