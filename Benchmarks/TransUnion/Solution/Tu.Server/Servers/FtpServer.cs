using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Principal;

namespace Tu.Servers
{
    public class FtpServer : IFtpServer
    {
        readonly Uri _serverUri;
        readonly ICredentials _credentials;

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="serverUri">The server URI - all operations are relative to this path.</param>
        /// <param name="credentials">The credentials; may be <code>null</code>.</param>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/16</date>
        /// <exception cref="System.ArgumentNullException">serverUri</exception>
        public FtpServer(string serverUri, ICredentials credentials = null)
        {
            if(string.IsNullOrWhiteSpace(serverUri))
            {
                throw new ArgumentNullException("serverUri");
            }
            _serverUri = new Uri(serverUri);
            _credentials = credentials;
        }

        #endregion

        #region Upload

        /// <summary>
        /// Uploads the given data to the file specified by the relative URI.
        /// </summary>
        /// <param name="relativeUri">The relative URI to the file on the FTP server.</param>
        /// <param name="data">The data to be uploaded.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/16</date>
        /// <exception cref="System.ArgumentNullException">
        /// relativeUri
        /// or
        /// data
        /// </exception>
        public bool Upload(string relativeUri, string data)
        {
            if(string.IsNullOrWhiteSpace(relativeUri))
            {
                throw new ArgumentNullException("relativeUri");
            }
            if(data == null)
            {
                throw new ArgumentNullException("data");
            }

            bool success;
            var request = CreateRequest(relativeUri, WebRequestMethods.Ftp.UploadFile);

            try
            {
                var requestStream = request.GetRequestStream();

                var streamWriter = new StreamWriter(requestStream);
                streamWriter.Write(data);
                streamWriter.Flush();
                streamWriter.Close();

                requestStream.Close();

                var response = (FtpWebResponse)request.GetResponse();
                success = response.StatusCode == FtpStatusCode.ClosingData;
                response.Close();
            }
            catch(Exception)
            {
                success = false;
            }

            return success;
        }

        #endregion

        #region Download

        /// <summary>
        /// Downloads the string contents represented by the given file on the server.
        /// </summary>
        /// <param name="relativeUri">The relative URI to the file on the FTP server.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/19</date>
        /// <exception cref="System.ArgumentNullException">relativeUri</exception>
        public string Download(string relativeUri)
        {
            if(string.IsNullOrWhiteSpace(relativeUri))
            {
                throw new ArgumentNullException("relativeUri");
            }

            string result = null;
            var request = CreateRequest(relativeUri, WebRequestMethods.Ftp.DownloadFile);

            var response = (FtpWebResponse)request.GetResponse();

            var responseStream = response.GetResponseStream();
            if(responseStream != null)
            {
                var reader = new StreamReader(responseStream);
                result = reader.ReadToEnd();
                responseStream.Close();
            }
            response.Close();

            return result;
        }

        #endregion

        #region Rename

        public bool Rename(string fromRelativeUri, string toRelativeUri)
        {
            if(string.IsNullOrWhiteSpace(fromRelativeUri))
            {
                throw new ArgumentNullException("fromRelativeUri");
            }
            if(string.IsNullOrWhiteSpace(toRelativeUri))
            {
                throw new ArgumentNullException("toRelativeUri");
            }

            var request = CreateRequest(fromRelativeUri, WebRequestMethods.Ftp.Rename);
            request.RenameTo = toRelativeUri;

            try
            {
                var response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Delete

        public bool Delete(string relativeUri)
        {
            if(string.IsNullOrWhiteSpace(relativeUri))
            {
                throw new ArgumentNullException("relativeUri");
            }

            var request = CreateRequest(relativeUri, WebRequestMethods.Ftp.DeleteFile);
            try
            {
                var response = (FtpWebResponse)request.GetResponse();

                response.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        #endregion

        #region CreateRequest

        FtpWebRequest CreateRequest(string relativeUri, string method)
        {
            var uri = new Uri(_serverUri, new Uri(relativeUri, UriKind.Relative));
            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = method;

            /* When in doubt, use these options */
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = true;

            if(_credentials != null)
            {
                request.Credentials = _credentials;
                request.ImpersonationLevel = TokenImpersonationLevel.Delegation;
                request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
            }
            else
            {
                request.ImpersonationLevel = TokenImpersonationLevel.Anonymous;
                request.AuthenticationLevel = AuthenticationLevel.None;
            }

            return request;
        }

        #endregion
    }
}
