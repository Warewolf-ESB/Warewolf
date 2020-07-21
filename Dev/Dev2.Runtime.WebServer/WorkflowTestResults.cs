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
using Dev2.Data;
using System.Collections.Generic;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer
{
    public class WorkflowTestResults
    {
        public WorkflowTestResults()
        {
        }

        public WorkflowTestResults(IWarewolfResource res)
        {
            Resource = res;
        }

        public IWarewolfResource Resource { get; }
        public List<IServiceTestModelTO> Results { get; } = new List<IServiceTestModelTO>();
        public bool HasTestResults => Results.Count > 0;

        public void Add(IServiceTestModelTO result)
        {
            Results.Add(new ServiceTestModelTO
            {
                OldTestName = result.OldTestName,
                TestName = result.TestName,
                TestPassed = IsTestPassed(result),
                TestInvalid = IsTestInValid(result),
                TestFailing = IsTestFailing(result),
                Result = new TestRunResult
                {
                    TestName = result.TestName,
                    Message = IsTestInValid(result) ? "Test has no selected nodes" : result.FailureMessage,
                    RunTestResult = IsTestInValid(result) ? RunResult.TestInvalid : IsTestFailing(result) ? RunResult.TestFailed : RunResult.TestPassed,
                }
            });
        }

        private static bool IsTestFailing(IServiceTestModelTO test)
        {
            return IsTestInValid(test) is false && IsTestPassed(test) is false;
        }

        private static bool IsTestPassed(IServiceTestModelTO test)
        {
            return IsTestInValid(test) is false && test.TestFailing is false;
        }

        private static bool IsTestInValid(IServiceTestModelTO test)
        {
            return test.TestSteps is null || test.TestSteps.Count is 0;
        }
    }
}
