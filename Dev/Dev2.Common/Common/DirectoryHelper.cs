using System;
using System.IO;

namespace Dev2.Common.Common
{
    public static class DirectoryHelper
    {
        public static void Copy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if(!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if(!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = dir.GetFiles();
            foreach(var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if(copySubDirs)
            {
                foreach(var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    Copy(subdir.FullName, temppath, true);
                }
            }
        }

        public static void CleanUp(string path)
        {
            if(path != null)
            {
                var di = new DirectoryInfo(path);
                if(di.Exists)
                {
                    DeleteFileSystemInfo(di);
                }
            }
        }

        static void DeleteFileSystemInfo(FileSystemInfo fsi)
        {
            CheckIfDeleteIsValid(fsi);
            fsi.Attributes = FileAttributes.Normal;
            var di = fsi as DirectoryInfo;

            if(di != null)
            {
                foreach(var dirInfo in di.GetFileSystemInfos())
                {
                    DeleteFileSystemInfo(dirInfo);
                }
            }
            fsi.Delete();
        }

        static void CheckIfDeleteIsValid(FileSystemInfo fsi)
        {
            //var fileAttributes = fsi.Attributes & FileAttributes.System;
            //if(fileAttributes == FileAttributes.System)
            //{
            //    throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            //}
            if(fsi.FullName.ToLower() == @"C:\".ToLower())
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.ToLower() == @"C:\Windows\System".ToLower())
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.System), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
            if(fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Programs), StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}", fsi.FullName));
            }
        }
    }
}
