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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Hangfire;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.States;
using Warewolf.Auditing;
using Warewolf.Security.Encryption;
using Dev2JsonSerializer = Dev2.Common.Serializers.Dev2JsonSerializer;
using LogLevel = Warewolf.Logging.LogLevel;

namespace Warewolf.Driver.Persistence.Drivers
{
    public class HangfireScheduler : IPersistenceScheduler
    {
        private IStateNotifier _stateNotifier = null;
        private JobStorage _jobStorage;
        private IBackgroundJobClient _client;

        public HangfireScheduler()
        {
            _jobStorage = new SqlServerStorage(ConnectionString);
            _client = new BackgroundJobClient(_jobStorage);
        }

        public HangfireScheduler(IBackgroundJobClient client, JobStorage jobStorage)
        {
            _jobStorage = jobStorage;
            _client = client;
        }

        public string ResumeJob(IDSFDataObject dsfDataObject, string jobId, bool overrideVariables, string environment)
        {
            try
            {
                var monitoringApi = _jobStorage.GetMonitoringApi();
                var jobDetails = monitoringApi.JobDetails(jobId);
                var jobIsScheduled = jobDetails.History.Where(i =>
                    i.StateName == "Enqueued" ||
                    i.StateName == "Processing" ||
                    i.StateName == "Succeeded" ||
                    i.StateName == "ManuallyResumed");
                if (jobIsScheduled.Any())
                {
                    return GlobalConstants.Failed;
                }

                var values = jobDetails.Job.Args[0] as Dictionary<string, StringBuilder>;
                values.TryGetValue("environment", out StringBuilder persistedEnvironment);
                var decryptEnvironment = persistedEnvironment.ToString().CanBeDecrypted() ? DpapiWrapper.Decrypt(persistedEnvironment.ToString()) : persistedEnvironment.ToString();
                if (overrideVariables)
                {
                    if (values.ContainsKey("environment"))
                    {
                        values["environment"] = new StringBuilder(environment);
                    }
                }
                else
                {
                    values["environment"] = new StringBuilder(decryptEnvironment);
                }

                var workflowResume = new WorkflowResume();
                var result = workflowResume.Execute(values, null);
                var serializer = new Dev2JsonSerializer();
                var executeMessage = serializer.Deserialize<ExecuteMessage>(result);
                if (executeMessage.HasError)
                {
                    var failedState = new FailedState(new Exception(executeMessage.Message?.ToString()));
                    _client.ChangeState(jobId, failedState, ScheduledState.StateName);
                    return GlobalConstants.Failed;
                }

                values.TryGetValue("resourceID", out StringBuilder workflowId);
                values.TryGetValue("environment", out StringBuilder environments);
                values.TryGetValue("startActivityId", out StringBuilder startActivityId);
                values.TryGetValue("versionNumber", out StringBuilder versionNumber);

                _stateNotifier = dsfDataObject.StateNotifier;
                var audit = new Audit
                {
                    WorkflowID = workflowId?.ToString(),
                    Environment = environments?.ToString(),
                    VersionNumber = versionNumber?.ToString(),
                    NextActivityId = startActivityId?.ToString(),
                    AuditDate = DateTime.Now,
                    AuditType = "LogResumeExecutionState",
                    LogLevel = LogLevel.Info
                };

                _stateNotifier?.LogAdditionalDetail(audit, nameof(ResumeJob));
                var manuallyResumedState = new ManuallyResumedState(environments?.ToString());
                var currentState = jobDetails.History.OrderBy(s => s.CreatedAt).LastOrDefault();
                _client.ChangeState(jobId, manuallyResumedState, currentState?.StateName);
                return GlobalConstants.Success;
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(ResumeJob), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.Message);
            }
        }

        public string ScheduleJob(enSuspendOption suspendOption, string suspendOptionValue, Dictionary<string, StringBuilder> values)
        {
            var suspensionDate = DateTime.Now;
            var resumptionDate = CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            var state = new ScheduledState(resumptionDate.ToUniversalTime());

            var jobId = _client.Create(() => ResumeWorkflow(values, null), state);
            return jobId;
        }


        [ExcludeFromCodeCoverage]
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public string ResumeWorkflow(Dictionary<string, StringBuilder> values, PerformContext context)
        {
            try
            {
                //This method is intercepted in the HangfireServer Performing method
                return GlobalConstants.Success;
            }
            catch (Exception)
            {
                return GlobalConstants.Failed;
            }
        }

        private string ConnectionString
        {
            get
            {
                var payload = Config.Persistence.PersistenceDataSource.Payload;
                if (Config.Persistence.EncryptDataSource)
                {
                    payload = payload.CanBeDecrypted() ? DpapiWrapper.Decrypt(payload) : payload;
                }

                var source = new Dev2JsonSerializer().Deserialize<DbSource>(payload);
                return source.ConnectionString;
            }
        }

        public DateTime CalculateResumptionDate(DateTime persistenceDate, enSuspendOption suspendOption, string scheduleValue)
        {
            var resumptionDate = DateTime.UtcNow;
            switch (suspendOption)
            {
                case enSuspendOption.SuspendForDays:
                    resumptionDate = persistenceDate.AddDays(int.Parse(scheduleValue));
                    break;
                case enSuspendOption.SuspendForMonths:
                    resumptionDate = persistenceDate.AddMonths(int.Parse(scheduleValue));
                    break;
                case enSuspendOption.SuspendUntil:
                    resumptionDate = DateTime.Parse(scheduleValue);
                    break;
                case enSuspendOption.SuspendForHours:
                    resumptionDate = persistenceDate.AddHours(int.Parse(scheduleValue));
                    break;
                case enSuspendOption.SuspendForMinutes:
                    resumptionDate = persistenceDate.AddMinutes(int.Parse(scheduleValue));
                    break;
                case enSuspendOption.SuspendForSeconds:
                    resumptionDate = persistenceDate.AddSeconds(int.Parse(scheduleValue));
                    break;
            }

            return DateTime.ParseExact(resumptionDate.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), GlobalConstants.Dev2DotNetDefaultDateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}