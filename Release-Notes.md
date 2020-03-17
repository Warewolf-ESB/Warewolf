
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

Thursday 12 March 2020
======================

Features
--------
- Warewolf-Execution-Id can be read from headers of workflows for collating logs across servers and sub-workflows. It is written into the header of subsequent sub-workflows.
- Warewolf-Custom-Transaction-Id can be read from headers of workflows for collating logs across servers and sub-workflows. 
  It is written into the header of subsequent sub-workflows.
- Added Warewolf-Custom-Transaction-Id to the UI for RabbitMQ Publish. User can decide to use ExecutionID of the workflow, an existing Warewolf-Custom-Transaction-Id or a new Warewolf-Custom-Transaction-Id .
