using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Reflection;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// The find directory service
    /// </summary>
    public class GetDllListings : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            Dev2Logger.Log.Info("Get Dll Listings");
            string username = string.Empty;
            string domain = string.Empty;
            string password = string.Empty;

            StringBuilder tmp;

            values.TryGetValue("Username", out tmp);
            if(tmp != null)
            {
                username = tmp.ToString();
            }

            values.TryGetValue("Password", out tmp);
            if(tmp != null)
            {
                password = tmp.ToString();
            }
            values.TryGetValue("Domain", out tmp);
            if(tmp != null)
            {
                domain = tmp.ToString();
            }

            IntPtr accessToken = IntPtr.Zero;
            // ReSharper disable InconsistentNaming
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;


            try
            {
                if(username.Length > 0)
                {
                    domain = (domain.Length > 0 && domain != ".") ? domain : Environment.UserDomainName;
                    bool success = LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE,
                        LOGON32_PROVIDER_DEFAULT, ref accessToken);
                    if(success)
                    {
                        var identity = new WindowsIdentity(accessToken);
                        WindowsImpersonationContext context = identity.Impersonate();
                        // get the file attributes for file or directory
                        msg.HasError = false;
                        msg.Message = serializer.SerializeToBuilder(GetDllListing());
                        context.Undo();
                    }
                    else
                    {
                        msg.HasError = true;
                        msg.SetMessage("Login Failure : Unknown username or password");
                        
                    }
                }
                else
                {
                    msg.HasError = false;
                    msg.Message = serializer.SerializeToBuilder(GetDllListing());
                }
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                msg.HasError = true;
                msg.SetMessage(ex.Message);
            }

            return serializer.SerializeToBuilder(msg);
        }

        static List<DllListing> GetDllListing()
        {
            var completeList = new List<DllListing>();
            var fileSystemParent = new DllListing{Name = "FileSystem"};
            var gacItem = new DllListing { Name = "GAC" };
            

            var drives = DriveInfo.GetDrives();
            try
            {
                var listing = drives.Select(BuildDllListing);
                fileSystemParent.Children = listing.ToList();
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e.Message);
            }
            IAssemblyName assemblyName;
            IAssemblyEnum assemblyEnum = GAC.CreateGACEnum();
            var gacList = new List<DllListing>();
            while (GAC.GetNextAssembly(assemblyEnum, out assemblyName) == 0)
            {
                try
                {
                    gacList.Add(new DllListing{Name=GAC.GetDisplayName(assemblyName, ASM_DISPLAY_FLAGS.VERSION | ASM_DISPLAY_FLAGS.CULTURE | ASM_DISPLAY_FLAGS.PUBLIC_KEY_TOKEN)});
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e.Message);
                }
            }
            gacItem.Children = gacList;

            completeList.Add(fileSystemParent);
            completeList.Add(gacItem);
            return completeList;
        }

        static DllListing BuildDllListing(DriveInfo info)
        {
            
            try
            {
                var directory = info.RootDirectory;
                var dllListing = BuildDllListing(directory);
                return dllListing;
                //                var directories = directory.EnumerateDirectories();
                //                var files = directory.EnumerateFiles("*.dll");
                //                foreach(var directoryInfo in directories)
                //                {
                //                    dllListing.Children.Add(BuildDllListing(directoryInfo));
                //                }
                //                foreach (var fileInfo in files)
                //                {
                //                    dllListing.Children.Add(BuildDllListing(fileInfo));
                //                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("Error enumerating directory.",e);
            }
            return null;
        }

        static DllListing BuildDllListing(DirectoryInfo directory)
        {
            var dllListing = BuildDllListing(directory as FileSystemInfo);
            try
            {
                dllListing.Children = new List<DllListing>();
                var directories = directory.EnumerateDirectories();
                var files = directory.EnumerateFiles("*.dll");
                foreach (var directoryInfo in directories)
                {
                    dllListing.Children.Add(BuildDllListing(directoryInfo));
                }
                foreach(var fileInfo in files)
                {
                    dllListing.Children.Add(BuildDllListing(fileInfo));
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("Error enumerating directory.", e);
            }
            return dllListing;
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
                DataListSpecification = new StringBuilder("<DataList><Domain ColumnIODirection=\"Input\"/><Username ColumnIODirection=\"Input\"/><Password ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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

        #region Private Methods

        //We use the following to impersonate a user in the current execution environment
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);     

        #endregion
    }
}