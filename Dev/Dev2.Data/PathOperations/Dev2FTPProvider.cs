#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Renci.SshNet;
using Warewolf.Resource.Errors;
using System.Globalization;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Data.PathOperations
{
    [Serializable]
#pragma warning disable S101
    public class Dev2FTPProvider : IActivityIOOperationsEndPoint
#pragma warning restore S101
    {
        public interface IImplementation
        {
            string BuildValidPathForFtp(IActivityIOPath src, string fileName);
            List<string> ExtractList(string payload, Func<string, bool> matchFunc);
            bool EnableSsl(IActivityIOPath path);
            string ExtendedDirList(string path, string user, string pass, bool ssl, bool isNotCertVerifiable, string privateKeyFile);
            bool IsStandardFtp(IActivityIOPath path);
            bool IsDirectoryAlreadyPresent(IActivityIOPath path);
            bool CreateDirectorySftp(IActivityIOPath dst);
            bool CreateDirectoryStandardFtp(IActivityIOPath dst);
            IList<IActivityIOPath> ListDirectorySftp(IActivityIOPath src);
            IList<IActivityIOPath> ListDirectoryStandardFtp(IActivityIOPath src);
            bool DeleteOp(IList<IActivityIOPath> src);
            void DeleteHandler(IList<string> pathStack, string user, string pass, string privateKeyFile);
            int WriteToSftp(Stream src, IActivityIOPath dst);
            int WriteToFtp(Stream src, IActivityIOPath dst);
            void ReadFromFtp(IActivityIOPath path, ref Stream result);
            void ReadFromSftp(IActivityIOPath path, ref Stream result, List<string> filesToCleanup);
            bool IsFilePresent(IActivityIOPath path);
            enPathType PathIs(IActivityIOPath path);
            IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src);
        }
        readonly IImplementation _implementation;

        [ExcludeFromCodeCoverage]
        public Dev2FTPProvider()
            :this(new Implementation())
        {
        }

        public Dev2FTPProvider(IImplementation implementation)
        {
            _implementation = implementation;
        }

        const int SftpTimeoutSeconds = 10;
        public bool PathExist(IActivityIOPath dst)
        {
            var result = PathIs(dst) == enPathType.Directory ? _implementation.IsDirectoryAlreadyPresent(dst) : _implementation.IsFilePresent(dst);
            return result;
        }

        public Stream Get(IActivityIOPath path, List<string> filesToCleanup)
        {
            Stream result = null;
            try
            {
                if (_implementation.IsStandardFtp(path))
                {
                    _implementation.ReadFromFtp(path, ref result);
                }
                else
                {
                    Dev2Logger.Debug($"SFTP_GET:{path.Path}", GlobalConstants.WarewolfDebug);
                    _implementation.ReadFromSftp(path, ref result, filesToCleanup);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                var message = $"{ex.Message} ,  [{path.Path}]";
                throw new Exception(message, ex);
            }
            return result;
        }

        public int Put(Stream src, IActivityIOPath dst, IDev2CRUDOperationTO args, string whereToPut, List<string> filesToCleanup)
        {
            var result = -1;

            bool ok;

            if (args.Overwrite)
            {
                ok = true;
            }
            else
            {
                try
                {
                    using (Get(dst, filesToCleanup))
                    {
                        ok = false;
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    ok = true;
                }
            }

            if (ok)
            {
                using (src)
                {
                    try
                    {
                        result = _implementation.IsStandardFtp(dst) ? _implementation.WriteToFtp(src, dst) : _implementation.WriteToSftp(src, dst);
                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Error(@"Exception in Put command", ex, GlobalConstants.WarewolfError);
                        throw;
                    }
                }
            }
            return result;
        }

        public bool Delete(IActivityIOPath src)
        {
            try
            {
                if (PathIs(src) == enPathType.Directory)
                {
                    _implementation.DeleteHandler(new List<string> { src.Path }, src.Username, src.Password, src.PrivateKeyFile);
                }
                else
                {
                    if (!_implementation.DeleteOp(new List<IActivityIOPath> { src }))
                    {
                        Dev2Logger.Error($"Error Deleting Path: {src.Path}", GlobalConstants.WarewolfError);
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.Message, ex);
            }

            return true;
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOPath src)
        {
            return _implementation.IsStandardFtp(src) ? _implementation.ListDirectoryStandardFtp(src) : _implementation.ListDirectorySftp(src);
        }

        public bool CreateDirectory(IActivityIOPath dst, IDev2CRUDOperationTO args)
        {
            var result = false;
            bool okayToCreate;
            if (args.Overwrite)
            {
                if (_implementation.IsDirectoryAlreadyPresent(dst))
                {
                    Delete(dst);
                }
                okayToCreate = true;
            }
            else
            {
                okayToCreate = !_implementation.IsDirectoryAlreadyPresent(dst);
            }
            if (okayToCreate)
            {
                result = _implementation.IsStandardFtp(dst) ? _implementation.CreateDirectoryStandardFtp(dst) : _implementation.CreateDirectorySftp(dst);
            }
            return result;
        }

        public bool RequiresLocalTmpStorage() => true;

        public bool HandlesType(enActivityIOPathType type)
        {
            var result = type == enActivityIOPathType.FTPS || type == enActivityIOPathType.SFTP || type == enActivityIOPathType.FTP || type == enActivityIOPathType.FTPES;
            return result;
        }

        public enPathType PathIs(IActivityIOPath path) => _implementation.PathIs(path);

        public string PathSeperator() => @"/";

        static bool IsDirectory(string part) => Dev2ActivityIOPathUtils.IsDirectory(part) || part.ToLower(CultureInfo.InvariantCulture).Contains(@"<dir>");

        static bool IsFile(string part) => !IsDirectory(part);

        public IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src)
        {
            return _implementation.ListFoldersInDirectory(src);
        }

        public IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src)
        {
            List<string> dirs;
            try
            {
                var tmpDirData = _implementation.ExtendedDirList(src.Path, src.Username, src.Password, _implementation.EnableSsl(src), src.IsNotCertVerifiable, src.PrivateKeyFile);
                dirs = _implementation.ExtractList(tmpDirData, IsFile);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                var message = $"{ex.Message} : [{src.Path}]";
                throw new Exception(message, ex);
            }
            return dirs.Select(dir => _implementation.BuildValidPathForFtp(src, dir)).Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, src.PrivateKeyFile)).ToList();
        }

        public IActivityIOPath IOPath { get; set; }

        // Exclude from coverage due to this class not forming any part of the public interface, inputs, or outputs of Dev2FTPProvider
        [ExcludeFromCodeCoverage]
        class Implementation : IImplementation
        {
            readonly IFile _file;
            public Implementation()
                : this(new FileWrapper())
            {
            }
            public Implementation(IFile file)
            {
                _file = file;
            }

            public bool IsStandardFtp(IActivityIOPath path) => path.PathType == enActivityIOPathType.FTP || path.PathType == enActivityIOPathType.FTPES || path.PathType == enActivityIOPathType.FTPS;

            public void ReadFromFtp(IActivityIOPath path, ref Stream result)
            {
                var request = CreateFtpWebRequest(path);
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
                            var data = ftpStream.ToByteArray();
                            result = new MemoryStream(data);
                        }
                        else
                        {
                            throw new Exception(@"Fail");
                        }
                    }
                }
            }

            private static FtpWebRequest CreateFtpWebRequest(IActivityIOPath path) => (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path.Path));

            public void ReadFromSftp(IActivityIOPath path, ref Stream result, List<string> filesToCleanup)
            {
                var sftp = BuildSftpClient(path);
                var ftpPath = ExtractFileNameFromPath(path.Path);
                try
                {
                    var tempFileName = _file.GetTempFileName();
                    filesToCleanup.Add(tempFileName);
                    var data = sftp.ReadAllBytes(ftpPath);
                    _file.WriteAllBytes(tempFileName, data);
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

            string ExtractHostNameFromPath(string path)
            {
                if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri uriForSftp))
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
                if (hostName.ToLower(CultureInfo.InvariantCulture).StartsWith(@"localhost"))
                {
                    var ipAddress = new StringBuilder();
                    ipAddress.Append("127");
                    ipAddress.Append(".0");
                    ipAddress.Append(".0");
                    ipAddress.Append(".1");
                    hostName = hostName.Replace(@"localhost", ipAddress.ToString());
                }

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
                    Dev2Logger.Debug(@"Exception Creating SFTP Client", e, GlobalConstants.WarewolfError);

                    if (e.Message.Contains(@"timeout"))
                    {
                        throw new Exception(ErrorResource.ConnectionTimedOut);
                    }
                    if (e.Message.Contains(@"Auth failed"))
                    {
                        throw new Exception(string.Format(ErrorResource.IncorrectUsernameAndPassword, path.Path));
                    }
                    if (path.Path.Contains(@"\\"))
                    {
                        throw new Exception(string.Format(ErrorResource.BadFormatForSFTP, path.Path));
                    }
                    throw new Exception(string.Format(ErrorResource.ErrorConnectingToSFTP, path.Path));
                }
                return sftp;
            }

            string ExtractFileNameFromPath(string path)
            {
                if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri uriForSftp))
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

            public int WriteToFtp(Stream src, IActivityIOPath dst)
            {
                var request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(dst.Path));
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
                        var payload = src.ToByteArray();
                        var writeLen = payload.Length;
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

            public int WriteToSftp(Stream src, IActivityIOPath dst)
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

                                Dev2Logger.Debug(@"Exception WriteToSFTP", e, GlobalConstants.WarewolfDebug);
                                sftp.Disconnect();
                                sftp.Dispose();
                                throw new Exception(ErrorResource.FileNotCreated);
                            }
                        }
                    }
                }
                return result;
            }

            public IList<IActivityIOPath> ListDirectoryStandardFtp(IActivityIOPath src)
            {
                var result = new List<IActivityIOPath>();
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
                                        var uri = BuildValidPathForFtp(src, reader.ReadLine());
                                        result.Add(ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, true, src.PrivateKeyFile));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (WebException webEx)
                {
                    var webResponse = webEx.Response as FtpWebResponse;
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
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    throw;
                }

                return result;
            }

            public string BuildValidPathForFtp(IActivityIOPath src, string fileName)
            {
                if (src.Path.EndsWith(@"/") || fileName.StartsWith(@"/"))
                {
                    return $"{src.Path}{fileName}";
                }
                return $"{src.Path}/{fileName}";
            }

            public IList<IActivityIOPath> ListDirectorySftp(IActivityIOPath src)
            {
                var result = new List<IActivityIOPath>();
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

            public bool CreateDirectoryStandardFtp(IActivityIOPath dst)
            {
                FtpWebResponse response = null;
                try
                {
                    var request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(dst.Path));
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
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    // throw
                    return false;
                }
                finally
                {
                    response?.Close();
                }
                return true;
            }

            public bool CreateDirectorySftp(IActivityIOPath dst)
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
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                }
                finally
                {
                    sftp.Disconnect();
                    sftp.Dispose();
                }
                return result;
            }

            static string ConvertSslToPlain(string path)
            {
                var result = path;
                result = result.Replace(@"FTPS:", @"FTP:").Replace(@"ftps:", @"ftp:");
                return result;
            }

            public void DeleteHandler(IList<string> pathStack, string user, string pass, string privateKeyFile)
            {
                if (pathStack.Count > 0)
                {
                    var path = pathStack[0];
                    pathStack.RemoveAt(0);
                    var addBack = true;
                    var pathFromString = ActivityIOFactory.CreatePathFromString(path, user, pass, privateKeyFile);
                    IList<IActivityIOPath> allFiles = ListFilesInDirectory(pathFromString).GroupBy(a => a.Path).Select(g => g.First()).ToList();
                    var allDirs = ListFoldersInDirectory(pathFromString);
                    if (allDirs.Count == 0)
                    {
                        var tmpPath = pathFromString;
                        allFiles.Insert(allFiles.Count, tmpPath);
                        DeleteOp(allFiles);
                        addBack = false;
                    }
                    else
                    {
                        pathStack = pathStack.Union(allDirs.Select(ioPath => ioPath.Path)).ToList();
                    }
                    DeleteHandler(pathStack, user, pass, privateKeyFile);
                    if (addBack)
                    {
                        DeleteHandler(new List<string> { path }, user, pass, privateKeyFile);
                    }
                }
            }

            public string ExtendedDirList(string path, string user, string pass, bool ssl, bool isNotCertVerifiable, string privateKeyFile)
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
                    var req = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path));
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
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
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
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    throw new Exception(string.Format(ErrorResource.DirectoryNotFound, path));
                }
                finally
                {
                    sftp.Disconnect();
                    sftp.Dispose();
                }
                return result.ToString();
            }

            static void AddResults(SftpClient sftp, string fromPath, StringBuilder result)
            {
                var fileList = sftp.ListDirectory(fromPath);

                foreach (var filePath in fileList)
                {
                    var filename = filePath.Name;
                    if (filename == @".." || filename == @"." || filename.EndsWith(@"."))
                    {
                        continue;
                    }
                    result.AppendLine(filename);
                }
            }

            public List<string> ExtractList(string payload, Func<string, bool> matchFunc)
            {
                var result = new List<string>();

                var parts = GetParts(payload);
                foreach (string p in parts)
                {
                    var idx = p.LastIndexOf(@" ", StringComparison.Ordinal);
                    if (idx > 0)
                    {
                        var part = p.Substring(idx + 1).Trim();
                        if (matchFunc?.Invoke(p) ?? default(bool))
                        {
                            result.Add(part);
                        }
                    }
                    else
                    {
                        if (matchFunc?.Invoke(p) ?? default(bool))
                        {
                            result.Add(p);
                        }
                    }
                }
                return result;
            }

            static string[] GetParts(string payload)
            {
                var parts = payload.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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

            public bool DeleteOp(IList<IActivityIOPath> src)
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
                        var request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(activityIOPath.Path));
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
                        Dev2Logger.Error(message, e, GlobalConstants.WarewolfError);
                        throw new Exception(message);
                    }
                }
                return true;
            }

            public bool EnableSsl(IActivityIOPath path)
            {
                var result = path.PathType == enActivityIOPathType.FTPS || path.PathType == enActivityIOPathType.FTPES;
                return result;
            }

            public bool IsFilePresent(IActivityIOPath path)
            {
                var isFilePresent = IsStandardFtp(path) ? IsFilePresentStandardFtp(path) : IsFilePresentSftp(path);
                return isFilePresent;
            }

            bool IsFilePresentStandardFtp(IActivityIOPath path)
            {
                FtpWebResponse response = null;
                bool fileIsPresent;
                try
                {
                    var request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path.Path));
                    request.Method = WebRequestMethods.Ftp.GetFileSize;
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = EnableSsl(path);
                    if (path.Username != string.Empty)
                    {
                        request.Credentials = new NetworkCredential(path.Username, path.Password);
                    }
                    if (path.IsNotCertVerifiable)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                    }
                    using (response = (FtpWebResponse)request.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (StreamReader reader = new StreamReader(responseStream))
                                {
                                    Dev2Logger.Info("FTP file of size " + reader.ReadToEnd() + " found at " + path.Path, GlobalConstants.WarewolfInfo);
                                }
                            }
                        }
                    }
                    fileIsPresent = true;
                }
                catch (WebException wex)
                {
                    Dev2Logger.Error(this, wex, GlobalConstants.WarewolfError);
                    fileIsPresent = false;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    throw;
                }
                finally
                {
                    response?.Close();
                }
                return fileIsPresent;
            }

            bool IsFilePresentSftp(IActivityIOPath path)
            {
                var isAlive = false;
                try
                {
                    var listFilesInDirectory = ListFilesInDirectory(path);
                    if (listFilesInDirectory.Count > 0)
                    {
                        isAlive = true;
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    isAlive = false;
                }
                return isAlive;
            }

            public bool IsDirectoryAlreadyPresent(IActivityIOPath path)
            {
                var isAlive = IsStandardFtp(path) ? IsDirectoryAlreadyPresentStandardFtp(path) : IsDirectoryAlreadyPresentSftp(path);
                return isAlive;
            }

            public bool IsDirectoryAlreadyPresentStandardFtp(IActivityIOPath path)
            {
                FtpWebResponse response = null;
                bool isAlive;
                try
                {
                    var request = (FtpWebRequest)WebRequest.Create(ConvertSslToPlain(path.Path));
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = EnableSsl(path);

                    if (path.Username != string.Empty)
                    {
                        request.Credentials = new NetworkCredential(path.Username, path.Password);
                    }

                    if (path.IsNotCertVerifiable)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
                    }

                    using (response = (FtpWebResponse)request.GetResponse())
                    {

                        using (Stream responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (StreamReader reader = new StreamReader(responseStream))
                                {
                                    Dev2Logger.Info("FTP directory containing files " + reader.ReadToEnd() + " found at " + path.Path, GlobalConstants.WarewolfInfo);
                                }
                            }
                        }
                    }
                    isAlive = true;
                }
                catch (WebException wex)
                {
                    Dev2Logger.Error(this, wex, GlobalConstants.WarewolfError);
                    isAlive = false;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
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
                    if (arrayList == null || !arrayList.Any())
                    {
                        isAlive = false;
                    }
                }
                catch (Exception ex)
                {
                    isAlive = false;
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                }
                finally
                {
                    sftpClient.Dispose();
                }
                return isAlive;
            }

            bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            }

            public IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src)
            {
                List<string> dirs;
                try
                {
                    var tmpDirData = ExtendedDirList(src.Path, src.Username, src.Password, EnableSsl(src), src.IsNotCertVerifiable, src.PrivateKeyFile);
                    dirs = ExtractList(tmpDirData, IsFile);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    var message = $"{ex.Message} : [{src.Path}]";
                    throw new Exception(message, ex);
                }
                return dirs.Select(dir => BuildValidPathForFtp(src, dir)).Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, src.PrivateKeyFile)).ToList();
            }

            public enPathType PathIs(IActivityIOPath path)
            {
                var result = enPathType.File;
                if (Dev2ActivityIOPathUtils.IsDirectory(path.Path))
                {
                    result = enPathType.Directory;
                }
                return result;
            }

            public IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src)
            {
                List<string> dirs;
                try
                {
                    var tmpDirData = ExtendedDirList(src.Path, src.Username, src.Password, EnableSsl(src),
                                                        src.IsNotCertVerifiable, src.PrivateKeyFile);
                    dirs = ExtractList(tmpDirData, IsDirectory);

                    dirs.Remove(@".");
                    dirs.Remove(@"..");
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
                    var message = $"{ex.Message} : [{src.Path}]";
                    throw new Exception(message, ex);
                }
                return dirs.Select(dir => BuildValidPathForFtp(src, dir)).Select(uri => ActivityIOFactory.CreatePathFromString(uri, src.Username, src.Password, src.PrivateKeyFile)).ToList();
            }
        }
    }
}