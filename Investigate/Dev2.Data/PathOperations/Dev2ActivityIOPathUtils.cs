using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dev2.PathOperations {
    
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide common utilty function to the IOPath classes
    /// </summary>
    public static class Dev2ActivityIOPathUtils {

        /// <summary>
        /// Extract the full directory portion of a URI
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ExtractFullDirectoryPath(string path) {
            string result = path;
            StringBuilder tmpBuilder = new StringBuilder();

            if (!IsDirectory(path)) {
                char spliter = '/';

                string[] tmp = path.Split(spliter);

                if (tmp.Length == 1) {
                    spliter = '\\';
                    tmp = path.Split(spliter);
                }

                for (int i = 0; i < (tmp.Length - 1); i++) {
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
        public static string ExtractFileName(string path) {
            string result = string.Empty;

            if (!IsDirectory(path)) {
                Uri uri = new Uri(path);

                result = Path.GetFileName(uri.LocalPath);
            }

            return result;
        }

        /// <summary>
        /// Is the request a wild-char request
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsStarWildCard(string path) {
            bool result = false;

            Uri uri = new Uri(path);
           
            string fileName = Path.GetFileName(uri.LocalPath);

            if (fileName.Contains("*") || fileName.Contains("?")) {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Is the path a directory or file?
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectory(string path) {
            bool result = false;

            if (path.EndsWith("\\") || path.EndsWith("/")) {
                result = true;
            }
            else {
                int idx = path.LastIndexOf("\\");

                if (idx > 0) {
                    if (!path.Substring(idx).Contains(".")) {
                        result = true;
                    }
                }
                else {
                    idx = path.LastIndexOf("/");
                    if (idx > 0) {
                        if (!path.Substring(idx).Contains(".")) {
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
        public static string ConvertDirectoryListToXML(IList<IActivityIOPath> pathList) {

            StringBuilder result = new StringBuilder();

            foreach (IActivityIOPath p in pathList) {
                result.Append(p.ToXML());
            }

            return result.ToString();
        }

        public static enActivityIOPathType ExtractPathType(string path) {
            enActivityIOPathType result = enActivityIOPathType.Invalid;

            Array vals = Enum.GetValues(typeof(enActivityIOPathType));

            int pos = 0;

            while (pos < vals.Length && result == enActivityIOPathType.Invalid) {
                string toCheck = vals.GetValue(pos).ToString() + ":";
                string checkPath = path.ToUpper();
                if ( checkPath.StartsWith(toCheck) ) {
                    result = (enActivityIOPathType)vals.GetValue(pos);
                }

                pos++;
            }

            return result;
        }
    }
}
