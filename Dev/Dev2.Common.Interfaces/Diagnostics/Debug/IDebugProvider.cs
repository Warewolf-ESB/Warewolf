using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    public interface IFilePath
    {
        string Title { get; set; }
    }

    public interface IDirectoryPath
    {
        string RealPath { get; set; }
        string PathToSerialize { get; set; }

        void SetRealPath();

        void SetSerializePath();
    }
    public interface IDebugProvider
    {
        IEnumerable<IDebugState> GetDebugStates(string serverWebUri, IDirectoryPath directory, IFilePath path);
    }
}
