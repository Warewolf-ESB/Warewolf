/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDirectory
    {
        string[] GetFiles(string path);
        string CreateIfNotExists(string debugOutputPath);
        string[] GetLogicalDrives();
        bool Exists(string path);
        string[] GetFileSystemEntries(string path);
        string[] GetFileSystemEntries(string path, string searchPattern);
        string[] GetDirectories(string workspacePath);
        void Move(string directoryStructureFromPath, string directoryStructureToPath);
        void Delete(string directoryStructureFromPath, bool recursive);
    }
}