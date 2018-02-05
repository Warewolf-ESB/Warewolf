/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Common.Wrappers
{ // not required for code coverage this is simply a pass through required for unit testing
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
            var index = validPath.LastIndexOf("\\",StringComparison.InvariantCultureIgnoreCase);
            if (index != -1)
            {
                return path.Substring(index+1);
            }
            return path;
        }

        public void Move(string directoryStructureFromPath, string directoryStructureToPath)
        {
            FileSystem.MoveDirectory(directoryStructureFromPath,directoryStructureToPath,true);
        }

        public void Delete(string directoryStructureFromPath, bool recursive)
        {
            Directory.Delete(directoryStructureFromPath, recursive);
        }
    }
}