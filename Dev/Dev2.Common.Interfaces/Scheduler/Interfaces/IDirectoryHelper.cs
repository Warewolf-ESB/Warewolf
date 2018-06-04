/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface IDirectoryHelper
    {
        string[] GetFiles(string path);
        string CreateIfNotExists(string debugOutputPath);
        IEnumerable<string> GetFilesByExtensions(string path, params string[] extensions);
        void Copy(string sourceDirName, string destDirName, bool copySubDirs);
        void CleanUp(string path);
    }
}