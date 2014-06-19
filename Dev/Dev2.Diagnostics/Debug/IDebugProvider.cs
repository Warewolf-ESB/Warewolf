using System.Collections.Generic;

namespace Dev2.Diagnostics.Debug
{
    public interface IDebugProvider
    {
        IEnumerable<IDebugState> GetDebugStates(string serverWebUri, DirectoryPath directory, FilePath path);
    }
}
