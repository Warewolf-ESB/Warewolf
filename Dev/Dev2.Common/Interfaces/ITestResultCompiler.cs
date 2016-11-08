namespace Dev2.Common.Interfaces
{
    public interface ITestResultCompiler
    {
        bool GetFailingResult(RunResult runResult);
        bool GetPassingResult(RunResult runResult);
        bool GetTestInvalidResult(RunResult runResult);
        bool GetTestPendingResult(RunResult runResult);
    }
}