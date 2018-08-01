namespace Warewolf.Launcher
{
    public interface ITestResultsMerger
    {
        void MergeRetryResults(string originalResults, string retryResults);
    }
}
