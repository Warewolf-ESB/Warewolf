using System.Threading.Tasks;
using Dropbox.Api;

namespace Dev2.Activities.DropBox2016
{
    public interface IDropboxSingleExecutor<TResult> : IDropboxExecutor<TResult>
    {
        Task<TResult> ExecuteTask(DropboxClient client);
    }
}