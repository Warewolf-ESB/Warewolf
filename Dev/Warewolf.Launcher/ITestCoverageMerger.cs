using System.Collections.Generic;

namespace Warewolf.Launcher
{
    public interface ITestCoverageMerger
    {
        void MergeCoverageSnapshots(List<string> SnapshotPaths, string DestinationFilePath, string LogFilePath, string ToolPath);
    }
}
