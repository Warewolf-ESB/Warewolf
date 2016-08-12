using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;

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
                    var fileResource = new FileResource
                    {
                        ResourceName = fileInfo.Name,
                        ResourcePath = fileInfo.FullName,
                        ParentPath = fileInfo.DirectoryName,
                        Children = new List<FileResource>()
                    };
                    var files = Directory.GetFiles(fileResource.ResourcePath);
                    if (files.Any())
                    {
                        foreach (var file in files)
                        {
                            fileResource.Children.Add(AddFileChildren(file, fileResource));
                        }
                    }
                    var directories = Directory.GetDirectories(fileResource.ResourcePath);
                    if (directories.Any())
                    {
                        foreach (var directory in directories)
                        {
                            fileResource.Children.Add(AddDirectoryChildren(directory, fileResource));
                        }
                    }
                    fileResources.Add(fileResource);
                }

                return fileResources;
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

        private static FileResource AddFileChildren(string file, FileResource fileResource)
        {
            FileResource newFileResource = new FileResource();
            if (!file.Contains("VersionControl"))
            {
                FileInfo info = new FileInfo(file);
                newFileResource.ResourceName = info.Name;
                newFileResource.ResourcePath = info.FullName;
                newFileResource.ParentPath = fileResource.ResourcePath;
                newFileResource.Children = new List<FileResource>();
            }

            return newFileResource;
        }

        private static FileResource AddDirectoryChildren(string directory, FileResource fileResource)
        {
            FileResource newFileResource = new FileResource();
            if (!directory.Contains("VersionControl"))
            {
                FileInfo info = new FileInfo(directory);
                newFileResource.ResourceName = info.Name;
                newFileResource.ResourcePath = info.FullName;
                newFileResource.ParentPath = fileResource.ResourcePath;
                newFileResource.Children = new List<FileResource>();

                var files = Directory.GetFiles(newFileResource.ResourcePath);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        newFileResource.Children.Add(AddFileChildren(file, newFileResource));
                    }
                }

                var childDir = Directory.GetDirectories(newFileResource.ResourcePath);
                if (childDir.Any())
                {
                    foreach (var child in childDir)
                    {
                        AddDirectoryChildren(child, newFileResource);
                    }
                }
            }
            return newFileResource;
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
            var list = Directory.GetDirectories(_path, "*", SearchOption.AllDirectories).ToList();
            var fileInfos = list.Select(fileName => new FileInfo(fileName)).ToList();
            return fileInfos;
        }
    }
}