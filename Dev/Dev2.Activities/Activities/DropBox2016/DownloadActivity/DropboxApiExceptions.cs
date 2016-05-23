using System;
using Dev2.Common;

namespace Dev2.Activities.DropBox2016.DownloadActivity
{
    public class DropboxFileNotFoundException : Exception
    {
        public DropboxFileNotFoundException()
            : base(GlobalConstants.DropboxPathNotFoundException)
        {

        }

        
    }
    public class DropboxPathNotFileFoundException : Exception
    {
        public DropboxPathNotFileFoundException()
            : base(GlobalConstants.DropboxPathNotFileException)
        {

        }
    }public class DropboxFileMalformdedException : Exception
    {
        public DropboxFileMalformdedException()
            : base(GlobalConstants.DropboxPathMalformdedException)
        {

        }
    }
}
