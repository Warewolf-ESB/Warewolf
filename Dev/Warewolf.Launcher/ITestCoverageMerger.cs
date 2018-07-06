using System.Collections.Generic;

namespace Warewolf.Launcher
{
    interface ITestCoverageMerger
    {
        void MergeCoverageSnapshots(List<string> SnapshotPaths, string DestinationFilePath, string LogFilePath, string ToolPath);
    }
}
