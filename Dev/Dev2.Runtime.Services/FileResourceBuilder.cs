using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common;

namespace Dev2.Runtime
{
    public class FileResourceBuilder
    {
        private readonly IResourceHolder _resourceHolder;

        public FileResourceBuilder(IResourceHolder resourceHolder)
        {
            _resourceHolder = resourceHolder;
        }

        public IEnumerable<FileResource> Build()
        {
            try
            {
                var fileResources = new List<FileResource>();
                var filesList = _resourceHolder.GetFilesList();
                foreach (var fileInfo in filesList)
                {
                    var fileResource = new FileResource();
                    fileResource.Name = fileInfo.Name;
                    fileResource.ResourcePath = fileInfo.FullName;
                    fileResource.Children = new List<FileResource>();
                    var directories = Directory.GetDirectories(fileResource.Name);
                    foreach (var directory in directories)
                    {
                        FileInfo info = new FileInfo(directory);
                        fileResource.Children.Add(new FileResource
                        {
                            
                        });
                    }
                 
                }

                return new List<FileResource>();
            }
            catch (Exception ex) when (ex is AccessViolationException)
            {
                Dev2Logger.Error(ex.Message,ex);
                throw;
            }
            catch (Exception ex) when (ex is IOException)
            {
                Dev2Logger.Error(ex.Message, ex);
                throw;
            }
        }
    }

    public interface IResourceHolder
    {
        List<FileInfo> GetFilesList();
    }
    public class ResourceHolder : IResourceHolder
    {
        private readonly string _path;

        public ResourceHolder(string path)
        {
            _path = path;
        }

        public List<FileInfo> GetFilesList()
        {
            var list = Directory.GetDirectories(_path, ".*", SearchOption.AllDirectories).ToList();
            var fileInfos = list.Select(fileName => new FileInfo(fileName)).ToList();
            return fileInfos;
        }
    }
}