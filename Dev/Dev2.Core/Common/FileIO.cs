using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Common;
using Microsoft.Win32.SafeHandles;
using Unlimited.Framework;

// ReSharper disable CheckNamespace
namespace Dev2
{
    // ReSharper restore CheckNamespace

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


    public class FileIO
    {
        IFrameworkFileIO _ioProvider;

        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 
        const int LOGON32_LOGON_INTERACTIVE = 2;


        #region Delete
        public string Delete(Uri path, string userName = "", string password = "")
        {
            dynamic returnData = new UnlimitedObject();
            CreateProvider();

            try
            {
                _ioProvider.Delete(path, userName, password);
                returnData.Result = "File Deleted";
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }

        public string Delete(object parameterData)
        {
            object[] parameters = parameterData as object[];
            try
            {
                if(parameterData != null)
                {
                    if(parameters != null && parameters.Length >= 1)
                    {
                        Uri destinationPath = new Uri(parameters[0].ToString());
                        string userName = parameters[1].ToString();
                        string password = parameters[2].ToString();

                        return Delete(destinationPath, userName, password);
                    }

                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                Debug.WriteLine(new UnlimitedObject(ex).XmlString);
            }

            return string.Format("<Error>{0}</Error>", "Input data was in incorrect format");
        }
        #endregion

        #region Move
        public string Move(object parameterData)
        {
            object[] parameters = parameterData as object[];
            try
            {
                if(parameterData != null)
                {
                    if(parameters != null && parameters.Length >= 2)
                    {
                        Uri sourcePath = new Uri(parameters[0].ToString());
                        Uri destinationPath = new Uri(parameters[1].ToString());
                        string userName = parameters[2].ToString();
                        string password = parameters[3].ToString();

                        return Move(sourcePath, destinationPath, userName, password);
                    }

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(new UnlimitedObject(ex).XmlString);
            }

            return string.Format("<Error>{0}</Error>", "Input data was in incorrect format");

        }


        public string Move(Uri sourcePath, Uri destinationPath, string userName = "", string password = "")
        {
            dynamic returnData = new UnlimitedObject();
            CreateProvider();

            try
            {
                _ioProvider.Move(sourcePath, destinationPath, false, userName, password);
                returnData.Result = "File Moved";
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }

        #endregion

        #region Copy
        public string Copy(object parameterData)
        {
            object[] parameters = parameterData as object[];
            try
            {
                if(parameterData != null)
                {
                    if(parameters != null && parameters.Length >= 2)
                    {
                        Uri sourcePath = new Uri(parameters[0].ToString());
                        Uri destinationPath = new Uri(parameters[1].ToString());
                        string userName = parameters[2].ToString();
                        string password = parameters[3].ToString();

                        return Copy(sourcePath, destinationPath, false, userName, password);
                    }

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(new UnlimitedObject(ex).XmlString);
            }

            return string.Format("<Error>{0}</Error>", "Input data was in incorrect format");

        }


        public string Copy(Uri sourcePath, Uri destinationPath, bool overWrite, string userName = "", string password = "")
        {
            dynamic returnData = new UnlimitedObject();
            CreateProvider();

            try
            {
                _ioProvider.Copy(sourcePath, destinationPath, false, userName, password);
                returnData.Result = "File Copied";
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }

        #endregion

        #region Put

        public string CreateFileFromBase64String(object parameterData)
        {
            object[] parameters = parameterData as object[];

            try
            {
                if(parameterData != null)
                {
                    if(parameters != null && parameters.Length >= 2)
                    {

                        string base64Data = parameters[0].ToString();
                        Uri destinationPath = new Uri(parameters[1].ToString());
                        string userName = parameters[2].ToString();
                        string password = parameters[3].ToString();

                        return CreateFileFromBase64String(base64Data, destinationPath, userName, password);
                    }

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(new UnlimitedObject(ex).XmlString);
            }

            return string.Format("<Error>{0}</Error>", "Input data was in incorrect format");
        }

        public string CreateFileFromBase64String(string base64FileData, Uri destinationPath, string userName, string password)
        {
            dynamic returnData = new UnlimitedObject();
            CreateProvider();
            try
            {
                byte[] fileData = Convert.FromBase64String(base64FileData);

                using(MemoryStream ms = new MemoryStream(fileData))
                {
                    _ioProvider.Put(ms, destinationPath, true, userName, password);
                }


                returnData.Result = "File Created";
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }
        #endregion

        #region Get
        public string GetFileInBase64String(object parameterData)
        {
            object[] parameters = parameterData as object[];
            dynamic data = new UnlimitedObject();

            string userName = string.Empty;
            string password = string.Empty;

            try
            {
                Uri fileUri = null;
                if(parameterData != null)
                {
                    if(parameters != null)
                    {
                        fileUri = new Uri(parameters[0].ToString());
                        userName = parameters[1].ToString();
                        password = parameters[2].ToString();
                    }
                }
                return GetFileInBase64String(fileUri, userName, password);

            }
            catch(Exception ex)
            {
                string errorXml = new UnlimitedObject(ex).XmlString;
                data.Error = errorXml;
                Debug.WriteLine(errorXml);
            }

            return data.XmlString;

        }

        public string GetFileInBase64String(Uri sourceFilePath, string userName, string password)
        {
            dynamic returnData = new UnlimitedObject();
            CreateProvider();
            try
            {
                var fileStream = _ioProvider.Get(sourceFilePath, userName, password);
                returnData.FileBase64 = fileStream.ToBase64String();
                returnData.Result = "File Retrieved";
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }

        public Stream Get(string filePath, string userName = "", string password = "")
        {
            var fileUri = new Uri(filePath);
            CreateProvider();
            return _ioProvider.Get(fileUri, userName, password);
        }
        #endregion

        #region List

        public string GetFilePathsFromDirectory(Uri directory, string userName, string password)
        {
            CreateProvider();
            dynamic returnData = new UnlimitedObject();

            try
            {
                var fileList = _ioProvider.List(directory, userName, password);


                fileList.ToList().ForEach(c =>
                {
                    dynamic fileUri = new UnlimitedObject("FileData");
                    fileUri.FilePath = c.ToString();
                    returnData.AddResponse(fileUri);
                });
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }

            return returnData.XmlString;
        }

        public string GetFilePathsFromDirectory(object directoryPath)
        {
            var directoryParam = directoryPath as object[];
            string directory = string.Empty;
            string userName = string.Empty;
            string password = string.Empty;



            dynamic returnData = new UnlimitedObject();

            try
            {
                if(directoryParam != null)
                {
                    directory = directoryParam[0].ToString();
                    userName = directory[1].ToString(CultureInfo.InvariantCulture);
                    password = directory[2].ToString(CultureInfo.InvariantCulture);
                }


                return GetFilePathsFromDirectory(new Uri(directory), userName, password);
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }
        #endregion

        #region Create Directory

        public string CreateDirectory(object createDirectoryData)
        {
            dynamic returnData = new UnlimitedObject();


            try
            {
                object[] parameters = createDirectoryData as object[];
                if(parameters != null)
                {
                    string directory = parameters[0].ToString();
                    string userName = parameters[1].ToString();
                    string password = parameters[2].ToString();
                    _ioProvider.CreateDirectory(new Uri(directory), userName, password);
                }
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }

            return returnData.XmlString;

        }

        public string CreateDirectory(string directoryPath, string userName, string password)
        {
            dynamic returnData = new UnlimitedObject();


            try
            {
                _ioProvider.CreateDirectory(new Uri(directoryPath), userName, password);
                returnData.Result = "Directory Created";
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).ToString();
            }

            return returnData.XmlString;
        }

        #endregion
        private void CreateProvider()
        {
            _ioProvider = new FileSystem();
        }

        #region Permissions

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);


        /// <summary>
        /// Extracts the name of the user.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string ExtractUserName(string path)
        {
            string result = string.Empty;

            int idx = path.IndexOf("\\", StringComparison.Ordinal);

            if(idx > 0)
            {
                result = path.Substring((idx + 1));
            }

            return result;
        }

        /// <summary>
        /// Extracts the domain.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string ExtractDomain(string path)
        {
            string result = string.Empty;

            int idx = path.IndexOf("\\", StringComparison.Ordinal);

            if(idx > 0)
            {
                result = path.Substring(0, idx);
            }

            return result;
        }

        /// <summary>
        /// Checks the permissions.
        /// </summary>
        /// <param name="userAndDomain">The user and domain.</param>
        /// <param name="pass">The pass.</param>
        /// <param name="path">The path.</param>
        /// <param name="rights">The rights.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Failed to authenticate with user [  + userAndDomain +  ] for resource [  + path +  ] </exception>
        public static bool CheckPermissions(string userAndDomain, string pass, string path, FileSystemRights rights)
        {
            bool result;

            // handle UNC path
            try
            {
                string user = ExtractUserName(userAndDomain);
                string domain = ExtractDomain(userAndDomain);
                SafeTokenHandle safeTokenHandle;
                bool loginOk = LogonUser(user, domain, pass, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                if(loginOk)
                {
                    using(safeTokenHandle)
                    {

                        WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            // Do the operation here
                            result = CheckPermissions(newID, path, rights);

                            impersonatedUser.Undo(); // remove impersonation now
                        }
                    }
                }
                else
                {
                    // login failed
                    throw new Exception("Failed to authenticate with user [ " + userAndDomain + " ] for resource [ " + path + " ] ");
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw;
            }
            return result;
        }


        /// <summary>
        /// Checks the permissions.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="path">The path.</param>
        /// <param name="expectedRights">The expected rights.</param>
        /// <returns></returns>
        public static bool CheckPermissions(WindowsIdentity user, string path, FileSystemRights expectedRights)
        {
            FileInfo fi = new FileInfo(path);
            DirectoryInfo di = new DirectoryInfo(path);
            AuthorizationRuleCollection acl;

            if(fi.Exists)
            {
                acl = fi.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            else if(di.Exists)
            {
                acl = di.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            else
            {
                return false;
            }

            // gets rules that concern the user and his groups
            IEnumerable<AuthorizationRule> userRules = from AuthorizationRule rule in acl
                                                       where user.Groups != null && (user.User != null && (user.User.Equals(rule.IdentityReference)
                                                                                                           || user.Groups.Contains(rule.IdentityReference)))
                                                       select rule;

            FileSystemRights denyRights = 0;
            FileSystemRights allowRights = 0;

            // iterates on rules to compute denyRights and allowRights
            foreach(FileSystemAccessRule rule in userRules)
            {
                if(rule.AccessControlType.Equals(AccessControlType.Deny))
                {
                    denyRights = denyRights | rule.FileSystemRights;
                }
                else if(rule.AccessControlType.Equals(AccessControlType.Allow))
                {
                    allowRights = allowRights | rule.FileSystemRights;
                }
            }

            allowRights = allowRights & ~denyRights;

            // are rights sufficient?
            return (allowRights & expectedRights) == expectedRights;
        }

        #endregion Permissions
    }
}
