Feature: WorkflowExecutionLogging
      In order to get a detailed workflow execution log
      As a warewolf user
      I want to be able to list all entry to exit execution log points

  Scenario: Workflow execution Entry point detailed log
          Given a valid workflow
          And workflow execution entry point detailed logs are logged
          And a workflow stops on error has no logs
          Then a detailed execution completed log entry is created
          |  one  |  two   |three   |
          |value 1| value 2| value 3|

Scenario: Workflow execution entry point detailed logs
          Given a valid workflow
          And workflow execution entry point detailed logs are logged
          When a workflow stops on error
          Then a detailed on error log entry is created
          |  one  |  two   |three   |
          |value 1| value 2| value 3|

Scenario: Workflow execution completed detailed logs
          Given a valid workflow
          And workflow execution entry point detailed logs are logged
          And a workflow stops on error has no logs
          Then a detailed execution completed log entry is created
          |  one  |  two   |three   |
          |value 1| value 2| value 3|