using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Net;
using Dev2.Common.Interfaces.Wrappers;



namespace Dev2.Activities.DropBox2016.UploadActivity
{
    public class DropBoxUpload : IDropBoxUpload
    {
        private readonly IFilenameValidator _validator;

        
        private readonly WriteMode _writeMode;

        private readonly string _dropboxPath;
        private readonly string _fromPath;

        private DropBoxUpload(IFilenameValidator validator)
        {
            _validator = validator;
        }

        public DropBoxUpload(WriteMode writeMode, string dropboxPath, string fromPath)
            : this(new DropboxSoureFileValidator(fromPath))
        {
            _validator.Validate();
            _writeMode = writeMode;
            if (!string.IsNullOrWhiteSpace(dropboxPath) && !dropboxPath.StartsWith(@"/"))
            {
                dropboxPath = string.Concat(@"/", dropboxPath);
            }
            else
            {
                if (!string.IsNullOrEmpty(dropboxPath))
                {
                    dropboxPath = dropboxPath.Trim();
                }
            }
            _dropboxPath = dropboxPath;
            _fromPath = fromPath;
            InitializeCertPinning();
        }

        #region Implementation of IDropboxSingleExecutor

        public IDropboxResult ExecuteTask(IDropboxClientWrapper client)
        {
            try
            {
                using (var stream = new MemoryStream(File.ReadAllBytes(_fromPath)))
                {
                    FileMetadata uploadAsync = client.UploadAsync(_dropboxPath, _writeMode, true, null, false, stream).Result;
                    return new DropboxUploadSuccessResult(uploadAsync);
                }
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception.Message, GlobalConstants.WarewolfError);
                return exception.InnerException != null ? new DropboxFailureResult(exception.InnerException) : new DropboxFailureResult(exception);
            }
        }

        #endregion Implementation of IDropboxSingleExecutor

        private void InitializeCertPinning()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                var root = chain.ChainElements[chain.ChainElements.Count - 1];
                var publicKey = root.Certificate.GetPublicKeyString();

                return DropboxCertHelper.IsKnownRootCertPublicKey(publicKey);
            };
        }
    }
}