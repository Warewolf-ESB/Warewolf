#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            Dev2Logger.Info("Get Files", GlobalConstants.WarewolfInfo);

            values.TryGetValue("fileListing", out StringBuilder currentFolder);
            
            try
            {
                var src = DeserializeFileListing(currentFolder, serializer);
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

            return serializer.SerializeToBuilder(msg);
        }

        IFileListing DeserializeFileListing(StringBuilder currentFolder, Dev2JsonSerializer serializer)
        {
            if (currentFolder == null || currentFolder.Length == 0)
            {
                return null;
            }

            var jsonString = currentFolder.ToString();
            
            // Determine concrete type based on JSON properties
            var targetType = jsonString.Contains("\"Is32Bit\"", StringComparison.OrdinalIgnoreCase) || 
                           jsonString.Contains("\"ClsId\"", StringComparison.OrdinalIgnoreCase)
                ? typeof(DllListing)
                : typeof(FileListing);

            return serializer.Deserialize(jsonString, targetType) as IFileListing;
        }

        public List<IFileListing> GetFilesAndFolders(IFileListing src)
        {
            if (src == null)
            {
                return GetDriveListings();
            }

            if (src.IsDirectory)
            {
                return GetChildren(new DirectoryInfo(src.FullName));
            }

            return new List<IFileListing>();
        }

        List<IFileListing> GetDriveListings()
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                var listing = drives.Select(BuildFileListing).Where(l => l != null);
                return new List<IFileListing>(listing);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                return new List<IFileListing>();
            }
        }

        public IFileListing BuildFileListing(DriveInfo info)
        {
            try
            {
                var dllListing = BuildFileListing(info.RootDirectory);
                dllListing.IsDirectory = true;
                return dllListing;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, GlobalConstants.WarewolfError);
                return null;
            }
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
            var childList = new List<IFileListing>();

            if (!directory.Exists) return childList;

            try
            {
                var directories = directory.EnumerateDirectories()
                    .Where(d => d.Attributes != (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory))
                    .Select(d =>
                    {
                        var item = BuildFileListing((FileSystemInfo)d);
                        item.IsDirectory = true;
                        return item;
                    });

                childList.AddRange(directories);

                var files = directory.EnumerateFiles()
                    .Select(f =>
                    {
                        var item = BuildFileListing(f);
                        item.IsDirectory = false;
                        return item;
                    });

                childList.AddRange(files);
            }
            catch (UnauthorizedAccessException uae)
            {
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, uae, GlobalConstants.WarewolfError);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorEnumeratingDirectory, e, GlobalConstants.WarewolfError);
            }

            return childList;
        }

        public FileListing BuildFileListing(FileSystemInfo fileInfo)
        {
            return new FileListing 
            { 
                Name = fileInfo.Name, 
                FullName = fileInfo.FullName 
            };
        }

        public override DynamicService CreateServiceEntry() => 
            EsbManagementServiceEntry.CreateESBManagementServiceEntry(
                HandlesType(), 
                "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

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