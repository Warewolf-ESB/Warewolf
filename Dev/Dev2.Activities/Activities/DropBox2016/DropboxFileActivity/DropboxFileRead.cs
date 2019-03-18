#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using Dev2.Activities.DropBox2016.DownloadActivity;
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
        readonly bool _recursive;
        readonly string _path;
        readonly bool _includeMediaInfo;
        readonly bool _includeDeleted;

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

        public IDropboxResult ExecuteTask(IDropboxClient client)
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