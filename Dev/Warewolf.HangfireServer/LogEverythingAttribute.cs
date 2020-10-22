/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System.Collections.Generic;
using System.Text;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Warewolf.Driver.Resume;

namespace Warewolf.HangfireServer
{
    public class LogEverythingAttribute : JobFilterAttribute, IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IExecutionLogPublisher _logger;

        public LogEverythingAttribute(IExecutionLogPublisher logger)
        {
            _logger = logger;
        }

        public void OnCreating(CreatingContext context)
        {
            Logger.InfoFormat("Creating a job based on method `{0}`...", context.Job.Method.Name);
            _logger.Info("Creating a job based on method `{0}`...", context.Job.Method.Name);
        }

        public void OnCreated(CreatedContext context)
        {
            Logger.InfoFormat(
                "Job that is based on method `{0}` has been created with id `{1}`",
                context.Job.Method.Name,
                context.BackgroundJob?.Id);
            _logger.Info(

                "Job that is based on method `{0}` has been created with id `{1}`",
                context.Job.Method.Name,
                context.BackgroundJob?.Id);
        }

        public void OnPerforming(PerformingContext context)
        {
            Logger.InfoFormat("Starting to perform job `{0}`", context.BackgroundJob.Id);
            _logger.Info("Starting to perform job `{0}`", context.BackgroundJob.Id);

            var resumptionFactory = new ResumptionFactory();
            var resumption = resumptionFactory.New();
            if (resumption.Connect())
            {
                var values = context.BackgroundJob.Job.Args[0] as Dictionary<string, StringBuilder>;
                _logger.StartExecution("Starting process to resume workflow `{0}`", values);
                var result = resumption.Resume(values);
            }
            else
            {
                _logger.Error("Failed to perform job `{0}`, could not establish a connection.", context.BackgroundJob.Id);
            }
        }

        public void OnPerformed(PerformedContext context)
        {
            Logger.InfoFormat("Job `{0}` has been performed", context.BackgroundJob.Id);
            _logger.Info("Job `{0}` has been performed", context.BackgroundJob.Id);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                Logger.WarnFormat(
                    "Job `{0}` has been failed due to an exception `{1}`",
                    context.BackgroundJob.Id,
                    failedState.Exception);
                _logger.Warn(
                    "Job `{0}` has been failed due to an exception `{1}`",
                    context.BackgroundJob.Id,
                    failedState.Exception);
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.InfoFormat(
                "Job `{0}` state was changed from `{1}` to `{2}`",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);
            _logger.Info(
                "Job `{0}` state was changed from `{1}` to `{2}`",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.InfoFormat(
                "Job `{0}` state `{1}` was unapplied.",
                context.BackgroundJob.Id,
                context.OldStateName);
            _logger.Info(
                "Job `{0}` state `{1}` was unapplied.",
                context.BackgroundJob.Id,
                context.OldStateName);
        }
    }
}