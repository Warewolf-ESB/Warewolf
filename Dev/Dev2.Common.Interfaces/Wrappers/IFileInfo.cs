using System;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IFileInfo
    {
        DateTime CreationTime { get; }

        void Delete();
    }
}
