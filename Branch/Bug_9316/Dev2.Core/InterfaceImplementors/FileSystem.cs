using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Dev2 {
    public class FileSystem : IFrameworkFileIO {
        public void Delete(Uri path, string userName = "", string password = "") {
            File.Delete(path.LocalPath);
        }

        public Stream Get(Uri path, string userName = "", string password = "") {
            return new FileStream(path.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }


        public IList<Uri> List(Uri path, string userName = "", string password = "") {
            List<Uri> list = new List<Uri>();
            Directory.GetFiles(path.LocalPath).ToList().ForEach(item => list.Add(new Uri(item)));
            return list;
        }

        public void CreateDirectory(Uri path, string userName = "", string password = "") {
            Directory.CreateDirectory(path.LocalPath);
        }

        public void Put(System.IO.Stream data, Uri path, bool overwrite = false, string userName = "", string password = "") {
            File.WriteAllBytes(path.LocalPath, data.ToByteArray());
        }

        public void Copy(Uri sourcePath, Uri destinationPath, bool overWrite, string userName = "", string password = "") {

            File.Copy(sourcePath.LocalPath, destinationPath.LocalPath, overWrite);
        }

        public void Move(Uri sourcePath, Uri destinationPath, bool overWrite = false, string userName = "", string password = "") {
            if (overWrite) {
                File.Delete(destinationPath.LocalPath);
                File.Move(sourcePath.LocalPath, destinationPath.LocalPath);
            }
            else {
                File.Move(sourcePath.LocalPath, destinationPath.LocalPath);
            }
        }
    }
}
