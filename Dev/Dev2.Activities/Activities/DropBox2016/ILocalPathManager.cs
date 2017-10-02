
//https://msdn.microsoft.com/en-us/library/system.io.path.getdirectoryname(v=vs.110).aspx

using System.IO;
using Dev2.Common.Interfaces;

namespace Dev2.Activities.DropBox2016
{
    public class LocalPathManager : ILocalPathManager
    {
        private readonly string _fileName;

        public LocalPathManager(string fileName)
        {
            _fileName = fileName;
        }

        #region Implementation of ILocalPathManager

        private bool IsValid()
        {
            var directoryName = GetDirectoryName();
            return !string.IsNullOrEmpty(directoryName);
        }
        public string CreateValidFolder()
        {
            if (DirectoryExists())
            {
                return GetDirectoryName();
            }

            var directoryInfo = Directory.CreateDirectory(GetDirectoryName());
            return directoryInfo.FullName;
        }

        private bool DirectoryExists()
        {
            return IsValid() && Directory.Exists(GetDirectoryName());
        }

        public string GetDirectoryName()
        {
            var directoryName = Path.GetDirectoryName(_fileName);
            return directoryName;
        }

        public string GetFileName()
        {
            var fileName = Path.GetFileName(_fileName);
            return fileName;
        }


        #endregion

        public string GetFullFileName()
        {
            var fullFileName = Path.Combine(CreateValidFolder(), GetFileName());
            return fullFileName;
        }

        public bool FileExist()
        {
            return File.Exists(GetFullFileName());
        }
    }

}
