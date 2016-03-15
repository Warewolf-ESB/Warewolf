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
        private WriteMode _writeMode;
        private readonly string _dropboxPath;
        private readonly string _fromPath;

        public DropBoxUpload(bool mute, DateTime? clientModified, bool autoRename, WriteMode writeMode, string dropboxPath, string fromPath)
        {
            _mute = mute;
            _clientModified = clientModified;
            _autoRename = autoRename;
            _writeMode = writeMode;
            _dropboxPath = dropboxPath;
            _fromPath = fromPath;
        }

        #region Implementation of IDropboxSingleExecutor

        public FileMetadata ExecuteTask(DropboxClient client)
        {
            try
            {
                if (_writeMode == null)
                    _writeMode = WriteMode.Add.Instance;
                using (var stream = new MemoryStream(File.ReadAllBytes(_fromPath)))
                {
                    //var commitInfo = new CommitInfo("/" + _dropboxPath, _writeMode, _autoRename, _mute);
                    FileMetadata uploadAsync = client.Files.UploadAsync("/" + _dropboxPath, _writeMode, true, null, false, stream).Result;
                    return uploadAsync;
                }

            }
            catch (AggregateException exception)
            {
                var innerException = exception.InnerExceptions[0];
                return null;
            }
        }

        #endregion
    }
}