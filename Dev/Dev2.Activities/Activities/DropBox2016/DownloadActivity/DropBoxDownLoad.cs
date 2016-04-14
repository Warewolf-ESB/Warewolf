using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dropbox.Api;
using Dropbox.Api.Babel;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.DownloadActivity
{
    public class DropBoxDownLoad : IDropboxDownload
    {
        private readonly IFilenameValidator _validator;
        private readonly string _path;

        public DropBoxDownLoad(string path)
            : this(new DropboxSoureFileValidator(path))
        {
            _validator.Validate();
            if(!path.StartsWith(@"/"))
                path = string.Concat(@"/", path);
            _path = path;
        }

        private DropBoxDownLoad(IFilenameValidator validator)
        {
            _validator = validator;
        }

        #region Implementation of IDropboxSingleExecutor<IDropboxResult>

        [ExcludeFromCodeCoverage]
        public IDropboxResult ExecuteTask(DropboxClient client)
        {
            try
            {

                var downloadArg = new DownloadArg(_path);
                IDownloadResponse<FileMetadata> uploadAsync = client.Files.DownloadAsync(downloadArg).Result;
                return new DropboxDownloadSuccessResult(uploadAsync);
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
                    return exception.InnerException.Message.Contains("not_file") ? new DropboxFailureResult(new DropboxPathNotFileFoundException()) : new DropboxFailureResult(exception.InnerException);
                }
                return new DropboxFailureResult(exception);
            }
        }

        #endregion
    }
}