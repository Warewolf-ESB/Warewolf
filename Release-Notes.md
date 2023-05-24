Warewolf-2.8.3 (14 March 2023)
======================

Bug fixes
----------
- Updated the loading of the dependency graph to prevent it loading blank sometimes.
- Patch a bug with loading certain date time formats.
- The server max file size setting is respected more accurately.

Warewolf-2.8.2.9 (3 March 2023)
======================

Enhancements
----------
- A warning is logged if there is less than 10% total memory available.

Bug fixes
----------
- Reduced the installer size.
- patched about "PointCollection".
- Deleted Demo workflows no longer come back when the server restarts.

Warewolf-2.8.2 (16 February 2023)
======================

Enhancements
----------
- Don’t throw error and from sub workflow if error is handled in main workflow

Bug fixes
----------
- Fix POST tool reading escaped JSON into string
- Patched for my.warewolf.io missing some data

Warewolf-2.8.1.63 (19 January 2023)
======================

Bug fixes
----------
- Fix POST tool not reading custom headers issue

Warewolf-2.8.1.54 (13 December 2022)
======================
 
Enhancements
----------
- Add timeout option to POST activity
 
Bug fixes
----------
- Fix PointCollection error on workflow

Warewolf-2.8.1.46 (09 December 2022)
======================
 
Enhancements
----------
- Add checkbox to enable/disable trigger
- Default prefetch count on queue trigger to 1
 
Bug fixes
----------
- Increase timeout on queue trigger process to cater for long running workflows


Warewolf-2.8.1.42 (16 November 2022)
======================

Bug fixes
----------
- Fixed a threading issue on POST tool which was causing duplicate transactions under heavy load/multiple threads	
- Decision with blank match type shows as invalid.
- Advanced Recordset tool SQL validation shows errors in error dialog

2.8.1.21 (17 October 2022)
======================

Bug fixes
----------
- Patch for Studio crash when dropping a comment into a For Each or Select and Apply tool.
- Patch for errors logged when saving an existing WCF source.

2.8.1 (27 September 2022)
======================

Enhancements
----------
- Add functionality to be able to support assigning a variable from object property.

Bug fixes
----------
- Fixed bug where WCF Tool would crash warewolf server.
- Fixed bug with WCF Tool where service would not return data or translate variables.
- Passwords in file tools are no longer scrambled on deploy.
- The advanced recordset tool is now threadsafe.
- The server text log file only shows messages above the threshold set in the studio settings.
- Patched an error that was showing on the studio start page help video.
- Studio connection to the server is more stable at high memory usages.

2.8 (22 July 2022)
======================

Enhancements
----------
- Add prefetch value to help with RabbitMQ consumer performance using trigger queues
- Log data at the DEBUG logging level is now more detailed.

Bug Fixes 
----------
- JSON data input using the debug input window no longer  adds quotes around all property values.

2.7.10 (27 June 2022)
======================

Bug Fixes
----------
- Patched an error starting Warewolf server when ElasticSearch server is not already started.
- Patched redis cache to work with jmeter.

2.7.9 (17 June 2022)
======================

Bug Fixes
----------
- ElasticSearch logging errorlevel has changed to a more detailed level.
- Performance and memory improvements to the suspend tool.

2.7.8 (31 May 2022)
======================

Bug Fixes
----------
- Date/Time tool shows the correct date in the put field of the debug output view.
- Fixed issue where elasticsearch logging wasn't working on source with credentials.

2.7.7 (16 May 2022)
======================

Bug Fixes
----------
- Creating a test from debug defaults to the correct assert and mocks.

Enhancements
----------
- Allow hangfire persistence server to be used as a client or server using a checkbox in the persistance settings.

2.7.6 (2 May 2022)
======================

Bug Fixes
----------
- Patch for logging source settings sometimes not saving.
- Patch for workflow test input values blank sometimes.
- Patch for RabbitMQ tool sometimes not loading password value.

2.7.5.32 (21 April 2022)
======================

Bug Fixes
----------
- Changes to address Suspend tool memory spiking and general improvements to suspend tool.

2.7.5 (31 March 2022)
======================

Bug Fixes
----------
- Patched the debug output for workflows with multiple gate tools. The output appends to the correct gate tool now.
- Suspend tool shouldn't throw a colleciton modified error when processing large amounts of transactions.

2.7.4 (23 March 2022)
======================

Bug Fixes
----------
- Patch bug when using recordset indexes in a ForEach on CSV.
- RabbitMQ trigger can now pass object type variables to Warewolf workflows.
- Trigger queues default concurrency to 0 when saved with a blank concurrency.

2.7.3 (04 March 2022)
======================

Enhancements
----------
- Tests can now be written that manually resume workflows under tests that suspend their execution.

Bug Fixes
----------
Gate Tool
- Number of executions are now in line with the number of retries on failure.

2.7.2.1 (28 February 2022)
======================

Bug Fixes
----------
- Low level performance and stability improvements to the suspend tool.

2.7.1 (11 February 2022)
======================

