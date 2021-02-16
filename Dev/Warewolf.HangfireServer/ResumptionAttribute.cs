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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Warewolf.Auditing;
using Warewolf.Driver.Resume;
using Warewolf.Execution;
using LogLevel = Warewolf.Logging.LogLevel;

namespace HangfireServer
{
    // TODO: Refactor Hangfire implementations into separate wrappers.
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
        }

        public void OnCreated(CreatedContext context)
        {
            _hangfireLogger.InfoFormat(
                "Job that is based on method {0} has been created with id {1}",
                context.Job.Method.Name,
                context.BackgroundJob?.Id);
        }

        [ExcludeFromCodeCoverage]
        public void OnPerforming(PerformingContext context)
        {
            if (context is null)
            {
                _logger.Error("Failed to perform jobPerformingContext is null");
                return;
            }

            OnPerformResume(context);
        }

        public void OnPerformResume(PerformingContext context)
        {
            try
            {
                var jobArg = context.BackgroundJob.Job.Args[0];
                var backgroundJobId = context.BackgroundJob.Id;
                var resumptionFactory = _resumptionFactory ?? new ResumptionFactory();
                var resumption = resumptionFactory.New();
                if (resumption.Connect(_logger))
                {
                    _logger.Info("Performing Resume of job {0}, connection established.", backgroundJobId);
                    var values = jobArg as Dictionary<string, StringBuilder>;
                    LogResumption(values);
                    var result = resumption.Resume(values);
                    if (result.HasError)
                    {
                        throw new InvalidOperationException(result.Message?.ToString(), new Exception(result.Message?.ToString()));
                    }
                }
                else
                {
                    _logger.Error("Failed to perform job {0}, could not establish a connection.", backgroundJobId);
                    throw new InvalidOperationException("Failed to perform job " + backgroundJobId + " could not establish a connection.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to perform job {0}" + ex.InnerException);
                throw;
            }
        }

        private void LogResumption(Dictionary<string, StringBuilder> values)
        {
            values.TryGetValue("resourceID", out StringBuilder workflowId);
            values.TryGetValue("environment", out StringBuilder environment);
            values.TryGetValue("startActivityId", out StringBuilder startActivityId);
            values.TryGetValue("versionNumber", out StringBuilder versionNumber);
            values.TryGetValue("currentuserprincipal", out StringBuilder currentuserprincipal);
            var audit = new Audit
            {
                WorkflowID = workflowId?.ToString(),
                Environment = environment?.ToString(),
                VersionNumber = versionNumber?.ToString(),
                NextActivityId = startActivityId?.ToString(),
                AuditDate = DateTime.Now,
                AuditType = "LogResumeExecutionState",
                LogLevel = LogLevel.Info,
                ExecutingUser = currentuserprincipal.ToString()
            };
            _logger.LogResumedExecution(audit);
        }

        public void OnPerformed(PerformedContext context)
        {
            _hangfireLogger.InfoFormat("Job {0} has been performed", context.BackgroundJob.Id);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                _hangfireLogger.WarnFormat(
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
            _hangfireLogger.InfoFormat(
                "Job {0} state was changed from {1} to {2}",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _hangfireLogger.InfoFormat(
                "Job {0} state {1} was not applied.",
                context.BackgroundJob.Id,
                context.OldStateName);
        }
    }
}