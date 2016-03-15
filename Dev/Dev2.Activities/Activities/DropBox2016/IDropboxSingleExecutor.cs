using Dropbox.Api;

namespace Dev2.Activities.DropBox2016
{
    public interface IDropboxSingleExecutor<TResult> : IDropboxExecutor<TResult>
    {
        TResult ExecuteTask(DropboxClient client);
    }
}