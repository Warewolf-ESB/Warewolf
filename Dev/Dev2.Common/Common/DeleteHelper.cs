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
using System.IO;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Common.Common
{
    public interface IDeleteHelper
    {
        bool Delete(string path);
    }
    public class DeleteHelper : IDeleteHelper
    {
        readonly IFile _file;
        readonly IDirectory _directory;

        public DeleteHelper()
            : this(new FileWrapper(), new DirectoryWrapper())
        { }
        public DeleteHelper(IFile file, IDirectory dir)
        {
            this._file = file;
            this._directory = dir;
        }

        public bool Delete(string path)
        {
            if (path == null)
            {
                return false;
            }

            var dirRoot = Path.GetDirectoryName(path);
            var pattern = Path.GetFileName(path);
            // directory
            if (IsDirectory(path))
            {
                new DirectoryWrapper().CleanUp(path);
                return true;
            }

            // wild-card char
            if (path.IndexOf("*", StringComparison.Ordinal) >= 0)
            {
                if (dirRoot != null)
                {
                    var fileList = _directory.GetFileSystemEntries(dirRoot, pattern, SearchOption.TopDirectoryOnly);
                    foreach (string file in fileList)
                    {
                        DeletePath(file);
                    }
                }
            }
            else
            {
                // single file delete
                _file.Delete(path);
            }

            return true;
        }

        void DeletePath(string path)
        {
            if (IsDirectory(path))
            {
                _directory.Delete(path, true);
            }
            else
            {
                _file.Delete(path);
            }
        }

        bool IsDirectory(string path)
        {
            if (path.IndexOf("*", StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            var attr = _file.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }

            return false;
        }
    }
}