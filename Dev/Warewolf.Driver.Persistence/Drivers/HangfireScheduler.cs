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
using System.Globalization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Serializers;
using Dev2.Data.Interfaces.Enums;
using Dev2.Runtime.ServiceModel.Data;
using Hangfire;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.States;
using Warewolf.Security.Encryption;

namespace Warewolf.Driver.Persistence.Drivers
{
    public class HangfireScheduler : IPersistenceScheduler
    {
        public HangfireScheduler()
        {
        }
        public string ScheduleJob(enSuspendOption suspendOption, string suspendOptionValue, Dictionary<string, StringBuilder> values)
        {

            var suspensionDate = DateTime.Now;
            var resumptionDate = CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            var state = new ScheduledState(resumptionDate);

            var backgroundJobClient = new BackgroundJobClient(new SqlServerStorage(ConnectionString()));
            var jobId = backgroundJobClient.Create(() => ResumeWorkflow(values, null), state);
            return jobId;
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public string ResumeWorkflow(Dictionary<string, StringBuilder> values, PerformContext context)
        {
            try
            {
                //This method is intercepted in the HangfileServer Performing method
                return "success";
            }
            catch (Exception ex)
            {
                return "failed";
            }
        }

        private string ConnectionString()
        {
            var payload = Config.Persistence.PersistenceDataSource.Payload;
            if (Config.Persistence.EncryptDataSource)
            {
                payload = payload.CanBeDecrypted() ? DpapiWrapper.Decrypt(payload) : payload;
            }

            var source = new Dev2JsonSerializer().Deserialize<DbSource>(payload);
            return source.ConnectionString;
        }
        private DateTime CalculateResumptionDate(DateTime persistenceDate, enSuspendOption suspendOption, string scheduleValue)
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