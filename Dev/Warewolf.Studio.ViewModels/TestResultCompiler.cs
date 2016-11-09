using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class TestResultCompiler : ITestResultCompiler
    {

        public bool GetFailingResult(RunResult runResult)
        {
            var failResult = runResult == RunResult.TestFailed;
            return failResult;
        }

        public bool GetPassingResult(RunResult runResult)
        {
            var passingResult = runResult == RunResult.TestPassed;
            return passingResult;
        }

        public bool GetTestInvalidResult(RunResult runResult)
        {
            var invalidResult = runResult == RunResult.TestInvalid || runResult == RunResult.TestResourceDeleted || runResult == RunResult.TestResourcePathUpdated;
            return invalidResult;
        }

        public bool GetTestPendingResult(RunResult runResult)
        {
            var pendingResult = runResult != RunResult.TestFailed &&
                                  runResult != RunResult.TestPassed &&
                                  runResult != RunResult.TestInvalid &&
                                  runResult != RunResult.TestResourceDeleted &&
                                  runResult != RunResult.TestResourcePathUpdated;
            return pendingResult;
        }
    }
}