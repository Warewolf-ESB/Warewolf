#pragma warning disable
 using System;
using System.Net;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dropbox.Api;
using Dropbox.Api.Files;


namespace Dev2.Activities.DropBox2016.DeleteActivity
{
    public class DropboxDelete : IDropBoxDelete
    {
        readonly IFilenameValidator _validator;
        readonly string _dropboxPath;

        DropboxDelete(IFilenameValidator validator)
        {
            _validator = validator;
        }

        public DropboxDelete(string dropboxPath)
            : this(new DropboxSoureFileValidator(dropboxPath))
        {
            _validator.Validate();
            if (!dropboxPath.StartsWith(@"/"))
            {
                dropboxPath = string.Concat(@"/", dropboxPath);
            }

            _dropboxPath = dropboxPath;
            InitializeCertPinning();
        }

        #region Implementation of IDropboxSingleExecutor<IDropboxResult>
        public IDropboxResult ExecuteTask(IDropboxClient client)
        {
            try
            {
                var deleteAsync = client.DeleteAsync(_dropboxPath).Result as FileMetadata;
                return new DropboxDeleteSuccessResult(deleteAsync);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception.Message, GlobalConstants.WarewolfError);
                return exception.InnerException != null ? new DropboxFailureResult(exception.InnerException) : new DropboxFailureResult(exception);
            }
        }

        #endregion

        void InitializeCertPinning()
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
