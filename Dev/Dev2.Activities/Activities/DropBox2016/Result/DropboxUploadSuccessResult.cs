using Dev2.Common.Interfaces;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.Result
{
    public class DropboxUploadSuccessResult : IDropboxResult
    {
        private readonly FileMetadata _metadata;

        public DropboxUploadSuccessResult(FileMetadata metadata)
        {
            _metadata = metadata;
        }

        public FileMetadata GerFileMetadata()
        {
            return _metadata;
        }
    }
}