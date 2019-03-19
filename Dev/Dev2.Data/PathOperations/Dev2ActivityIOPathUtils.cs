#pragma warning disable
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
using System.Text;
using Dev2.Data.Interfaces.Enums;

using static System.IO.Path;

namespace Dev2.PathOperations
{
    public static class Dev2ActivityIOPathUtils
    {
        public static string ExtractFullDirectoryPath(string path)
        {
            var result = path;
            var tmpBuilder = new StringBuilder();

            if (!IsDirectory(path))
            {
                var spliter = '/';

                var tmp = path.Split(spliter);


                if (tmp.Length == 1)
                {
                    spliter = '\\';
                    tmp = path.Split(spliter);
                }

                for (int i = 0; i < tmp.Length - 1; i++)
                {
                    tmpBuilder.Append(tmp[i] + spliter);
                }

                result = tmpBuilder.ToString();
            }

            return result;
        }
        
        public static string ExtractFileName(string path)
        {
            string result;
            try
            {
                var uri = new Uri(path);
                result = Path.GetFileName(uri.LocalPath);
            }
            catch(Exception)
            {
                result = path;
            }

            return result;
        }
        
        public static bool IsStarWildCard(string path)
        {
            var result = false;
            var uri = new Uri(path);
            var fileName = Path.GetFileName(uri.LocalPath);
            if (fileName.Contains(@"*") || fileName.Contains(@"?"))
            {
                result = true;
            }

            return result;
        }
        
        public static bool IsDirectory(string path)
        {
            var result = false;

            if (path.Contains(@"ftp://") || path.Contains(@"ftps://") || path.Contains(@"sftp://"))
            {
                var ftpUri = new Uri(path);
                var isFile = ftpUri.LocalPath.Contains(@".");
                return !isFile;
            }

            if (path.EndsWith(@"\\", StringComparison.Ordinal) || path.EndsWith(@"/", StringComparison.Ordinal))
            {
                result = true;
            }
            else
            {
                var idx = path.LastIndexOf(@"\\", StringComparison.Ordinal);

                if (idx > 0)
                {
                    if (!path.Substring(idx).Contains(@"."))
                    {
                        result = true;
                    }
                }
                else
                {
                    result = IfFileNameContainsADot(path);
                }
            }

            return result;
        }

        private static bool IfFileNameContainsADot(string path)
        {
            bool result = false;
            int idx = path.LastIndexOf(@"/", StringComparison.Ordinal);
            if (idx > 0)
            {
                if (!path.Substring(idx).Contains(@"."))
                {
                    result = true;
                }
            }
            else
            {
                if (!path.Contains(@"."))
                {
                    result = true;
                }
            }

            return result;
        }

        public static enActivityIOPathType ExtractPathType(string path)
        {
            var result = enActivityIOPathType.Invalid;

            var vals = Enum.GetValues(typeof(enActivityIOPathType));

            var pos = 0;

            while (pos < vals.Length && result == enActivityIOPathType.Invalid)
            {
                var toCheck = vals.GetValue(pos) + @":";
                var checkPath = path.ToUpper();
                if (checkPath.StartsWith(toCheck))
                {
                    result = (enActivityIOPathType)vals.GetValue(pos);
                }

                pos++;
            }
            return result;
        }
    }
}
