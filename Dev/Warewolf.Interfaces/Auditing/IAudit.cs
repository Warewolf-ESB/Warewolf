/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Logging;

namespace Warewolf.Interfaces.Auditing
{
    public interface IAuditEntry
    {
        Exception Exception { get; set; }
        string AuditType { get; set; }
        LogLevel LogLevel { get; set; }
    }

    public interface IAudit : IAuditEntry
    {
        string AdditionalDetail { get; set; }
        DateTime AuditDate { get; set; }
        string Environment { get; set; }
        string ExecutingUser { get; set; }
        string ExecutionID { get; set; }
        string CustomTransactionID { get; set; }
        long ExecutionOrigin { get; set; }
        string ExecutionOriginDescription { get; set; }
        string ExecutionToken { get; set; }
        int Id { get; set; }
        bool IsRemoteWorkflow { get; set; }
        bool IsSubExecution { get; set; }
        string NextActivity { get; set; }
        string NextActivityId { get; set; }
        string NextActivityType { get; set; }
        string ParentID { get; set; }
        string PreviousActivity { get; set; }
        string PreviousActivityId { get; set; }
        string PreviousActivityType { get; set; }
        string ServerID { get; set; }
        string VersionNumber { get; set; }
        string WorkflowID { get; set; }
        string WorkflowName { get; set; }
    }
}
