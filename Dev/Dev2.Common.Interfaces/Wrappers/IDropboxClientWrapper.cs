using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDropboxClientWrapper
    {
        Task<FileMetadata> UploadAsync(string path, WriteMode mode = null, bool autorename = false, DateTime? clientModified = null, bool mute = false, Stream body = null);
        Task<Metadata> DeleteAsync(string path);
        Task<IDownloadResponse<FileMetadata>> DownloadAsync(DownloadArg downloadArg);
        Task<ListFolderResult> ListFolderAsync(ListFolderArg listFolderArg);
    }
}