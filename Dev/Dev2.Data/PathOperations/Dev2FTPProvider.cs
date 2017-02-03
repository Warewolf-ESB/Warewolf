/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.PathOperations.Enums;
using Dev2.PathOperations;
using Renci.SshNet;
using Warewolf.Resource.Errors;
// ReSharper disable ThrowFromCatchWithNoInnerException
// ReSharper disable ReturnTypeCanBeEnumerable.Local
// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace Dev2.Data.PathOperations
{ 
    [Serializable]
    // ReSharper disable InconsistentNaming
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Dev2FTPProvider : IActivityIOOperationsEndPoint
    // ReSharper restore InconsistentNaming
    {
        const int SftpTimeoutSeconds = 10;


        public bool PathExist(IActivityIOPath dst)
        {
            var result = PathIs(dst) == enPathType.Directory ? IsDirectoryAlreadyPresent(dst) : IsFilePresent(dst);
            return result;
        }

        public Stream Get(IActivityIOPath path, List<string> filesToCleanup)
        {


            Stream result = null;
            try
            {
                if (IsStandardFtp(path))
                {

                    ReadFromFtp(path, ref result);
                }
                else
                {
                    Dev2Logger.Debug($"SFTP_GET:{path.Path}");
                    ReadFromSftp(path, ref result, filesToCleanup);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex);
                var message = $"{ex.Message} ,  [{path.Path}]";
                throw new Exception(message, ex);
            }
            return result;
        }

        static bool IsStandardFtp(IActivityIOPath path)
        {
            return path.PathType == enActivityIOPathType.FTP || path.PathType == enActivityIOPathType.FTPES || path.PathType == enActivityIOPathType.FTPS;
        }

        void ReadFromFtp(IActivityIOPath path, ref Stream result)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path.Path));
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.KeepAlive = true;
            request.EnableSsl = EnableSsl(path);

            if (path.IsNotCertVerifiable)
            {
                ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
            }

            if (path.Username != string.Empty)
            {
                request.Credentials = new NetworkCredential(path.Username, path.Password);
            }

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var ftpStream = response.GetResponseStream())
                {

                    if (ftpStream != null && ftpStream.CanRead)
                    {
                        byte[] data = ftpStream.ToByteArray();
                        result = new MemoryStream(data);
                    }
                    else
                    {
                        throw new Exception(@"Fail");
                    }
                }
            }
        }

        void ReadFromSftp(IActivityIOPath path, ref Stream result, List<string> filesToCleanup)
        {
            var sftp = BuildSftpClient(path);
            var ftpPath = ExtractFileNameFromPath(path.Path);
            try
            {
                var tempFileName = BuildTempFileName();
                filesToCleanup.Add(tempFileName);
                var data = sftp.ReadAllBytes(ftpPath);
                File.WriteAllBytes(tempFileName, data);
                result = new FileStream(tempFileName, FileMode.Open);
                sftp.Disconnect();
            }
            catch (Exception ex)
            {
                sftp.Disconnect();
                sftp.Dispose();
                throw new Exception(ex.Message, ex);
            }
        }

        static string BuildTempFileName()
        {
            var tempFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            return tempFileName;
        }
        string ExtractHostNameFromPath(string path)
        {
            Uri uriForSftp;
            if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uriForSftp))
            {
                try
                {
                    return uriForSftp.Host;
                }
                catch (Exception)
                {
                    throw new Exception(ErrorResource.PathIsInIncorrectFormat);
                }
            }
            return string.Empty;
        }

        SftpClient BuildSftpClient(IActivityIOPath path)
        {

            var hostName = ExtractHostNameFromPath(path.Path);
            if (hostName.ToLower().StartsWith(@"localhost"))
                hostName = hostName.Replace(@"localhost", @"127.0.0.1");
            var methods = new List<AuthenticationMethod> { new PasswordAuthenticationMethod(path.Username, path.Password) };

            if (!string.IsNullOrEmpty(path.PrivateKeyFile))
            {
                var keyFile = string.IsNullOrEmpty(path.Password) ? new PrivateKeyFile(path.PrivateKeyFile) : new PrivateKeyFile(path.PrivateKeyFile, path.Password);
                var keyFiles = new[] { keyFile };
                methods.Add(new PrivateKeyAuthenticationMethod(path.Username, keyFiles));
            }
            var con = new ConnectionInfo(hostName, 22, path.Username, methods.ToArray());
            var sftp = new SftpClient(con) { OperationTimeout = new TimeSpan(0, 0, 0, SftpTimeoutSeconds) };

            try
            {
                sftp.Connect();
            }
            catch (Exception e)
            {
                Dev2Logger.Debug(@"Exception Creating SFTP Client",e);
                
                if(e.Message.Contains(@"timeout"))
                {
                    throw new Exception(ErrorResource.ConnectionTimedOut);
                }
                if(e.Message.Contains(@"Auth failed"))
                {
                    throw new Exception(string.Format(ErrorResource.IncorrectUsernameAndPassword, path.Path));
                }
                if(path.Path.Contains(@"\\"))
                {
                    throw new Exception(string.Format(ErrorResource.BadFormatForSFTP, path.Path));
                }
                throw new Exception(string.Format(ErrorResource.ErrorConnectingToSFTP, path.Path));
            }
            return sftp;
        }



        string ExtractFileNameFromPath(string path)
        {
            Uri uriForSftp;
            if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uriForSftp))
            {
                try
                {
                    return uriForSftp.AbsolutePath;
                }
                catch (Exception)
                {
                    throw new Exception(ErrorResource.PathIsInIncorrectFormat);
                }
            }
            return string.Empty;
        }

        public int Put(Stream src, IActivityIOPath dst, Dev2CRUDOperationTO args, string whereToPut, List<string> filesToCleanup)
        {
            var result = -1;

            bool ok;

            if (args.Overwrite)
            {
                ok = true;
            }
            else
            {
                // try and fetch the file, if not found ok because we not in Overwrite mode
                try
                {
                    using (Get(dst, filesToCleanup))
                    {
                        ok = false;
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(this, ex);
                    ok = true;
                }
            }

            if (ok)
            {
                try
                {
                    result = IsStandardFtp(dst) ? WriteToFtp(src, dst) : WriteToSftp(src, dst);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(@"Exception in Put command",ex);
                    throw;
                }
            }
            return result;
        }

        int WriteToFtp(Stream src, IActivityIOPath dst)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(dst.Path));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.EnableSsl = EnableSsl(dst);

            if (dst.Username != string.Empty)
            {
                request.Credentials = new NetworkCredential(dst.Username, dst.Password);
            }

            if (dst.IsNotCertVerifiable)
            {
                ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
            }

            request.ContentLength = src.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                using (src)
                {
                    byte[] payload = src.ToByteArray();
                    int writeLen = payload.Length;
                    requestStream.Write(payload, 0, writeLen);
                }
            }

            var result = (int)request.ContentLength;

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != FtpStatusCode.FileActionOK && response.StatusCode != FtpStatusCode.ClosingData)
                {
                    throw new Exception(ErrorResource.FileNotCreated);
                }
            }
            return result;
        }

        int WriteToSftp(Stream src, IActivityIOPath dst)
        {
            var result = -1;
            if (dst != null)
            {
                var sftp = BuildSftpClient(dst);
                if (src != null)
                {
                    using (src)
                    {

                        try
                        {
                            var path = ExtractFileNameFromPath(dst.Path);
                            sftp.UploadFile(src, path);
                            result = (int)src.Length;
                            sftp.Disconnect();
                            sftp.Dispose();
                        }
                        catch (Exception e)
                        {

                            Dev2Logger.Debug(@"Exception WriteToSFTP",e);
                            sftp.Disconnect();
                            sftp.Dispose();
                            throw new Exception(ErrorResource.FileNotCreated);
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
                if (PathIs(src) == enPathType.Directory)
                {
                    DeleteHandler(new List<string> { src.Path }, src.Username, src.Password, src.PrivateKeyFile);
                }
                else
                {
                    if (!DeleteOp(new List<IActivityIOPath> { src }))
                    {
                        Dev2Logger.Error($"Error Deleting Path: {src.Path}");
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex);
                throw new Exception(ex.Message, ex);
            }

            return true;
        }


        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src)
        {
            return IsStandardFtp(src) ? ListDirectoryStandardFtp(src) : ListDirectorySftp(src);
        }

        IList<IActivityIOPath> ListDirectoryStandardFtp(IActivityIOPath src)
        {
            List<IActivityIOPath> result = new List<IActivityIOPath>();
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(src.Path));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSsl(src);

                if (src.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(src.Username, src.Password);
                }

                if (src.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }

                using (FtpWebResponse response = request.GetResponse() as FtpWebResponse)
                {
                    using (Stream responseStream = response?.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (StreamReader reader = new StreamReader(responseStream))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string uri = BuildValidPathForFtp(src, reader.ReadLine());
                                    result.Add(ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, true, src.PrivateKeyFile));
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                FtpWebResponse webResponse = webEx.Response as FtpWebResponse;
                {
                    if (webResponse?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        throw new DirectoryNotFoundException(string.Format(ErrorResource.DirectoryNotFound, src.Path));
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex);
                throw;
            }

            return result;
        }

        static string BuildValidPathForFtp(IActivityIOPath src, string fileName)
        {
            if (src.Path.EndsWith(@"/") || fileName.StartsWith(@"/"))
            {
                return $"{src.Path}{fileName}";
            }
            return $"{src.Path}/{fileName}";
        }

        IList<IActivityIOPath> ListDirectorySftp(IActivityIOPath src)
        {
            List<IActivityIOPath> result = new List<IActivityIOPath>();
            var sftp = BuildSftpClient(src);
            try
            {
                var fromPath = ExtractFileNameFromPath(src.Path);
                var fileList = sftp.ListDirectory(fromPath).Select(a => a.Name);
                result.AddRange(from string file in fileList
                                where !file.EndsWith(@"..") && !file.EndsWith(@".")
                                select BuildValidPathForFtp(src, file)
                                    into uri
                                select ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, src.PrivateKeyFile));
            }
            catch (Exception)
            {
                throw new DirectoryNotFoundException(string.Format(ErrorResource.DirectoryNotFound, src.Path));
            }
            finally
            {
                sftp.Dispose();
            }

            return result;
        }

        public bool CreateDirectory(IActivityIOPath dst, Dev2CRUDOperationTO args)
        {
            bool result = false;

            bool ok;

            if (args.Overwrite)
            {
                if (IsDirectoryAlreadyPresent(dst))
                {
                    Delete(dst);
                }
                ok = true;
            }
            else
            {
                ok = !IsDirectoryAlreadyPresent(dst);
            }

            if (ok)
            {

                result = IsStandardFtp(dst) ? CreateDirectoryStandardFtp(dst) : CreateDirectorySftp(dst);
            }
            return result;
        }

        bool CreateDirectoryStandardFtp(IActivityIOPath dst)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(dst.Path));
                request.Method = WebRequestMethods.Ftp.MakeDirectory;

                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = EnableSsl(dst);

                if (dst.Username != string.Empty)
                {
                    request.Credentials = new NetworkCredential(dst.Username, dst.Password);
                }

                if (dst.IsNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }
                using (response = (FtpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != FtpStatusCode.PathnameCreated)
                    {
                        throw new Exception(@"Fail");
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex);
            }
            finally
            {
                response?.Close();
            }
            return true;
        }

        bool CreateDirectorySftp(IActivityIOPath dst)
        {
            var sftp = BuildSftpClient(dst);
            bool result;
            try
            {
                var fromPath = ExtractFileNameFromPath(dst.Path);
                sftp.CreateDirectory(fromPath);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Dev2Logger.Error(this, ex);
            }
            finally
            {
                sftp.Disconnect();
                sftp.Dispose();
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
            if (Dev2ActivityIOPathUtils.IsDirectory(path.Path))
            {
                result = enPathType.Directory;
            }
            return result;
        }

        public string PathSeperator()
        {
            return @"/";
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
                                                 src.IsNotCertVerifiable, src.PrivateKeyFile);
                dirs = ExtractList(src.Path, tmpDirData, IsDirectory);

                dirs.Remove(@".");
                dirs.Remove(@"..");
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex);
                string message = $"{ex.Message} : [{src.Path}]";
                throw new Exception(message, ex);
            }
            return dirs.Select(dir => BuildValidPathForFtp(src, dir)).Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, src.PrivateKeyFile)).ToList();
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
                var tmpDirData = ExtendedDirList(src.Path, src.Username, src.Password, EnableSsl(src), src.IsNotCertVerifiable, src.PrivateKeyFile);
                dirs = ExtractList(src.Path, tmpDirData, IsFile);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex);
                string message = $"{ex.Message} : [{src.Path}]";
                throw new Exception(message, ex);
            }
            return dirs.Select(dir => BuildValidPathForFtp(src, dir)).Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, src.PrivateKeyFile)).ToList();
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

            result = result.Replace(@"FTPS:", @"FTP:").Replace(@"ftps:", @"ftp:");

            return result;
        }

        void DeleteHandler(IList<string> pathStack, string user, string pass, string privateKeyFile)
        {
            if (pathStack.Count > 0)
            {
                string path = pathStack[0];
                pathStack.RemoveAt(0);

                bool addBack = true;

                var pathFromString = ActivityIOFactory.CreatePathFromString(path, user, pass, privateKeyFile);
                IList<IActivityIOPath> allFiles = ListFilesInDirectory(pathFromString).GroupBy(a => a.Path).Select(g => g.First()).ToList();
                IList<IActivityIOPath> allDirs = ListFoldersInDirectory(pathFromString);

                if (allDirs.Count == 0)
                {
                    IActivityIOPath tmpPath = pathFromString;
                    allFiles.Insert(allFiles.Count, tmpPath);
                    DeleteOp(allFiles);
                    addBack = false;
                }
                else
                {
                    // more dirs to process 
                    pathStack = pathStack.Union(allDirs.Select(ioPath => ioPath.Path)).ToList();
                }

                DeleteHandler(pathStack, user, pass, privateKeyFile);

                if (addBack)
                {
                    // remove the dir now all its sub-dirs are gone ;)
                    DeleteHandler(new List<string> { path }, user, pass, privateKeyFile);
                }
            }
        }

        string ExtendedDirList(string path, string user, string pass, bool ssl, bool isNotCertVerifiable, string privateKeyFile)
        {
            if (path.Contains(@"sftp://"))
            {
                return ExtendedDirListSftp(path, user, pass, privateKeyFile);
            }
            return ExtendedDirListStandardFtp(path, user, pass, ssl, isNotCertVerifiable);
        }

        string ExtendedDirListStandardFtp(string path, string user, string pass, bool ssl, bool isNotCertVerifiable)
        {
            FtpWebResponse resp = null;
            string result = null;
            try
            {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path));

                if (user != string.Empty)
                {
                    req.Credentials = new NetworkCredential(user, pass);
                }

                req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                req.KeepAlive = false;
                req.EnableSsl = ssl;

                if (isNotCertVerifiable)
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                }

                using (resp = (FtpWebResponse)req.GetResponse())
                {

                    using (Stream stream = resp.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex);
                throw;
            }
            finally
            {
                resp?.Close();
            }
            return result;
        }

        string ExtendedDirListSftp(string path, string user, string pass, string privateKeyFile)
        {
            var result = new StringBuilder();
            var pathFromString = ActivityIOFactory.CreatePathFromString(path, user, pass, privateKeyFile);
            var sftp = BuildSftpClient(pathFromString);
            try
            {
                var fromPath = ExtractFileNameFromPath(pathFromString.Path);
                AddResults(sftp, fromPath, result);
            }
            catch (Exception ex)
            {
                sftp.Dispose();
                Dev2Logger.Error(this, ex);
                throw new Exception(string.Format(ErrorResource.DirectoryNotFound, path));
            }
            finally
            {
                sftp.Disconnect();
                sftp.Dispose();
            }
            return result.ToString();
        }

        private static void AddResults(SftpClient sftp, string fromPath, StringBuilder result)
        {
            var fileList = sftp.ListDirectory(fromPath);

            foreach (var filePath in fileList)
            {
                string filename = filePath.Name;
                if (filename == @".." || filename == @"." || filename.EndsWith(@"."))
                {
                    continue;
                }
                result.AppendLine(filename);
            }
        }

        /// <summary>
        /// Extract dirs from dir list
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="payload"></param>
        /// <param name="matchFunc"></param>
        /// <returns></returns>
        private List<string> ExtractList(string basePath, string payload, Func<string, bool> matchFunc)
        {
            List<string> result = new List<string>();

            var parts = GetParts(payload);
            foreach (string p in parts)
            {
                int idx = p.LastIndexOf(@" ", StringComparison.Ordinal);
                if (idx > 0)
                {
                    string part = p.Substring(idx + 1).Trim();
                    if (matchFunc(p))
                    {
                        // directory -- add it
                        if (!basePath.EndsWith(@"/"))
                        {
                            basePath += @"/";
                        }
                        result.Add(part);
                    }
                }
                else
                {
                    if (matchFunc(p))
                    {
                        result.Add(p);
                    }
                }
            }
            return result;
        }

        static bool IsDirectory(string part)
        {
            return Dev2ActivityIOPathUtils.IsDirectory(part) || part.ToLower().Contains(@"<dir>");
        }

        static bool IsFile(string part)
        {
            return !IsDirectory(part);
        }

        static string[] GetParts(string payload)
        {
            string[] parts = payload.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                parts = parts[0].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (parts.Length == 1)
            {
                parts = parts[0].Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return parts;
        }

        private bool DeleteOp(IList<IActivityIOPath> src)
        {
            return src.All(IsStandardFtp) ? DeleteUsingStandardFtp(src) : DeleteUsingSftp(src);
        }

        bool DeleteUsingStandardFtp(IList<IActivityIOPath> src)
        {
            FtpWebResponse response = null;


            foreach (var activityIOPath in src)
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(activityIOPath.Path));

                    request.Method = PathIs(activityIOPath) == enPathType.Directory ? WebRequestMethods.Ftp.RemoveDirectory : WebRequestMethods.Ftp.DeleteFile;

                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = EnableSsl(activityIOPath);

                    if (activityIOPath.IsNotCertVerifiable)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                    }

                    if (activityIOPath.Username != string.Empty)
                    {
                        request.Credentials = new NetworkCredential(activityIOPath.Username, activityIOPath.Password);
                    }

                    using (response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != FtpStatusCode.FileActionOK)
                        {
                            throw new Exception(@"Fail");
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception(string.Format(ErrorResource.CouldNotDelete, activityIOPath.Path), exception);
                }
                finally
                {
                    response?.Close();
                }

            }
            return true;
        }

        bool DeleteUsingSftp(IList<IActivityIOPath> src)
        {


            foreach (var activityIOPath in src)
            {
                try
                {
                    using (var sftp = BuildSftpClient(activityIOPath))
                    {
                        var fromPath = ExtractFileNameFromPath(activityIOPath.Path);
                        if (PathIs(activityIOPath) == enPathType.Directory)
                        {

                            sftp.DeleteDirectory(fromPath);
                        }

                        else
                        {

                            sftp.DeleteFile(fromPath);
                        }
                    }

                }
                catch (Exception e)
                {
                    var message = string.Format(ErrorResource.CouldNotDelete, activityIOPath.Path);
                    Dev2Logger.Error(message, e);
                    throw new Exception(message);
                }
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
            var isFilePresent = IsStandardFtp(path) ? IsFilePresentStandardFtp(path) : IsFilePresentSftp(path);
            return isFilePresent;
        }

        bool IsFilePresentStandardFtp(IActivityIOPath path)
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
                Dev2Logger.Error(this, wex);
                isAlive = false;
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(this, ex);
                throw;
            }
            finally
            {
                response?.Close();
            }

            return isAlive;
        }

        bool IsFilePresentSftp(IActivityIOPath path)
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
                Dev2Logger.Error(this, ex);
                isAlive = false;
            }
            return isAlive;
        }

        private bool IsDirectoryAlreadyPresent(IActivityIOPath path)
        {
            var isAlive = IsStandardFtp(path) ? IsDirectoryAlreadyPresentStandardFtp(path) : IsDirectoryAlreadyPresentSftp(path);

            return isAlive;
        }

        bool IsDirectoryAlreadyPresentStandardFtp(IActivityIOPath path)
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
                Dev2Logger.Error(this, wex);
                isAlive = false;
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(this, ex);
                throw;
            }
            finally
            {
                response?.Close();
            }
            return isAlive;
        }

        bool IsDirectoryAlreadyPresentSftp(IActivityIOPath path)
        {
            var sftpClient = BuildSftpClient(path);
            var isAlive = true;
            try
            {
                var ftpPath = ExtractFileNameFromPath(path.Path);
                var arrayList = sftpClient.ListDirectory(ftpPath);
                if(arrayList == null || !arrayList.Any())
                {
                    isAlive = false;
                }
            }
            catch(Exception ex)
            {
                isAlive = false;
                Dev2Logger.Error(this, ex);
            }
            finally
            {
                sftpClient.Dispose();
            }
            return isAlive;
        }

        private bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion Private Methods
    }
}
