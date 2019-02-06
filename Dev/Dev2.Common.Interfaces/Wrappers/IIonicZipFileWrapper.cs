using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IZipEntry
    {
        void Extract(string extractFromPath, FileOverwrite overwrite);
    }

    public interface IIonicZipFileWrapper : IEnumerable<IZipEntry>, IDisposable
    {
        string Password { get; set; }
    }
    public interface IIonicZipFileWrapperFactory
    {
        IIonicZipFileWrapper Read(string tempFile);
        IIonicZipFileWrapper Read(Stream stream);
    }
}
