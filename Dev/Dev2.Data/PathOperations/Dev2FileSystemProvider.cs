
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.PathOperations.Interfaces;
using Microsoft.Win32.SafeHandles;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.PathOperations
{

    /// <summary>
    /// Used for internal security reasons
    /// </summary>

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide file system IO operations to the File activities
    /// </summary>
    [Serializable]
    public class Dev2FileSystemProvider : IActivityIOOperationsEndPoint
    {
        private static readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 
        // ReSharper disable once InconsistentNaming
        const int LOGON32_LOGON_INTERACTIVE = 2;

        public IActivityIOPath IOPath
        {
            get;
            set;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Stream Get(IActivityIOPath path, List<string> filesToCleanup)
        {
            Stream result;

            if(!RequiresAuth(path))
            {
                if(File.Exists(path.Path))
                {
                    result = new MemoryStream(File.ReadAllBytes(path.Path));
                }
                else
                {
                    throw new Exception("File not found [ " + path.Path + " ]");
                }
            }
            else
            {
                try
                {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;

                    string user = ExtractUserName(path);
                    string domain = ExtractDomain(path);
                    bool loginOk = LogonUser(user, domain, path.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                    if(loginOk)
                    {
                        using(safeTokenHandle)
                        {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                // Do the operation here

                                result = new MemoryStream(File.ReadAllBytes(path.Path));

                                impersonatedUser.Undo(); // remove impersonation now
                            }
                        }
                    }
                    else
                    {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + path.Username + " ] for resource [ " + path.Path + " ] ");
                    }
                }
                catch(Exception ex)
                {
                    Dev2Logger.Log.Error(ex);
                    throw new Exception(ex.Message, ex);
                }

            }

            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public int Put(Stream src, IActivityIOPath dst, Dev2CRUDOperationTO args, string whereToPut, List<string> filesToCleanup)
        {
            int result = -1;
            using (src)
            {
                //2013.05.29: Ashley Lewis for bug 9507 - default destination to source directory when destination is left blank or if it is not a rooted path
                if (!Path.IsPathRooted(dst.Path))
                {
                    //get just the directory path to put into
                    if (whereToPut != null)
                    {
                        //Make the destination directory equal to that directory
                        dst = ActivityIOFactory.CreatePathFromString(whereToPut + "\\" + dst.Path, dst.Username, dst.Password);
                    }
                }
                if ((args.Overwrite) || (!args.Overwrite && !FileExist(dst)))
                {
                    _fileLock.EnterWriteLock();
                    try{
                        if (!RequiresAuth(dst))
                        {
                            using (src)
                            {
                                File.WriteAllBytes(dst.Path, src.ToByteArray());
                                result = (int)src.Length;
                            }
                        }
                        else
                        {
                            // handle UNC path
                            SafeTokenHandle safeTokenHandle;
                            bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                            if (loginOk)
                            {
                                using (safeTokenHandle)
                                {

                                    WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                    using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                                    {
                                        // Do the operation here
                                        using (src)
                                        {
                                            File.WriteAllBytes(dst.Path, src.ToByteArray());
                                            result = (int)src.Length;
                                        }

                                        // remove impersonation now
                                        impersonatedUser.Undo();
                                    }
                                }
                            }
                            else
                            {
                                // login failed
                                throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                            }
                        }
                    }
                    finally
                    {
                        _fileLock.ExitWriteLock();
                    }
                }
            }
            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool Delete(IActivityIOPath src)
        {
            bool result;
            try
            {

                if(!RequiresAuth(src))
                {
                    // We need sense check the value passed in ;)
                    result = DeleteHelper.Delete(src.Path);
                }
                else
                {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;
                    bool loginOk = LogonUser(ExtractUserName(src), ExtractDomain(src), src.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                    if(loginOk)
                    {
                        using(safeTokenHandle)
                        {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                // Do the operation here

                                result = DeleteHelper.Delete(src.Path);


                                // remove impersonation now
                                impersonatedUser.Undo();
                            }
                        }
                    }
                    else
                    {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + src.Username + " ] for resource [ " + src.Path + " ] ");
                    }
                }

            }
            catch(Exception)
            {
                //File is not found problem during delete
                result = false;
            }
            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src)
        {
            return ListDirectoriesAccordingToType(src, ReadTypes.FilesAndFolders);
        }

        public string ExtendedDirList(string path, string user, string pass, bool ssl, bool isNotCertVerifiable)
        {
            return "";
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool PathExist(IActivityIOPath dst)
        {
            bool result;

            if(!RequiresAuth(dst))
            {
                result = PathIs(dst) == enPathType.Directory ? Directory.Exists(dst.Path) : File.Exists(dst.Path);
            }
            else
            {

                try
                {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;
                    bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                    if(loginOk)
                    {
                        using(safeTokenHandle)
                        {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                // Do the operation here

                                result = PathIs(dst) == enPathType.Directory ? Directory.Exists(dst.Path) : File.Exists(dst.Path);

                                // remove impersonation now
                                impersonatedUser.Undo();
                            }
                            newID.Dispose();
                        }
                    }
                    else
                    {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                    }
                }
                catch(Exception ex)
                {
                    Dev2Logger.Log.Error(ex);
                    throw;
                }
            }

            return result;
        }

        /*
         * Check for the existence of each directory?!
         */
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool CreateDirectory(IActivityIOPath dst, Dev2CRUDOperationTO args)
        {
            bool result = false;

            if(args.Overwrite)
            {
                if(!RequiresAuth(dst))
                {
                    if(DirectoryExist(dst))
                    {
                        Delete(dst);
                    }
                    Directory.CreateDirectory(dst.Path);
                    result = true;
                }
                else
                {
                    try
                    {
                        // handle UNC path
                        SafeTokenHandle safeTokenHandle;
                        bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                        if(loginOk)
                        {
                            using(safeTokenHandle)
                            {

                                WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                                {
                                    // Do the operation here

                                    if(DirectoryExist(dst))
                                    {
                                        Delete(dst);
                                    }
                                    Directory.CreateDirectory(dst.Path);
                                    result = true;

                                    // remove impersonation now
                                    impersonatedUser.Undo();
                                }
                            }
                        }
                        else
                        {
                            // login failed, oh no!
                            throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                        }
                    }
                    catch(Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                        throw;
                    }
                }
            }
            else if(!args.Overwrite && !DirectoryExist(dst))
            {
                if(!RequiresAuth(dst))
                {
                    Directory.CreateDirectory(dst.Path);
                    result = true;
                }
                else
                {

                    try
                    {
                        // handle UNC path
                        SafeTokenHandle safeTokenHandle;
                        bool loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                        if(loginOk)
                        {
                            using(safeTokenHandle)
                            {

                                WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                                {
                                    // Do the operation here

                                    Directory.CreateDirectory(dst.Path);
                                    result = true;

                                    // remove impersonation now
                                    impersonatedUser.Undo();
                                }
                                newID.Dispose();
                            }
                        }
                        else
                        {
                            // login failed
                            throw new Exception("Failed to authenticate with user [ " + dst.Username + " ] for resource [ " + dst.Path + " ] ");
                        }
                    }
                    catch(Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                        throw;
                    }
                }
            }

            return result;
        }

        public bool RequiresLocalTmpStorage()
        {
            return false;
        }


        public bool HandlesType(enActivityIOPathType type)
        {

            return (type == enActivityIOPathType.FileSystem);
        }

        public enPathType PathIs(IActivityIOPath path)
        {
            enPathType result = enPathType.File;

            if(path.Path.StartsWith("\\\\"))
            {
                if(Dev2ActivityIOPathUtils.IsDirectory(path.Path))
                {
                    result = enPathType.Directory;
                }
            }
            else
            {
                //  && FileExist(path)
                if(FileExist(path) || DirectoryExist(path))
                {
                    if(!Dev2ActivityIOPathUtils.IsStarWildCard(path.Path))
                    {
                        FileAttributes fa = File.GetAttributes(path.Path);

                        if((fa & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            result = enPathType.Directory;
                        }
                    }
                }
                else
                {
                    if(Dev2ActivityIOPathUtils.IsDirectory(path.Path))
                    {
                        result = enPathType.Directory;
                    }
                }
            }

            return result;
        }

        public string PathSeperator()
        {
            return "\\";
        }

        /// <summary>
        /// Get folder listing for source
        /// </summary>
        /// <returns></returns>
        public IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src)
        {
            return ListDirectoriesAccordingToType(src, ReadTypes.Folders);
        }

        /// <summary>
        /// Get folder listing for source
        /// </summary>
        /// <returns></returns>
        public IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src)
        {
            return ListDirectoriesAccordingToType(src, ReadTypes.Files);
        }

        #region Private Methods

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        private string ExtractUserName(IPathAuth path)
        {
            string result = string.Empty;

            int idx = path.Username.IndexOf("\\", StringComparison.Ordinal);

            if(idx > 0)
            {
                result = path.Username.Substring((idx + 1));
            }

            return result;
        }

        private string ExtractDomain(IActivityIOPath path)
        {
            if(path == null)
            {
                throw new ArgumentNullException("path");
            }
            string result = string.Empty;

            int idx = path.Username.IndexOf("\\", StringComparison.Ordinal);

            if(idx > 0)
            {
                result = path.Username.Substring(0, idx);
            }

            return result;
        }

        private bool FileExist(IActivityIOPath path)
        {
            bool result = File.Exists(path.Path);

            return result;
        }

        private bool DirectoryExist(IActivityIOPath dir)
        {
            bool result = Directory.Exists(dir.Path);
            return result;
        }

        private bool RequiresAuth(IActivityIOPath path)
        {
            bool result = path.Username != string.Empty;

            return result;
        }

        private IList<IActivityIOPath> ListDirectoriesAccordingToType(IActivityIOPath src, ReadTypes type)
        {
            IList<IActivityIOPath> result = new List<IActivityIOPath>();

            string path = src.Path;

            if(!path.EndsWith("\\") && PathIs(src) == enPathType.Directory)
            {
                path += "\\";
            }

            if(!RequiresAuth(src))
            {
                try
                {

                    IEnumerable<string> dirs;

                    if(!Dev2ActivityIOPathUtils.IsStarWildCard(path))
                    {
                        if(Directory.Exists(path))
                        {
                            dirs = GetDirectoriesForType(path, string.Empty, type);
                        }
                        else
                        {
                            throw new Exception("The Directory does not exist.");
                        }
                    }
                    else
                    {
                        // we have a wild-char path ;)
                        string baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(path);
                        string pattern = Dev2ActivityIOPathUtils.ExtractFileName(path);

                        dirs = GetDirectoriesForType(baseDir, pattern, type);
                    }

                    if(dirs != null)
                    {
                        foreach(string d in dirs)
                        {
                            result.Add(ActivityIOFactory.CreatePathFromString(d, src.Username, src.Password, true));
                        }
                    }
                }
                catch(Exception)
                {
                    throw new Exception("Directory not found [ " + src.Path + " ] ");
                }
            }
            else
            {

                try
                {
                    // handle UNC path
                    SafeTokenHandle safeTokenHandle;
                    bool loginOk = LogonUser(ExtractUserName(src), ExtractDomain(src), src.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                    if(loginOk)
                    {
                        using(safeTokenHandle)
                        {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                // Do the operation here

                                try
                                {

                                    IEnumerable<string> dirs;

                                    if(!Dev2ActivityIOPathUtils.IsStarWildCard(path))
                                    {
                                        dirs = GetDirectoriesForType(path, string.Empty, type);
                                    }
                                    else
                                    {
                                        // we have a wild-char path ;)
                                        string baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(path);
                                        string pattern = Dev2ActivityIOPathUtils.ExtractFileName(path);

                                        dirs = GetDirectoriesForType(baseDir, pattern, type);
                                    }

                                    if(dirs != null)
                                    {
                                        foreach(string d in dirs)
                                        {
                                            result.Add(ActivityIOFactory.CreatePathFromString(d, src.Username, src.Password));
                                        }
                                    }

                                }
                                catch(Exception)
                                {
                                    throw new Exception("Directory not found [ " + src.Path + " ] ");
                                }

                                // remove impersonation now
                                impersonatedUser.Undo();
                                newID.Dispose();
                            }
                        }
                    }
                    else
                    {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + src.Username + " ] for resource [ " + src.Path + " ] ");
                    }
                }
                catch(Exception ex)
                {
                    Dev2Logger.Log.Error(ex);
                    throw;
                }

            }

            return result;
        }

        private static IEnumerable<string> GetDirectoriesForType(string path, string pattern, ReadTypes type)
        {
            if(type == ReadTypes.Files)
            {
                if(string.IsNullOrEmpty(pattern))
                {
                    return Directory.EnumerateFiles(path);
                }
                return Directory.EnumerateFiles(path, pattern);
            }
            if(type == ReadTypes.Folders)
            {
                if(string.IsNullOrEmpty(pattern))
                {
                    return Directory.EnumerateDirectories(path);
                }
                return Directory.EnumerateDirectories(path, pattern);
            }

            if(string.IsNullOrEmpty(pattern))
            {
                return Directory.EnumerateFileSystemEntries(path);
            }
            return Directory.EnumerateFileSystemEntries(path, pattern);
        }

        #endregion
    }
}
