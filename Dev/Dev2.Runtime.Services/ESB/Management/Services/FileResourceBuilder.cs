using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class FileResourceBuilder : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        private readonly IResourceHolder _resourceHolder;

        public FileResourceBuilder(IResourceHolder resourceHolder)
        {
            _resourceHolder = resourceHolder;
        }

        public FileResourceBuilder()
        {
            _resourceHolder = new ResourceHolder(EnvironmentVariables.AppDataPath);
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
                        Children = new List<IFileResource>()
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
                Dev2Logger.Error(ex.Message,ex, GlobalConstants.WarewolfError);
                throw;
            }
            catch (Exception ex) when (ex is IOException)
            {
                Dev2Logger.Error(ex.Message, ex, GlobalConstants.WarewolfError);
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
                newFileResource.Children = new List<IFileResource>();
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
                newFileResource.Children = new List<IFileResource>();

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

        public string HandlesType()
        {
            return "FileResourceBuilder";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var list = Build().ToList();
            var serializer = new Dev2JsonSerializer();

            return serializer.SerializeToBuilder(new ExecuteMessage() { HasError = false, Message = serializer.SerializeToBuilder(list) });
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
    }
}