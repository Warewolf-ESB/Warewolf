/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Microsoft.Win32.SafeHandles;
using Warewolf.Resource.Errors;



namespace Dev2.PathOperations
{
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle() => CloseHandle(handle);
    }
    
    [Serializable]
    public class Dev2FileSystemProvider : IActivityIOOperationsEndPoint
    {
        static readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 

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

            if (!RequiresAuth(path))
            {
                if (File.Exists(path.Path))
                {
                    result = new MemoryStream(File.ReadAllBytes(path.Path));
                }
                else
                {
                    var error = string.Format(ErrorResource.FileNotFound, path.Path);
                    throw new Exception(error);
                }
            }
            else
            {
                try
                {
                    // handle UNC path

                    var user = ExtractUserName(path);
                    var domain = ExtractDomain(path);
                    var loginOk = LogonUser(user, domain, path.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeTokenHandle);


                    if (loginOk)
                    {
                        using (safeTokenHandle)
                        {

                            var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
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
                        throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser,path.Username, path.Path));
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    throw new Exception(ex.Message, ex);
                }

            }

            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public int Put(Stream src, IActivityIOPath dst, IDev2CRUDOperationTO args, string whereToPut, List<string> filesToCleanup)
        {
            var result = -1;
            using (src)
            {
                if (!Path.IsPathRooted(dst.Path))
                {
                    //get just the directory path to put into
                    if (whereToPut != null)
                    {
                        //Make the destination directory equal to that directory
                        dst = ActivityIOFactory.CreatePathFromString(whereToPut + "\\" + dst.Path, dst.Username, dst.Password,dst.PrivateKeyFile);
                    }
                }
                if (args.Overwrite || !args.Overwrite && !FileExist(dst))
                {
                    _fileLock.EnterWriteLock();
                    try
                    {
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
                            var loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeTokenHandle);


                            if (loginOk)
                            {
                                using (safeTokenHandle)
                                {

                                    var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
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
                                throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, dst.Username, dst.Path));
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

                if (!RequiresAuth(src))
                {
                    // We need sense check the value passed in ;)
                    result = DeleteHelper.Delete(src.Path);
                }
                else
                {
                    // handle UNC path
                    var loginOk = LogonUser(ExtractUserName(src), ExtractDomain(src), src.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeTokenHandle);

                    if (loginOk)
                    {
                        using (safeTokenHandle)
                        {

                            var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
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
                        throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, src.Username, src.Path));
                    }
                }

            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Error getting file: "+src.Path,ex, GlobalConstants.WarewolfError);
                result = false;
            }
            return result;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src) => ListDirectoriesAccordingToType(src, ReadTypes.FilesAndFolders);

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool PathExist(IActivityIOPath dst)
        {
            bool result;

            if (!RequiresAuth(dst))
            {
                result = PathIs(dst) == enPathType.Directory ? Directory.Exists(dst.Path) : File.Exists(dst.Path);
            }
            else
            {

                try
                {
                    // handle UNC path
                    var loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeTokenHandle);


                    if (loginOk)
                    {
                        using (safeTokenHandle)
                        {

                            var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
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
                        throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, dst.Username, dst.Path));
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    throw;
                }
            }

            return result;
        }

        /*
         * Check for the existence of each directory?!
         */
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool CreateDirectory(IActivityIOPath dst, IDev2CRUDOperationTO args)
        {
            var result = false;

            if (args.Overwrite)
            {
                if (!RequiresAuth(dst))
                {
                    if (DirectoryExist(dst))
                    {
                        Delete(dst);
                    }
                    Directory.CreateDirectory(dst.Path);
                    result = true;
                }
                else
                {
                    result = CreateDirectoryWithAuthAndOverwrite(dst);
                }
            }
            else
            {
                if (!args.Overwrite && !DirectoryExist(dst))
                {
                    if (!RequiresAuth(dst))
                    {
                        Directory.CreateDirectory(dst.Path);
                        result = true;
                    }
                    else
                    {
                        result = CreateDirectoryWithAuthAndNoOverwrite(dst);
                    }
                }
            }

            return result;
        }

        bool CreateDirectoryWithAuthAndNoOverwrite(IActivityIOPath dst)
        {
            bool result;
            try
            {
                // handle UNC path
                var loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {

                        var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
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
                    throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, dst.Username, dst.Path));
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                throw;
            }

            return result;
        }

        bool CreateDirectoryWithAuthAndOverwrite(IActivityIOPath dst)
        {
            bool result;
            try
            {
                // handle UNC path
                var loginOk = LogonUser(ExtractUserName(dst), ExtractDomain(dst), dst.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {

                        var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            // Do the operation here

                            if (DirectoryExist(dst))
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
                    throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, dst.Username, dst.Path));
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                throw;
            }

            return result;
        }

        public bool RequiresLocalTmpStorage() => false;

        public bool HandlesType(enActivityIOPathType type) => type == enActivityIOPathType.FileSystem;

        public enPathType PathIs(IActivityIOPath path)
        {
            var result = enPathType.File;

            if (path.Path.StartsWith("\\\\"))
            {
                if (Dev2ActivityIOPathUtils.IsDirectory(path.Path))
                {
                    result = enPathType.Directory;
                }
            }
            else
            {
                //  && FileExist(path)
                if (FileExist(path) || DirectoryExist(path))
                {
                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(path.Path))
                    {
                        var fa = File.GetAttributes(path.Path);

                        if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            result = enPathType.Directory;
                        }
                    }
                }
                else
                {
                    if (Dev2ActivityIOPathUtils.IsDirectory(path.Path))
                    {
                        result = enPathType.Directory;
                    }
                }
            }

            return result;
        }

        public string PathSeperator() => "\\";

        /// <summary>
        /// Get folder listing for source
        /// </summary>
        /// <returns></returns>
        public IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src) => ListDirectoriesAccordingToType(src, ReadTypes.Folders);

        /// <summary>
        /// Get folder listing for source
        /// </summary>
        /// <returns></returns>
        public IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src) => ListDirectoriesAccordingToType(src, ReadTypes.Files);

        #region Private Methods

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        string ExtractUserName(IPathAuth path)
        {
            var result = string.Empty;

            var idx = path.Username.IndexOf("\\", StringComparison.Ordinal);

            if (idx > 0)
            {
                result = path.Username.Substring(idx + 1);
            }

            return result;
        }

        string ExtractDomain(IActivityIOPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            var result = string.Empty;

            var idx = path.Username.IndexOf("\\", StringComparison.Ordinal);

            if (idx > 0)
            {
                result = path.Username.Substring(0, idx);
            }

            return result;
        }

        bool FileExist(IActivityIOPath path)
        {
            var result = File.Exists(path.Path);

            return result;
        }

        bool DirectoryExist(IActivityIOPath dir)
        {
            var result = Directory.Exists(dir.Path);
            return result;
        }

        bool RequiresAuth(IActivityIOPath path)
        {
            var result = path.Username != string.Empty;

            return result;
        }

        IList<IActivityIOPath> ListDirectoriesAccordingToType(IActivityIOPath src, ReadTypes type)
        {
            IList<IActivityIOPath> result = new List<IActivityIOPath>();

            var path = src.Path;

            if (!path.EndsWith("\\") && PathIs(src) == enPathType.Directory)
            {
                path += "\\";
            }

            if (!RequiresAuth(src))
            {
                try
                {

                    IEnumerable<string> dirs;

                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(path))
                    {
                        if (Directory.Exists(path))
                        {
                            dirs = GetDirectoriesForType(path, string.Empty, type);
                        }
                        else
                        {
                            throw new Exception(string.Format(ErrorResource.DirectoryDoesNotExist, path));
                        }
                    }
                    else
                    {
                        // we have a wild-char path ;)
                        var baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(path);
                        var pattern = Dev2ActivityIOPathUtils.ExtractFileName(path);

                        dirs = GetDirectoriesForType(baseDir, pattern, type);
                    }

                    if (dirs != null)
                    {
                        foreach (string d in dirs)
                        {
                            result.Add(ActivityIOFactory.CreatePathFromString(d, src.Username, src.Password, true, src.PrivateKeyFile));
                        }
                    }
                }
                catch (Exception)
                {
                    throw new Exception(string.Format(ErrorResource.DirectoryNotFound, src.Path));
                }
            }
            else
            {

                try
                {
                    // handle UNC path
                    var loginOk = LogonUser(ExtractUserName(src), ExtractDomain(src), src.Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeTokenHandle);

                    if (loginOk)
                    {
                        using (safeTokenHandle)
                        {

                            var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                // Do the operation here

                                try
                                {

                                    IEnumerable<string> dirs;

                                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(path))
                                    {
                                        dirs = GetDirectoriesForType(path, string.Empty, type);
                                    }
                                    else
                                    {
                                        // we have a wild-char path ;)
                                        var baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(path);
                                        var pattern = Dev2ActivityIOPathUtils.ExtractFileName(path);

                                        dirs = GetDirectoriesForType(baseDir, pattern, type);
                                    }

                                    if (dirs != null)
                                    {
                                        foreach (string d in dirs)
                                        {
                                            result.Add(ActivityIOFactory.CreatePathFromString(d, src.Username, src.Password, src.PrivateKeyFile));
                                        }
                                    }

                                }
                                catch (Exception)
                                {
                                    throw new Exception(string.Format(ErrorResource.DirectoryNotFound, src.Path));
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
                        throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, src.Username, src.Path));
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    throw;
                }

            }

            return result;
        }

        static IEnumerable<string> GetDirectoriesForType(string path, string pattern, ReadTypes type)
        {
            if (type == ReadTypes.Files)
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    return Directory.EnumerateFiles(path);
                }
                return Directory.EnumerateFiles(path, pattern);
            }
            if (type == ReadTypes.Folders)
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    return Directory.EnumerateDirectories(path);
                }
                return Directory.EnumerateDirectories(path, pattern);
            }

            if (string.IsNullOrEmpty(pattern))
            {
                return Directory.EnumerateFileSystemEntries(path);
            }
            return Directory.EnumerateFileSystemEntries(path, pattern);
        }

        #endregion
    }
}
