using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dropbox.Api;
using Dropbox.Api.Files;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.DropBox2016.DeleteActivity
{
    public class DropboxDelete : IDropBoxDelete
    {
        private readonly IFilenameValidator _validator;
        private readonly string _dropboxPath;

        private DropboxDelete(IFilenameValidator validator)
        {
            _validator = validator;
        }

        public DropboxDelete(string dropboxPath)
            : this(new DropboxSoureFileValidator(dropboxPath))
        {
            _validator.Validate();
            if (!dropboxPath.StartsWith(@"/"))
                dropboxPath = string.Concat(@"/", dropboxPath);
            _dropboxPath = dropboxPath;
            InitializeCertPinning();
        }

        public bool IsValid { get; set; }

        #region Implementation of IDropboxSingleExecutor<IDropboxResult>
        [ExcludeFromCodeCoverage]
        public IDropboxResult ExecuteTask(DropboxClient client)
        {
            try
            {
                FileMetadata deleteAsync = client.Files.DeleteAsync(_dropboxPath).Result as FileMetadata;
                return new DropboxDeleteSuccessResult(deleteAsync);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception.Message);
                return exception.InnerException != null ? new DropboxFailureResult(exception.InnerException) : new DropboxFailureResult(exception);
            }
        }

        #endregion

        public void Validate()
        {
            if (!string.IsNullOrEmpty(_dropboxPath))
                IsValid = true;
        }
        [ExcludeFromCodeCoverage]
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
