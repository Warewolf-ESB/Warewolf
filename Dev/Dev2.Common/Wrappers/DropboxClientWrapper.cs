#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Wrappers;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;


namespace Dev2.Common.Wrappers
{

    public interface IDropboxClientFactory
    {
        IDropboxClient New(string accessToken, HttpClient httpClient);
        IDropboxClient CreateWithSecret(string accessToken);
    }

    public class DropboxClientWrapperFactory : IDropboxClientFactory
    {
        public IDropboxClient New(string accessToken, HttpClient httpClient)
        {
            return new DropboxClientWrapper(accessToken, httpClient);
        }

        IDropboxClient IDropboxClientFactory.CreateWithSecret(string accessToken)
        {
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            return New(accessToken, httpClient);
        }
    }

    [ExcludeFromCodeCoverage]
    public class DropboxClientWrapper : IDropboxClient
    {
        readonly DropboxClient _client;

        public DropboxClientWrapper(string accessToken, HttpClient httpClient)
        {
            _client = new DropboxClient(accessToken, new DropboxClientConfig(GlobalConstants.UserAgentString) { HttpClient = httpClient });
        }
        public DropboxClientWrapper(DropboxClient client)
        {
            _client = client;
        }

        public Task<FileMetadata> UploadAsync(string path) => UploadAsync(path, null, false, null, false, null);

        public Task<FileMetadata> UploadAsync(string path, WriteMode mode, bool autorename, DateTime? clientModified, bool mute, Stream body) => _client.Files.UploadAsync(path, mode, autorename, clientModified, mute, body);

        public Task<Metadata> DeleteAsync(string path) => _client.Files.DeleteAsync(path);

        public Task<IDownloadResponse<FileMetadata>> DownloadAsync(DownloadArg downloadArg) => _client.Files.DownloadAsync(downloadArg);

        public Task<ListFolderResult> ListFolderAsync(ListFolderArg listFolderArg) => _client.Files.ListFolderAsync(listFolderArg);

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
