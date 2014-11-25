/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;

namespace Dev2.Common.Common
{
    public static class DirectoryHelper
    {
        public static void Copy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    Copy(subdir.FullName, temppath, true);
                }
            }
        }

        public static void CleanUp(string path)
        {
            if (path != null)
            {
                var di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    DeleteFileSystemInfo(di);
                }
            }
        }

        private static void DeleteFileSystemInfo(FileSystemInfo fsi)
        {
            CheckIfDeleteIsValid(fsi);
            fsi.Attributes = FileAttributes.Normal;
            var di = fsi as DirectoryInfo;

            if (di != null)
            {
                foreach (FileSystemInfo dirInfo in di.GetFileSystemInfos())
                {
                    DeleteFileSystemInfo(dirInfo);
                }
            }
            fsi.Delete();
        }

        private static void CheckIfDeleteIsValid(FileSystemInfo fsi)
        {
            if (fsi.FullName.ToLower() == @"C:\".ToLower())
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.ToLower() == @"C:\Windows\System".ToLower())
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.System),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format("Not allowed to delete system files/directories. {0}",
                    fsi.FullName));
            }
        }
    }
}