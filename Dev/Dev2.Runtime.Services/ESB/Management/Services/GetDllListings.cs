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
using GACManagerApi;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class GetDllListings : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            Dev2Logger.Info("Get Dll Listings", GlobalConstants.WarewolfInfo);
            StringBuilder dllListing;

            values.TryGetValue("currentDllListing", out dllListing);
            if (dllListing != null)
            {
                var src = serializer.Deserialize(dllListing.ToString(), typeof(IFileListing)) as IFileListing;
                try
                {
                    msg.HasError = false;
                    var fileListings = GetDllListing(src);
                    msg.Message = serializer.SerializeToBuilder(fileListings);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    msg.HasError = true;
                    msg.SetMessage(ex.Message);
                }
            }

            return serializer.SerializeToBuilder(msg);
        }

        static List<IFileListing> GetDllListing(IFileListing src)
        {
            var completeList = new List<IFileListing>();
            var fileSystemParent = new DllListing { Name = "File System", IsDirectory = true };
            var gacItem = new DllListing { Name = "GAC", IsDirectory = true };

            if (src == null)
            {
                var drives = DriveInfo.GetDrives().Where(info => info.DriveType!=DriveType.CDRom && info.DriveType!=DriveType.Removable);
                try
                {
                    var listing = drives.Select(BuildDllListing);
                    fileSystemParent.Children = listing.ToList();
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, GlobalConstants.WarewolfError);
                }
                var enumAssembly = new AssemblyCacheEnumerator();
                var assemblyName = enumAssembly.GetNextAssembly();
                var gacList = new List<IFileListing>();
                while (assemblyName != null)
                {
                    //  Create the assembly description.
                    
                    try
                    {
                        var displayName = new AssemblyDescription(assemblyName).DisplayName;
                        var name = GlobalConstants.GACPrefix + displayName;
                        gacList.Add(new DllListing { Name = displayName, FullName = name, IsDirectory = false });
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e.Message, GlobalConstants.WarewolfError);
                    }
                    //  Create an assembly view model.
                    assemblyName = enumAssembly.GetNextAssembly();
                }
                gacItem.Children = gacList;

                completeList.Add(fileSystemParent);
                completeList.Add(gacItem);
            }
            else
            {
                if(src.IsDirectory)
                completeList = GetChildrenForDllListing(new DirectoryInfo(src.FullName));
            }
            return completeList;
        }

        static IFileListing BuildDllListing(DriveInfo info)
        {

            try
            {
                var directory = info.RootDirectory;
                var dllListing = BuildDllListing(directory);
                dllListing.IsDirectory = true;
                return dllListing;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, GlobalConstants.WarewolfError);
            }
            return null;
        }

        static DllListing BuildDllListing(DirectoryInfo directory)
        {
            var dllListing = BuildDllListing(directory as FileSystemInfo);
            try
            {
                dllListing.Children = GetChildrenForDllListing(directory);

            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, GlobalConstants.WarewolfError);
            }
            return dllListing;
        }

        static List<IFileListing> GetChildrenForDllListing(DirectoryInfo directory)
        {
            var directories = directory.EnumerateDirectories();
            var childList = new List<IFileListing>();
            foreach (var directoryInfo in directories)
            {
                if (directoryInfo.Attributes != (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory))
                {
                    var directoryItem = BuildDllListing((FileSystemInfo)directoryInfo);
                    directoryItem.IsDirectory = true;
                    childList.Add(directoryItem);
                }
            }
            var files = directory.EnumerateFiles("*.dll");
            foreach (var fileInfo in files)
            {
                var fileItem = BuildDllListing(fileInfo);
                fileItem.IsDirectory = false;
                childList.Add(fileItem);
            }
            return childList;
        }

        static DllListing BuildDllListing(FileSystemInfo fileInfo)
        {
            var dllListing = new DllListing { Name = fileInfo.Name, FullName = fileInfo.FullName };
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
            return "GetDllListingsService";
        }
    }
}
