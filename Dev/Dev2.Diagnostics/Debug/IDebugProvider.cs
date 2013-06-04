using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    public interface IDebugProvider
    {
        IEnumerable<DebugState> GetDebugStates(string serverWebUri, DirectoryPath directory, FilePath path);
    }
}
