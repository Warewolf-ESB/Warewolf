Feature: PublishRabbitMQ
	In order to publish a message to a queue
	As a Warewolf user
	I want a tool that performs this action

Scenario: Open new RabbitMQ Publish Tool
	Given I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	And Edit Button is Disabled
	And RabbitMQ Source is Enabled
	And Queue Name is Enabled
	And Message is Enabled
	And Result is Enabled
	When I Click New Source
	Then the New RabbitMQ Publish Source window is opened
	
Scenario: Editing RabbitMQ Publish Tool Source
	Given I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	And Edit Button is Disabled
	And RabbitMQ Source is Enabled
	And Queue Name is Enabled
	And Message is Enabled
	And Result is Enabled
	When I Select "Test (localhost)" as the Rabbit source
	Then Edit Button is Enabled
	And I Click Edit Source
	Then the "Test (localhost)" RabbitMQ Publish Source window is opened

Scenario: Change RabbitMQ Publish Source
	Given I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	And Edit Button is Disabled
	And RabbitMQ Source is Enabled
	And Queue Name is Enabled
	And Message is Enabled
	And Result is Enabled
	When I Select "Test (localhost)" as the Rabbit source
	Then Edit Button is Enabled
	And I set Message equals "test123"
	And I set Queue Name equals "Testing Publish"
	When I change RabbitMq source from "Test (localhost)" to "BackupSource"
	Then Message equals "test123"
	Then Queue Name equals "Testing Publish"
