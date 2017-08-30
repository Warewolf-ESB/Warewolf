using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Net;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Activities.DropBox2016.DropboxFileActivity
{
    public class DropboxFileRead : IDropboxFileRead
    {
        private readonly bool _recursive;
        private readonly string _path;
        private readonly bool _includeMediaInfo;
        private readonly bool _includeDeleted;

        public DropboxFileRead(bool recursive, string path, bool includeMediaInfo, bool includeDeleted)
        {
            _recursive = recursive;
            _path = path;
            _includeMediaInfo = includeMediaInfo;
            _includeDeleted = includeDeleted;
            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }
            else if (!string.IsNullOrWhiteSpace(path) && !path.StartsWith(@"/"))
            {
                path = string.Concat(@"/", path);
            }
            else
            {
                if (!string.IsNullOrEmpty(path))
                {
                    path = path.Trim();
                }
            }
            _path = path;
            InitializeCertPinning();
        }

        public IDropboxResult ExecuteTask(IDropboxClientWrapper client)
        {
            try
            {
                var listFolderArg = new ListFolderArg(_path, _recursive, _includeMediaInfo, _includeDeleted);
                var listFolderResult = client.ListFolderAsync(listFolderArg).Result;
                return new DropboxListFolderSuccesResult(listFolderResult);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception.Message, GlobalConstants.WarewolfError);
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