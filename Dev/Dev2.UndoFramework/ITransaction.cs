using System;

namespace Dev2.UndoFramework
{
    public interface ITransaction : IDisposable
    {
        IMultiAction AccumulatingAction { get; }

        bool IsDelayed { get; set; }
    }
}

