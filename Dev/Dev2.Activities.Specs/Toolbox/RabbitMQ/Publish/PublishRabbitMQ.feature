Feature: PublishRabbitMQ
	In order to publish a message to a queue
	As a Warewolf user
	I want a tool that performs this action

Scenario: Open new Rabbit MQ Publish and Select a source
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

Scenario: Publish message with no Queue Name
	Given I open New Workflow
	And I drag RabbitMQPublish tool onto the design surface
	And I Select "Test (localhost)" as a Rabbit Source
	When the publish rabbitMQ tool is executed
	Then the result will be ""
	And the execution has "AN" error
	And the debug inputs as  
	| Queue Name    | Message	|
	| ""			| "Test123" |
	And the debug output as 
	|               |
	| [[result]] =  |