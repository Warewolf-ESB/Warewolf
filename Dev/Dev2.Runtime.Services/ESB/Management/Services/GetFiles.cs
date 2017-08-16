using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class GetFiles : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            Dev2Logger.Info("Get Files", "Warewolf Info");
            StringBuilder currentFolder;

            values.TryGetValue("fileListing", out currentFolder);
            if (currentFolder != null)
            {
                var src = serializer.Deserialize(currentFolder.ToString(), typeof(IFileListing)) as IFileListing;
                try
                {
                    msg.HasError = false;
                    var filesAndFolders = GetFilesAndFolders(src);
                    msg.Message = serializer.SerializeToBuilder(filesAndFolders);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, "Warewolf Error");
                    msg.HasError = true;
                    msg.SetMessage(ex.Message);
                }
            }
            else
            {
                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(GetFilesAndFolders(null));
            }

            return serializer.SerializeToBuilder(msg);
        }

        public List<IFileListing> GetFilesAndFolders(IFileListing src)
        {
            var completeList = new List<IFileListing>();
      

            if (src == null)
            {
                var drives = DriveInfo.GetDrives();
                try
                {
                    var listing = drives.Select(BuildFileListing);
            
                    return new List<IFileListing>(listing);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, "Warewolf Error");
                }

            }
            else
            {
                if(src.IsDirectory)
                    completeList = GetChildren(new DirectoryInfo(src.FullName));
            }
            return completeList;
        }

        public IFileListing BuildFileListing(DriveInfo info)
        {

            try
            {
                var directory = info.RootDirectory;
                var dllListing = BuildFileListing(directory);
                dllListing.IsDirectory = true;
                return dllListing;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, "Warewolf Error");
            }
            return null;
        }

        public FileListing BuildFileListing(DirectoryInfo directory)
        {
            var dllListing = BuildFileListing(directory as FileSystemInfo);
            try
            {
                dllListing.Children = GetChildren(directory);

            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, "Warewolf Error");
            }
            return dllListing;
        }

        public List<IFileListing> GetChildren(DirectoryInfo directory)
        {
            var directories = directory.EnumerateDirectories();
            var childList = new List<IFileListing>();
            foreach (var directoryInfo in directories)
            {
                if (directoryInfo.Attributes != (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory))
                {
                    var directoryItem = BuildFileListing((FileSystemInfo)directoryInfo);
                    directoryItem.IsDirectory = true;
                    childList.Add(directoryItem);
                }
            }
            var files = directory.EnumerateFiles();
            foreach (var fileInfo in files)
            {
                var fileItem = BuildFileListing(fileInfo);
                fileItem.IsDirectory = false;
                childList.Add(fileItem);
            }
            return childList;
        }

        public FileListing BuildFileListing(FileSystemInfo fileInfo)
        {
            var dllListing = new FileListing { Name = fileInfo.Name, FullName = fileInfo.FullName };
            return dllListing;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            ServiceAction findDirectoryServiceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "GetFiles";
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }

    public interface IGetFiles
    {
        FileListing BuildFileListing(FileSystemInfo fileInfo);

        IFileListing BuildFileListing(DriveInfo info);

        FileListing BuildFileListing(DirectoryInfo directory);

        List<IFileListing> GetChildren(DirectoryInfo directory);

        List<IFileListing> GetFilesAndFolders(IFileListing src);
    }
}