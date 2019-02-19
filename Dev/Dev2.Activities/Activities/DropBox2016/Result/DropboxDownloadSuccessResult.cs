using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;

namespace Dev2.Activities.DropBox2016.Result
{
    public class DropboxDownloadSuccessResult : IDropboxResult
    {
        readonly IDownloadResponse<FileMetadata> _uploadAsync;

        public DropboxDownloadSuccessResult(IDownloadResponse<FileMetadata> uploadAsync)
        {
            _uploadAsync = uploadAsync;
        }

        public FileMetadata Response { get => _uploadAsync.Response; }

        public virtual IDownloadResponse<FileMetadata> GetDownloadResponse() => _uploadAsync;

        internal Task<byte[]> GetContentAsByteArrayAsync()
        {
            return _uploadAsync.GetContentAsByteArrayAsync();
        }
    }
}