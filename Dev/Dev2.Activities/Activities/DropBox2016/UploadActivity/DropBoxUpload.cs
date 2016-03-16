using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.UploadActivity
{
    public interface IDropBoxUpload : IDropboxSingleExecutor<IDropboxResult>
    {
    }

    public class DropBoxUpload : IDropBoxUpload
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
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
        public IDropboxResult ExecuteTask(DropboxClient client)
        {
            try
            {
                if (!IsValid)
                    return null;
                using (var stream = new MemoryStream(File.ReadAllBytes(_fromPath)))
                {
                    FileMetadata uploadAsync = client.Files.UploadAsync("/" + _dropboxPath, _writeMode, true, null, false, stream).Result;
                    return new DropboxSuccessResult(uploadAsync);
                }

            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception.Message);
                return new DropboxFailureResult(exception);
            }
        }

        #endregion

        public void Validate()
        {
            if (_writeMode != null && !string.IsNullOrEmpty(_dropboxPath) && !string.IsNullOrEmpty(_fromPath))
                IsValid = true;
        }
    }
}