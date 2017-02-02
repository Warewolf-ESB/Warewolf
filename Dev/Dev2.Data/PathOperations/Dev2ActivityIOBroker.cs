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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.PathOperations.Extension;
using Ionic.Zip;
using Warewolf.Resource.Errors;
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable NonLocalizedString

// ReSharper disable CheckNamespace
namespace Dev2.PathOperations
// ReSharper restore CheckNamespace
{

    // ReSharper disable InconsistentNaming
    internal class Dev2ActivityIOBroker : IActivityOperationsBroker
    {
        private readonly IFile _fileWrapper;
        private readonly ICommon _common;
        private static readonly ReaderWriterLockSlim FileLock = new ReaderWriterLockSlim();
        private const string ResultOk = @"Success";
        private const string ResultBad = @"Failure";
        private readonly List<string> _filesToDelete;

        public Dev2ActivityIOBroker()
            : this(new FileWrapper(), new Data.Util.CommonDataUtils())
        {
            _filesToDelete = new List<string>();
        }

        public Dev2ActivityIOBroker(IFile fileWrapper, ICommon common)
        {
            _fileWrapper = fileWrapper;
            _common = common;
            _filesToDelete = new List<string>();
        }

        public string Get(IActivityIOOperationsEndPoint path, bool deferredRead = false)
        {
            try
            {

                byte[] bytes;
                using (var s = path.Get(path.IOPath, _filesToDelete))
                {
                    bytes = new byte[s.Length];
                    s.Position = 0;
                    s.Read(bytes, 0, (int)s.Length);
                }

                return Encoding.UTF8.GetString(bytes);

            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, Dev2PutRawOperationTO args)
        {
            var result = ResultOk;
            try
            {
                FileLock.EnterWriteLock();
                if (dst.RequiresLocalTmpStorage())
                {
                    var tmp = CreateTmpFile();
                    switch (args.WriteType)
                    {
                        case WriteType.AppendBottom:
                            using (var s = dst.Get(dst.IOPath, _filesToDelete))
                            {
                                _fileWrapper.WriteAllBytes(tmp, s.ToByteArray());
                                _fileWrapper.AppendAllText(tmp, args.FileContents);
                            }
                            break;
                        case WriteType.AppendTop:
                            using (var s = dst.Get(dst.IOPath, _filesToDelete))
                            {
                                _fileWrapper.WriteAllText(tmp, args.FileContents);
                                _common.AppendToTemp(s, tmp);
                            }
                            break;
                        default:
                            WriteDataToFile(args, tmp);
                            break;
                    }
                    result = MoveTmpFileToDestination(dst, tmp, result);
                }
                else
                {
                    if (_fileWrapper.Exists(dst.IOPath.Path))
                    {
                        var tmp = CreateTmpFile();
                        switch (args.WriteType)
                        {
                            case WriteType.AppendBottom:
                                _fileWrapper.AppendAllText(dst.IOPath.Path, args.FileContents);
                                result = ResultOk;
                                break;
                            case WriteType.AppendTop:
                                using (var s = dst.Get(dst.IOPath, _filesToDelete))
                                {
                                    _fileWrapper.WriteAllText(tmp, args.FileContents);
                                    _common.AppendToTemp(s, tmp);
                                    result = MoveTmpFileToDestination(dst, tmp, result);
                                    RemoveTmpFile(tmp);
                                }
                                break;
                            default:
                                WriteDataToFile(args, tmp);
                                result = MoveTmpFileToDestination(dst, tmp, result);
                                RemoveTmpFile(tmp);
                                break;
                        }
                    }
                    else
                    {
                        Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(true);
                        CreateEndPoint(dst, newArgs, true);
                        var path = dst.IOPath.Path;
                        WriteDataToFile(args, path);                       
                    }
                }
            }
            finally
            {
                FileLock.ExitWriteLock();
                for (var index = _filesToDelete.Count - 1; index > 0; index--)
                {
                    var name = _filesToDelete[index];
                    RemoveTmpFile(name);
                }
            }
            return result;
        }

        private void WriteDataToFile(Dev2PutRawOperationTO args, string path)
        {
            if(IsBase64(args.FileContents))
            {
                var data = GetBytesFromBase64String(args);
                _fileWrapper.WriteAllBytes(path, data);
            }
            else
            {
                _fileWrapper.WriteAllText(path, args.FileContents);
            }
        }

        private static byte[] GetBytesFromBase64String(Dev2PutRawOperationTO args)
        {
            var data = Convert.FromBase64String(args.FileContents.Replace(@"Content-Type:BASE64", @""));
            return data;
        }

        private string MoveTmpFileToDestination(IActivityIOOperationsEndPoint dst, string tmp, string result)
        {
            using (Stream s = new MemoryStream(_fileWrapper.ReadAllBytes(tmp)))
            {
                Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(true);

                if (!dst.PathExist(dst.IOPath))
                {
                    CreateEndPoint(dst, newArgs, true);
                }
                if (dst.Put(s, dst.IOPath, newArgs, null, _filesToDelete) < 0)
                {
                    result = ResultBad;
                }
            }
            return result;
        }

        public string Delete(IActivityIOOperationsEndPoint src)
        {
            var result = ResultOk;
            try
            {
                if (!src.Delete(src.IOPath))
                {
                    result = ResultBad;
                }
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
            return result;
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src, ReadTypes readTypes)
        {
            if (readTypes == ReadTypes.FilesAndFolders)
            {
                return src.ListDirectory(src.IOPath);
            }
            return readTypes == ReadTypes.Files ? src.ListFilesInDirectory(src.IOPath) : src.ListFoldersInDirectory(src.IOPath);
        }

        public string Create(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
        {
            string result;
            try
            {
                _common.ValidateEndPoint(dst, args);
                result = CreateEndPoint(dst, args, createToFile);
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
            return result;
        }

        public string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            string result;
            try
            {
                result = ValidateRenameSourceAndDesinationTypes(src, dst, args);
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
            return result;
        }

        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            string status;
            try
            {
                status = ValidateCopySourceDestinationFileOperation(src, dst, args, () =>
                    {
                        if (src.RequiresLocalTmpStorage())
                        {
                            if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                            {
                                dst.IOPath.Path = dst.Combine(GetFileNameFromEndPoint(src));
                            }

                            using (var s = src.Get(src.IOPath, _filesToDelete))
                            {
                                dst.Put(s, dst.IOPath, args, Path.IsPathRooted(src.IOPath.Path) ? Path.GetDirectoryName(src.IOPath.Path) : null, _filesToDelete);
                                s.Close();
                                s.Dispose();
                            }
                        }
                        else
                        {
                            var sourceFile = new FileInfo(src.IOPath.Path);
                            if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                            {
                                dst.IOPath.Path = dst.Combine(sourceFile.Name);
                            }

                            using (var s = src.Get(src.IOPath, _filesToDelete))
                            {
                                if (sourceFile.Directory != null)
                                {
                                    dst.Put(s, dst.IOPath, args, sourceFile.Directory.ToString(), _filesToDelete);
                                }
                            }
                        }
                        return ResultOk;
                    });
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
            return status;
        }

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,Dev2CRUDOperationTO args)
        {
            string result;

            try
            {
                result = Copy(src, dst, args);

                if (result.Equals(ResultOk))
                {
                    src.Delete(src.IOPath);
                }
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }

            return result;
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2UnZipOperationTO args)
        {
            string status;

            try
            {
                status = ValidateUnzipSourceDestinationFileOperation(src, dst, args, () =>
                    {
                        ZipFile zip;
                        var tempFile = string.Empty;

                        if (src.RequiresLocalTmpStorage())
                        {
                            var tmpZip = CreateTmpFile();
                            using (var s = src.Get(src.IOPath, _filesToDelete))
                            {
                                _fileWrapper.WriteAllBytes(tmpZip, s.ToByteArray());
                            }

                            tempFile = tmpZip;
                            zip = ZipFile.Read(tempFile);
                        }
                        else
                        {
                            zip = ZipFile.Read(src.Get(src.IOPath, _filesToDelete));
                        }

                        if (dst.RequiresLocalTmpStorage())
                        {
                            // unzip locally then Put the contents of the archive to the dst end-point
                            var tempPath = _common.CreateTmpDirectory();
                            _common.ExtractFile(args, zip, tempPath);
                            var endPointPath = ActivityIOFactory.CreatePathFromString(tempPath, string.Empty, string.Empty);
                            var endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(endPointPath);
                            Move(endPoint, dst, new Dev2CRUDOperationTO(args.Overwrite));
                        }
                        else
                        {
                            _common.ExtractFile(args, zip, dst.IOPath.Path);
                        }

                        if (src.RequiresLocalTmpStorage())
                        {
                            _fileWrapper.Delete(tempFile);
                        }

                        return ResultOk;
                    });
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }

            return status;
        }

        #region Private Methods

        private string CreateEndPoint(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
        {
            var result = ResultOk;
            var activityIOPath = dst.IOPath;
            var dirParts = MakeDirectoryParts(activityIOPath, dst.PathSeperator());

            var deepestIndex = -1;
            var startDepth = dirParts.Count - 1;

            var pos = startDepth;

            while (pos >= 0 && deepestIndex == -1)
            {
                var tmpPath = ActivityIOFactory.CreatePathFromString(dirParts[pos], activityIOPath.Username,
                                                                                 activityIOPath.Password, true, activityIOPath.PrivateKeyFile);
                try
                {
                    if (dst.ListDirectory(tmpPath) != null)
                    {
                        deepestIndex = pos;
                    }
                }
                catch (Exception)
                {
                    //Note that we doing a recursive create should the directory not exists
                }
                finally
                {
                    pos--;
                }
            }

            pos = deepestIndex + 1;
            var ok = true;

            var origPath = dst.IOPath;

            while (pos <= startDepth && ok)
            {
                var toCreate = ActivityIOFactory.CreatePathFromString(dirParts[pos], dst.IOPath.Username,
                                                                                  dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile);
                dst.IOPath = toCreate;
                ok = CreateDirectory(dst, args);
                pos++;
            }

            dst.IOPath = origPath;

            if (!ok)
            {
                result = ResultBad;
            }
            else if (dst.PathIs(dst.IOPath) == enPathType.File && createToFile)
            {
                if (!CreateFile(dst, args))
                {
                    result = ResultBad;
                }
            }

            return result;
        }

        private bool IsBase64(string payload)
        {
            return payload.StartsWith(@"Content-Type:BASE64");
        }

        private IList<string> MakeDirectoryParts(IActivityIOPath path, string splitter)
        {
            string[] tmp;

            IList<string> result = new List<string>();

            var splitOn = splitter.ToCharArray();

            if (_common.IsNotFtpTypePath(path))
            {
                tmp = path.Path.Split(splitOn);

            }
            else
            {
                var splitValues = path.Path.Split(new[] { @"://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                splitValues.RemoveAt(0);
                var newPath = string.Join(@"/", splitValues);
                tmp = newPath.Split(splitOn);
            }

            // remove trailing file entry if exist
            var candiate = tmp[tmp.Length - 1];
            var len = tmp.Length;
            if (candiate.Contains(@"*.") || candiate.Contains(@"."))
            {
                len = tmp.Length - 1;
            }

            var builderPath = string.Empty;
            // build up URI parts from root down
            for (var i = 0; i < len; i++)
            {
                if (!string.IsNullOrWhiteSpace(tmp[i]))
                {
                    builderPath += tmp[i] + splitter;
                    if (!_common.IsNotFtpTypePath(path) && !builderPath.Contains(@"://"))
                    {
                        var splitValues = path.Path.Split(new[] { @"://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        builderPath = splitValues[0] + @"://" + builderPath;
                    }
                    result.Add(_common.IsUncFileTypePath(path) ? @"\\" + builderPath : builderPath);
                }
            }
            return result;
        }

        private bool CreateDirectory(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            var result = dst.CreateDirectory(dst.IOPath, args);
            return result;
        }

        private bool CreateFile(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {

            var result = true;

            var tmp = CreateTmpFile();
            using (Stream s = new MemoryStream(_fileWrapper.ReadAllBytes(tmp)))
            {

                if (dst.Put(s, dst.IOPath, args, null, _filesToDelete) < 0)
                {
                    result = false;
                }

                s.Close();
            }

            return result;
        }



        /// <summary>
        /// Transfer the contents of the directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        bool TransferDirectoryContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            ValidateSourceAndDestinationContents(src, dst, args);

            if (args.DoRecursiveCopy)
            {
                RecursiveCopy(src, dst, args);
            }

            var srcContents = src.ListFilesInDirectory(src.IOPath);
            var result = true;
            var origDstPath = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(dst.IOPath.Path);

            if (!dst.PathExist(dst.IOPath))
            {
                CreateDirectory(dst, args);
            }
            foreach (var p in srcContents)
            {
                result = PerformTransfer(src, dst, args, origDstPath, p, result);
            }
            Dev2Logger.Debug($"Transfered: {src.IOPath.Path}");
            return result;
        }

        private bool PerformTransfer(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, string origDstPath, IActivityIOPath p, bool result)
        {
            try
            {
                if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                {
                    var cpPath =
                        ActivityIOFactory.CreatePathFromString(
                            $"{origDstPath}{dst.PathSeperator()}{Dev2ActivityIOPathUtils.ExtractFileName(p.Path)}",
                            dst.IOPath.Username,
                            dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile);
                    var path = cpPath.Path;
                    DoFileTransfer(src, dst, args, cpPath, p, path, ref result);
                }
                else if (args.Overwrite || !dst.PathExist(dst.IOPath))
                {
                    var tmp = origDstPath + @"\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);
                    var path = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password, dst.IOPath.PrivateKeyFile);
                    DoFileTransfer(src, dst, args, path, p, path.Path, ref result);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
            }
            return result;
        }

        private void DoFileTransfer(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, IActivityIOPath dstPath, IActivityIOPath p, string path, ref bool result)
        {
            if (args.Overwrite || !dst.PathExist(dstPath))
            {
                result = TransferFile(src, dst, args, path, p, result);
            }
        }

        private bool TransferFile(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, string path, IActivityIOPath p, bool result)
        {
            var tmpPath = ActivityIOFactory.CreatePathFromString(path, dst.IOPath.Username, dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile);
            var tmpEp = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
            var whereToPut = GetWhereToPut(src, dst);
            using (var s = src.Get(p, _filesToDelete))
            {
                if (tmpEp.Put(s, tmpEp.IOPath, args, whereToPut, _filesToDelete) < 0)
                {
                    result = false;
                }
                s.Close();
                s.Dispose();
            }
            return result;
        }

        void RecursiveCopy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            try
            {
                var srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                Task.WaitAll(srcContentsFolders.Select(sourcePath => Task.Run(() =>
                    {
                        var sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(sourcePath);
                        IList<string> dirParts = sourceEndPoint.IOPath.Path.Split(sourceEndPoint.PathSeperator().ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        var destinationPath = ActivityIOFactory.CreatePathFromString(dst.Combine(dirParts.Last()), dst.IOPath.Username, dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile);
                        var destinationEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(destinationPath);
                        dst.CreateDirectory(destinationPath, args);
                        TransferDirectoryContents(sourceEndPoint, destinationEndPoint, args);
                    })).ToArray());
            }
            catch (AggregateException e)
            {
                var message = e.InnerExceptions.Where(exception => !string.IsNullOrEmpty(exception?.Message)).Aggregate(string.Empty, (current, exception) => current + exception.Message + "\r\n");
                throw new Exception(message, e);
            }
        }

        /// <summary>
        /// Transfer the contents of the directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        void ValidateSourceAndDestinationContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            if (!args.Overwrite)
            {
                var srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                foreach (var sourcePath in srcContentsFolders)
                {
                    var sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(sourcePath);

                    IList<string> dirParts = sourceEndPoint.IOPath.Path.Split(sourceEndPoint.PathSeperator().ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var directory = dirParts.Last();
                    var destinationPath = ActivityIOFactory.CreatePathFromString(dst.Combine(directory), dst.IOPath.Username, dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile);

                    var destinationEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(destinationPath);

                    if (destinationEndPoint.PathExist(destinationEndPoint.IOPath))
                    {
                        ValidateSourceAndDestinationContents(sourceEndPoint, destinationEndPoint, args);
                    }
                }


                var srcContents = src.ListFilesInDirectory(src.IOPath);
                var dstContents = dst.ListFilesInDirectory(dst.IOPath);

                var sourceFileNames = srcContents.Select(srcFile => GetFileNameFromEndPoint(src, srcFile)).ToList();
                var destinationFileNames = dstContents.Select(dstFile => GetFileNameFromEndPoint(dst, dstFile)).ToList();

                if (destinationFileNames.Count > 0)
                {
                    var commonFiles = sourceFileNames.Where(destinationFileNames.Contains).ToList();

                    if (commonFiles.Count > 0)
                    {
                        var fileNames = commonFiles.Aggregate("",
                                                                 (current, commonFile) =>
                                                                 current + "\r\n" + commonFile);
                        throw new Exception(string.Format(ErrorResource.FileExistInDestinationFolder, fileNames));
                    }
                }
            }
        }

        static string GetWhereToPut(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
        {
            if (src.IOPath.PathType == enActivityIOPathType.FileSystem)
            {
                return Path.GetDirectoryName(src.IOPath.Path);
            }
            if (dst.IOPath.PathType == enActivityIOPathType.FileSystem)
            {
                return Path.GetDirectoryName(src.IOPath.Path);
            }
            return null;
        }

        /// <summary>
        /// Creates a tmp file
        /// </summary>
        /// <returns></returns>
        private string CreateTmpFile()
        {
            try
            {
                var tmpFile = Path.GetTempFileName();
                _filesToDelete.Add(tmpFile);
                return tmpFile;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                throw;
            }

        }

        /// <summary>
        /// Remove a tmp file
        /// </summary>
        /// <param name="path"></param>
        private void RemoveTmpFile(string path)
        {
            try
            {
                _fileWrapper.Delete(path);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                throw;
            }

        }

        private void EnsureFilesDontExists(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
        {
            if (dst.PathExist(dst.IOPath))
            {
                if (dst.PathIs(dst.IOPath) == enPathType.File)
                {
                    throw new Exception(ErrorResource.FileWithSameNameExist);
                }
                var dstContents = dst.ListDirectory(dst.IOPath);
                var destinationFileNames = dstContents.Select(dstFile => GetFileNameFromEndPoint(dst, dstFile));
                var sourceFile = GetFileNameFromEndPoint(src);

                if (destinationFileNames.Contains(sourceFile))
                {
                    throw new Exception(string.Format(ErrorResource.FileExistInDestinationFolder, sourceFile));
                }
            }
        }

        private string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint)
        {
            string pathSeperator = endPoint.PathSeperator();
            return endPoint.IOPath.Path.Split(pathSeperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }

        private string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint, IActivityIOPath path)
        {
            var pathSeperator = endPoint.PathSeperator();
            return path.Path.Split(pathSeperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args)
        {
            string status;

            try
            {
                status = ValidateZipSourceDestinationFileOperation(src, dst, args, () =>
                {
                    string tempFileName;

                    if (src.PathIs(src.IOPath) == enPathType.Directory || Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                    {
                        tempFileName = ZipDirectoryToALocalTempFile(src, args);
                    }
                    else
                    {
                        tempFileName = ZipFileToALocalTempFile(src, args);
                    }

                    return TransferTempZipFileToDestination(src, dst, args, tempFileName);
                });
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
            return status;
        }

        string ZipFileToALocalTempFile(IActivityIOOperationsEndPoint src, Dev2ZipOperationTO args)
        {
            var packFile = src.IOPath.Path;
            var tempFileName = CreateTmpFile();

            if (src.RequiresLocalTmpStorage())
            {
                string tempDir = _common.CreateTmpDirectory();
                var tmpFile = Path.Combine(tempDir,
                                           src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                                 StringSplitOptions.RemoveEmptyEntries)
                                              .Last());
                packFile = tmpFile;
                using (var s = src.Get(src.IOPath, _filesToDelete))
                {
                    _fileWrapper.WriteAllBytes(tmpFile, s.ToByteArray());
                }
            }

            using (var zip = new ZipFile())
            {
                if (args.ArchivePassword != string.Empty)
                {
                    zip.Password = args.ArchivePassword;
                }
                zip.CompressionLevel = _common.ExtractZipCompressionLevel(args.CompressionRatio);
                zip.AddFile(packFile, ".");
                zip.Save(tempFileName);
            }

            return tempFileName;
        }

        string ZipDirectoryToALocalTempFile(IActivityIOOperationsEndPoint src, Dev2ZipOperationTO args)
        {
            var tmpDir = _common.CreateTmpDirectory();
            var tempFilename = CreateTmpFile();
            var tmpPath = ActivityIOFactory.CreatePathFromString(tmpDir, "", "");
            var tmpEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);

            TransferDirectoryContents(src, tmpEndPoint, new Dev2CRUDOperationTO(true));
            using (var zip = new ZipFile())
            {
                zip.SaveProgress += (sender, eventArgs) =>
                {
                    if (eventArgs.CurrentEntry != null)
                    {
                        Dev2Logger.Debug($"Event Type: {eventArgs.EventType} Total Entries: {eventArgs.EntriesTotal} Entries Saved: {eventArgs.EntriesSaved} Current Entry: {eventArgs.CurrentEntry.FileName}");
                    }
                };
                if (args.ArchivePassword != string.Empty)
                {
                    zip.Password = args.ArchivePassword;
                }

                zip.CompressionLevel = _common.ExtractZipCompressionLevel(args.CompressionRatio);

                var toAdd = ListDirectory(tmpEndPoint, ReadTypes.FilesAndFolders);
                foreach (var p in toAdd)
                {
                    if (tmpEndPoint.PathIs(p) == enPathType.Directory)
                    {
                        var directoryPathInArchive = p.Path.Replace(tmpPath.Path, "");
                        zip.AddDirectory(p.Path, directoryPathInArchive);
                    }
                    else
                    {
                        zip.AddFile(p.Path, ".");
                    }
                }
                zip.Save(tempFilename);
            }

            DirectoryHelper.CleanUp(tmpDir);

            return tempFilename;
        }

        string TransferTempZipFileToDestination(IActivityIOOperationsEndPoint src,
                                                        IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args,
                                                        string tmpZip)
        {
            string result;
            using (Stream s2 = new MemoryStream(_fileWrapper.ReadAllBytes(tmpZip)))
            {
                dst =
                    ActivityIOFactory.CreateOperationEndPointFromIOPath(
                        ActivityIOFactory.CreatePathFromString(dst.IOPath.Path, dst.IOPath.Username,
                                                               dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile));

                var zipTransferArgs = new Dev2CRUDOperationTO(args.Overwrite);

                result = ResultOk;

                if (src.RequiresLocalTmpStorage())
                {
                    if (dst.Put(s2, dst.IOPath, zipTransferArgs, null, _filesToDelete) < 0)
                    {
                        result = ResultBad;
                    }
                }
                else
                {
                    var fileInfo = new FileInfo(src.IOPath.Path);
                    if (fileInfo.Directory != null && Path.IsPathRooted(fileInfo.Directory.ToString()))
                    {
                        if (dst.Put(s2, dst.IOPath, zipTransferArgs, fileInfo.Directory.ToString(), _filesToDelete) < 0)
                        {
                            result = ResultBad;
                        }
                    }
                    else
                    {
                        if (dst.Put(s2, dst.IOPath, zipTransferArgs, null, _filesToDelete) < 0)
                        {
                            result = ResultBad;
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        #region Validations

        string ValidateRenameSourceAndDesinationTypes(IActivityIOOperationsEndPoint src,
                                                              IActivityIOOperationsEndPoint dst,
                                                              Dev2CRUDOperationTO args)
        {
            if (src.PathIs(src.IOPath) != dst.PathIs(dst.IOPath))
            {
                throw new Exception(ErrorResource.SourceAndDestinationNOTFilesOrDirectory);
            }
            if (dst.PathExist(dst.IOPath))
            {
                if (!args.Overwrite)
                {
                    throw new Exception(ErrorResource.DestinationDirectoryExist);
                }
                dst.Delete(dst.IOPath);
            }

            return Move(src, dst, args);
        }

        string ValidateCopySourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                  IActivityIOOperationsEndPoint dst,
                                                                  Dev2CRUDOperationTO args,
                                                                  Func<string> performAfterValidation)
        {
            var result = ResultOk;
            _common.ValidateSourceAndDestinationPaths(src, dst);
            var opStatus = CreateEndPoint(dst, args, dst.PathIs(dst.IOPath) == enPathType.Directory);
            if (!opStatus.Equals("Success"))
            {
                throw new Exception(string.Format(ErrorResource.RecursiveDirectoryCreateFailed, dst.IOPath.Path));
            }
            if (src.PathIs(src.IOPath) == enPathType.Directory)
            {
                if (!TransferDirectoryContents(src, dst, args))
                {
                    result = ResultBad;
                }
            }
            else
            {
                if (!args.Overwrite)
                {
                    EnsureFilesDontExists(src, dst);
                }

                if (!Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                {
                    return performAfterValidation();
                }
                if (!TransferDirectoryContents(src, dst, args))
                {
                    result = ResultBad;
                }
            }

            return result;
        }

        private string ValidateUnzipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                   IActivityIOOperationsEndPoint dst,
                                                                   Dev2UnZipOperationTO args,
                                                                   Func<string> performAfterValidation)
        {
            _common.ValidateSourceAndDestinationPaths(src, dst);

            if (dst.PathIs(dst.IOPath) != enPathType.Directory)
            {
                throw new Exception(ErrorResource.DestinationMustBeADirectory);
            }

            if (src.PathIs(src.IOPath) != enPathType.File)
            {
                throw new Exception(ErrorResource.SourceMustBeAFile);
            }

            if (!args.Overwrite && dst.PathExist(dst.IOPath))
            {
                throw new Exception(ErrorResource.DestinationDirectoryExist);
            }

            return performAfterValidation();
        }

        string ValidateZipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                 IActivityIOOperationsEndPoint dst,
                                                                 Dev2ZipOperationTO args,
                                                                 Func<string> performAfterValidation)
        {
            _common.AddMissingFileDirectoryParts(src, dst);


            if (dst.PathIs(dst.IOPath) == enPathType.Directory)
            {
                var sourcePart =
                    src.IOPath.Path.Split(src.PathSeperator().ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                       .Last();
                if (src.PathIs(src.IOPath) == enPathType.File)
                {
                    var fileInfo = new FileInfo(sourcePart);
                    dst.IOPath.Path = dst.Combine(sourcePart.Replace(fileInfo.Extension, ".zip"));
                }
                else
                {
                    dst.IOPath.Path = dst.IOPath.Path + ".zip";
                }
            }
            else
            {
                var sourcePart =
                    dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                       .Last();
                var fileInfo = new FileInfo(sourcePart);
                dst.IOPath.Path = dst.IOPath.Path.Replace(fileInfo.Extension, ".zip");
            }

            if (!args.Overwrite && dst.PathExist(dst.IOPath))
            {
                throw new Exception(ErrorResource.DestinationFileAlreadyExists);
            }

            var opStatus = CreateEndPoint(dst, new Dev2CRUDOperationTO(args.Overwrite),
                                             dst.PathIs(dst.IOPath) == enPathType.Directory);
            if (!opStatus.Equals(ResultOk))
            {
                throw new Exception(string.Format(ErrorResource.RecursiveDirectoryCreateFailed, dst.IOPath.Path));
            }

            return performAfterValidation();
        }
        #endregion
    }
}

