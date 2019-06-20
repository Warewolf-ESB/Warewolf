/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.IO;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDirectoryInfo
    {
        string FullName { get; }
    }
    public interface IDirectory
    {
        DirectoryInfo GetParent(string path);
        string[] GetFiles(string path);
        IEnumerable<IFileInfo> GetFileInfos(string path);
        string CreateIfNotExists(string path);
        string[] GetLogicalDrives();
        bool Exists(string path);
        string[] GetFileSystemEntries(string path);
        string[] GetFileSystemEntries(string path, string searchPattern);
        string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption);
        string[] GetDirectories(string path);
        string[] GetDirectoriesCreateIfNotExists(string path);
        string[] GetDirectories(string path, string pattern);
        string GetDirectoryName(string path);
        void Move(string directoryStructureFromPath, string directoryStructureToPath);
        void Delete(string directoryStructureFromPath, bool recursive);
        IDirectoryInfo CreateDirectory(string dir);
        IEnumerable<string> EnumerateFiles(string path);
        IEnumerable<string> EnumerateFileSystemEntries(string path);
        IEnumerable<string> EnumerateDirectories(string path);
        IEnumerable<string> EnumerateDirectories(string path, string pattern);
        IEnumerable<string> EnumerateFiles(string path, string pattern);
        IEnumerable<string> EnumerateFileSystemEntries(string path, string pattern);
        IEnumerable<string> GetFilesByExtensions(string path, params string[] extensions);
        void Copy(string sourceDirName, string destDirName, bool copySubDirs);
        void CleanUp(string path);
        bool IsSystemFolder(FileSystemInfo fsi);
    }
}