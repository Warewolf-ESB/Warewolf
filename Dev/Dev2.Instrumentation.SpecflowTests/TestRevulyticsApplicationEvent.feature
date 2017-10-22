Feature: TestRevulyticsApplicationEvent
	This test is to be done for tracking application event in 
	revulytics logging


@TestApplicationEvent
Scenario: Test Application event logging in revultytics
	Given I have Application Tracker instance 
	And I will enable application tracking
	When I call the track event method with event category and event name
	Then I will disable application tracking

