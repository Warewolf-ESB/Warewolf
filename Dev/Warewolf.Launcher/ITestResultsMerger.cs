namespace Warewolf.Launcher
{
    interface ITestResultsMerger
    {
        void MergeRetryResults(string originalResults, string retryResults);
    }
}
