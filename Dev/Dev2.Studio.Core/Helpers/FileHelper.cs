using System;
using System.IO;
using System.Reflection;
using Dev2.Common;
using Dev2.Studio.Core.Interfaces;
using Ionic.Zip;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Gets the ouput path.
        /// </summary>
        public static string GetUniqueOutputPath(string extension)
        {
            var path = Path.Combine(new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                StringResources.App_Data_Directory,
                StringResources.Feedback_Recordings_Directory,
                Guid.NewGuid().ToString() + extension
            });
            return path;
        }

        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="outputTxt">The text to output to the path</param>
        /// <param name="outputPath">The output path.</param>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public static void CreateTextFile(string outputTxt, string outputPath)
        {
            EnsurePathIsvalid(outputPath, ".txt");
            var fs = File.Open(outputPath,
                                      FileMode.OpenOrCreate,
                                      FileAccess.Write);
            using(var writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                writer.Write(outputTxt);
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
                throw new IOException("Output path is invalid.");
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
            if(directory == null) return null;
            var path = Path.Combine(directory, uri);
            return path;
        }

        public static string GetServerLogTempPath(IEnvironmentModel environmentModel)
        {
            // PBI 9598 - 2013.06.10 - TWR : environmentModel may be null for disconnected scenario's
            if(environmentModel == null)
            {
                return string.Empty;
            }

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FetchCurrentServerLogService";
            string serverLogData = environmentModel.Connection.ExecuteCommand(dataObj.XmlString, environmentModel.Connection.WorkspaceID, GlobalConstants.NullDataListID);
            string uniqueOutputPath = GetUniqueOutputPath(".txt");
            CreateTextFile(serverLogData, uniqueOutputPath);
            string sourceDirectoryName = Path.GetDirectoryName(uniqueOutputPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(uniqueOutputPath);
            string destinationArchiveFileName = Path.Combine(sourceDirectoryName, fileNameWithoutExtension + ".zip");
            ZipFile zip = new ZipFile();
            zip.AddFile(uniqueOutputPath, ".");
            zip.Save(destinationArchiveFileName);
            return destinationArchiveFileName;
        }
    }
}
