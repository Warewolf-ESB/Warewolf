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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Extension;
using Ionic.Zip;
using Warewolf.Resource.Errors;

namespace Dev2.PathOperations
{
    public interface IActivityIOBrokerMainDriver : IActivityIOBrokerDriver
    {
        string WriteToRemoteTempStorage(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args, string tmp);
        void WriteToLocalTempStorage(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args, string tmp);
        string TransferTempZipFileToDestination(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
            IDev2ZipOperationTO args, string tmpZip);
        string MoveTmpFileToDestination(IActivityIOOperationsEndPoint dst, string tmp);
        bool WriteDataToFile(IDev2PutRawOperationTO args, IActivityIOOperationsEndPoint dst);
        /*string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint src)*/
        string ZipDirectoryToALocalTempFile(IActivityIOOperationsEndPoint src, IDev2ZipOperationTO args);
        string ZipFileToALocalTempFile(IActivityIOOperationsEndPoint src, IDev2ZipOperationTO args);
    }

    internal class ActivityIOBrokerMainDriver : ActivityIOBrokerBaseDriver, IActivityIOBrokerMainDriver
    {
        [ExcludeFromCodeCoverage]
        internal ActivityIOBrokerMainDriver()
        {
        }
        internal ActivityIOBrokerMainDriver(IFile file, ICommon common)
            : base (file, common)
        {
        }

        public string WriteToRemoteTempStorage(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args, string tmp)
        {
            switch (args.WriteType)
            {
                case WriteType.AppendBottom:
                    var fileContent = Encoding.ASCII.GetBytes(args.FileContents);
                    var putResult = PerformPut(fileContent, dst, false);
                    return putResult ? ResultOk : ResultBad;

                case WriteType.AppendTop:
                    using (var s = dst.Get(dst.IOPath, _filesToDelete))
                    {
                        _fileWrapper.WriteAllText(tmp, args.FileContents);
                        using (var temp = new FileStream(tmp, FileMode.Append))
                        {
                            s.CopyTo(temp);
                        }
                        return MoveTmpFileToDestination(dst, tmp);
                    }

                default:
                    var res = WriteDataToFile(args, dst);
                    return res ? ResultOk : ResultBad;
            }
        }


        public void WriteToLocalTempStorage(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args, string tmp)
        {
            switch (args.WriteType)
            {
                case WriteType.AppendBottom:
                    using (var s = dst.Get(dst.IOPath, _filesToDelete))
                    {
                        _fileWrapper.WriteAllBytes(tmp, s.ToByteArray());
                        _fileWrapper.AppendAllText(tmp, args.FileContents);
                    }
                    return;
                case WriteType.AppendTop:
                    using (var s = dst.Get(dst.IOPath, _filesToDelete))
                    {
                        _fileWrapper.WriteAllText(tmp, args.FileContents);
                        using (var temp = new FileStream(tmp, FileMode.Append))
                        {
                            s.CopyTo(temp);
                        }
                    }
                    return;
                default:
                    _fileWrapper.AppendAllText(tmp, args.FileContents);
                    return;
            }
        }

