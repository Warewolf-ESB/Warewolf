using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDropboxClientWrapper
    {
        Task<FileMetadata> UploadAsync(string path);
        Task<FileMetadata> UploadAsync(string path, WriteMode mode, bool autorename, DateTime? clientModified, bool mute, Stream body);
        Task<Metadata> DeleteAsync(string path);
        Task<IDownloadResponse<FileMetadata>> DownloadAsync(DownloadArg downloadArg);
        Task<ListFolderResult> ListFolderAsync(ListFolderArg listFolderArg);
    }
}