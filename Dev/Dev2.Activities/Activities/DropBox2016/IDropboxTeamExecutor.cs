using System.Threading.Tasks;
using Dropbox.Api;

namespace Dev2.Activities.DropBox2016
{
    public interface IDropboxTeamExecutor<TResult> : IDropboxExecutor<TResult>
    {
        Task<TResult> ExecuteTask(DropboxTeamClient client);
    }
}