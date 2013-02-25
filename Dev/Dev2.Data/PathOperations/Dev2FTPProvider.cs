using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

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
       
        public bool PathExist(IActivityIOPath dst) {
            bool result = false;

            if (PathIs(dst) == enPathType.Directory) {
                result = IsDirectoryAlreadyPresent(dst);
            }
            else {
                result = IsFilePresent(dst);
            }

            return result;
        }

        public Stream Get(IActivityIOPath path) {
            
            FtpWebRequest request = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            Stream result = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(path.Path));
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.KeepAlive = true;
                request.EnableSsl = EnableSSL(path);

                if (path.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }

                if (path.Username != string.Empty) {
                    request.Credentials = new NetworkCredential(path.Username, path.Password);
                }

                response = (FtpWebResponse)request.GetResponse();
                ftpStream = response.GetResponseStream();

                if (ftpStream.CanRead) {
                    byte[] data = ftpStream.ToByteArray();
                    result = new MemoryStream(data);
                }
                else {
                    throw new Exception("Fail");
                }
            }
            catch (Exception) {
                throw;
            }
            finally {
                if (ftpStream != null) {
                    ftpStream.Close();
                }
                if (response != null) {
                    response.Close();
                }
            }

            return result;
        }

        public int Put(Stream src, IActivityIOPath dst, Dev2CRUDOperationTO args) {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            int result = -1;

            bool ok = false;

            if (args.Overwrite) {
                ok = true;
            }
            else {
                // try and fetch the file, if not found ok because we not in Overwrite mode
                try {
                    Get(dst).Close();
                    ok = false;
                }
                catch (Exception) {
                    ok = true;
                }
            }

            if (ok) {
                try {
                    request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(dst.Path));
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = EnableSSL(dst);                    

                    if (dst.Username != string.Empty) {
                        request.Credentials = new NetworkCredential(dst.Username, dst.Password);
                    }

                    if (dst.IsNotCertVerifiable)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
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

                    result = (int)request.ContentLength;

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
            catch (Exception) {
                throw;
            }

            return result;
        }

        
        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src) {
            List<IActivityIOPath> result = new List<IActivityIOPath>();
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(src.Path));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSSL(src);

                if (src.Username != string.Empty) {
                    request.Credentials = new NetworkCredential(src.Username, src.Password);
                }

                if (src.IsNotCertVerifiable)
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
                            string uri = string.Format("{0}{1}", src.Path, reader.ReadLine());
                            result.Add(ActivityIOFactory.CreatePathFromString(uri));
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    responseStream.Close();
                    responseStream.Dispose();
                }
            }
            catch (WebException webEx) {
                FtpWebResponse webResponse = webEx.Response as FtpWebResponse;
                if (webResponse != null) {
                    if (webResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) {
                        throw new DirectoryNotFoundException(string.Format("Directory '{0}' was not found", src.Path));
                    }
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

            return result;

        }

        public bool CreateDirectory(IActivityIOPath dst, Dev2CRUDOperationTO args) {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            bool result = false;

            bool ok = false;

            if (args.Overwrite) {
                // delete if it already present
                if (IsDirectoryAlreadyPresent(dst)) {
                    Delete(dst);
                }
                ok = true;
            }
            else {
                // does not exist, ok to create
                ok = !(IsDirectoryAlreadyPresent(dst));
            }

            if (ok) {

                try {
                    request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(dst.Path));
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = EnableSSL(dst);

                    if (dst.Username != string.Empty) {
                        request.Credentials = new NetworkCredential(dst.Username, dst.Password);
                    }

                    if (dst.IsNotCertVerifiable)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                    }
                    
                    response = (FtpWebResponse)request.GetResponse();
                    if (response.StatusCode == FtpStatusCode.PathnameCreated) {
                        result = true;
                    }
                    else {
                        throw new Exception("Fail");
                    }
                }
                catch (Exception) {                    
                    throw;
                }
                finally {
                    if (response != null) {
                        response.Close();
                    }
                    else {
                        result = false;
                    }
                }
            }

            return result;
        }

        public bool RequiresLocalTmpStorage() {
            return true;
        }

        public bool HandlesType(enActivityIOPathType type) {
            bool result = false;

            if (type == enActivityIOPathType.FTPS || type == enActivityIOPathType.FTP || type == enActivityIOPathType.FTPES) {
                result = true;
            }

            return result;
        }

        public enPathType PathIs(IActivityIOPath path) {
            enPathType result = enPathType.File;

            // WARN : here for now because FTP has no way of knowing of the user wants a directory or file?!?!
            if(Dev2ActivityIOPathUtils.IsDirectory(path.Path)){
                result = enPathType.Directory;
            }

            return result;
        }

        public string PathSeperator() {
            return "/";
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
        private void DeleteHandler(IList<string> pathStack, string user, string pass, bool ssl, bool IsNotCertVerifiable)
        {

            if (pathStack.Count > 0)
            {
                string path = pathStack[0];
                pathStack.RemoveAt(0);

                bool addBack = true;

                string tmpDirData = ExtendedDirList(path, user, pass, ssl, IsNotCertVerifiable);

                List<string> dirs = ExtractDirectoryList(path, tmpDirData);
                if (dirs.Count == 0)
                {
                    // delete path ;)
                    IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(path, user, pass);
                    DeleteOp(tmpPath);
                    addBack = false;
                }
                else
                {
                    // more dirs to process 
                    pathStack = pathStack.Union(dirs).ToList();
                }

                DeleteHandler(pathStack, user, pass, ssl,IsNotCertVerifiable);

                if (addBack)
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
        private string ExtendedDirList(string path, string user, string pass, bool ssl, bool IsNotCertVerifiable)
        {
            FtpWebRequest req = null;
            FtpWebResponse resp = null;
            StringBuilder result = new StringBuilder();

            try
            {
                req = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(path));

                if (user != string.Empty)
                {
                    req.Credentials = new NetworkCredential(user, pass);
                }

                req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                req.KeepAlive = false;
                req.EnableSsl = ssl;

                if (IsNotCertVerifiable)
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
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (resp != null)
                {
                    resp.Close();
                }
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

            char token = '\n';

            string[] parts = payload.Split(token);

            if (parts.Length == 1)
            {
                token = '\r';
                parts = payload.Split(token);
            }

            if (parts.Length > 1)
            {
                foreach (string p in parts)
                {
                    int idx = p.LastIndexOf(" ");
                    if (idx > 0)
                    {
                        string part = p.Substring((idx + 1)).Trim();
                        if (p.ToLower().StartsWith("d"))
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


        private bool DeleteOp(IActivityIOPath src)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            bool result = false;

            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(ConvertSSLToPlain(src.Path));

                if (PathIs(src) == enPathType.Directory)
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

                if (src.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                }

                if (src.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(src.Username, src.Password);
                }

                response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode == FtpStatusCode.FileActionOK)
                {
                    result = true;
                }
                else
                {
                    throw new Exception("Fail");
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return result;
        }


        private bool EnableSSL(IActivityIOPath path)
        {
            bool result = false;

            if (path.PathType == enActivityIOPathType.FTPS || path.PathType == enActivityIOPathType.FTPES)
            {
                result = true;
            }

            return result;
        }

        private bool IsFilePresent(IActivityIOPath path)
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

                if (path.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(path.Username, path.Password);
                }

                if (path.IsNotCertVerifiable)
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
            catch (WebException)
            {
                isAlive = false;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return isAlive;
        }

        private bool IsDirectoryAlreadyPresent(IActivityIOPath path)
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

                if (path.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(path.Username, path.Password);
                }

                if (path.IsNotCertVerifiable)
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
            catch (WebException)
            {
                isAlive = false;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
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
