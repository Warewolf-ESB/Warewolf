/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace Dev2.Common.Wrappers
{ // not required for code coverage this is simply a pass through required for unit testing
    public class DirectoryWrapper : IDirectory
    {
        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string CreateIfNotExists(string debugOutputPath)
        {
            if (!Directory.Exists(debugOutputPath))
            {
                return Directory.CreateDirectory(debugOutputPath).Name;
            }

            return debugOutputPath;
        }

        public string[] GetLogicalDrives()
        {
            return Directory.GetLogicalDrives();
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetFileSystemEntries(string path)
        {
            return Directory.GetFileSystemEntries(path);
        }

        public string[] GetFileSystemEntries(string path, string searchPattern)
        {
            return Directory.GetFileSystemEntries(path, searchPattern);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
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