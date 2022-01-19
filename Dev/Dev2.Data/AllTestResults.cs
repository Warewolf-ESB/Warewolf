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

namespace Dev2.Data
{
    public class AllTestResults
    {
        public AllTestResults()
        {
            StartTime = DateTime.Now;
        }

        public DateTime StartTime { get; }
        public DateTime EndTime { get; set; }
        public List<WorkflowTestResults> Results { get; } = new List<WorkflowTestResults>();

        public void Add(WorkflowTestResults taskResult)
        {
            Results.Add(taskResult);
        }
    }
}
