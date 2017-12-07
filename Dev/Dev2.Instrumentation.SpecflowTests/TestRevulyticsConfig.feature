Feature: TestRevulyticsConfig
	This test will read the app config and create revulytics
	config.IF any key is not provided it will throw argument null
	exception

@TestConfig
Scenario: Test revultyics config
	Given I have revulytics config and i will read the keys
	And I will create revulytics configuration
	Then I will check whether result is equal to ok
