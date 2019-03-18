#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Scheduler.Interfaces;

namespace Dev2.Common.Common
{
    public class DirectoryHelper : IDirectoryHelper
    {
        public string[] GetFiles(string path) => Directory.GetFiles(path);

        /// <summary>
        /// This needs to be remove at Version 3.0
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFilesByExtensions(string path, params string[] extensions)
        {
            var dir = new DirectoryInfo(path);
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }

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

        public void Copy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

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

        public void CleanUp(string path)
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
            if (string.Equals(fsi.FullName, @"C:\", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles,
                    fsi.FullName));
            }
            if (string.Equals(fsi.FullName, @"C:\Windows\System", StringComparison.CurrentCultureIgnoreCase))
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

        public string CreateIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                return Directory.CreateDirectory(path).Name;
            }

            return path;
        }
    }
}