Enhancements
----------
- Execution variables can now be included with our execution logs when this fearure is enabled.

Bug fixes
----------
- New Test added will now alway be the selected test 

2.7 (21 January 2022)
======================

Bug fixes
----------
- Studio is persistenting the correct input values for workflow with the same name

8 December 2021
======================

Bug fixes
----------
- Hotfix for RabbitMQ connections dropping due to incorrect number concurrent connections being openned

6 December 2021
======================

Enhancements
----------------
Test Coverage
- Coverage summary can now show all the nodes contained by each workflow on a folder path selected on .coverage and .coverage.json
- Child nodes are now included with our coverage percentage and denoted similarly to the Decision tool on our HTML render .coverage

23 November 2021
======================

Bug fixes
----------
- Patched UI for inner activities for workflow tests not rendering
- The error 500 on .coverage run has been resolved for tests containing Switch and Decision nodes
- The manual selection of nodes into test node list and debug output generated tests are now sharing constant behavior
- The Gate tool can be added to a Sequence tool

Enhancements
----------------
Test Coverage
- Child nodes are now include with our coverage percentage and denoted similarly to Decision tool on our HTML render .coverage
- Coverage report shows total mocked nodes count
- Can now run coverage for a single test
- Mocked node now show as covered nodes
- Gate tool can now be added as child node with tools like the Sequence tool, etc. 7
- All tools have been adjusted to toggle the Mock and Assert option.

05 November 2021
======================

Bug fixes
----------
- Deleted workflow tests no longer contribute to overall test coverage
- Fixed a crash that happenned in RabbitMQ queue workers when changing the number of concurrent RabbitMQ queues

Enhancements
------------
- Coverage shows not covered node count

25 October 2021
======================

Bug fixes
----------
- Gate and suspend tools contribute to overall test coverage percentage if they are executed during a test
- @ symbol doesn't get stripped out of assign object tool after the first execution
- Post tool recognizes variables in it's inputs and result textboxes

27 September 2021
======================

Bug fixes
----------
- Total coverage shows correct average of all workflow's coverage
- Coverage for workflows with spaces in their path is showing the correct coverage percentage
- JSON objects recordsets map all fields and arrays to recordsets correctly

13 September 2021
======================

Bug fixes
----------
- Fix For Each bug where suspend only runs once
- Fix duplicate records issue on suspend/resume
- Fix "resume node not found" error on resume

Enhancements
------------
- General performance inmprovements

27 August 2021
======================
Bug fixes
----------
- General Suspend/Resume tool stability
- Suspend/Resume tool can handle more suspended executions simultaneously.

Enhancements
------------
- Resume execution is now logged into the audit log.
- New values added to coverage JSON output.

2 August 2021
======================
Bug fixes
----------
- Fixed an issue with the manual resume tool

Enhancements
------------
- Studio/Server connection stability improvements

23 July 2021
======================
Enhancements
----------------
- Post tool now supports x-www-form-urlencoded format.

Bug fixes
-----------	
- Fixed crash when suspending workflows that contain recursive loops.

15 July 2021
======================
Enhancements
----------------
- Server supports new json and xml formats for running folder and server wide workflow tests and coverage.
- Duplicate UI shows the progress of long running workflow folder duplicate operations.

Bug fixes
-----------	
- Gate tool not showing the correct "Match Type" options.

29 June 2021
================

Enhancements
----------------
Post
- XML web response can now be converted into JSON object.
Duplicate Folder
- Now enables a continuous execution on resource duplication failure.

11 June 2021
================

Bug Fixes
-----------
- The Studio splash page no longer shows "Server Unavailable" and starts up correctly.
- There are no longer duplicates in the Errors().Message recordset
- Improved Performance and Logging on Duplicate Folder functionality.

28 May 2021
================
Enhancements
----------------
General
- Added a 'Show Password' functionality to password fields

Bug Fixes
-----------
Manual Resumption
- Override input variables accepts same variable name as suspended workflow input variables.

Debug
- Resolved 'internal server error' bug when debug in browser option is selected.


18 May 2021
================
Bug Fixes
-----------
Service	
- The service tool is no longer returning duplicated InnerError tagged error messages when using the On Error Recordset.

Input Variables	
- The Input Variables no longer throws non existent object: { input_variable_name } when you have a payload with only one JSON array value.

Debug Input	
- Debug Input View remembers the state of an object.

16 April 2021
================
Enhancements
----------------
General
- Performance enhancements to switching between workflows

Web
- Warewolf server variable’s error and exceptions have been adjusted to omit user value object failures as these can contain sensitive information.
- Posting an empty body to a Warewolf server workflow that uses the body to calculate the response, will now return a more detailed failing object message with failing object name as response.
 

Bug Fixes
-----------
- Debug Input objects are now saving consistently

Web
- Warewolf server variable’s errors have been adjusted to omit execution environment inner error duplicates.

OpenAPI
- OpenAPI is now no longer braking on public requests.

All tools
- On Error is no longer failing and returning an incorrect error message.

30 March 2021
================
Enhancements
----------------
General
- Performance enhancements to the workflows

