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
using System.Security.Principal;
using System.Text;
using Dev2.Data.Interfaces.Enums;
using Dev2.Interfaces;

namespace Warewolf.Driver.Persistence
{
    public interface IPersistedValues
    {
        Guid ResourceId { get; set; }
        int VersionNumber { get; set; }
        string SuspendedEnvironment { get; set; }
        Guid StartActivityId { get; set; }
        IPrincipal ExecutingUser { get; set; }
    }
    public interface IPersistenceScheduler
    {
        IPersistedValues GetPersistedValues(string jobId);
        string ResumeJob(IDSFDataObject dsfDataObject, string jobId, bool overrideVariables, string environment);
        string ScheduleJob(enSuspendOption suspendOption, string suspendOptionValue, Dictionary<string, StringBuilder> values);
        string ManualResumeWithOverrideJob(IDSFDataObject dsfDataObject, string jobId);
    }
}