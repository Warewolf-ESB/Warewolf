using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Dev2.PathOperations;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Security.Principal;
using System.Net;
using Ionic.Zip;
using Dev2.Integration.Tests;

namespace Unlimited.UnitTest.Framework.PathOperationTests {

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
    
    public static class PathIOTestingUtils {

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);


        public static  void CreateAuthedUNCPath(string path, bool isDir = false) {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // handle UNC path
            SafeTokenHandle safeTokenHandle;

            byte[] data = new byte[3];

            data[0] = (byte)'a';
            data[1] = (byte)'b';
            data[2] = (byte)'c';

            CreateUNCFile(path, isDir, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, data);
        }

        public static void CreateAuthedUNCPath(string path, string dataString, bool isDir = false)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // handle UNC path
            SafeTokenHandle safeTokenHandle;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(dataString);
            CreateUNCFile(path, isDir, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, data);
        }

        private static void CreateUNCFile(string path, bool isDir, int LOGON32_LOGON_INTERACTIVE, int LOGON32_PROVIDER_DEFAULT,
                                          byte[] data)
        {
            SafeTokenHandle safeTokenHandle;
            try
            {
                bool loginOk = LogonUser(ParserStrings.PathOperations_Correct_Username, "DEV2",
                                         ParserStrings.PathOperations_Correct_Password, LOGON32_LOGON_INTERACTIVE,
                                         LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {
                        WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            if (!isDir)
                            {
                                // Do the operation here
                                File.WriteAllBytes(path, data);
                            }
                            else
                            {
                                Directory.CreateDirectory(path);
                            }

                            // remove impersonation now
                            impersonatedUser.Undo();
                        }
                    }
                }
                else
                {
                    // login failed
                    throw new Exception("Failed to authenticate for resource [ " + path + " ] ");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string ReadUNCFile(string path)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // handle UNC path
            SafeTokenHandle safeTokenHandle;

            string contents;

            try
            {
                bool loginOk = LogonUser(ParserStrings.PathOperations_Correct_Username, "DEV2",
                                         ParserStrings.PathOperations_Correct_Password, LOGON32_LOGON_INTERACTIVE,
                                         LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {
                        var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            contents = File.ReadAllText(path);
                            // remove impersonation now
                            impersonatedUser.Undo();
                        }
                    }
                }
                else
                {
                    // login failed
                    throw new Exception("Failed to authenticate for resource [ " + path + " ] ");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return contents;
        }

        public static string CreateTmpFile(string dir) {
            string fileName = Guid.NewGuid() + ".test";

            byte[] data = new byte[3];
            data[0] = (byte)'a';
            data[1] = (byte)'b';
            data[2] = (byte)'c';

            File.WriteAllBytes(dir + "\\"+ fileName, data);

            return (dir + "\\" + fileName);
        }
        
        public static void CreateTmpFile(string dir , string fileName)
        {
            byte[] data = new byte[3];
            data[0] = (byte)'d';
            data[1] = (byte)'e';
            data[2] = (byte)'f';

            File.WriteAllBytes(dir + "\\" + fileName, data);
        }

        public static void DeleteTmpDir(string path) {
            Directory.Delete(path, true);
        }

        public static string CreateTmpDirectory() {
            string dstDir = System.IO.Path.GetTempPath() + Guid.NewGuid() + "_dir";

            Directory.CreateDirectory(dstDir);

            return dstDir;
        }

        public static string CreateFileFTP(string basePath, string userName, string password, bool ftps) {
            string path = basePath + Guid.NewGuid() + ".test";

            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.KeepAlive = false;

                request.EnableSsl = ftps;

                if (userName != string.Empty) {
                    request.Credentials = new NetworkCredential(userName, password);
                }

                byte[] data = new byte[3];

                data[0] = (byte)'a';
                data[1] = (byte)'b';
                data[2] = (byte)'c';

                request.ContentLength = data.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, Convert.ToInt32(data.Length));
                requestStream.Close();

                response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode != FtpStatusCode.FileActionOK && response.StatusCode != FtpStatusCode.ClosingData) {
                    throw new Exception("File was not created");
                }
            }
            catch (Exception) {
                throw;
            }
            finally {
                if (response != null) {
                    response.Close();
                }
            }

            return path;
        }

        public static string CreateFileFTP(string basePath, string userName, string password, bool ftps, string fileName, string dataString, bool createDirectory)
        {

            FtpWebRequest request = null;
            FtpWebResponse response = null;
            string remoteDirectoy = basePath + Guid.NewGuid();
            string path = remoteDirectoy + "/" + fileName;

            try
            {
                if (createDirectory)
                {
                    request = (FtpWebRequest) FtpWebRequest.Create(remoteDirectoy);
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = ftps;
                    if (userName != string.Empty)
                    {
                        request.Credentials = new NetworkCredential(userName, password);
                    }
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    request.GetResponse();


                    request = (FtpWebRequest) FtpWebRequest.Create(path);
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = ftps;
                    if (userName != string.Empty)
                    {
                        request.Credentials = new NetworkCredential(userName, password);
                    }

                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    byte[] data = System.Text.Encoding.UTF8.GetBytes(dataString);
                    request.ContentLength = data.Length;

                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(data, 0, Convert.ToInt32(data.Length));
                    requestStream.Close();

                    response = (FtpWebResponse) request.GetResponse();
                    if (response.StatusCode != FtpStatusCode.FileActionOK &&
                        response.StatusCode != FtpStatusCode.ClosingData)
                    {
                        throw new Exception("File was not created");
                    }
                }
            }
            catch (Exception exception)
            {
                var ex = exception;
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return path;
        }

        public static string CreateFilesFTP(string basePath, string userName, string password, bool ftps, string fileName, string dataString, bool createDirectory)
        {
            string remoteDirectoy = basePath + Guid.NewGuid() + "/";
            string path = remoteDirectoy  + fileName;

            try
            {
                if (createDirectory)
                {
                    IActivityIOPath pathFromString = ActivityIOFactory.CreatePathFromString(remoteDirectoy, userName, password);
                    IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(pathFromString);

                    FTPPro.Put(null, pathFromString, new Dev2CRUDOperationTO(false), null);
                    
                    byte[] byteArray = Encoding.UTF8.GetBytes(dataString);

                    Stream dataStream = new MemoryStream(byteArray);
                    
                       pathFromString = ActivityIOFactory.CreatePathFromString(path, userName, password);
                       FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(pathFromString);
                       FTPPro.Put(dataStream, pathFromString, new Dev2CRUDOperationTO(true), null);
                    

                    //if (userName != string.Empty)
                    //{
                    //    request.Credentials = new NetworkCredential(userName, password);
                    //}
                  


                    //if (userName != string.Empty)
                    //{
                    //    request.Credentials = new NetworkCredential(userName, password);
                    //}



                    //byte[] data = System.Text.Encoding.UTF8.GetBytes(dataString);
                    

                    //Stream requestStream = request.GetRequestStream();
                    //requestStream.Write(data, 0, Convert.ToInt32(data.Length));
                    //requestStream.Close();

                    //response = (FtpWebResponse)request.GetResponse();
                    //if (response.StatusCode != FtpStatusCode.FileActionOK &&
                    //    response.StatusCode != FtpStatusCode.ClosingData)
                    //{
                    //    throw new Exception("File was not created");
                    //}
                }
            }
            catch (Exception exception)
            {
                var ex = exception;
                throw;
            }
            finally
            {
                //if (response != null)
                //{
                //    response.Close();
                //}
            }

            return path;
        }


        public static string ReadFileFTP(string path, string userName, string password)
        {
            string data = string.Empty;

            try
            {
                using (var request = new WebClient())
                {
                    if (userName != string.Empty)
                    {
                        request.Credentials = new NetworkCredential(userName, password);
                    }
                    data = System.Text.Encoding.Default.GetString(request.DownloadData(path));
                }
            }
            catch
            {

            }

            return data;
        }

        public static void DeleteFTP(string path, string userName, string password, bool ftps) {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(path));
                if (string.IsNullOrEmpty(Path.GetFileName(path))) {
                    request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                }
                else {
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                }

                if (ftps)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = ftps;
                if (userName != string.Empty) {
                    request.Credentials = new NetworkCredential(userName, password);
                }
                response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode != FtpStatusCode.FileActionOK) {
                    throw new Exception("File delete did not complete successfully");
                }
            }
            finally {
                if (response != null) {
                    response.Close();
                }
            }
        }

        public static Stream FileExistsFTP(string path, string userName, string password, bool isFTPS) {
            FtpWebRequest request = null;
            FtpWebResponse response = null;
            Stream ftpStream = Stream.Null;
            try {
                request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = isFTPS;
                if (userName != string.Empty) {
                    request.Credentials = new NetworkCredential(userName, password);
                }
                response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode == FtpStatusCode.DataAlreadyOpen) {
                    ftpStream = response.GetResponseStream();
                }
            }
            finally {
                if (response != null) {
                    ftpStream = response.GetResponseStream();
                    //response.Close();
                }
            }
            return ftpStream;
        }

        public static string ZipFile(string path, string[] files) {//, string userName, string password) {
            //SafeTokenHandle safeTokenHandel = CreateAuthenticationToken(userName, password);
            string zipFileName = string.Format("{0}{1}", path, "\\TestZipFile.zip");
            //if (safeTokenHandel != null) {
                //WindowsIdentity id = new WindowsIdentity(safeTokenHandel.DangerousGetHandle());
                using (ZipFile zipFile = new ZipFile(path)) {
                        zipFile.AddFiles(files, false, string.Empty); 
                    zipFile.Save(path + "\\TestZipFile.zip");
                    zipFile.Dispose();
                }
            //}
                return zipFileName;
        }

        public static string ZipFile(string path, string[] files, string password) {//, string userName, string password) {
            //SafeTokenHandle safeTokenHandel = CreateAuthenticationToken(userName, password);
            string zipFileName = string.Format("{0}{1}", path, "\\TestZipFile.zip");
            //if (safeTokenHandel != null) {
            //WindowsIdentity id = new WindowsIdentity(safeTokenHandel.DangerousGetHandle());
            using (ZipFile zipFile = new ZipFile(path)) {
                zipFile.Password = password;
                zipFile.AddFiles(files, false, string.Empty);
                zipFile.Save(path + "\\TestZipFile.zip");
                zipFile.Dispose();
            }
            //}
            return zipFileName;
        }

        public static void UnZipFile(string zipFileName, string userName, string password) {
            SafeTokenHandle safeTokenHandel = CreateAuthenticationToken(userName, password);
            if (safeTokenHandel != null) {
                WindowsIdentity id = new WindowsIdentity(safeTokenHandel.DangerousGetHandle());
                using (ZipFile zipFile = new ZipFile(zipFileName)) {
                    zipFile.ExtractAll(zipFileName + "\\TempZipDir\\", ExtractExistingFileAction.OverwriteSilently);
                    zipFile.Dispose();
                }
            }
        }

        private static SafeTokenHandle CreateAuthenticationToken(string username, string password) {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // handle UNC path
            SafeTokenHandle safeTokenHandle;
            bool loginOk = LogonUser(ParserStrings.PathOperations_Correct_Username, "DEV2", ParserStrings.PathOperations_Correct_Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);
            if (loginOk) {
                return safeTokenHandle;
            }
            else {
                return null;
            }
        }


        public static void DeleteAuthedUNCPath(string path) {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // handle UNC path
            SafeTokenHandle safeTokenHandle;

            try {
                bool loginOk = LogonUser(ParserStrings.PathOperations_Correct_Username, "DEV2", ParserStrings.PathOperations_Correct_Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                if (loginOk) {
                    using (safeTokenHandle) {

                        WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate()) {
                            // Do the operation here
                            if (Dev2ActivityIOPathUtils.IsDirectory(path)) {
                                Directory.Delete(path, true);
                            }
                            else {
                                File.Delete(path);
                            }

                            // remove impersonation now
                            impersonatedUser.Undo();
                        }
                    }
                }
                else {
                    // login failed
                    throw new Exception("Failed to authenticate for resource [ " + path + " ] ");
                }
            }
            catch (Exception e) {
                throw e;
            }
        }
        public static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static string ConvertSSLToPlain(string path)
        {
            string result = path;

            result = result.Replace("FTPS:", "FTP:").Replace("ftps:", "ftp:");

            return result;
        }

    }
}