        static bool PerformPut(byte[] fileContent, IActivityIOOperationsEndPoint dst, bool overwrite)
        {
            using (Stream s = new MemoryStream(fileContent))
            {
                var putOperation = dst.Put(s, dst.IOPath, new Dev2CRUDOperationTO(overwrite), null, new List<string>());
                if (putOperation >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool WriteDataToFile(IDev2PutRawOperationTO args, IActivityIOOperationsEndPoint dst)
        {
            var isBase64 = args.FileContents.StartsWith(@"Content-Type:BASE64", StringComparison.InvariantCulture);
            if (isBase64)
            {
                var data = GetBytesFromBase64String(args);
                return PerformPut(data, dst, true);
            }
            else
            {
                var fileContent = Encoding.ASCII.GetBytes(args.FileContents);
                return PerformPut(fileContent, dst, true);
            }
        }

        static byte[] GetBytesFromBase64String(IDev2PutRawOperationTO args)
        {
            var data = Convert.FromBase64String(args.FileContents.Replace(@"Content-Type:BASE64", @""));
            return data;
        }

        public string MoveTmpFileToDestination(IActivityIOOperationsEndPoint dst, string tmp)
        {
            using (Stream s = new MemoryStream(_fileWrapper.ReadAllBytes(tmp)))
            {
                var newArgs = new Dev2CRUDOperationTO(true);

                if (!dst.PathExist(dst.IOPath) && CreateEndPoint(dst, newArgs, true) == ResultBad)
                {
                    return ResultBad;
                }
                if (dst.Put(s, dst.IOPath, newArgs, null, _filesToDelete) >= 0)
                {
                    return ResultOk;
                }
            }
            return ResultBad;
        }

        protected bool TransferDirectoryContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            if (!args.Overwrite)
            {
                ValidateSourceAndDestinationContents(src, dst, args);
            }

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
            // TODO: cleanup this code so that result is easier to follow
            foreach (var p in srcContents)
            {
                result = PerformTransfer(src, dst, args, origDstPath, p, result);
            }
            Dev2Logger.Debug($"Transfered: {src.IOPath.Path}", GlobalConstants.WarewolfDebug);
            return result;
        }

        bool PerformTransfer(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, string origDstPath, IActivityIOPath p, bool result)
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
                else
                {
                    if (args.Overwrite || !dst.PathExist(dst.IOPath))
                    {
                        var tmp = origDstPath + @"\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);
                        var path = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password, dst.IOPath.PrivateKeyFile);
                        DoFileTransfer(src, dst, args, path, p, path.Path, ref result);
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }
            return result;
        }

        void DoFileTransfer(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, IActivityIOPath dstPath, IActivityIOPath p, string path, ref bool result)
        {
            if (args.Overwrite || !dst.PathExist(dstPath))
            {
                result = TransferFile(src, dst, args, path, p, ref result);
            }
        }

        bool TransferFile(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, string path, IActivityIOPath p, ref bool result)
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

        void RecursiveCopy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            try
            {
                var srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                // TODO: should not do parallel io if the operations are on the same physical disk? check type of OperationsEndpoint first? delegate via polymorphism to the endpoints?
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

        void ValidateSourceAndDestinationContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
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

        public string ZipFileToALocalTempFile(IActivityIOOperationsEndPoint src, IDev2ZipOperationTO args)
        {
            var packFile = src.IOPath.Path;
            var tempFileName = CreateTmpFile();

            if (src.RequiresLocalTmpStorage())
            {
                var tempDir = _common.CreateTmpDirectory();
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

        public string ZipDirectoryToALocalTempFile(IActivityIOOperationsEndPoint src, IDev2ZipOperationTO args)
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
                        Dev2Logger.Debug($"Event Type: {eventArgs.EventType} Total Entries: {eventArgs.EntriesTotal} Entries Saved: {eventArgs.EntriesSaved} Current Entry: {eventArgs.CurrentEntry.FileName}", GlobalConstants.WarewolfDebug);
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
            var dir = new DirectoryWrapper();
            dir.CleanUp(tmpDir);

            return tempFilename;
        }

        public string TransferTempZipFileToDestination(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2ZipOperationTO args, string tmpZip)
        {
            using (Stream s2 = new MemoryStream(_fileWrapper.ReadAllBytes(tmpZip)))
            {
                var activityIOOperationsEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(
                        ActivityIOFactory.CreatePathFromString(dst.IOPath.Path, dst.IOPath.Username,
                                                               dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile));

                var zipTransferArgs = new Dev2CRUDOperationTO(args.Overwrite);

                if (src.RequiresLocalTmpStorage())
                {
                    if (activityIOOperationsEndPoint.Put(s2, activityIOOperationsEndPoint.IOPath, zipTransferArgs, null, _filesToDelete) >= 0)
                    {
                        return ResultOk;
                    }
                }
                else
                {
                    var fileInfo = new FileInfo(src.IOPath.Path);
                    if (fileInfo.Directory != null && Path.IsPathRooted(fileInfo.Directory.ToString()))
                    {
                        if (activityIOOperationsEndPoint.Put(s2, activityIOOperationsEndPoint.IOPath, zipTransferArgs, fileInfo.Directory.ToString(), _filesToDelete) >= 0)
                        {
                            return ResultOk;
                        }
                    }
                    else
                    {
                        if (activityIOOperationsEndPoint.Put(s2, activityIOOperationsEndPoint.IOPath, zipTransferArgs, null, _filesToDelete) >= 0)
                        {
                            return ResultOk;
                        }
                    }
                }
            }
            return ResultBad;
        }
    }
}
