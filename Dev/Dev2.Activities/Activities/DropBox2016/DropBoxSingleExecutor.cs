using Dropbox.Api;

namespace Dev2.Activities.DropBox2016
{
    public class DropBoxSingleExecutor<TResult>
    {
        private readonly IDropboxSingleExecutor<TResult> _singleExecutor;
        private readonly DropboxClient _client;

        protected DropBoxSingleExecutor(IDropboxSingleExecutor<TResult> singleExecutor, DropboxClient client)
        {
            _singleExecutor = singleExecutor;
            _client = client;
        }

        public void ExecuteDropBoxOperation()
        {
            _singleExecutor.ExecuteTask(_client);
        }
    }
}