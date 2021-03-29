﻿/*
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

        public IPersistedValues GetPersistedValues(string jobId)
        {
            var scheduler = _persistenceScheduler ?? GetScheduler();
            if (scheduler is null)
            {
                throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
            }
            return scheduler.GetPersistedValues(jobId);
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

        public string ManualResumeWithOverrideJob(IDSFDataObject dsfDataObject, string jobId)
        {
            var scheduler = _persistenceScheduler ?? GetScheduler();
            if (scheduler is null)
            {
                throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
            }
            return scheduler.ManualResumeWithOverrideJob(dsfDataObject, jobId);
        }

        public string CreateAndScheduleJob(enSuspendOption suspendOption, string suspendOptionValue, Dictionary<string, StringBuilder> values)
        {
            var scheduler = _persistenceScheduler ?? GetScheduler();
            if (scheduler is null)
            {
                throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
            }

            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);
            return jobId;
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