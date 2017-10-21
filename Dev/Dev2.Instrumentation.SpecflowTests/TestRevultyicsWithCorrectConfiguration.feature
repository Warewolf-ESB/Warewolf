Feature: TestRevultyicsWithCorrectConfiguration
	In order to avoid any incorrect configuration
	This test will check the status of revultyics based on 
	the appconfig keys.

@testRevulyticsWithCorrectConfiguration
Scenario: Revulytics configuration validation
	Given I have app config file I will read the revulytics app config keys
	And I will create revultyics configuration
	Then I will check the status of revulytics configuration
