using System;
using System.IO;

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
            using (var writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
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

            if (string.Compare(extension, validExtension, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new InvalidOperationException("The output path can only be to a 'xml' or 'zip' file.");
            }

            if (path.Exists)
            {
                throw new IOException("File specified in the output path already exists.");
            }

            if (path.Directory == null)
            {
                throw new IOException("Output path is invalid.");
            }

            if (!path.Directory.Exists)
            {
                path.Directory.Create();
            }
        }
    }
}
