using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.UploadActivity
{
    public interface IDropBoxUpload : IDropboxSingleExecutor<FileMetadata>
    {
    }

    public class DropBoxUpload : IDropBoxUpload
    {
        private WriteMode _writeMode;
        private readonly string _dropboxPath;
        private readonly string _fromPath;

        public DropBoxUpload(WriteMode writeMode, string dropboxPath, string fromPath)
        {
            _writeMode = writeMode;
            _dropboxPath = dropboxPath;
            _fromPath = fromPath;
            Validate();
        }

        public bool IsValid { get; set; }

        #region Implementation of IDropboxSingleExecutor
        [ExcludeFromCodeCoverage]
        public FileMetadata ExecuteTask(DropboxClient client)
        {
            try
            {
                if (!IsValid)
                    return null;
                using (var stream = new MemoryStream(File.ReadAllBytes(_fromPath)))
                {
                    FileMetadata uploadAsync = client.Files.UploadAsync("/" + _dropboxPath, _writeMode, true, null, false, stream).Result;
                    return uploadAsync;
                }

            }
            catch (AggregateException exception)
            {
                var innerException = exception.InnerExceptions[0];
                Dev2Logger.Error(innerException.Message);
                return null;
            }
        }

        #endregion

        public void Validate()
        {
            if(_writeMode != null && !string.IsNullOrEmpty(_dropboxPath) && !string.IsNullOrEmpty(_fromPath))
                IsValid = true;
        }
    }
}