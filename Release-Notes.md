Monday 29 June 2020
======================
Features
--------
- The logging data source has the option not to be encrypted in the settings file. Default is to encrypt the data source.
- Log Levels for Execution Logging can now be configured in Logging Settings. Default is DEBUG.
- EnableDetailedLogging in serverSettings is now defaulted to true.

Bug Fixes
---------
- Test Coverage
  ~ Test Coverage summary now no longer throws when a coverage report is required for a non-existing workflow.
  ~ Complexed nodes like decision and switch tools no longer throws when a test coverage report is requested.
  ~ All tests are accounted for and the invalid tests have been included in the summary count.
  ~ When test type is changed -from the test editor- from Assert to Mock this will now update the Test Coverage reports coverage percentage.

- Run Tests
  ~ The invalid tests are now returned with a message stating the reason and the result as invalid.


Monday 8 June 2020
======================
Bug Fixes
---------
- Validate SearchIndex on the Elasticsearch Source.
- Fixed the log levels for execution logging.


Tuesday 21 April 2020
======================
Features
--------
- Test Coverage can now be run on workflows, folders containing workflows and on the host. 
	~ Run All Tests 
          - works for workflow, all workflows in a selected folder, and all workflow in a selected host
          - right-click workflow, folder, or host from Warewolf studio
          - {workflowpath}.tests when accessing from url
          - updated the json format
          - each test run creates a coverage report

       ~ Run Coverage
         - works for workflow, all workflows in a selected folder, and all workflow in a selected host.
         - right-click workflow, folder, or host from Warewolf studio
         - {workflowpath}.coverage when accessing from url

- Logging has been enhanced. 
	~ All workflow executions are logged if EnableDetailedLogging has been set to true in serverSettings.json.
	~ Elasticsearch 7.6 can now be used as a logging source. The default is still Sqlite. 
		1. Download and install Elasticsearch with Docker: https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html
		2. Create an Elasticsearch data source in warewolf.
	  	3. In settings select the Elasticsearch source created from the dropdown and save. Default in the dropddown referres to Sqlite.
	  Note: If the logging source is changed, the server will need to be restarted.

Bug Fixes
---------
- Studio can now start even if there are corrupt workflows in the active workspace


Thursday 12 March 2020
======================

Features
--------
- ExecutionId of workflows is carried across remote workflow executions
- Warewolf-Custom-Transaction-Id, when this header is set the value will optionally be used for RabbitMQ correlation
- Added Warewolf-Custom-Transaction-Id to the UI for RabbitMQ Publish. User can decide to use ExecutionID of the workflow, an existing Warewolf-Custom-Transaction-Id or a new Warewolf-Custom-Transaction-Id.
- Object assign tool can now deserialize json arrays

Bug Fixes
---------
- Numbers starting with "0." are no longer converted to strings
- Numbers ending in 0 are no longer converted to strings
- When assigning values in Object assign tool, do not lose type information
- DsfNumberFormatActivity should always save strings
- Removed a field IsGate from activities as the studio was failing to load them
	~ Workflows created on previous versions of Warewolf after 2.3.1 and before 2.4.3 are affected.


Tue Feb 18 2020
===============

Features
--------
- Gate Tool - ability to retry sections of a workflow
- Redis Cache Tool - Cache another tools outputs
- Redis Cache Delete Tool -  delete a cache entry by name

Mon Jan 20 2020
===============

Bug Fixes
---------
- Queue Trigger View does not show scrollbars when on a smaller screen
- RabbitMQ error was not posting to Dead Letter Queue
- Failures on execution were not being posted to the trigger queue execution history
- Double restart of the trigger when changes are saved.
- Connection errors when the QueueWorker communicated with the Logger.
- Queue Trigger View does not show scrollbars when on a smaller screen

