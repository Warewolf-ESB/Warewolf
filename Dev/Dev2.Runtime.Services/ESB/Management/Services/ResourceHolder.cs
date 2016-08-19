using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ResourceHolder : IResourceHolder
    {
        private readonly string _path;

        public ResourceHolder(string path)
        {
            _path = path;
        }

        public List<FileInfo> GetFilesList()
        {
            var list = Directory.GetDirectories(_path, "*", SearchOption.TopDirectoryOnly).Where(a => a.Contains("Resources")).ToList();
            var fileInfos = list.Select(fileName => new FileInfo(fileName)).ToList();
            return fileInfos;
        }
    }
}