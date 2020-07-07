/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System.IO;

namespace Dev2.Common.Wrappers
{
    public class FilePathWrapper : IFilePath
    {
        public string GetFileName(string path) => Path.GetFileName(path);

        public string Combine(params string[] paths) => Path.Combine(paths);

        public bool IsPathRooted(string path) => Path.IsPathRooted(path);

        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);
    }
}