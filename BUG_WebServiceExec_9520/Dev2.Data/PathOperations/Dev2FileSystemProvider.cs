using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Dev2.Common;
using Dev2.Common.Common;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace Dev2.PathOperations {

    /// <summary>
    /// Used for internal security reasons
    /// </summary>
    
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeTokenHandle()
            : base(true) {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle() {
            return CloseHandle(handle);
        }
    }

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide file system IO operations to the File activities
    /// </summary>
    [Serializable]
    public class Dev2FileSystemProvider : IActivityIOOperationsEndPoint {

        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 
        const int LOGON32_LOGON_INTERACTIVE = 2;

        // TODO : Implement as per Unlimited.Framework.Plugins.FileSystem in the Unlimited.Framework.Plugins project
        // Make sure to replace Uri with IActivity references

        public Dev2FileSystemProvider() { }

        public IActivityIOPath IOPath {
            get;
            set;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Stream Get(IActivityIOPath path) {
            Stream result = null;

            if (!RequiresAuth(path)) {
                result = new MemoryStream(File.ReadAllBytes(path.Path));
            }
            else {
                // handle UNC path
                SafeTokenHandle safeTokenHandle;

                try {
                    string user = ExtractUserName(path);
                    string domain = ExtractDomain(path);
                    bool loginOk = LogonUser(user, domain, path.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                    if (loginOk) {
                        using (safeTokenHandle) {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                // Do the operation here

                                result = new MemoryStream(File.ReadAllBytes(path.Path));

                                impersonatedUser.Undo(); // remove impersonation now
                            }
                        }
                    }
                    else {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + path.Username + " ] for resource [ " + path.Path + " ] ");
                    }
                }
                catch (Exception ex) {
                    ServerLogger.LogError(ex);
                    throw;
                }

            }
            
            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public int Put(Stream src, IActivityIOPath dst, Dev2CRUDOperationTO args) {
            int result = -1;

            if ( (args.Overwrite) || (!args.Overwrite && !FileExist(dst))) {
                if (!RequiresAuth(dst)) {
                    File.WriteAllBytes(dst.Path, src.ToByteArray());
                    result = (int)src.Length;
                }
                else {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;

                    try {
                        bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                        if (loginOk) {
                            using (safeTokenHandle) {

                                WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                    // Do the operation here

                                    File.WriteAllBytes(dst.Path, src.ToByteArray());
                                    result = (int)src.Length;

                                    // remove impersonation now
                                    impersonatedUser.Undo(); 
                                }
                            }
                        }
                        else {
                            // login failed
                            throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                        }
                    }
                    catch (Exception ex) {
                        ServerLogger.LogError(ex);
                        throw;
                    }
                }
            }

            src.Close();

            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool Delete(IActivityIOPath src) {
            bool result = false;

            try {
                if (!RequiresAuth(src)) {
                    if (File.Exists(src.Path)) {
                        File.Delete(src.Path);
                        result = true;
                    }
                    else if (Directory.Exists(src.Path)) {
                        DisplayAndWriteError(src.Path);
                        DirectoryHelper.CleanUp(src.Path);
                        result = true;
                    }
                    else {
                        result = false;
                    }
                }
                else {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;

                    try {
                        bool loginOk = LogonUser(ExtractUserName(src), ExtractDomain(src), src.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                        if (loginOk) {
                            using (safeTokenHandle) {

                                WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                    // Do the operation here

                                    if (PathIs(src) == enPathType.File) {
                                         if (File.Exists(src.Path)) {
                                            File.Delete(src.Path);
                                             result = true;
                                        }

                                    }
                                    else {
                                        if (Directory.Exists(src.Path)) {
                                            DirectoryHelper.CleanUp(src.Path);
                                            result = true;
                                        }
                                    }

                                    // remove impersonation now
                                    impersonatedUser.Undo();
                                }
                            }
                        }
                        else {
                            // login failed
                            throw new Exception("Failed to authenticate with user [ " + src.Username + " ] for resource [ " + src.Path + " ] ");
                        }
                    }
                    catch (Exception ex) {
                        ServerLogger.LogError(ex);
                        throw;
                    }
                }
            }
            catch (Exception) {
                // might be a directory instead
                if (!RequiresAuth(src)) {
                    DisplayAndWriteError(src.Path);
                    DirectoryHelper.CleanUp(src.Path);
                    result = true;
                }
                else {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;

                    try {
                        bool loginOk = LogonUser(ExtractUserName(src), ExtractDomain(src), src.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                        if (loginOk) {
                            using (safeTokenHandle) {

                                WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                    // Do the operation here

                                    DirectoryHelper.CleanUp(src.Path);
                                    result = true;

                                    // remove impersonation now
                                    impersonatedUser.Undo();
                                }
                                newID.Dispose();
                            }
                        }
                        else {
                            // login failed
                            throw new Exception("Failed to authenticate with user [ " + src.Username + " ] for resource [ " + src.Path + " ] ");
                        }
                    }
                    catch (Exception ex) {
                        ServerLogger.LogError(ex);
                        throw;
                    }
                }
            }

            return result;
        }

        void DisplayAndWriteError(string path)
        {
            var msg = string.Format("Attempt to delete: {0}", path);
            ServerLogger.LogTrace(msg);            
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src) {
            IList<IActivityIOPath> result = new List<IActivityIOPath>();

            string path = src.Path;

            if (!path.EndsWith("\\") && PathIs(src) == enPathType.Directory) {
                path += "\\";
            }

            if (!RequiresAuth(src))
            {
                try
                {
                    
                    IEnumerable<string> dirs = null;

                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(path)) 
                    {
                        if (Directory.Exists(path))
                        {
                            dirs = Directory.EnumerateFileSystemEntries(path);
                        }
                        else
                        {
                            return null;// throw new Exception("Directory not found [ " + src.Path + " ] ");
                        }
                    }
                    else {
                        // we have a wildchar path ;)
                        string baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(path);
                        string pattern = Dev2ActivityIOPathUtils.ExtractFileName(path);

                        dirs = Directory.EnumerateFileSystemEntries(baseDir, pattern);
                    }

                    if (dirs != null) {
                        foreach (string d in dirs) {
                            result.Add(ActivityIOFactory.CreatePathFromString(d));
                        }
                    }
                }
                catch (Exception) {
                    throw new Exception("Directory not found [ " + src.Path + " ] ");
                }
            }
            else {
                // handle UNC path
                SafeTokenHandle safeTokenHandle;

                try {
                    bool loginOk = LogonUser(ExtractUserName(src), ExtractDomain(src), src.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                    if (loginOk) {
                        using (safeTokenHandle) {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                // Do the operation here

                                try {

                                    IEnumerable<string> dirs = null;

                                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(path)) {
                                        dirs = Directory.EnumerateFileSystemEntries(path);
                                    }
                                    else {
                                        // we have a wildchar path ;)
                                        string baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(path);
                                        string pattern = Dev2ActivityIOPathUtils.ExtractFileName(path);

                                        dirs = Directory.EnumerateFileSystemEntries(baseDir, pattern);
                                    }

                                    if (dirs != null) {
                                        foreach (string d in dirs) {
                                            result.Add(ActivityIOFactory.CreatePathFromString(d));
                                        }
                                    }

                                }
                                catch (Exception) {
                                    throw new Exception("Directory not found [ " + src.Path + " ] ");
                                }

                                // remove impersonation now
                                impersonatedUser.Undo();
                                newID.Dispose();
                            }
                        }
                    }
                    else {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + src.Username + " ] for resource [ " + src.Path + " ] ");
                    }
                }
                catch (Exception ex) {
                    ServerLogger.LogError(ex);
                    throw;
                }

            }

            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool PathExist(IActivityIOPath dst) {
            bool result = false;

            if (!RequiresAuth(dst)) {
                if (PathIs(dst) == enPathType.Directory){
                    result = Directory.Exists(dst.Path);
                }
                else {
                    result = File.Exists(dst.Path);
                }
            }
            else {
                // handle UNC path
                SafeTokenHandle safeTokenHandle;

                try {
                    bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                    if (loginOk) {
                        using (safeTokenHandle) {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                // Do the operation here

                                if (PathIs(dst) == enPathType.Directory){
                                    result = Directory.Exists(dst.Path);
                                }
                                else {
                                    result = File.Exists(dst.Path);
                                }

                                // remove impersonation now
                                impersonatedUser.Undo();
                            }
                            newID.Dispose();
                        }
                    }
                    else {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                    }
                }
                catch (Exception ex) {
                    ServerLogger.LogError(ex);
                    throw;
                }
            }

            return result;
        }

        /*
         * Check for the existance of each directory?!
         */
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool CreateDirectory(IActivityIOPath dst, Dev2CRUDOperationTO args) {
            bool result = false;

            if (args.Overwrite) {
                if (!RequiresAuth(dst)) {
                    if (DirectoryExist(dst)) {
                        Delete(dst);
                    }
                    Directory.CreateDirectory(dst.Path);
                    result = true;
                }
                else {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;

                    try {
                        bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                        if (loginOk) {
                            using (safeTokenHandle) {

                                WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                    // Do the operation here

                                    if (DirectoryExist(dst)) {
                                        Delete(dst);
                                    }
                                    Directory.CreateDirectory(dst.Path);
                                    result = true;

                                    // remove impersonation now
                                    impersonatedUser.Undo();
                                }
                            }
                        }
                        else {
                            // login failed, oh no!
                            throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                        }
                    }
                    catch (Exception ex) {
                        ServerLogger.LogError(ex);
                        throw;
                    }
                }
            }
            else if (!args.Overwrite && !DirectoryExist(dst)) {
                if (!RequiresAuth(dst)) {
                    Directory.CreateDirectory(dst.Path);
                    result = true;
                }
                else {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;

                    try {
                        bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                        if (loginOk) {
                            using (safeTokenHandle) {

                                WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                                    // Do the operation here

                                    Directory.CreateDirectory(dst.Path);
                                    result = true;

                                    // remove impersonation now
                                    impersonatedUser.Undo();
                                }
                                newID.Dispose();
                            }
                        }
                        else {
                            // login failed
                            throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                        }
                    }
                    catch (Exception ex) {
                        ServerLogger.LogError(ex);
                        throw;
                    }
                }
            }

            return result;
        }

        public bool RequiresLocalTmpStorage() {
            return false;
        }


        public bool HandlesType(enActivityIOPathType type) {
            
            return (type == enActivityIOPathType.FileSystem);
        }

        public enPathType PathIs(IActivityIOPath path) {
            enPathType result = enPathType.File;

            if (path.Path.StartsWith("\\\\")) {
                if (Dev2ActivityIOPathUtils.IsDirectory(path.Path)) {
                    result = enPathType.Directory;
                }
            }
            else {
                //  && FileExist(path)
                if (FileExist(path) || DirectoryExist(path)) {
                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(path.Path)) {
                        FileAttributes fa = File.GetAttributes(path.Path);

                        if ((fa & FileAttributes.Directory) == FileAttributes.Directory) {
                            result = enPathType.Directory;
                        }
                    }
                }
                else {
                    if (Dev2ActivityIOPathUtils.IsDirectory(path.Path)) {
                        result = enPathType.Directory;
                    }
                }
            }

            return result;
        }

        public string PathSeperator() {
            return "\\";
        }

        #region Private Methods

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        private string ExtractUserName(IActivityIOPath path) {
            string result = string.Empty;

            int idx = path.Username.IndexOf("\\");

            if (idx > 0) {
                result = path.Username.Substring((idx+1));
            }

            return result;
        }

        private string ExtractDomain(IActivityIOPath path) {
            string result = string.Empty;

            int idx = path.Username.IndexOf("\\");

            if (idx > 0) {
                result = path.Username.Substring(0, idx);
            }

            return result;
        }

        private bool FileExist(IActivityIOPath path) {
            bool result = false;

            result = File.Exists(path.Path);

            return result;
        }

        private bool DirectoryExist(IActivityIOPath dir) {
            bool result = Directory.Exists(dir.Path);
            return result;
        }

        private bool RequiresAuth(IActivityIOPath path) {

            bool result = false;

            if (path.Username != string.Empty) {
                result = true;
            }

            return result;
        }

        #endregion
    }
}
