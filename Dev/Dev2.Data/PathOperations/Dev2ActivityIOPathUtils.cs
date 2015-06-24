
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide common utilty function to the IOPath classes
    /// </summary>
    public static class Dev2ActivityIOPathUtils
    {

        /// <summary>
        /// Extract the full directory portion of a URI
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ExtractFullDirectoryPath(string path)
        {
            string result = path;
            StringBuilder tmpBuilder = new StringBuilder();

            if(!IsDirectory(path))
            {
                char spliter = '/';

                string[] tmp = path.Split(spliter);


                if(tmp.Length == 1)
                {
                    spliter = '\\';
                    tmp = path.Split(spliter);
                }

                for(int i = 0; i < (tmp.Length - 1); i++)
                {
                    tmpBuilder.Append(tmp[i] + spliter);
                }

                result = tmpBuilder.ToString();
            }

            return result;
        }

        /// <summary>
        /// Extract the file name from the URI
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ExtractFileName(string path)
        {
            string result;

            try
            {
                if(!IsDirectory(path))
                {
                    Uri uri = new Uri(path);
                    result = Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    Uri uri = new Uri(path);
                    result = Path.GetFileName(uri.LocalPath);
                }
            }
            catch(Exception)
            {
                result = path;
            }

            return result;
        }

        /// <summary>
        /// Is the request a wild-char request
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsStarWildCard(string path)
        {
            bool result = false;

            Uri uri = new Uri(path);

            string fileName = Path.GetFileName(uri.LocalPath);

            if(fileName != null && (fileName.Contains("*") || fileName.Contains("?")))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Is the path a directory or file?
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectory(string path)
        {
            bool result = false;

            if(path.Contains("ftp://") || path.Contains("ftps://") || path.Contains("sftp://"))
            {
                var ftpUri = new Uri(path);
                var isFile = ftpUri.LocalPath.Contains(".");
                return !isFile;
            }

            if(path.EndsWith("\\") || path.EndsWith("/"))
            {
                result = true;
            }
            else
            {
                int idx = path.LastIndexOf("\\", StringComparison.Ordinal);

                if(idx > 0)
                {
                    if(!path.Substring(idx).Contains("."))
                    {
                        result = true;
                    }
                }
                else
                {
                    idx = path.LastIndexOf("/", StringComparison.Ordinal);
                    if(idx > 0)
                    {
                        if(!path.Substring(idx).Contains("."))
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// convert a list of directory paths into XML
        /// </summary>
        /// <param name="pathList"></param>
        /// <returns></returns>
        public static string ConvertDirectoryListToXML(IList<IActivityIOPath> pathList)
        {

            StringBuilder result = new StringBuilder();

            foreach(IActivityIOPath p in pathList)
            {
                result.Append(p.ToXML());
            }

            return result.ToString();
        }

        public static enActivityIOPathType ExtractPathType(string path)
        {
            enActivityIOPathType result = enActivityIOPathType.Invalid;

            Array vals = Enum.GetValues(typeof(enActivityIOPathType));

            int pos = 0;

            while(pos < vals.Length && result == enActivityIOPathType.Invalid)
            {
                string toCheck = vals.GetValue(pos) + ":";
                string checkPath = path.ToUpper();
                if(checkPath.StartsWith(toCheck))
                {
                    result = (enActivityIOPathType)vals.GetValue(pos);
                }

                pos++;
            }
            return result;
        }
    }
}