Manual Resumption
- Objects can be manipulated using the Sequence Tool and passed as inputs to override objects in the suspended workflow

 
Bug Fixes
-----------
Web
- Classic Web Post tool manual body can now Generate Outputs object.
- Web tools with no request payload like the Delete and Get will now not add the auto generated Content-Type : application/json. This fixes  the header misuse errors.

Studio
- Data Sources are now loading on start up

Manual Resumption
- Input variable from the workflow can be passed as inputs into suspended workflow

19 March 2021
================
Enhancements
---------
Web
- Posting an empty body to a Warewolf server workflow that uses the body to calculate the response, will now return a failing object message instead of an empty response.
- Update help text and tooltips.

Logging
- Logging all errors to log server.
- Enriched error messages for user experience.

Bug Fixes
---------
Triggers
- Resolved QueueWorker concurrency not reflecting changes.

Suspend Execution and Manual Resumption
- Resolved Windows authentication connection errors.

Logging
- All error messages are logged per Warewolf Log Level in Warewolf Settings.

5 March 2021
================
Enhancements
-------------
Triggers
	- Throttle number of concurrent threads, increase thread count, improve queue efficiency.


4 March 2021
================
Enhancements
-------------
- Posting an empty body to a Warewolf server workflow that uses the body to calculate the response, will now return an empty response instead of an exception.

Bug Fixes
---------
- File with .txt extension can now be streamed currently into Warewolf server using the form-data stream.


22 February 2021
================
Enhancements
-------------
OpenApi 
   - Generate OpenApi from folder level

Manual Resumption
   - Override individual input variables

Web Post
   - Web Post Tool can now post multi-part form data

Bug Fixes
---------
File Read
File Write
   - Read and Write from UNC path no longer gives safe handle error


9 February 2021
===============
Bug Fixes
---------
Hangfire
   - Fixed error where logs where getting stuck in a processing state
 
Triggers
   -Trigger will now execute within Kubernetes Cluster

19 January 2021
======================
Bug Fixes
---------
RabbitMQPublish
- No longer fails when CorrelationId is null

Suspend Execution Tool
- Date format bug fix

18 January 2021
======================
Bug Fixes
---------
Studio
- Studio no longer freezes when remote server becomes inactive

14 January 2021
======================
Enhancements
------------
Suspend Execution Tool
Manual Resumption Tool:
- Workflow will be resumed with the same user the workflow was suspended with

Bug Fixes
---------
POST and GET Method Tools
- Genarate Outputs no longer returning base64 string

11 February 2021
======================

Allow OpenAPI(Swagger) request at folder level

10 December 2020
======================
Enhancements
---------
Workflow Execution
- 'DispatchForAfterState' log intercepted null debug state as WarewolfInfo and not WarewolfError.

Swagger
- Updated Swagger Specification to OpenApi 3.0.1 Specification.

Web
- Xml+Soap payload can now be parsed to environment variables.

Bug Fixes
---------
Suspend Execution Tool
- Added validation to Save Persistence Settings.
- InputMapping with variable and non-variable expression.

Test Framework
- Resolved equality error on a null recordset.

Web
- POST, PUT, GET and DELETE Method Tools response no longer returning base64 string.
- Data posted from outside Warewolf is no longer converted to base64 string.


9 February 2021
=================
Bug Fixes
---------
Hangfire
   - Fixed error where logs where getting stuck in a processing state

25 November 2020
======================
Features 
------------ 
Suspend Execution Tool: 
  - Ability to suspend the execution of a workflow for a stipulated period.
  - Ability to create test cases where the workflow will execute to completion.
  
Manual Resumption Tool: 
  - Ability to manually resume a previously suspended execution of a workflow.
  - Ability to create test cases where the activity acts as an input and output for validation.

Enhancements
---------
Test Framework
  - Workflow input values now follow through to the test cases.

Bug Fixes
---------
Test Framework
  - Resolved 'Resource not found' when Object passed in as Null input.


Monday 2 November 2020
======================
Enhancements
---------
- Web Put Method can now use a Base64 formatted file content to upload all common media types to a specified destination.

Tuesday 13 October 2020
======================
Enhancements
---------
- Web Get Method can now download all common media types.
- Write File Tool can now use a Base64 formatted file content to write a file to a specified destination.
- Use Cases folder has been added under the Examples folder for more examples of use to the above enhancements.

Friday 28 August 2020
======================
Bug Fixes
---------
- Manual CorrelationID can now evaluate Warewolf variables.
- The log server no longer returns object null error.

Friday 31 July 2020
======================
Features
---------
- Allow triggers to be deployed from the studio.
- Added counts for Tests and Triggers per resource selection to the deploy view.

Enhancements
---------
- Warewolf Logger memory usage improvements.

Friday 17 July 2020
======================
Bug Fixes
---------
- Test Coverage Summary counts are now updated for the selected folder

Monday 13 July 2020
======================
Bug Fixes
---------
- Redis Cache tool - Resolved recordset output not caching correctly.

Features
--------
- Added more logging to the Trigger Queue

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

