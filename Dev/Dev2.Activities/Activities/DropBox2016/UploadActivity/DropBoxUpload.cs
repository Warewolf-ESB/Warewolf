using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.UploadActivity
{
    public class DropBoxUpload : IDropboxSingleExecutor<FileMetadata>
    {
        private readonly bool _mute;
        private readonly DateTime? _clientModified;
        private readonly bool _autoRename;
        private readonly WriteMode _writeMode;
        private readonly string _path;
        private readonly Stream _stream;

        public DropBoxUpload(bool mute, DateTime? clientModified, bool autoRename, WriteMode writeMode, string path, Stream stream)
        {
            _mute = mute;
            _clientModified = clientModified;
            _autoRename = autoRename;
            _writeMode = writeMode;
            _path = path;
            _stream = stream;
        }

        #region Implementation of IDropboxSingleExecutor

        public async Task<FileMetadata> ExecuteTask(DropboxClient client)
        {
            var uploadAsync = await client.Files.UploadAsync(new CommitInfo(_path, _writeMode, _autoRename, _clientModified, _mute), _stream);
            return uploadAsync;
        }

        #endregion
    }
}