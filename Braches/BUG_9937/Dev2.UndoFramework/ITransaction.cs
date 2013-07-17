namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;

    public interface ITransaction : IDisposable
    {
        IMultiAction AccumulatingAction { get; }

        bool IsDelayed { get; set; }
    }
}

