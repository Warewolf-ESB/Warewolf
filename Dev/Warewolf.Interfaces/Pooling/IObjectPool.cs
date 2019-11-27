using System;

namespace Warewolf.Interfaces.Pooling
{
    public interface IObjectPool<T> : IDisposable where T : class, new()
    {
        T AcquireObject();
        void ReleaseObject(T concrete);
    }
}
