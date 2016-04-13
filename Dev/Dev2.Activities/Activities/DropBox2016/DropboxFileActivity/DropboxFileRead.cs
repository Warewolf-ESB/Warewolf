using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.DropboxFileActivity
{
    public class DropboxFileRead : IDropboxFileRead
    {
        private readonly IFilenameValidator _validator;
        private readonly bool _recursive;
        private readonly string _path;
        private readonly bool _includeMediaInfo;
        private readonly bool _includeDeleted;

        public DropboxFileRead(bool recursive, string path, bool includeMediaInfo, bool includeDeleted)
            :this(new DropboxFileNameValidator(path))
        {
            _validator.Validate();
            _recursive = recursive;
            _path = path;
            _includeMediaInfo = includeMediaInfo;
            _includeDeleted = includeDeleted;
            if (!path.StartsWith(@"/"))
                path = string.Concat(@"/", path);
            _path = path;
            InitializeCertPinning();
        }

        public DropboxFileRead(IFilenameValidator validator)
        {
            _validator = validator;
        }

        public IDropboxResult ExecuteTask(DropboxClient client)
        {
            try
            {
                var listFolderArg = new ListFolderArg(_path, _recursive, _includeMediaInfo, _includeDeleted);
                var listFolderResult = client.Files.ListFolderAsync(listFolderArg).Result;
                return new DropboxListFolderSuccesResult(listFolderResult);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception.Message);
                var hasInnerExc = exception.InnerException != null;
                if (hasInnerExc)
                {
                    if (exception.InnerException.Message.Contains("not_found"))
                    {
                        return new DropboxFailureResult(new DropboxFileNotFoundException());
                    } 
                    
                    if (exception.InnerException.Message.Contains("malformed"))
                    {
                        return new DropboxFailureResult(new DropboxFileMalformdedException());
                    }
                    return exception.InnerException.Message.Contains("not_file") ? new DropboxFailureResult(new DropboxPathNotFileFoundException()) : new DropboxFailureResult(exception.InnerException);
                }
                return new DropboxFailureResult(exception);
            }
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
