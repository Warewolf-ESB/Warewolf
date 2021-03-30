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
using System.Linq;
using System.Security.Principal;
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
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;
using Dev2JsonSerializer = Dev2.Common.Serializers.Dev2JsonSerializer;
using LogLevel = Warewolf.Logging.LogLevel;

namespace Warewolf.Driver.Persistence.Drivers
{
    public class PersistedValues : IPersistedValues
    {
        public Guid ResourceId { get; set; }
        public int VersionNumber { get; set; }
        public string SuspendedEnvironment { get; set; }
        public Guid StartActivityId { get; set; }
        public IPrincipal ExecutingUser { get; set; }
    }
    //TODO: This class needs to be refactored to the same standard. We either throw an exception or return an Error string, not both
    public class HangfireScheduler : IPersistenceScheduler
    {
        private IStateNotifier _stateNotifier = null;
        private readonly JobStorage _jobStorage;
        private readonly IBackgroundJobClient _client;
        private readonly IPersistedValues _persistedValues;

        [ExcludeFromCodeCoverage]
        public HangfireScheduler()
        {
            _jobStorage = SqlServerStorage();
            _client = new BackgroundJobClient(_jobStorage);
            _persistedValues = new PersistedValues();
        }

        [ExcludeFromCodeCoverage]
        private SqlServerStorage SqlServerStorage()
        {
            try
            {
                return new SqlServerStorage(ConnectionString);
            }
            catch
            {
                throw new Exception(ErrorResource.HangfireSqlServerStorageConnectionError);
            }
        }

        public HangfireScheduler(IBackgroundJobClient client, JobStorage jobStorage, IPersistedValues persistedValues)
        {
            _jobStorage = jobStorage;
            _client = client;
            _persistedValues = persistedValues;
        }

        public IPersistedValues GetPersistedValues(string jobId)
        {
            var monitoringApi = _jobStorage.GetMonitoringApi();
            var jobDetails = monitoringApi.JobDetails(jobId);
            const string errMsg = "Failed: ";
            if (jobDetails is null)
            {
                throw new Exception(errMsg + ErrorResource.ManualResumptionSuspensionEnvBlank);
            }

            var currentState = jobDetails.History.OrderBy(s => s.CreatedAt).LastOrDefault();

            if (currentState?.StateName == "Succeeded" || currentState?.StateName == "ManuallyResumed")
            {
                throw new Exception(errMsg + ErrorResource.ManualResumptionAlreadyResumed);
            }
            if (currentState?.StateName == "Enqueued")
            {
                throw new Exception(errMsg + ErrorResource.ManualResumptionEnqueued);
            }
            if (currentState?.StateName == "Processing")
            {
                throw new Exception(errMsg + ErrorResource.ManualResumptionProcessing);
            }

            if (jobDetails.Job.Args[0] is Dictionary<string, StringBuilder> values)
            {
                values.TryGetValue("resourceID", out var resourceId);
                values.TryGetValue("versionNumber", out var versionNumber);
                values.TryGetValue("environment", out var persistedEnvironment);
                values.TryGetValue("startActivityId", out var startActivity);
                values.TryGetValue("currentuserprincipal", out var currentUserPrincipal);

                var suspendedEnvironment = persistedEnvironment.ToString().CanBeDecrypted()
                    ? SecurityEncryption.Decrypt(persistedEnvironment.ToString())
                    : persistedEnvironment.ToString();

                var startActivityId = startActivity.ToString().CanBeDecrypted()
                    ? SecurityEncryption.Decrypt(startActivity.ToString())
                    : startActivity.ToString();

                var userPrinciple = currentUserPrincipal.ToString().CanBeDecrypted()
                    ? SecurityEncryption.Decrypt(currentUserPrincipal.ToString())
                    : currentUserPrincipal.ToString();

                var executingUser = BuildClaimsPrincipal(userPrinciple);

                _persistedValues.ResourceId = Guid.Parse(resourceId?.ToString() ?? string.Empty);
                _persistedValues.VersionNumber = int.Parse(versionNumber?.ToString() ?? string.Empty);
                _persistedValues.SuspendedEnvironment = suspendedEnvironment;
                _persistedValues.StartActivityId = Guid.Parse(startActivityId);
                _persistedValues.ExecutingUser = executingUser;

                return _persistedValues;
            }

            return null;
        }

        private static string GetUnqualifiedName(string userName)
        {
            if (userName.Contains("\\"))
            {
                return userName.Split('\\').Last().Trim();
            }

            return userName;
        }

        private static IPrincipal BuildClaimsPrincipal(string currentUserPrincipal)
        {
            var unqualifiedUserName = GetUnqualifiedName(currentUserPrincipal).Trim();
            var genericIdentity = new GenericIdentity(unqualifiedUserName);
            return new GenericPrincipal(genericIdentity, new string [0]);
        }

        public string ResumeJob(IDSFDataObject dsfDataObject, string jobId, bool overrideVariables, string environment)
        {
            try
            {
                var monitoringApi = _jobStorage.GetMonitoringApi();
                var jobDetails = monitoringApi.JobDetails(jobId);
                var errMsg = "Failed: ";
                if (jobDetails == null)
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionSuspensionEnvBlank);
                }
                var currentState = jobDetails.History.OrderBy(s => s.CreatedAt).LastOrDefault();

