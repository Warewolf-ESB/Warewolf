using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dev2.Common.Common;
using Dev2.Data.PathOperations.Enums;
using Dev2.PathOperations;
using Dev2.Providers.Logs;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;

namespace Dev2.Data.PathOperations
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide FTP and FTPS IO operations
    /// </summary>
    [Serializable]
    public class Dev2FTPProvider : IActivityIOOperationsEndPoint
    {
        const int SFTPTimeoutMilliseconds = 3000;

        // TODO : Implement as per Unlimited.Framework.Plugins.FileSystem in the Unlimited.Framework.Plugins project
        // Make sure to replace Uri with IActivity references

        public bool PathExist(IActivityIOPath dst)
        {
            var result = PathIs(dst) == enPathType.Directory ? IsDirectoryAlreadyPresent(dst) : IsFilePresent(dst);
            return result;
        }

        public Stream Get(IActivityIOPath path)
        {

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
                this.LogError(ex);
                var message = string.Format("{0} ,  [{1}]", ex.Message, path.Path);
                throw new Exception(message, ex);
            }
            return result;
        }

        static bool IsStandardFTP(IActivityIOPath path)
        {
            return path.PathType == enActivityIOPathType.FTP || path.PathType == enActivityIOPathType.FTPES || path.PathType == enActivityIOPathType.FTPS;
        }

        void ReadFromFTP(IActivityIOPath path, ref Stream result)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path.Path));
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.KeepAlive = true;
            request.EnableSsl = EnableSsl(path);

            if(path.IsNotCertVerifiable)
            {
                ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
            }

            if(path.Username != string.Empty)
            {
                request.Credentials = new NetworkCredential(path.Username, path.Password);
            }

            using(var response = (FtpWebResponse)request.GetResponse())
            {
                using(var ftpStream = response.GetResponseStream())
                {

                    if(ftpStream != null && ftpStream.CanRead)
                    {
                        byte[] data = ftpStream.ToByteArray();
                        result = new MemoryStream(data);
                    }
                    else
                    {
                        throw new Exception("Fail");
                    }
                }
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
                result = new FileStream(tempFileName, FileMode.Open);
                sftp.Close();
            }
            catch(SftpException ex)
            {
                sftp.Close();
                throw new Exception(ex.message, ex);
            }
            catch(Exception ex)
            {
                sftp.Close();
                throw new Exception(ex.Message, ex);
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
            var sftp = new Sftp(hostName, path.Username, path.Password, SFTPTimeoutMilliseconds);

            try
            {
                sftp.Connect();
            }
            catch(Exception e)
            {
                if(e.Message.Contains("timeout"))
                {
                    throw new Exception("Connection timed out.");
                }
                if(e.Message.Contains("Auth failed"))
                {
                    throw new Exception(string.Format("Incorrect user name and password for {0}", path.Path));
                }
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

        public int Put(Stream src, IActivityIOPath dst, Dev2CRUDOperationTO args, DirectoryInfo whereToPut)
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
                    using(Get(dst))
                    {
                        ok = false;
                    }
                }
                catch(Exception ex)
                {
                    this.LogError(ex);
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
                    this.LogError(ex);
                    throw;
                }
            }
            return result;
        }

        int WriteToFTP(Stream src, IActivityIOPath dst)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(dst.Path));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.EnableSsl = EnableSsl(dst);

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
                using(src)
                {
                    byte[] payload = src.ToByteArray();
                    int writeLen = payload.Length;
                    requestStream.Write(payload, 0, writeLen);
                }
            }

            var result = (int)request.ContentLength;

            using(var response = (FtpWebResponse)request.GetResponse())
            {
                if(response.StatusCode != FtpStatusCode.FileActionOK && response.StatusCode != FtpStatusCode.ClosingData)
                {
                    throw new Exception("File was not created");
                }
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
                        catch(Exception ex)
                        {
                            Debug.Write(ex);
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
        public bool Delete(IActivityIOPath src)
        {
            try
            {
                // directory delete
                if(PathIs(src) == enPathType.Directory)
                {
                    DeleteHandler(new List<string> { src.Path }, src.Username, src.Password);
                }
                else
                {
                    DeleteOp(src); // file delete
                }
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                throw new Exception(ex.Message, ex);
            }

            return true;
        }


        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src)
        {
            return IsStandardFTP(src) ? ListDirectoryStandardFTP(src) : ListDirectorySFTP(src);
        }

        IList<IActivityIOPath> ListDirectoryStandardFTP(IActivityIOPath src)
        {
            List<IActivityIOPath> result = new List<IActivityIOPath>();
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(src.Path));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSsl(src);

                if(src.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(src.Username, src.Password);
                }

                if(src.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }

                using(response = (FtpWebResponse)request.GetResponse())
                {

                    using(Stream responseStream = response.GetResponseStream())
                    {
                        if(responseStream != null)
                        {
                            using(StreamReader reader = new StreamReader(responseStream))
                            {
                                while(!reader.EndOfStream)
                                {
                                    string uri = BuildValidPathForFTP(src, reader.ReadLine());
                                    result.Add(ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, true));
                                }
                            }
                        }
                    }
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
                this.LogError(ex);
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
                result.AddRange(from string file in fileList
                                where file != ".."
                                select BuildValidPathForFTP(src, file)
                                    into uri
                                    select ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password));
            }
            catch(SftpException)
            {
                throw new DirectoryNotFoundException(string.Format("Directory '{0}' was not found", src.Path));
            }
            catch(Exception ex)
            {
                this.LogError(ex);
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

            bool ok;

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
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(dst.Path));
                request.Method = WebRequestMethods.Ftp.MakeDirectory;

                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSsl(dst);

                if(dst.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(dst.Username, dst.Password);
                }

                if(dst.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }
                using(response = (FtpWebResponse)request.GetResponse())
                {
                    if(response.StatusCode != FtpStatusCode.PathnameCreated)
                    {
                        throw new Exception("Fail");
                    }
                }
            }
            catch(Exception ex)
            {
                this.LogError(ex);
            }
            finally
            {
                if(response != null)
                {
                    response.Close();
                }
            }
            return true;
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
                this.LogError(ex);
            }
            finally
            {
                sftp.Close();
            }
            return result;
        }

        public bool RequiresLocalTmpStorage()
        {
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
            List<string> dirs;
            try
            {
                var tmpDirData = ExtendedDirList(src.Path, src.Username, src.Password, EnableSsl(src),
                                                 src.IsNotCertVerifiable);
                dirs = ExtractDirectoryList(src.Path, tmpDirData);

                // remove the this directory ;)
                dirs.Remove(".");
                // remove th directory up too ;)
                dirs.Remove("..");
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                string message = string.Format("{0} : [{1}]", ex.Message, src.Path);
                throw new Exception(message, ex);
            }
            return dirs.Select(dir => BuildValidPathForFTP(src, dir)).Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password)).ToList();
        }

        /// <summary>
        /// Get folder listing for source
        /// </summary>
        /// <returns></returns>
        public IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src)
        {
            List<string> dirs;
            try
            {
                var tmpDirData = ExtendedDirList(src.Path, src.Username, src.Password, EnableSsl(src),
                                                 src.IsNotCertVerifiable);
                dirs = ExtractFileList(src.Path, tmpDirData);
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                string message = string.Format("{0} : [{1}]", ex.Message, src.Path);
                throw new Exception(message, ex);
            }
            return dirs.Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password)).ToList();
        }

        #region Private Methods

        public IActivityIOPath IOPath
        {
            get;
            set;
        }

        /// <summary>
        /// Converts the SSL automatic plain.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string ConvertSslToPlain(string path)
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
        void DeleteHandler(IList<string> pathStack, string user, string pass)
        {
            if(pathStack.Count > 0)
            {
                string path = pathStack[0];
                pathStack.RemoveAt(0);

                bool addBack = true;

                IList<IActivityIOPath> allFiles = ListFilesInDirectory(ActivityIOFactory.CreatePathFromString(path, user, pass));
                IList<IActivityIOPath> allDirs = ListFoldersInDirectory(ActivityIOFactory.CreatePathFromString(path, user, pass));

                if(allDirs.Count == 0)
                {
                    // delete path ;)
                    foreach(var file in allFiles)
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

                DeleteHandler(pathStack, user, pass);

                if(addBack)
                {
                    // remove the dir now all its sub-dirs are gone ;)
                    DeleteHandler(new List<string> { path }, user, pass);
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
        /// <param name="isNotCertVerifiable"></param>
        /// <returns></returns>
        string ExtendedDirList(string path, string user, string pass, bool ssl, bool isNotCertVerifiable)
        {
            if(path.Contains("sftp://"))
            {
                return ExtendedDirListSFTP(path, user, pass);
            }
            return ExtendedDirListStandardFTP(path, user, pass, ssl, isNotCertVerifiable);
        }

        string ExtendedDirListStandardFTP(string path, string user, string pass, bool ssl, bool isNotCertVerifiable)
        {
            FtpWebResponse resp = null;
            string result = null;
            try
            {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path));

                if(user != string.Empty)
                {
                    req.Credentials = new NetworkCredential(user, pass);
                }

                req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                req.KeepAlive = false;
                req.EnableSsl = ssl;

                if(isNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }

                using(resp = (FtpWebResponse)req.GetResponse())
                {

                    using(Stream stream = resp.GetResponseStream())
                    {
                        if(stream != null)
                        {
                            using(var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                throw;
            }
            finally
            {
                if(resp != null)
                {
                    resp.Close();
                }
            }
            return result;
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
                    if(filename.Contains("..") || filename.Contains("."))
                    {
                        continue;
                    }
                    result.AppendLine(filePath.ToString());
                }
            }
            catch(Exception ex)
            {
                sftp.Close();
                this.LogError(ex);
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

            if(parts.Length > 1)
            {
                foreach(string p in parts)
                {
                    int idx = p.LastIndexOf(" ", StringComparison.Ordinal);
                    if(idx > 0)
                    {
                        string part = p.Substring((idx + 1)).Trim();
                        if(IsDirectory(p))
                        {
                            // directory -- add it
                            if(!basePath.EndsWith("/"))
                            {
                                basePath += "/";
                            }
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

            if(parts.Length > 1)
            {
                foreach(string p in parts)
                {
                    int idx = p.LastIndexOf(" ", StringComparison.Ordinal);
                    if(idx > 0)
                    {
                        string part = p.Substring((idx + 1)).Trim();
                        if(!IsDirectory(p))
                        {
                            // directory -- add it
                            if(!basePath.EndsWith("/"))
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

        internal bool DeleteOp(IActivityIOPath src)
        {
            return IsStandardFTP(src) ? DeleteUsingStandardFTP(src) : DeleteUsingSFTP(src);
        }

        bool DeleteUsingStandardFTP(IActivityIOPath src)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(src.Path));

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
                request.EnableSsl = EnableSsl(src);

                if(src.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }

                if(src.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(src.Username, src.Password);
                }

                using(response = (FtpWebResponse)request.GetResponse())
                {
                    if(response.StatusCode != FtpStatusCode.FileActionOK)
                    {
                        throw new Exception("Fail");
                    }
                }
            }
            catch(Exception exception)
            {
                throw new Exception(string.Format("Could not delete {0}. Please check the path exists.", src.Path), exception);
            }
            finally
            {
                if(response != null)
                {
                    response.Close();
                }
            }
            return true;
        }

        bool DeleteUsingSFTP(IActivityIOPath src)
        {
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
            }
            catch(Exception)
            {
                throw new Exception(string.Format("Could not delete {0}. Please check the path exists.", src.Path));
            }
            finally
            {
                sftp.Close();
            }
            return true;
        }

        private static bool EnableSsl(IActivityIOPath path)
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
            FtpWebResponse response = null;

            bool isAlive;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path.Path));
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSsl(path);

                if(path.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(path.Username, path.Password);
                }

                if(path.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }

                using(response = (FtpWebResponse)request.GetResponse())
                {

                    using(Stream responseStream = response.GetResponseStream())
                    {
                        if(responseStream != null)
                        {
                            using(StreamReader reader = new StreamReader(responseStream))
                            {

                                if(reader.EndOfStream)
                                {
                                    // just check for exception, slow I know, but not sure how else to tackle this                  
                                }
                            }
                        }
                    }
                }

                // exception will be thrown if not present
                isAlive = true;
            }
            catch(WebException wex)
            {
                this.LogError(wex);
                isAlive = false;
            }
            catch(Exception ex)
            {
                this.LogError(ex);
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
            var isAlive = false;
            try
            {
                var listFilesInDirectory = ListFilesInDirectory(path);
                if(listFilesInDirectory.Count > 0)
                {
                    isAlive = true;
                }
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                isAlive = false;
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
            FtpWebResponse response = null;

            bool isAlive;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path.Path));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSsl(path);

                if(path.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(path.Username, path.Password);
                }

                if(path.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }

                using(response = (FtpWebResponse)request.GetResponse())
                {

                    using(Stream responseStream = response.GetResponseStream())
                    {
                        if(responseStream != null)
                        {
                            using(StreamReader reader = new StreamReader(responseStream))
                            {

                                if(reader.EndOfStream)
                                {
                                    // just check for exception, slow I know, but not sure how else to tackle this                  
                                }
                            }
                        }
                    }
                }

                // exception will be thrown if not present
                isAlive = true;
            }
            catch(WebException wex)
            {
                this.LogError(wex);
                isAlive = false;
            }
            catch(Exception ex)
            {
                this.LogError(ex);
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
                this.LogError(ex);
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
