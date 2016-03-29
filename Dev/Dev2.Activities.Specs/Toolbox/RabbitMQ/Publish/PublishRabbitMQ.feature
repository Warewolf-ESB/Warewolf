Feature: PublishRabbitMQ
	In order to publish a message to a queue
	As a Warewolf user
	I want a tool that performs this action

Scenario: Open new RabbitMQ Publish and Select a source
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	And Edit Button is Disabled
	And The QueueName and Message and Result are Disabled
	When I Select "Test (localhost)" as a Rabbit Source
	Then The QueueName and Message and Result are Enabled
	And Edit Button is Enabled

Scenario: Open new RabbitMQ Publish and Create new source
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	When I Click New Source
	Then CreateNewSource is executed

Scenario: Open new RabbitMQ Publish and Select a source and Edit
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
    And New Button is Enabled
	And Edit Button is Disabled
	And I Select "Test (localhost)" as a Rabbit Source
	And Edit Button is Enabled
	When I Click Edit Source
	Then EditSource is executed