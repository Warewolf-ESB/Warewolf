
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

