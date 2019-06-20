#pragma warning disable
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
using Dev2.Common.Interfaces.Wrappers;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Warewolf.Resource.Errors;
using SearchOption = System.IO.SearchOption;

namespace Dev2.Common.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class DirectoryInfoWrapper : IDirectoryInfo
    {
        readonly DirectoryInfo _info;
        public DirectoryInfoWrapper(DirectoryInfo info)
        {
            _info = info;
        }
        public string FullName => _info.FullName;
    }

    [ExcludeFromCodeCoverage]
    public class DirectoryWrapper : IDirectory
    {
        public string[] GetFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                return new string[0];
            }
            return Directory.GetFiles(path);
        }
        public IEnumerable<IFileInfo> GetFileInfos(string path)
        {
            foreach (var info in new DirectoryInfo(path).GetFiles())
            {
                yield return new FileInfoWrapper(info);
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

        public string[] GetLogicalDrives() => Directory.GetLogicalDrives();

        public bool Exists(string path) => Directory.Exists(path);

        public string[] GetFileSystemEntries(string path) => Directory.GetFileSystemEntries(path);

        public string[] GetFileSystemEntries(string path, string searchPattern) => Directory.GetFileSystemEntries(path, searchPattern);
        public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption) => Directory.GetFileSystemEntries(path, searchPattern, searchOption);

        public string[] GetDirectories(string path) => Directory.GetDirectories(path);
        public string[] GetDirectoriesCreateIfNotExists(string path)
        {
            CreateIfNotExists(path);
            return Directory.GetDirectories(path);
        }

        public string[] GetDirectories(string path, string pattern) => Directory.GetDirectories(path, pattern, SearchOption.AllDirectories);

        public string GetDirectoryName(string path)
        {
            var validPath = path.TrimEnd('\\');
            var index = validPath.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
            if (index != -1)
            {
                return path.Substring(index + 1);
            }
            return path;
        }

        public void Move(string directoryStructureFromPath, string directoryStructureToPath)
        {
            FileSystem.MoveDirectory(directoryStructureFromPath, directoryStructureToPath, true);
        }

        public void Delete(string directoryStructureFromPath, bool recursive)
        {
            Directory.Delete(directoryStructureFromPath, recursive);
        }

        public IDirectoryInfo CreateDirectory(string dir)
        {
            try
            {
                var info = Directory.CreateDirectory(dir);
                return new DirectoryInfoWrapper(info);
            }
            catch (ArgumentNullException ane)
            {
                Action action = () => throw new ArgumentNullException(string.Format(ErrorResource.ErrorCreatingDirectory, dir, ane.Message));
                action();
            }
            catch (ArgumentException ae)
            {
                Action action = () => throw new Exception(string.Format(ErrorResource.ErrorCreatingDirectory, dir, ae.Message));
                action();
            }
            return null;
        }

        public IEnumerable<string> EnumerateFiles(string path)
            => Directory.EnumerateFiles(path);
        public IEnumerable<string> EnumerateDirectories(string path)
            => Directory.EnumerateDirectories(path);
        public IEnumerable<string> EnumerateFileSystemEntries(string path)
            => Directory.EnumerateFileSystemEntries(path);
        public IEnumerable<string> EnumerateFiles(string path, string pattern)
            => Directory.EnumerateFiles(path, pattern);
        public IEnumerable<string> EnumerateDirectories(string path, string pattern)
            => Directory.EnumerateDirectories(path, pattern);
        public IEnumerable<string> EnumerateFileSystemEntries(string path, string pattern)
            => Directory.EnumerateFileSystemEntries(path, pattern);

        public DirectoryInfo GetParent(string path)
        {
            return Directory.GetParent(path);
        }

        /// <summary>
        /// This needs to be remove at Version 3.0
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFilesByExtensions(string path, params string[] extensions)
        {
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

        void DeleteFileSystemInfo(FileSystemInfo fsi)
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

        void CheckIfDeleteIsValid(FileSystemInfo fsi)
        {
            if (IsSystemFolder(fsi))
            {
                void thrower()
                {
                    throw new NotSupportedException(string.Format(ErrorResource.CannotDeleteSystemFiles, fsi.FullName));
                }
                thrower();
            }
        }
        public bool IsSystemFolder(FileSystemInfo fsi)
        {
            var result = string.Equals(fsi.FullName, @"C:\", StringComparison.CurrentCultureIgnoreCase);
            result |= string.Equals(fsi.FullName, @"C:\Windows\System", StringComparison.CurrentCultureIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.System), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools), StringComparison.OrdinalIgnoreCase);
            result |= fsi.FullName.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Programs), StringComparison.OrdinalIgnoreCase);
            return result;
        }
    }
}