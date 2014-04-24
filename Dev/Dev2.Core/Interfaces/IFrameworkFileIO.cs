using System;
using System.Collections.Generic;
using System.IO;

namespace Dev2
{
    public interface IFrameworkFileIO
    {
        Stream Get(Uri path, string userName = "", string password = "");
        void Put(Stream data, Uri path, bool overwrite = false, string userName = "", string password = "");
        void Delete(Uri path, string userName = "", string password = "");
        IList<Uri> List(Uri path, string userName = "", string password = "");
        void CreateDirectory(Uri path, string userName = "", string password = "");
        void Copy(Uri sourcePath, Uri destinationPath, bool overWrite = false, string userName = "", string password = "");
        void Move(Uri sourcePath, Uri destinationPath, bool overWrite = false, string userName = "", string password = "");
    }
}
