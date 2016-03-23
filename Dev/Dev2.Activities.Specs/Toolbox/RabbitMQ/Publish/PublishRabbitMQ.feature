Feature: PublishRabbitMQ
	In order to publish a message to a queue
	As a Warewolf user
	I want a tool that performs this action

Scenario: Open new Rabbit MQ Publish
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	And Edit Button is Disabled
	When I Select "Test RabbitMQ Source" as a Rabbit Source
	Then The QueueName is enabled is Enabled
	Then The Message is enabled is Enabled
	Then The Result is enabled is Enabled

Scenario: Open new Rabbit MQ Publish and Create New Source
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	When I Click New Source
	Then A new tab is opened