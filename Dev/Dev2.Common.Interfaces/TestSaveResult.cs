using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Common.Interfaces
{
    public class TestRunResult
    {
        public string TestName { get; set; }
        public RunResult RunTestResult { get; set; }
        public string Message { get; set; }
        public IList<IDebugState> DebugForTest { get; set; }
    }

    public enum RunResult
    {
        None,
        TestPassed,
        TestFailed,
        TestInvalid,
        TestResourceDeleted,
        TestResourcePathUpdated,
        TestPending
    }
}