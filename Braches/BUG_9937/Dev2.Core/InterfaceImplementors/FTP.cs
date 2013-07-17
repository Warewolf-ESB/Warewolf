using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2;
using System.IO;
using System.Net;
using Dev2.Common;

namespace Dev2 {
    public class FTP : IFrameworkFileIO {
        public void Delete(Uri path,string userName="", string password="") {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = path.Scheme == "ftps";
                request.Credentials = new NetworkCredential(userName, password);
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

        public Stream Get(Uri path, string userName = "", string password = "") {
            FtpWebRequest request = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            string dataString = string.Empty;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = path.Scheme == "ftps";
                request.Credentials = new NetworkCredential(userName, password);
                response = (FtpWebResponse)request.GetResponse();
                ftpStream = response.GetResponseStream();

                if (ftpStream.CanRead) {
                    dataString = ftpStream.ToBase64String();
                }
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
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

            if (!string.IsNullOrEmpty(dataString)) {
                return new MemoryStream(Convert.FromBase64String(dataString));
            }
            else {
                return null;
            }
        }

        public IList<Uri> List(Uri path,string userName="", string password="") {
            List<Uri> returnPaths = new List<Uri>();
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = path.Scheme == "ftps";
                request.Credentials = new NetworkCredential(userName, password);

                response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                
                while (!reader.EndOfStream) {
                    string localPath = path.LocalPath;
                    if (!path.LocalPath.EndsWith("/")) {
                        localPath += "/";
                    }
                    string uri = string.Format("{0}://{1}{2}{3}", path.Scheme, path.Host, localPath, reader.ReadLine());
                    returnPaths.Add(new Uri(uri));
                }
            }
            catch (WebException webEx) {
                FtpWebResponse webResponse = webEx.Response as FtpWebResponse;
                if (webResponse != null) {
                    if (webResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) {
                        throw new DirectoryNotFoundException(string.Format("Directory '{0}' was not found", path.OriginalString));
                    }
                }
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;
            }
            finally {
                if (response != null) {
                    response.Close();
                }
            }

            return returnPaths;
        }

        public void CreateDirectory(Uri path, string userName = "", string password = "") {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = path.Scheme == "ftps";
                request.Credentials = new NetworkCredential(userName, password);

                response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode != FtpStatusCode.PathnameCreated) {
                    throw new Exception("Directory was not created");
                }
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;
            }
            finally {
                if (response != null) {
                    response.Close();
                }
            }
        }

        public void Put(Stream data, Uri path, bool overwrite = false, string userName = "", string password = "") {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try {
                request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = path.Scheme == "ftps";
                request.Credentials = new NetworkCredential(userName, password);

                request.ContentLength = data.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data.ToByteArray(), 0, Convert.ToInt32(data.Length));
                requestStream.Close();

                response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode != FtpStatusCode.FileActionOK && response.StatusCode != FtpStatusCode.ClosingData) {
                    throw new Exception("File was not created");
                }
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;
            }
            finally {
                if (response != null) {
                    response.Close();
                }
            }
        }

        public void Copy(Uri sourcePath, Uri destinationPath, bool Overwrite, string userName = "", string password = "") {
            using (var fileStream = Get(sourcePath, userName, password)) {
                Put(fileStream, destinationPath, true, userName, password);
            }
        }

        public void Move(Uri sourcePath, Uri destinationPath, bool overWrite, string userName = "", string password = "") {
            Copy(sourcePath, destinationPath, overWrite, userName, password);

            Delete(sourcePath, userName, password);
        }
    }
}
