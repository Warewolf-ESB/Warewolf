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
using Warewolf.Triggers;

namespace Warewolf.Interfaces.Auditing
{
    public interface IAuditQueryable
    {
        string Query { get; set; }
        IEnumerable<IAudit> QueryLogData(Dictionary<string, StringBuilder> values);
        IEnumerable<IExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values);
    }
}