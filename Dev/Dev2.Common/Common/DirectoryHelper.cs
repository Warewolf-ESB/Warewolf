/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Warewolf.Resource.Errors;

namespace Dev2.Common.Common
{
    public static class DirectoryHelper
    {
        
        /// <summary>
        /// This needs to be remove at Version 3.0
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFilesByExtensions(string path, params string[] extensions)
        {
            var dir = new DirectoryInfo(path);
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            var _files = new List<string>();
            foreach (string ext in extensions)
            {
                var fyles = Directory.GetFiles(path, string.Format("*{0}", ext));
                foreach (var item in fyles)
                {
                    _files.Add(item);
                }
            }
            return _files;
        }

        public static void Copy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            var files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
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

        static void DeleteFileSystemInfo(FileSystemInfo fsi)
        {
            CheckIfDeleteIsValid(fsi);
            fsi.Attributes = FileAttributes.Normal;

            if (fsi is DirectoryInfo di)
            {
                foreach (FileSystemInfo dirInfo in di.GetFileSystemInfos())
                {
                    DeleteFileSystemInfo(dirInfo);
                }
            }
            fsi.Delete();
        }

        static void CheckIfDeleteIsValid(FileSystemInfo fsi)
        {
            if (fsi.FullName.ToLower() == @"C:\".ToLower())
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.ToLower() == @"C:\Windows\System".ToLower())
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.System),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
        }
    }
}