Feature: PublishRabbitMQ
	In order to publish a message to a queue
	As a Warewolf user
	I want a tool that performs this action

Scenario: Open new RabbitMQ Publish and Select a source
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	And Edit Button is Disabled
	When I Select "Test (localhost)" as a Rabbit Source
	Then The QueueName and Message and Result are Enabled

Scenario: Open new Rabbit MQ Publish and Create New Source
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	When I Click New Source
	Then A new tab is opened
