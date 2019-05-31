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
using System.Net;
using System.Reflection;
using System.Text;
using Dev2.Common;
using Warewolf.Resource.Errors;

namespace Dev2.Studio.Core.Helpers
{
    public static class FileHelper
    {
        // Used to migrate Dev2 -> Warewolf 
        const string NewPath = @"Warewolf\";
        const string OldPath = @"Dev2\";

        /// <summary>
        /// Gets the ouput path.
        /// </summary>
        public static string GetUniqueOutputPath(string extension)
        {
            var path = Path.Combine(new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                StringResources.App_Data_Directory,
                Guid.NewGuid() + extension
            });
            return path;
        }

        public static void CreateTextFile(StringBuilder outputTxt, string outputPath)
        {
            Dev2Logger.Info("Create Text File for output path: " + outputPath, GlobalConstants.WarewolfInfo);

            EnsurePathIsvalid(outputPath, ".txt");
            var fs = File.Open(outputPath, FileMode.OpenOrCreate, FileAccess.Write);
            using (var writer = new StreamWriter(fs, Encoding.UTF8))
            {
                Dev2Logger.Info("Writing a text file", GlobalConstants.WarewolfInfo);
                writer.Write(outputTxt);
            }
        }

        public static string GetDebugItemTempFilePath(string uri)
        {
            Dev2Logger.Info("Get Debug Item Temp File Path for uri: " + uri, GlobalConstants.WarewolfInfo);

            using (var client = new WebClient { Credentials = CredentialCache.DefaultCredentials })
            {
                var serverLogData = client.UploadString(uri, "");
                var value = serverLogData.Replace("<DataList><Dev2System.ManagmentServicePayload>", "").Replace("</Dev2System.ManagmentServicePayload></DataList>", "");
                var uniqueOutputPath = GetUniqueOutputPath(".txt");
                CreateTextFile(new StringBuilder(value), uniqueOutputPath);
                return uniqueOutputPath;
            }
        }

        /// <summary>
        /// Ensures the path isvalid.
        /// </summary>
        /// <exception cref="System.IO.IOException">File specified in the output path already exists.</exception>
        public static void EnsurePathIsvalid(string outputPath, string validExtension)
        {
            var path = new FileInfo(outputPath);
            var extension = Path.GetExtension(outputPath);

            if(string.Compare(extension, validExtension, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new InvalidOperationException("The output path can only be to a 'xml' or 'zip' file.");
            }

            if(path.Exists)
            {
                throw new IOException("File specified in the output path already exists.");
            }

            if(path.Directory == null)
            {
                throw new IOException(ErrorResource.InvalidOutputPath);
            }

            if(!path.Directory.Exists)
            {
                path.Directory.Create();
            }
        }

        /// <summary>
        /// Gets the full path based on a uri and the current assembly.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>3/6/2013</date>
        public static string GetFullPath(string uri)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            if(directory == null)
            {
                return null;
            }

            var path = Path.Combine(directory, uri);
            return path;
        }

        public static void MigrateTempData(string rootPath)
        {
            var fullNewPath = Path.Combine(rootPath, NewPath);
            var fullOldPath = Path.Combine(rootPath, OldPath);

            if (!Directory.Exists(fullOldPath))
            {
                return;//no old data to migrate
            }

            if(!Directory.Exists(fullNewPath))
            {
                Directory.Move(fullOldPath, fullNewPath);
            }
        }
    }
}
