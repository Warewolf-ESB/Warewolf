using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Activities.DropBox2016
{
    public interface IDropboxSingleExecutor<out TResult> : IDropboxExecutor
    {
        TResult ExecuteTask(IDropboxClientWrapper client);
    }
}