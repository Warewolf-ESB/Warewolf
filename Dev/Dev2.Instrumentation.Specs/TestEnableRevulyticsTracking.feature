Feature: TestEnableRevulyticsTracking
	This test will chceck the status of revulytics tracker to be running

@TestEnableTracking
Scenario: Test Enable Revulytics tracking
	Given I have revulytics instance 
	And I will call the EnableAppplicationTracker method
	Then I will check the status of revulytics tracker
