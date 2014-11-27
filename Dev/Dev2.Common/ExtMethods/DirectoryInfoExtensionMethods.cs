/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;

namespace Dev2.Common.ExtMethods
{
    public static class DirectoryInfoExtensionMethods
    {
        public static void Copy(this DirectoryInfo dir, string destDirectory, bool recursive, bool overrideFiles)
        {
            if (dir == null)
            {
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(
                    "Source directory does not exist or could not be found: "
                    + dir.FullName));
            }

            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirectory, file.Name);
                bool fileExists = File.Exists(temppath);

                if (fileExists && overrideFiles)
                {
                    file.CopyTo(temppath, true);
                }
                else if (!fileExists)
                {
                    file.CopyTo(temppath, false);
                }
            }

            if (recursive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirectory, subdir.Name);
                    Copy(subdir, temppath, true, overrideFiles);
                }
            }
        }
    }
}