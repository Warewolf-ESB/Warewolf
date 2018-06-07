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
using Dev2.Common.Interfaces.Scheduler.Interfaces;

namespace Dev2.Common.Common
{
    public static class DeleteHelper
    {
        public static IDirectoryHelper DirectoryHelperInstance()
        {
            return new DirectoryHelper();
        }
        public static bool Delete(string path)
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
                DirectoryHelperInstance().CleanUp(path);
                return true;
            }

            // wild-card char
            if (path.IndexOf("*", StringComparison.Ordinal) >= 0)
            {
                if (dirRoot != null)
                {
                    var fileList = Directory.GetFileSystemEntries(dirRoot, pattern, SearchOption.TopDirectoryOnly);
                    foreach (string file in fileList)
                    {
                        DeletePath(file);
                    }
                }
            }
            else
            {
                // single file delete
                File.Delete(path);
            }

            return true;
        }

        static void DeletePath(string path)
        {
            if (IsDirectory(path))
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }

        static bool IsDirectory(string path)
        {
            if (path.IndexOf("*", StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            var attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }

            return false;
        }
    }
}