                if (currentState?.StateName == "Succeeded" || currentState?.StateName == "ManuallyResumed")
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionAlreadyResumed);
                }

                if (currentState?.StateName == "Enqueued")
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionEnqueued);
                }

                if (currentState?.StateName == "Processing")
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionProcessing);
                }

                var values = jobDetails.Job.Args[0] as Dictionary<string, StringBuilder>;
                values.TryGetValue("environment", out StringBuilder persistedEnvironment);
                var decryptEnvironment = persistedEnvironment.ToString().CanBeDecrypted() ? SecurityEncryption.Decrypt(persistedEnvironment.ToString()) : persistedEnvironment.ToString();
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

                values.TryGetValue("currentuserprincipal", out StringBuilder currentUserPrincipal);
                var decryptCurrentUserPrincipal = currentUserPrincipal.ToString().CanBeDecrypted() ? SecurityEncryption.Decrypt(currentUserPrincipal.ToString()) : currentUserPrincipal.ToString();
                if (values.ContainsKey("currentuserprincipal"))
                {
                    values["currentuserprincipal"] = new StringBuilder(decryptCurrentUserPrincipal);
                }

                var workflowResume = new WorkflowResume();
                var result = workflowResume.Execute(values, null);
                var serializer = new Dev2JsonSerializer();
                var executeMessage = serializer.Deserialize<ExecuteMessage>(result);
                if (executeMessage.HasError)
                {
                    var failedState = new FailedState(new Exception(executeMessage.Message?.ToString()));
                    _client.ChangeState(jobId, failedState, ScheduledState.StateName);
                    throw new Exception(executeMessage.Message?.ToString());
                }

                values.TryGetValue("resourceID", out StringBuilder workflowId);
                values.TryGetValue("environment", out StringBuilder environments);
                values.TryGetValue("startActivityId", out StringBuilder startActivityId);
                values.TryGetValue("versionNumber", out StringBuilder versionNumber);
                values.TryGetValue("currentprincipal", out StringBuilder currentprincipal);

                _stateNotifier = dsfDataObject.StateNotifier;
                var audit = new Audit
                {
                    WorkflowID = workflowId?.ToString(),
                    Environment = environments?.ToString(),
                    VersionNumber = versionNumber?.ToString(),
                    NextActivityId = startActivityId?.ToString(),
                    AuditDate = DateTime.Now,
                    AuditType = "LogResumeExecutionState",
                    LogLevel = LogLevel.Info,
                    User = currentprincipal?.ToString()
                };

                _stateNotifier?.LogAdditionalDetail(audit, nameof(ResumeJob));
                var manuallyResumedState = new ManuallyResumedState(environments?.ToString());
                _client.ChangeState(jobId, manuallyResumedState, currentState?.StateName);
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(ResumeJob), ex, GlobalConstants.WarewolfError);
                throw ex;
            }

            return GlobalConstants.Success;
        }

        public string ManualResumeWithOverrideJob(IDSFDataObject dsfDataObject, string jobId)
        {
            try
            {
                var monitoringApi = _jobStorage.GetMonitoringApi();
                var jobDetails = monitoringApi.JobDetails(jobId);
                var errMsg = "Failed: ";
                if (jobDetails == null)
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionSuspensionEnvBlank);
                }

                var currentState = jobDetails.History.OrderBy(s => s.CreatedAt).LastOrDefault();

                if (currentState?.StateName == "Succeeded" || currentState?.StateName == "ManuallyResumed")
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionAlreadyResumed);
                }

                if (currentState?.StateName == "Enqueued")
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionEnqueued);
                }

                if (currentState?.StateName == "Processing")
                {
                    throw new Exception(errMsg + ErrorResource.ManualResumptionProcessing);
                }

                if (dsfDataObject.Environment.HasErrors())
                {
                    var message = dsfDataObject.Environment.FetchErrors();
                    var failedState = new FailedState(new Exception(message));
                    _client.ChangeState(jobId, failedState, ScheduledState.StateName);
                    throw new Exception(message);
                }

                var manuallyResumedState = new ManuallyResumedState(dsfDataObject.Environment.ToJson());
                _client.ChangeState(jobId, manuallyResumedState, currentState?.StateName);
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(ManualResumeWithOverrideJob), ex, GlobalConstants.WarewolfError);
                throw ex;
            }

            return GlobalConstants.Success;
        }

        public string ScheduleJob(enSuspendOption suspendOption, string suspendOptionValue, Dictionary<string, StringBuilder> values)
        {
            string jobId;
            try
            {
                var suspensionDate = DateTime.Now;
                var resumptionDate = CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
                var state = new ScheduledState(resumptionDate.ToUniversalTime());

                jobId = _client.Create(() => ResumeWorkflow(values, null), state);
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(ScheduleJob), ex, GlobalConstants.WarewolfError);
                throw ex;
            }

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

        [ExcludeFromCodeCoverage]
        private string ConnectionString
        {
            get
            {
                var payload = Config.Persistence.PersistenceDataSource.Payload;
                if (Config.Persistence.EncryptDataSource)
                {
                    payload = payload.CanBeDecrypted() ? SecurityEncryption.Decrypt(payload) : payload;
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

            return resumptionDate;
        }
    }
}