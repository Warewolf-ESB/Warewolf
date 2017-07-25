@Utility
Feature: ConsumeRabbitMQ
	In order to consume a message from a queue
	As a Warewolf user
	I want a tool that performs this action

Scenario: I Create a new Consume tool
Given I create New Workflow
	And I drag RabbitMQConsume tool onto the design surface    
	And EditButton is Disabled
	And RabbitMq Source is Enabled
	And Queue Name is disabled
	And Prefech is disabled
	And ReQueue is disabled
	And New button is Enabled	
	When I click new source
	Then the RabbitMQ Source window is opened

Scenario: Editing existing consume tool
	Given I create New Workflow
	And I drag RabbitMQConsume tool onto the design surface    
	And EditButton is Disabled
	And RabbitMq Source is Enabled
	And Queue Name is disabled
	And Prefech is disabled
	And ReQueue is disabled
	And New button is Enabled	
	When I select "localhost" from source list as the source
	Then EditButton is Enabled
	And I click Edit source
	Then the RabbitMQ Source window is opened with localhost source

Scenario: Editing existing consume tool then changing values
Given I create New Workflow
	And I drag RabbitMQConsume tool onto the design surface    
	And EditButton is Disabled
	And RabbitMq Source is Enabled
	And Queue Name is disabled
	And Prefech is disabled
	And ReQueue is disabled
	And New button is Enabled	
	When I select "localhost" from source list as the source
	Then EditButton is Enabled
	And I click Edit source	
	And I set QueueName to "TestingQueue"
	When I change the source from "localhost" to "NewSource"
	Then QueueName equals "anotherQueue"
	Then QueueName equals "TestingQueue"

Scenario: Executing without selecting the source
Given I execute tool without a source
	And I drag RabbitMQConsume tool onto the design surface 	
	And EditButton is Disabled
	And RabbitMq Source is Enabled
	And Queue Name is disabled
	And Prefech is disabled
	And ReQueue is disabled
	And New button is Enabled	
	When I hit F-six to execute tool 
	Then Empty source error is Returned

Scenario: Executing using non-existing Queue
Given I drag RabbitMQConsume tool onto the design surface    
	And EditButton is Disabled
	And RabbitMq Source is Enabled
	And Queue Name is disabled
	And Prefech is disabled
	And ReQueue is disabled
	And New button is Enabled	
	When I click new source
	Then the RabbitMQ Source window is opened		
	And I add the new source
	When I hit F-six to execute tool  
	Then No queue error is Returned
