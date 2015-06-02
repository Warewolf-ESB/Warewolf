
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Dev2.Data.Enums
{
    /// <summary>
    /// Enum to for the foreach activity
    /// </summary>
    public enum enForEachType
    {
        [Description("* in Range")]
        InRange,
        [Description("* in CSV")]
        InCSV,
        [Description("No. of Executes")]
        NumOfExecution,
        [Description("* in Recordset")]
        InRecordset
    }
}
