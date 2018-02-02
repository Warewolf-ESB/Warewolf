using System;
using System.IO;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Wrappers;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;

namespace Dev2.Common.Wrappers
{
    public class DropboxClientWrapper : IDropboxClientWrapper
    {
        readonly DropboxClient _client;

        public DropboxClientWrapper(DropboxClient client)
        {
            _client = client;
        }

        public Task<FileMetadata> UploadAsync(string path) => UploadAsync(path, null, false, null, false, null);

        public Task<FileMetadata> UploadAsync(string path, WriteMode mode, bool autorename, DateTime? clientModified, bool mute, Stream body) => _client.Files.UploadAsync(path, mode, autorename, clientModified, mute, body);

        public Task<Metadata> DeleteAsync(string path) => _client.Files.DeleteAsync(path);

        public Task<IDownloadResponse<FileMetadata>> DownloadAsync(DownloadArg downloadArg) => _client.Files.DownloadAsync(downloadArg);

        public Task<ListFolderResult> ListFolderAsync(ListFolderArg listFolderArg) => _client.Files.ListFolderAsync(listFolderArg);
    }
}
