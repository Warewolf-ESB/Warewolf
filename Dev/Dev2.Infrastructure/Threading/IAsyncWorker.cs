using System;
using System.Threading.Tasks;

namespace Dev2.Threading
{
    public interface IAsyncWorker
    {
        Task Start(Action backgroundAction, Action uiAction);
    }
}
