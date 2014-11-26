@ignore
Feature: PluginServiceExecution
	In order to use PluginService 
	As a Warewolf user
	I want a tool that calls the PluginServices into the workflow

@mytag
Scenario:Executing PluginService using input data
	     Given I have a PluginService "emails"
		 And the input host is "smtp.gmail.com" 
		 And the input port is "25" 
		 And the input from is "warewolfteam@dev2.co.za" 
		 And the input to is "development@dev2.co.za" 
		 And the input subject is "email from dev2"
		 And the input body is "email from warewolf plugin"
	     And the output is mapped as "[[result]]"
	     When the Service is executed
	     Then the execution has "NO" error
		 And the debug input as 
		 | Host           | Port | From Account            | To Account             | Subject         | Body                       |
		 | smtp.gmail.com | 25   | warewolfteam@dev2.co.za | development@dev2.co.za | email from dev2 | email from warewolf plugin |
		 And the debug output as 
         |[[result]] = Email sent from warewolfteam@dev2.co.za|
		

Scenario:Executing PluginService with recordset as input data
	     Given I have a PluginService "emails"
		 And the input host is "[[rec(*).a]]" 
		 And the input port is "[[res(*).a]]" 
		 And the input from is "warewolfteam@dev2.co.za" 
		 And the input to is "development@dev2.co.za" 
		 And the input subject is "email from dev2"
		 And the input body is "email from warewolf plugin"
	     And the output is mapped as "[[result]]"
	     When the Service is executed
	     Then the execution has "AN" error
		 And the debug input as 
		 | Host         | Port         | From Account            | To Account             | Subject         | Body                       |
		 | [[rec(*).a]] | [[res(*).a]] | warewolfteam@dev2.co.za | development@dev2.co.za | email from dev2 | email from warewolf plugin |
		 And the debug output as 
         |[[result]] = |


Scenario:Executing PluginService with scalar as input data
	     Given I have a PluginService "emails"
		 And the input host is "[[scalar]]" 
		 And the input port is "25" 
		 And the input from is "warewolfteam@dev2.co.za" 
		 And the input to is "development@dev2.co.za" 
		 And the input subject is "email from dev2"
		 And the input body is "email from warewolf plugin"
	     And the output is mapped as "[[result]]"
	     When the Service is executed
	     Then the execution has "AN" error
		 And the debug input as 
		 | Host         | Port         | From Account            | To Account             | Subject         | Body                       |
		 | [[rec(*).a]] | [[res(*).a]] | warewolfteam@dev2.co.za | development@dev2.co.za | email from dev2 | email from warewolf plugin |
		 And the debug output as 
         |[[result]] = |

Scenario: Executing a puginservice with invalid input data
         Given I have a PluginService "IntegrationTestPluginEmptyToNull"
		 And the input sender is "warewolf@dev2.co.za"
		 And the input testtype is "warewolf@dev2.co.za"
		 And the output is mapped as "[[result]]"
		 When the Service is executed
		 Then the execution has "AN" error
		 And the debug input as 
		 | Sender              | TestType            |
		 | warewolf@dev2.co.za | warewolf@dev2.co.za |
		 And the debug output as
		 |[[result]] = |