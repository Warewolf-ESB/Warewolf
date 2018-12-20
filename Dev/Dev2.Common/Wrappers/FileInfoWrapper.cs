using System;
using System.IO;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Wrappers
{
    public class FileInfoWrapper : IFileInfo
    {
        readonly FileInfo _fileInfo;

        public FileInfoWrapper(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public DateTime CreationTime => _fileInfo.CreationTime;

        public void Delete()
        {
            _fileInfo.Delete();
        }
    }
}
