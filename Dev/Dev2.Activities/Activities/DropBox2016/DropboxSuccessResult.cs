using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016
{
    public class DropboxSuccessResult : FileMetadata, IDropboxResult
    {
        private readonly FileMetadata _metadata;

        public DropboxSuccessResult(FileMetadata metadata)
        {
            _metadata = metadata;
        }

        public FileMetadata GerFileMetadata()
        {
            return _metadata;
        }
    }
}