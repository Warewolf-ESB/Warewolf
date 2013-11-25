using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Dev2.Common;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide FTP and FTPS IO operations
    /// </summary>
    [Serializable]
    public class Dev2FTPProvider : IActivityIOOperationsEndPoint {

        // TODO : Implement as per Unlimited.Framework.Plugins.FileSystem in the Unlimited.Framework.Plugins project
        // Make sure to replace Uri with IActivity references

        public Dev2FTPProvider() { }
       
        public bool PathExist(IActivityIOPath dst)
        {
            var result = PathIs(dst) == enPathType.Directory ? IsDirectoryAlreadyPresent(dst) : IsFilePresent(dst);
            return result;
        }

        public Stream Get(IActivityIOPath path) {
            
            Stream result = null;
            try
            {
                if(IsStandardFTP(path))
                {
                    ReadFromFTP(path, ref result);
                }
                else
                {
                    ReadFromSFTP(path, ref result);
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw new Exception(ex.Message, ex);
            }
            return result;
        }

        static bool IsStandardFTP(IActivityIOPath path)
        {
            return path.PathType == enActivityIOPathType.FTP || path.PathType == enActivityIOPathType.FTPES || path.PathType == enActivityIOPathType.FTPS;
        }

        void ReadFromFTP(IActivityIOPath path, ref Stream result)
        {
            FtpWebRequest request;
            request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(path.Path));
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.KeepAlive = true;
            request.EnableSsl = EnableSSL(path);

            if(path.IsNotCertVerifiable)
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            }

            if(path.Username != string.Empty)
            {
                request.Credentials = new NetworkCredential(path.Username, path.Password);
            }

            var response = (FtpWebResponse)request.GetResponse();
            var ftpStream = response.GetResponseStream();

            if(ftpStream.CanRead)
            {
                byte[] data = ftpStream.ToByteArray();
                result = new MemoryStream(data);
                response.Close();
                ftpStream.Close();
            }
            else
            {
                response.Close();
                ftpStream.Close();
                throw new Exception("Fail");
            }
        }
        
        void ReadFromSFTP(IActivityIOPath path, ref Stream result)
        {
            var sftp = BuildSftpClient(path);
            var ftpPath = ExtractFileNameFromPath(path.Path);
            try
            {
                var tempFileName = BuildTempFileName();
                sftp.Get(ftpPath, tempFileName);
                result = new FileStream(tempFileName,FileMode.Open);
                sftp.Close();
            }
            catch(Exception)
            {
                sftp.Close();
                throw new Exception("Fail");
            }
        }

        static string BuildTempFileName()
        {
            var tempFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            return tempFileName;
        }

        Sftp BuildSftpClient(IActivityIOPath path)
        {
            var hostName = ExtractHostNameFromPath(path.Path);
            var sftp = new Sftp(hostName, path.Username, path.Password);
            
            try
            {
                sftp.Connect();
            }
            catch(Exception e)
            {
                if(path.Path.Contains("\\"))
                {
                    throw new Exception(string.Format("Bad format for SFTP. Path {0}. Please correct path.", path.Path));
                }
                throw new Exception(string.Format("Error connecting to SFTP location {0}.", path.Path));
            }
            return sftp;
        }

        string ExtractHostNameFromPath(string path)
        {
            Uri uriForSftp;
            if(Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uriForSftp))
            {
                try
                {
                    return uriForSftp.Host;
                }
                catch(Exception)
                {
                    throw new Exception("The path is in the incorrect format.");
                }
            }
            return "";
        }
        
        string ExtractFileNameFromPath(string path)
        {
            Uri uriForSftp;
            if(Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uriForSftp))
            {
                try
                {
                    return uriForSftp.AbsolutePath;
                }
                catch(Exception)
                {
                    throw new Exception("The path is in the incorrect format.");
                }
            }
            return "";
        }

        public int Put(Stream src, IActivityIOPath dst, Dev2CRUDOperationTO args, DirectoryInfo WhereToPut)
        {
            var result = -1;

            bool ok;

            if(args.Overwrite)
            {
                ok = true;
            }
            else
            {
                // try and fetch the file, if not found ok because we not in Overwrite mode
                try
                {
                    Get(dst).Close();
                    ok = false;
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    ok = true;
                }
            }

            if(ok)
            {
                try
                {
                    result = IsStandardFTP(dst) ? WriteToFTP(src, dst) : WriteToSFTP(src, dst);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    throw;
                }
            }
            return result;
        }

        int WriteToFTP(Stream src, IActivityIOPath dst)
        {
            FtpWebRequest request;
            request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(dst.Path));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.EnableSsl = EnableSSL(dst);

            if(dst.Username != string.Empty)
            {
                request.Credentials = new NetworkCredential(dst.Username, dst.Password);
            }

            if(dst.IsNotCertVerifiable)
            {
                ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
            }

            request.ContentLength = src.Length;
            using(Stream requestStream = request.GetRequestStream())
            {
                byte[] payload = src.ToByteArray();
                int writeLen = payload.Length;
                requestStream.Write(payload, 0, writeLen);
                requestStream.Close();
                requestStream.Dispose();
            }

            var result = (int)request.ContentLength;

            var response = (FtpWebResponse)request.GetResponse();
            if(response.StatusCode != FtpStatusCode.FileActionOK && response.StatusCode != FtpStatusCode.ClosingData)
            {
                throw new Exception("File was not created");
            }
            return result;
        }
        
        int WriteToSFTP(Stream src, IActivityIOPath dst)
        {
            var result = -1;
            if(dst != null)
            {
                var sftp = BuildSftpClient(dst);
                if(src != null)
                {
                    using(src)
                    {
                        byte[] payload = src.ToByteArray();
                        var tempFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
                        File.WriteAllBytes(tempFileName, payload);
                        try
                        {
                            var path = ExtractFileNameFromPath(dst.Path);
                            sftp.Put(tempFileName, path);
                            result = payload.Length;
                            sftp.Close();
                        }
                        catch(Exception exception)
                        {
                            sftp.Close();
                            throw new Exception("File was not created");
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Public entry point to this method
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public bool Delete(IActivityIOPath src) {
            bool result = false;

            try {
                // directory delete
                if (PathIs(src) == enPathType.Directory){
                    DeleteHandler(new List<string> { src.Path }, src.Username, src.Password, EnableSSL(src),src.IsNotCertVerifiable);
                }
                else {
                    DeleteOp(src); // file delete
                }

                result = true;
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;
            }

            return result;
        }


        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src)
        {
            return IsStandardFTP(src) ? ListDirectoryStandardFTP(src) : ListDirectorySFTP(src);
        }

        IList<IActivityIOPath> ListDirectoryStandardFTP(IActivityIOPath src)
        {
            List<IActivityIOPath> result = new List<IActivityIOPath>();
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(src.Path));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSSL(src);

                if(src.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(src.Username, src.Password);
                }

                if(src.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }

                response = (FtpWebResponse)request.GetResponse();

                using(Stream responseStream = response.GetResponseStream())
                {
                    using(StreamReader reader = new StreamReader(responseStream))
                    {
                        while(!reader.EndOfStream)
                        {
                            string uri = BuildValidPathForFTP(src, reader.ReadLine());
                            result.Add(ActivityIOFactory.CreatePathFromString(uri,src.Username,src.Password, true));
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    responseStream.Close();
                    responseStream.Dispose();
                }
            }
            catch(WebException webEx)
            {
                FtpWebResponse webResponse = webEx.Response as FtpWebResponse;
                if(webResponse != null)
                {
                    if(webResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        throw new DirectoryNotFoundException(string.Format("Directory '{0}' was not found", src.Path));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw;
            }
            finally
            {
                if(response != null)
                {
                    response.Close();
                }
            }

            return result;
        }

        static string BuildValidPathForFTP(IActivityIOPath src, string fileName)
        {
            if(src.Path.EndsWith("/"))
            {
                return string.Format("{0}{1}", src.Path, fileName);
            }
            return string.Format("{0}/{1}", src.Path, fileName);
        }

        IList<IActivityIOPath> ListDirectorySFTP(IActivityIOPath src)
        {
            List<IActivityIOPath> result = new List<IActivityIOPath>();
            var sftp = BuildSftpClient(src);
            try
            {
                var fromPath = ExtractFileNameFromPath(src.Path);
                var fileList = sftp.GetFileList(fromPath);
                foreach(string file in fileList)
                {
                    if(file != "..")
                    {
                        string uri = BuildValidPathForFTP(src, file);
                        result.Add(ActivityIOFactory.CreatePathFromString(uri,src.Username,src.Password));
                    }
                }

            }
            catch(SftpException webEx)
            {
                throw new DirectoryNotFoundException(string.Format("Directory '{0}' was not found", src.Path));
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw;
            }
            finally
            {
                sftp.Close();
            }

            return result;
        }

        public bool CreateDirectory(IActivityIOPath dst, Dev2CRUDOperationTO args)
        {
            bool result = false;

            bool ok = false;

            if(args.Overwrite)
            {
                // delete if it already present
                if(IsDirectoryAlreadyPresent(dst))
                {
                    Delete(dst);
                }
                ok = true;
            }
            else
            {
                // does not exist, ok to create
                ok = !(IsDirectoryAlreadyPresent(dst));
            }

            if(ok)
            {

                result = IsStandardFTP(dst) ? CreateDirectoryStandardFTP(dst) : CreateDirectorySFTP(dst);
            }
            return result;
        }

        bool CreateDirectoryStandardFTP(IActivityIOPath dst)
        {
            FtpWebRequest request;
            FtpWebResponse response = null;
            bool result;
            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(dst.Path));
                request.Method = WebRequestMethods.Ftp.MakeDirectory;

                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSSL(dst);

                if(dst.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(dst.Username, dst.Password);
                }

                if(dst.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }
                response = (FtpWebResponse)request.GetResponse();
                if(response.StatusCode == FtpStatusCode.PathnameCreated)
                {
                    result = true;
                }
                else
                {
                    throw new Exception("Fail");
                }
            }
            catch(Exception ex)
            {
                result = true;
                ServerLogger.LogError(ex);
            }
            finally
            {
                if(response != null)
                {
                    response.Close();
                }
            }
            return result;
        }
        
        bool CreateDirectorySFTP(IActivityIOPath dst)
        {
            var sftp = BuildSftpClient(dst);
            bool result;
            try
            {
                var fromPath = ExtractFileNameFromPath(dst.Path);
                sftp.Mkdir(fromPath);
                result = true;
            }
            catch(Exception ex)
            {
                result = false;
                ServerLogger.LogError(ex);
            }
            finally
            {
                sftp.Close();
            }
            return result;
        }

        public bool RequiresLocalTmpStorage() {
            return true;
        }

        public bool HandlesType(enActivityIOPathType type)
        {

            var result = type == enActivityIOPathType.FTPS || type == enActivityIOPathType.SFTP || type == enActivityIOPathType.FTP || type == enActivityIOPathType.FTPES;
            return result;
        }

        public enPathType PathIs(IActivityIOPath path)
        {
            enPathType result = enPathType.File;

            // WARN : here for now because FTP has no way of knowing of the user wants a directory or file?!?!
            if(Dev2ActivityIOPathUtils.IsDirectory(path.Path))
            {
                result = enPathType.Directory;
            }
            return result;
        }

        public string PathSeperator()
        {
            return "/";
        }

        /// <summary>
        /// Get folder listing for source
        /// </summary>
        /// <returns></returns>
        public IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src)
        {
            var dirs = new List<string>();
            try
            {
                var tmpDirData = ExtendedDirList(src.Path, src.Username, src.Password, EnableSSL(src),
                                                 src.IsNotCertVerifiable);
                dirs = ExtractDirectoryList(src.Path, tmpDirData);
            }
            catch (Exception ex)
            {
                ServerLogger.LogError(ex);
                throw new Exception(ex.Message, ex);
            }
            return dirs.Select(dir => BuildValidPathForFTP(src, dir)).Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password)).ToList();
        }

        /// <summary>
        /// Get folder listing for source
        /// </summary>
        /// <returns></returns>
        public IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src)
        {
            var dirs = new List<string>();
            try
            {
                var tmpDirData = ExtendedDirList(src.Path, src.Username, src.Password, EnableSSL(src),
                                                 src.IsNotCertVerifiable);
                 dirs = ExtractFileList(src.Path, tmpDirData);
            }
            catch (Exception ex)
            {
                ServerLogger.LogError(ex);
                throw  new Exception(ex.Message, ex);
            }
            return dirs.Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password)).ToList();
        }

        #region Private Methods

        public IActivityIOPath IOPath
        {
            get;
            set;
        }

        private string ConvertSSLToPlain(string path)
        {
            string result = path;

            result = result.Replace("FTPS:", "FTP:").Replace("ftps:", "ftp:");

            return result;
        }

        /// <summary>
        /// Recursive handler
        /// </summary>
        /// <param name="pathStack"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="ssl"></param>
        void DeleteHandler(IList<string> pathStack, string user, string pass, bool ssl, bool IsNotCertVerifiable)
        {
            if(pathStack.Count > 0)
            {
                string path = pathStack[0];
                pathStack.RemoveAt(0);

                bool addBack = true;

                IList<IActivityIOPath> allFiles = ListFilesInDirectory(ActivityIOFactory.CreatePathFromString(path, user, pass));
                IList<IActivityIOPath> allDirs = ListFoldersInDirectory(ActivityIOFactory.CreatePathFromString(path, user, pass));

                //string tmpDirData = ExtendedDirList(path, user, pass, ssl, IsNotCertVerifiable);

                //List<string> dirs = ExtractDirectoryList(path, tmpDirData);

                if(allDirs.Count == 0)
                {
                    // delete path ;)
                    foreach (var file in allFiles)
                    {
                        DeleteOp(file);
                    }
                    IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(path, user, pass);
                    DeleteOp(tmpPath);
                    addBack = false;
                }
                else
                {
                    // more dirs to process 
                    pathStack = pathStack.Union(allDirs.Select(ioPath => ioPath.Path)).ToList();
                }

                DeleteHandler(pathStack, user, pass, ssl, IsNotCertVerifiable);

                if(addBack)
                {
                    // remove the dir now all its sub-dirs are gone ;)
                    DeleteHandler(new List<string> { path }, user, pass, ssl, IsNotCertVerifiable);
                }
            }
        }

        /// <summary>
        /// Get the extended dir listing for internal use
        /// </summary>
        /// <param name="path"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="ssl"></param>
        /// <returns></returns>
        string ExtendedDirList(string path, string user, string pass, bool ssl, bool IsNotCertVerifiable)
        {
            if(path.Contains("sftp://"))
            {
                return ExtendedDirListSFTP(path, user, pass);
            }
            return ExtendedDirListStandardFTP(path, user, pass, ssl, IsNotCertVerifiable);
        }

        string ExtendedDirListStandardFTP(string path, string user, string pass, bool ssl, bool IsNotCertVerifiable)
        {
            FtpWebRequest req = null;
            FtpWebResponse resp = null;
            StringBuilder result = new StringBuilder();

            try
            {
                req = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(path));

                if(user != string.Empty)
                {
                    req.Credentials = new NetworkCredential(user, pass);
                }

                req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                req.KeepAlive = false;
                req.EnableSsl = ssl;

                if(IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }

                resp = (FtpWebResponse)req.GetResponse();

                using(Stream ios = resp.GetResponseStream())
                {
                    int bufLen = 2048; // read 2k at a time
                    byte[] data = new byte[bufLen];
                    int len = ios.Read(data, 0, bufLen);

                    while(len != 0)
                    {
                        result.Append(Encoding.UTF8.GetString(data));
                        len = ios.Read(data, 0, bufLen);
                    }

                    ios.Close();
                    ios.Dispose();
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw;
            }
            finally
            {
                if(resp != null)
                {
                    resp.Close();
                }
            }
            return result.ToString();
        }

        string ExtendedDirListSFTP(string path, string user, string pass)
        {
            var result = new StringBuilder();
            var pathFromString = ActivityIOFactory.CreatePathFromString(path, user, pass);
            var sftp = BuildSftpClient(pathFromString);
            try
            {
                var fromPath = ExtractFileNameFromPath(pathFromString.Path);
                var fileList = sftp.GetExtendedFileList(fromPath);
                sftp.Close();
                foreach(ChannelSftp.LsEntry filePath in fileList)
                {
                    string filename = filePath.getFilename();
                    if(filename.Contains(".."))
                    {
                        continue;
                    }
                    result.AppendLine(filePath.ToString());
                }
            }
            catch(Exception ex)
            {
                sftp.Close();
                ServerLogger.LogError(ex);
                throw new Exception(string.Format("Path not found {0}. Please ensure that it exists", path));
            }
            return result.ToString();
        }

        /// <summary>
        /// Extract dirs from dir list
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        private List<string> ExtractDirectoryList(string basePath, string payload)
        {
            List<string> result = new List<string>();

            var parts = GetParts(payload);

            if (parts.Length > 1)
            {
                foreach (string p in parts)
                {
                    int idx = p.LastIndexOf(" ");
                    if (idx > 0)
                    {
                        string part = p.Substring((idx + 1)).Trim();
                        if (IsDirectory(p))
                        {
                            // directory -- add it
                            if (!basePath.EndsWith("/"))
                            {
                                basePath += "/";
                            }
                            //result.Add(basePath + part);
                            result.Add(part);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Extract files from dir list
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        private List<string> ExtractFileList(string basePath, string payload)
            {
            List<string> result = new List<string>();

            var parts = GetParts(payload);

            if (parts.Length > 1)
            {
                foreach (string p in parts)
                {
                    int idx = p.LastIndexOf(" ");
                    if (idx > 0)
                    {
                        string part = p.Substring((idx + 1)).Trim();
                        if (!IsDirectory(p))
                        {
                            // directory -- add it
                            if (!basePath.EndsWith("/"))
                            {
                                basePath += "/";
                            }
                            result.Add(basePath + part);
                        }
                    }
                }
            }
            return result;
        }

        static bool IsDirectory(string part)
        {
            return part.ToLower().StartsWith("d") || part.ToLower().Contains("<dir>");
        }

        static string[] GetParts(string payload)
        {
            char token = '\n';

            string[] parts = payload.Split(token);

            if(parts.Length == 1)
            {
                token = '\r';
                parts = payload.Split(token);
            }
            return parts;
        }

        bool DeleteOp(IActivityIOPath src)
        {
            return IsStandardFTP(src) ? DeleteUsingStandardFTP(src) : DeleteUsingSFTP(src);
        }

        bool DeleteUsingStandardFTP(IActivityIOPath src)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            bool result = false;

            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(src.Path));

                if(PathIs(src) == enPathType.Directory)
                {
                    request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                }
                else
                {
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                }

                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSSL(src);

                if(src.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }

                if(src.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(src.Username, src.Password);
                }

                response = (FtpWebResponse)request.GetResponse();
                if(response.StatusCode == FtpStatusCode.FileActionOK)
                {
                    result = true;
                }
                else
                {
                    throw new Exception("Fail");
                }
            }
            catch (Exception exception)
            {
                result = false;
                throw new Exception(string.Format("Could not delete {0}. Please check the path exists.", src.Path));
            }
            finally
            {
                if(response != null)
                {
                    response.Close();
                }
            }
            return result;
        }

        bool DeleteUsingSFTP(IActivityIOPath src)
        {
            bool result;
            var sftp = BuildSftpClient(src);
            try
            {
                var fromPath = ExtractFileNameFromPath(src.Path);
                if(PathIs(src) == enPathType.Directory)
                {
                    sftp.DeleteDirectory(fromPath);
                }
                else
                {
                    sftp.DeleteFile(fromPath);
                }
                result = true;
            }
            catch(Exception exception)
            {
                result = false;
                throw new Exception(string.Format("Could not delete {0}. Please check the path exists.", src.Path));
            }
            finally
            {
               sftp.Close();
            }
            return result;
        }

        private bool EnableSSL(IActivityIOPath path)
        {
            var result = path.PathType == enActivityIOPathType.FTPS || path.PathType == enActivityIOPathType.FTPES;
            return result;
        }

        private bool IsFilePresent(IActivityIOPath path)
        {
            var isFilePresent = IsStandardFTP(path) ? IsFilePresentStandardFTP(path) : IsFilePresentSFTP(path);
            return isFilePresent;
        }

        bool IsFilePresentStandardFTP(IActivityIOPath path)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            bool isAlive = false;

            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(path.Path));
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSSL(path);

                if(path.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(path.Username, path.Password);
                }

                if(path.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }

                response = (FtpWebResponse)request.GetResponse();

                using(Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream);

                    if(reader.EndOfStream)
                    {
                        // just check for exception, slow I know, but not sure how else to tackle this                  
                    }
                    reader.Close();
                    reader.Dispose();
                }

                // exception will be thrown if not present
                isAlive = true;
            }
            catch(WebException wex)
            {
                ServerLogger.LogError(wex);
                isAlive = false;
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw;
            }
            finally
            {
                if(response != null)
                {
                    response.Close();
                }
            }

            return isAlive;
        }
        
        bool IsFilePresentSFTP(IActivityIOPath path)
        {
            var sftp = BuildSftpClient(path);
            bool isAlive = false;

            try
            {
                var fromPath = ExtractFileNameFromPath(path.Path);
                var tempFileName = BuildTempFileName();
                sftp.Get(fromPath, tempFileName);
                if(File.ReadAllBytes(tempFileName).Length != 0)
                {
                    isAlive = true;
                }
            }
            catch(SftpException ftpException)
            {
                ServerLogger.LogError(ftpException);
                isAlive = false;
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw;
            }
            finally
            {
                sftp.Close();
            }

            return isAlive;
        }

        private bool IsDirectoryAlreadyPresent(IActivityIOPath path)
        {
            var isAlive = IsStandardFTP(path) ? IsDirectoryAlreadyPresentStandardFTP(path) : IsDirectoryAlreadyPresentSFTP(path);

            return isAlive;
        }

        bool IsDirectoryAlreadyPresentStandardFTP(IActivityIOPath path)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            bool isAlive = false;

            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(path.Path));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSSL(path);

                if(path.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(path.Username, path.Password);
                }

                if(path.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }

                response = (FtpWebResponse)request.GetResponse();

                using(Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream);

                    if(reader.EndOfStream)
                    {
                        // just check for exception, slow I know, but not sure how else to tackle this                  
                    }
                    reader.Close();
                    reader.Dispose();
                }

                // exception will be thrown if not present
                isAlive = true;
            }
            catch(WebException wex)
            {
                ServerLogger.LogError(wex);
                isAlive = false;
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw;
            }
            finally
            {
                if(response != null)
                {
                    response.Close();
                }
            }
            return isAlive;
        }

        bool IsDirectoryAlreadyPresentSFTP(IActivityIOPath path)
        {
            var sftpClient = BuildSftpClient(path);
            var isAlive = true;
            try
            {
                var ftpPath = ExtractFileNameFromPath(path.Path);
                var arrayList = sftpClient.GetFileList(ftpPath);
                if(arrayList == null || arrayList.Count < 1)
                {
                    isAlive = false;    
                }
            }
            catch(Exception ex)
            {
                isAlive = false;
                ServerLogger.LogError(ex);
            }
            finally
            {
                sftpClient.Close();
            }
            return isAlive;
        }

        public bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        
        #endregion Private Methods
    }
}
