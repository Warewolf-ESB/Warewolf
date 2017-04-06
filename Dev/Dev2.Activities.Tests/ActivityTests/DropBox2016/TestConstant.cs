using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    public static class TestConstant
    {
        public static readonly Lazy<FileMetadata> FileMetadataInstance = new Lazy<FileMetadata>(() =>
        {
            var mock = new Mock<FileMetadata>();
            var fileMetadata = mock.Object;
            return fileMetadata;
        });

        public static readonly Lazy<Exception> ExceptionInstance = new Lazy<Exception>(() =>
        {
            var exception = new Exception(ErrorMessage);
            return exception;
        });

        public static readonly Lazy<IDropboxClientWrapper> DropboxClientInstance = new Lazy<IDropboxClientWrapper>(() =>  new DropboxClientWrapper(new DropboxClient("random.net")));
        private const string ErrorMessage = "Error Messege";
        public static readonly Lazy<IDownloadResponse<FileMetadata>> FileDownloadResponseInstance = new Lazy<IDownloadResponse<FileMetadata>>(() =>
        {
            var mock = new Mock<IDownloadResponse<FileMetadata>>();
            return mock.Object;
        });

        public static readonly Lazy<ListFolderResult> ListFolderResultInstance=new Lazy<ListFolderResult>(() =>
        {
            
    
            var folderMetadata = new FolderMetadata("a","a","a","1"){AsFolder = { }};
            var metadata = new DeletedMetadata("deleted", "deleted", "deleted"){AsFolder = {}};
            var folderMetadata1 = new FolderMetadata("c","c","c","3");
            var metadata1 = new FolderMetadata("d","d","d","4");
            IEnumerable<Metadata> entries = new Metadata[] {folderMetadata, metadata1, metadata, folderMetadata1};
            var listFolderResult = new ListFolderResult(entries,"3",false);
            return listFolderResult;
        });
    }
}