#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
