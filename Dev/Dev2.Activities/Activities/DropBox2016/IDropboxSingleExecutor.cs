using Dev2.Common.Interfaces;
using Dropbox.Api;

namespace Dev2.Activities.DropBox2016
{
    public interface IDropboxSingleExecutor<out TResult> : IDropboxExecutor
    {
        TResult ExecuteTask(DropboxClient client);
    }
}