using Dropbox.Api.Babel;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.Result
{
    public class DropboxDownloadSuccessResult : IDropboxResult
    {
        private readonly IDownloadResponse<FileMetadata> _uploadAsync;

        public DropboxDownloadSuccessResult(IDownloadResponse<FileMetadata> uploadAsync)
        {
            _uploadAsync = uploadAsync;
        }

        public IDownloadResponse<FileMetadata> GetDownloadResponse()
        {
            return _uploadAsync;
        }
    }
}