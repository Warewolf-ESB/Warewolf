/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data;

namespace Dev2.Data
{

    public class SingleTestNodesCovered : ISingleTestNodesCovered
    {
        public SingleTestNodesCovered(string testName, IEnumerable<IServiceTestStep> testSteps)
        {
            TestName = testName;
            TestNodesCovered = testSteps?.Select(step => new WorkflowNode
            {
                ActivityID = step.ActivityID,
                UniqueID = step.UniqueID,
                StepDescription = step.StepDescription,
                MockSelected = step.MockSelected
            }).ToList<IWorkflowNode>() ?? new List<IWorkflowNode>();
        }

        public string TestName { get; }
        public List<IWorkflowNode> TestNodesCovered { get; }

    }
}
