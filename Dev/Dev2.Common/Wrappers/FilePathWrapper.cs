/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        public string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }
        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }
    }
}