using System;

namespace Dev2.Common.Interfaces.UndoFramework
{
    public interface ITransaction : IDisposable
    {
        IMultiAction AccumulatingAction { get; }

        bool IsDelayed { get; set; }
    }
}

