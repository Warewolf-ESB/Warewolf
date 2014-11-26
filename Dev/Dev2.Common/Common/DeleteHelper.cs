/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;

namespace Dev2.Common.Common
{
    public class DeleteHelper
    {
        public static bool Delete(string path)
        {
            if (path == null)
            {
                return false;
            }

            string dirRoot = Path.GetDirectoryName(path);
            string pattern = Path.GetFileName(path);
            // directory
            if (IsDirectory(path))
            {
                DirectoryHelper.CleanUp(path);
                return true;
            }

            // wild-card char
            if (path.IndexOf("*", StringComparison.Ordinal) >= 0)
            {
                if (pattern != null && dirRoot != null)
                {
                    string[] fileList = Directory.GetFileSystemEntries(dirRoot, pattern, SearchOption.TopDirectoryOnly);
                    foreach (string file in fileList)
                    {
                        if (IsDirectory(file))
                        {
                            //it's a directory
                            Directory.Delete(file, true);
                        }
                        else
                        {
                            // we can before, we want to avoid deleting an already deleted file in sub-directory
                            File.Delete(file);
                        }
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


        private static bool IsDirectory(string path)
        {
            if (path.IndexOf("*", StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            FileAttributes attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }

            return false;
        }
    }
}