using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetFiles : DefaultEsbManagementEndpoint, IGetFiles
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            Dev2Logger.Info("Get Files", GlobalConstants.WarewolfInfo);

            values.TryGetValue("fileListing", out StringBuilder currentFolder);
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
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
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
                    Dev2Logger.Error(e.Message, GlobalConstants.WarewolfError);
                }

            }
            else
            {
                if(src.IsDirectory)
                {
                    completeList = GetChildren(new DirectoryInfo(src.FullName));
                }
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
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, GlobalConstants.WarewolfError);
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
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, GlobalConstants.WarewolfError);
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetFiles";
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