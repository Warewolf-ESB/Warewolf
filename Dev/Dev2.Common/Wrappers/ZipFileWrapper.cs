
using Dev2.Common.Interfaces.Wrappers;
using System.IO.Compression;

namespace Dev2.Common.Wrappers
{
    public class ZipFileWrapper : IZipFile
    {
        public void CreateFromDirectory(string sourceDirectory, string destinationZippedDirectory)
        {
            ZipFile.CreateFromDirectory(sourceDirectory, destinationZippedDirectory);
        }
    }
}