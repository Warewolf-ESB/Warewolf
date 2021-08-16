/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Warewolf.Auditing;
using Warewolf.Driver.Resume;
using Warewolf.Execution;
using Warewolf.HangfireServer;

namespace HangfireServer
{
    // TODO: Refactor Hangfire implementations into separate wrappers.
    // TODO: All these methods should be tested
    public class ResumptionAttribute : JobFilterAttribute, IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private static readonly ILog _hangfireLogger = LogProvider.GetCurrentClassLogger();
        private readonly IExecutionLogPublisher _logger;
        private readonly IResumptionFactory _resumptionFactory;
        
        public ResumptionAttribute(IExecutionLogPublisher logger, IResumptionFactory resumptionFactory)
        {
            _logger = logger;
            _resumptionFactory = resumptionFactory;
        }

        public void OnCreating(CreatingContext context)
        {
            _hangfireLogger.InfoFormat("Creating a job based on method {0}...", context.Job.Method.Name);
            _logger.Info("Creating a job based on method {"+ context.Job.Method.Name + "}...");
        }

        public void OnCreated(CreatedContext context)
        {
            _hangfireLogger.InfoFormat(
                "Job that is based on method {0} has been created with id {1}",
                context.Job.Method.Name,
                context.BackgroundJob?.Id);
            _logger.Info("Job that is based on method {"+ context.Job.Method.Name + "} has been created with id {"+ context.BackgroundJob?.Id + "}");
        }

        public void OnPerforming(PerformingContext context)
        {
            if (context is null)
            {
                _logger.Error("Failed to perform jobPerformingContext is null");
                return;
            }

            //OnPerformResume(context);
        }

        public void OnPerformResume(PerformingContext context)
        {
            var resumeWorkflow = new WarewolfResumeWorkflow(_logger, context, _resumptionFactory);
            resumeWorkflow.PerformResumptionAsync();
        }

        public void OnPerformed(PerformedContext context)
        {
            _hangfireLogger.InfoFormat("Job {0} has been performed", context.BackgroundJob.Id);
            _logger.Info("Job {"+ context.BackgroundJob.Id + "} has been performed ");

            if(context.Exception != null)
            {
                LogJobPerfomedOnSchedulerException(context.BackgroundJob, context.Exception, "HasException", context.Result.ToString());
            }
            if (context.Canceled)
            {
                LogJobPerfomedOnSchedulerException(context.BackgroundJob, context.Exception, "WasCanceled", context.Result.ToString());
            }
            if (context.ExceptionHandled)
            {
                LogJobPerfomedOnSchedulerException(context.BackgroundJob, context.Exception, "ExceptionHandled", context.Result.ToString());
            }

        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                _hangfireLogger.WarnFormat(
                    "Job {0} has been failed due to an exception {1}",
                    context.BackgroundJob.Id,
                    failedState.Exception);
                _logger.Warn("Job {" + context.BackgroundJob.Id + "} has been failed due to an exception {" + failedState.Exception + "}");

                if (IsReapiting(context.TraversedStates, "Failed"))
                {
                    LogJobPerfomedOnSchedulerException(context.BackgroundJob, failedState.Exception, context.CandidateState.ToString(), context.TraversedStates.ToString());
                    _logger.Warn("Job {" + context.BackgroundJob.Id + "} has been failed before and again now due to {" + failedState.Exception + "}, TraversedStates: {" + context.TraversedStates.ToString() + "}");
                }
            }
            
            if (IsReapiting(context.TraversedStates, "Enqueued"))
            {
                LogJobPerfomedOnSchedulerException(context.BackgroundJob, null, context.CandidateState.ToString(), context.TraversedStates.ToString());
                _logger.Warn("Job {" + context.BackgroundJob.Id + "} has been enqueued before and again now due to {" + context.CandidateState.Reason + "}, TraversedStates: {"+ context.TraversedStates.ToString() +"}");
            }

        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _hangfireLogger.InfoFormat(
                "Job {0} state was changed from {1} to {2}",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);

            _logger.Info("Job {"+ context.BackgroundJob.Id + "} state was changed from {"+ context.OldStateName + "} to {"+ context.NewState.Name + "}");
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _hangfireLogger.WarnFormat(
                "Job {0} state {1} was not applied.",
                context.BackgroundJob.Id,
                context.OldStateName);

            _logger.Warn("Job {"+ context.BackgroundJob.Id + "} state {"+ context.OldStateName + "} was not applied.");
        }

        //Note: this can be done better later
        private void LogJobPerfomedOnSchedulerException(BackgroundJob backgroundJob, Exception exception, string schedulerState, string schedulerAdditionalDetail)
        {
            var jobArg = backgroundJob.Job.Args[0];
            var values = jobArg as Dictionary<string, StringBuilder>;

            values.TryGetValue("resourceID", out var resourceId);
            values.TryGetValue("startActivityId", out var startActivityId);
            values.TryGetValue("versionNumber", out var versionNumber);
            values.TryGetValue("currentuserprincipal", out var currentuserprincipal);

            _logger.LogResumedExecution(new Audit
            {
                AuditDate = DateTime.Now,
                WorkflowID = resourceId?.ToString(),
                Environment = string.Empty,
                VersionNumber = versionNumber?.ToString(),
                NextActivityId = startActivityId?.ToString(),
                AuditType = $"LogResumeExecutionState",
                LogLevel = (Warewolf.Logging.LogLevel)LogLevel.Info,
                ExecutingUser = currentuserprincipal?.ToString(),
                Exception = exception,
                Status = schedulerState,
                AdditionalDetail = schedulerAdditionalDetail
            });
        }

        private static bool IsReapiting(IState[] context, string status)
        {
            return context.Where(o => o.Name == status)
                    .FirstOrDefault() != null;
        }
    }

}