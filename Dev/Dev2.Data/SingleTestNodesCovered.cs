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
using System.Linq;
using Dev2.Common.Interfaces;
using Warewolf.Data;

namespace Dev2.Data
{

    public class SingleTestNodesCovered : ISingleTestNodesCovered
    {
        public SingleTestNodesCovered(string testName, IEnumerable<IServiceTestStep> testSteps)
        {
            TestName = testName;
            TestNodesCovered = testSteps?
                            .Select(step => step.ToWorkflowNode())
                            .ToList()
                            ?? new List<IWorkflowNode>();
        }

        public string TestName { get; }
        public List<IWorkflowNode> TestNodesCovered { get; }

    }

    public static class SingleTestNodesCoveredExtention
    {
        public static IWorkflowNode ToWorkflowNode(this IServiceTestStep step)
        {
            return new WorkflowNode
            {
                ActivityID = step.ActivityID != Guid.Empty ? step.ActivityID : step.UniqueID,
                UniqueID = step.UniqueID,
                StepDescription = step.StepDescription,
                MockSelected = step.MockSelected,
                ChildNodes = step.Children?.Select(o => ToWorkflowNode(o)).ToList() ?? new List<IWorkflowNode>()
            };
        }
    }
}
