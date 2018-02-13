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
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations;
using Microsoft.Win32.SafeHandles;
using Warewolf.Resource.Errors;
using Dev2.Data.PathOperations.Operations;

namespace Dev2.PathOperations
{

    /// <summary>
    /// Used for internal security reasons
    /// </summary>

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
        readonly LogonProvider _logOnprovider;

        public Dev2FileSystemProvider()
        {
            _logOnprovider = new LogonProvider();
        }

        public IActivityIOPath IOPath
        {
            get;
            set;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Stream Get(IActivityIOPath path, List<string> filesToCleanup)
            => new DoGetAction(path).ExecuteOperation();


        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public int Put(Stream src, IActivityIOPath dst, IDev2CRUDOperationTO args, string whereToPut, List<string> filesToCleanup)
            => new DoPutAction(src, dst, args, whereToPut).ExecuteOperation();



        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool Delete(IActivityIOPath src)
            => new DoDeleteOperation(src).ExecuteOperation();

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
                    var loginOk = _logOnprovider.DoLogon(username, domain, dst.Password, dst.Path, out SafeTokenHandle safeTokenHandle);
                    if (loginOk)
                    {
                        using (safeTokenHandle)
                        {

                            var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                result = PathIs(dst) == enPathType.Directory ? Directory.Exists(dst.Path) : File.Exists(dst.Path);
                                impersonatedUser.Undo();
                            }
                            newID.Dispose();
                        }
                    }
                    else
                    {
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
                var loginOk = _logOnprovider.DoLogon(username, domain, dst.Password, dst.Path, out SafeTokenHandle safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {

                        var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            Directory.CreateDirectory(dst.Path);
                            result = true;
                            impersonatedUser.Undo();
                        }
                        newID.Dispose();
                    }
                }
                else
                {
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
                var loginOk = _logOnprovider.DoLogon(username, domain, dst.Password, dst.Path, out SafeTokenHandle safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {

                        var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            if (DirectoryExist(dst))
                            {
                                Delete(dst);
                            }
                            Directory.CreateDirectory(dst.Path);
                            result = true;
                            impersonatedUser.Undo();
                        }
                    }
                }
                else
                {
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

        static bool FileExist(IActivityIOPath path) => File.Exists(path.Path);
        static bool DirectoryExist(IActivityIOPath dir) => Directory.Exists(dir.Path);
        static bool RequiresAuth(IActivityIOPath path) => path.Username != string.Empty;
        public IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src) => ListDirectoriesAccordingToType(src, ReadTypes.Folders);
        public IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src) => ListDirectoriesAccordingToType(src, ReadTypes.Files);

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src) => ListDirectoriesAccordingToType(src, ReadTypes.FilesAndFolders);

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
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    throw new Exception(string.Format(ErrorResource.DirectoryNotFound, src.Path));
                }
            }
            else
            {

                try
                {
                    var loginOk = _logOnprovider.DoLogon(username, domain, src.Password, src.Path, out SafeTokenHandle safeTokenHandle);

                    if (loginOk)
                    {
                        using (safeTokenHandle)
                        {

                            var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                try
                                {
                                    IEnumerable<string> dirs;

                                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(path))
                                    {
                                        dirs = GetDirectoriesForType(path, string.Empty, type);
                                    }
                                    else
                                    {
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
                                catch (Exception ex)
                                {
                                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                                    throw new Exception(string.Format(ErrorResource.DirectoryNotFound, src.Path));
                                }

                                impersonatedUser.Undo();
                                newID.Dispose();
                            }
                        }
                    }
                    else
                    {
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

        public void WriteDataToFile(IDev2PutRawOperationTO args, string path, IFile fileWrapper)
        {
            if (!RequiresAuth(IOPath))
            {
                DoWrite(args, path, fileWrapper);
            }
            else
            {
                var loginOk = _logOnprovider.DoLogon(username, domain, IOPath.Password, IOPath.Path, out SafeTokenHandle safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {

                        var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            DoWrite(args, path, fileWrapper);
                            impersonatedUser.Undo();
                            newID.Dispose();
                        }
                    }
                }
            }
        }

        void DoWrite(IDev2PutRawOperationTO args, string path, IFile fileWrapper)
        {
            try
            {
                if (IsBase64(args.FileContents))
                {
                    var data = GetBytesFromBase64String(args);
                    fileWrapper.WriteAllBytes(path, data);
                }
                else
                {
                    fileWrapper.WriteAllText(path, args.FileContents);
                }

            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, IOPath.Username));
            }
        }

        static byte[] GetBytesFromBase64String(IDev2PutRawOperationTO args) => Convert.FromBase64String(args.FileContents.Replace(@"Content-Type:BASE64", @""));
        static bool IsBase64(string fileContents) => fileContents.StartsWith(@"Content-Type:BASE64");
    }
}

