/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Collections.Generic;
using System.Text;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Warewolf.Auditing;
using Warewolf.Driver.Resume;
using LogLevel = Warewolf.Logging.LogLevel;

namespace Warewolf.HangfireServer
{
    public class ResumptionAttribute : JobFilterAttribute, IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IExecutionLogPublisher _logger;

        public ResumptionAttribute(IExecutionLogPublisher logger)
        {
            _logger = logger;
        }

        public void OnCreating(CreatingContext context)
        {
            Logger.InfoFormat("Creating a job based on method {0}...", context.Job.Method.Name);
        }

        public void OnCreated(CreatedContext context)
        {
            Logger.InfoFormat(
                "Job that is based on method {0} has been created with id {1}",
                context.Job.Method.Name,
                context.BackgroundJob?.Id);
        }

        public void OnPerforming(PerformingContext context)
        {
            var resumptionFactory = new ResumptionFactory();
            var resumption = resumptionFactory.New();
            if (resumption.Connect())
            {
                var values = context.BackgroundJob.Job.Args[0] as Dictionary<string, StringBuilder>;
                LogResumption(values);
                resumption.Resume(values);
            }
            else
            {
                _logger.Error("Failed to perform job {0}, could not establish a connection.", context.BackgroundJob.Id);
            }
        }

        private void LogResumption(Dictionary<string, StringBuilder> values)
        {
            values.TryGetValue("resourceID", out StringBuilder workflowId);
            values.TryGetValue("environment", out StringBuilder environment);
            values.TryGetValue("startActivityId", out StringBuilder startActivityId);
            values.TryGetValue("versionNumber", out StringBuilder versionNumber);

            var audit = new Audit
            {
                WorkflowID = workflowId?.ToString(),
                Environment = environment?.ToString(),
                VersionNumber = versionNumber?.ToString(),
                NextActivityId = startActivityId?.ToString(),
                AuditDate = DateTime.Now,
                AuditType = "LogResumeExecutionState",
                LogLevel = LogLevel.Info
            };
            _logger.LogResumedExecution(audit);
        }

        public void OnPerformed(PerformedContext context)
        {
            Logger.InfoFormat("Job {0} has been performed", context.BackgroundJob.Id);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                Logger.WarnFormat(
                    "Job {0} has been failed due to an exception {1}",
                    context.BackgroundJob.Id,
                    failedState.Exception);
                _logger.Warn(
                    "Job {0} has been failed due to an exception {1}",
                    context.BackgroundJob.Id,
                    failedState.Exception);
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.InfoFormat(
                "Job {0} state was changed from {1} to {2}",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.InfoFormat(
                "Job {0} state {1} was unapplied.",
                context.BackgroundJob.Id,
                context.OldStateName);
        }
    }
}