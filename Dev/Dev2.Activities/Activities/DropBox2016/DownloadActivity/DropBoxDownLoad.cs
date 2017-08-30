using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dropbox.Api.Files;
using System;
using Dev2.Common.Interfaces.Wrappers;
using Dropbox.Api.Stone;

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
            if (!string.IsNullOrWhiteSpace(path) && !path.StartsWith(@"/"))
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
        }

        private DropBoxDownLoad(IFilenameValidator validator)
        {
            _validator = validator;
        }

        #region Implementation of IDropboxSingleExecutor<IDropboxResult>

        public IDropboxResult ExecuteTask(IDropboxClientWrapper client)
        {
            try
            {
                var downloadArg = new DownloadArg(_path);
                IDownloadResponse<FileMetadata> uploadAsync = client.DownloadAsync(downloadArg).Result;
                return new DropboxDownloadSuccessResult(uploadAsync);
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
                    return exception.InnerException.Message.Contains("not_file") ? new DropboxFailureResult(new DropboxPathNotFileFoundException()) : new DropboxFailureResult(exception.InnerException);
                }
                return new DropboxFailureResult(exception);
            }
        }

        #endregion Implementation of IDropboxSingleExecutor<IDropboxResult>
    }
}