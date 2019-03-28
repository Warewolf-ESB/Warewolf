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

        public string[] GetDirectories(string workspacePath) => Directory.GetDirectories(workspacePath);

        public string[] GetDirectories(string path, string pattern) => Directory.GetDirectories(path, pattern, System.IO.SearchOption.AllDirectories);

        public static string GetDirectoryName(string path)
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
    }
}