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
using System.Text;
using Dev2.Common;
using Dev2.Data.Interfaces.Enums;
using Dev2.Interfaces;
using Warewolf.Driver.Persistence.Drivers;
using Warewolf.Resource.Errors;

namespace Warewolf.Driver.Persistence
{
    public class PersistenceExecution : IPersistenceExecution
    {
        private readonly IPersistenceScheduler _persistenceScheduler;

        [ExcludeFromCodeCoverage]
        public PersistenceExecution()
        {

        }
        public PersistenceExecution(IPersistenceScheduler persistenceScheduler)
        {
            _persistenceScheduler = persistenceScheduler;
        }

        public string ResumeJob(IDSFDataObject dsfDataObject, string jobId, bool overrideVariables,string environment)
        {
            var scheduler = _persistenceScheduler ?? GetScheduler();
            if (scheduler is null)
            {
                throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
            }
            return scheduler.ResumeJob(dsfDataObject,jobId, overrideVariables, environment);
        }

        public string CreateAndScheduleJob(enSuspendOption suspendOption, string suspendOptionValue, Dictionary<string, StringBuilder> values)
        {
            var scheduler = _persistenceScheduler ?? GetScheduler();
            if (scheduler is null)
            {
                throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
            }
            if (string.IsNullOrEmpty(suspendOptionValue))
            {
                var message = string.Format(ErrorResource.SuspendOptionValueNotSet, GetSuspendVaidationMessageType(suspendOption));
                throw new Exception(message);
            }
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);
            return jobId;
        }

        private string GetSuspendVaidationMessageType(enSuspendOption suspendOption)
        {
            switch (suspendOption)
            {
                case enSuspendOption.SuspendUntil:
                    return "Date";
                case enSuspendOption.SuspendForSeconds:
                    return "Seconds";
                case enSuspendOption.SuspendForMinutes:
                    return "Minutes";
                case enSuspendOption.SuspendForHours:
                    return "Hours";
                case enSuspendOption.SuspendForDays:
                    return "Days";
                case enSuspendOption.SuspendForMonths:
                    return "Months";
                default:
                    return "";
            }
        }

        [ExcludeFromCodeCoverage]
        private static IPersistenceScheduler GetScheduler()
        {
            if (Config.Persistence.PersistenceScheduler == nameof(Hangfire))
            {
                return new HangfireScheduler();
            }

            return null;
        }
    }
}