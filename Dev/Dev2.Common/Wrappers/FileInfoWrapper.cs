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
using System.IO;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Wrappers
{
    public class FileInfoWrapper : IFileInfo
    {
        readonly FileInfo _fileInfo;

        public FileInfoWrapper(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public DateTime CreationTime => _fileInfo.CreationTime;

        public void Delete()
        {
            _fileInfo.Delete();
        }
        public string Name => _fileInfo.Name;

        public IDirectoryInfo Directory => new DirectoryInfoWrapper(_fileInfo.Directory);
    }
}
