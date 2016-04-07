using System;
using Dropbox.Api;
using Dropbox.Api.Babel;
using Dropbox.Api.Files;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    public static class TestConstant
    {
        public static Lazy<FileMetadata> FileMetadataInstance = new Lazy<FileMetadata>(() =>
        {
            var mock = new Mock<FileMetadata>();
            var fileMetadata = mock.Object;
            return fileMetadata;
        });

        public static Lazy<Exception> ExceptionInstance = new Lazy<Exception>(() =>
        {
            var exception = new Exception(ErrorMessage);
            return exception;
        });

        public static Lazy<DropboxClient> DropboxClientInstance = new Lazy<DropboxClient>(() => new DropboxClient("random.net"));
        public static string ErrorMessage = "Error Messege";
        public static Lazy<IDownloadResponse<FileMetadata>> FileDownloadResponseInstance = new Lazy<IDownloadResponse<FileMetadata>>(() =>
        {
            var mock = new Mock<IDownloadResponse<FileMetadata>>();
            return mock.Object;
        });

    }
}