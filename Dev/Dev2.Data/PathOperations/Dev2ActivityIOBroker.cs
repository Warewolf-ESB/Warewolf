
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Binary_Objects;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.PathOperations.Extension;
using Ionic.Zip;

// ReSharper disable CheckNamespace
namespace Dev2.PathOperations
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a concrete impl of the IActivityOperationBroker to facilitate IO operations
    /// </summary>
    // ReSharper disable InconsistentNaming
    internal class Dev2ActivityIOBroker : IActivityOperationsBroker
    {
        private static readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim();
        const string ResultOk = "Success";
        const string ResultBad = "Failure";
        private static List<string> _filesToDelete;

        public Dev2ActivityIOBroker()
        {
            _filesToDelete = new List<string>();
        }

        // See interfaces summary's for more detail

        public string Get(IActivityIOOperationsEndPoint path, bool deferredRead = false)
        {
            string result;
            try
            {

                // TODO : we need to chunk this in
                if(!deferredRead)
                {
                    byte[] bytes;
                    using(var s = path.Get(path.IOPath, _filesToDelete))
                    {
                        bytes = new byte[s.Length];
                        s.Position = 0;
                        s.Read(bytes, 0, (int)s.Length);
                    }

                    // TODO : Remove the need for this ;(
                    return Encoding.UTF8.GetString(bytes);
                }

                // If we want to defer the read of data, just return the file name ;)
                // Serialize to binary and return 
                BinaryDataListUtil bdlUtil = new BinaryDataListUtil();
                result = bdlUtil.SerializeDeferredItem(path);
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
            return result;
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, Dev2PutRawOperationTO args)
        {
            var result = ResultOk;

            // directory put?
            // wild char put?
            try
            {
                _fileLock.EnterWriteLock();
                if(dst.RequiresLocalTmpStorage())
                {
                    var tmp = CreateTmpFile();
                    switch(args.WriteType)
                    {
                        case WriteType.AppendBottom:
                            using(var s = dst.Get(dst.IOPath, _filesToDelete))
                            {
                                File.WriteAllBytes(tmp, s.ToByteArray());
                                File.AppendAllText(tmp, args.FileContents);
                            }
                            break;
                        case WriteType.AppendTop:
                            using(var s = dst.Get(dst.IOPath, _filesToDelete))
                            {
                                File.WriteAllText(tmp, args.FileContents);
                                AppendToTemp(s, tmp);
                            }
                            break;
                        default:
                            if(IsBase64(args.FileContents))
                            {
                                var data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                                File.WriteAllBytes(tmp, data);
                            }
                            else
                            {
                                File.WriteAllText(tmp, args.FileContents);
                            }
                            break;
                    }
                    result = MoveTmpFileToDestination(dst, tmp, result);
                }
                else
                {
                    if(File.Exists(dst.IOPath.Path))
                    {

                        var tmp = CreateTmpFile();
                        switch(args.WriteType)
                        {
                            case WriteType.AppendBottom:
                                File.AppendAllText(dst.IOPath.Path, args.FileContents);
                                result = ResultOk;
                                break;
                            case WriteType.AppendTop:
                                using(var s = dst.Get(dst.IOPath, _filesToDelete))
                                {
                                    File.WriteAllText(tmp, args.FileContents);

                                    AppendToTemp(s, tmp);
                                    result = MoveTmpFileToDestination(dst, tmp, result);
                                    RemoveTmpFile(tmp);
                                }
                                break;
                            default:
                                if(IsBase64(args.FileContents))
                                {
                                    var data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                                    File.WriteAllBytes(tmp, data);
                                }
                                else
                                {
                                    File.WriteAllText(tmp, args.FileContents);
                                }
                                result = MoveTmpFileToDestination(dst, tmp, result);
                                RemoveTmpFile(tmp);
                                break;
                        }
                    }
                    else
                    {
                        // we can write directly to the file
                        Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(true);

                        CreateEndPoint(dst, newArgs, true);

                        if(IsBase64(args.FileContents))
                        {
                            var data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                            File.WriteAllBytes(dst.IOPath.Path, data);
                        }
                        else
                        {
                            File.WriteAllText(dst.IOPath.Path, args.FileContents);
                        }
                    }
                }
            }
            finally
            {
                _fileLock.ExitWriteLock();
                for(var index = _filesToDelete.Count-1; index > 0; index--)
                {
                    var name = _filesToDelete[index];
                    RemoveTmpFile(name);
                }
            }
            return result;
        }

        string MoveTmpFileToDestination(IActivityIOOperationsEndPoint dst, string tmp, string result)
        {
            using(Stream s = new MemoryStream(File.ReadAllBytes(tmp)))
            {
                Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(true);

                //MO : 22-05-2012 : If the file doesnt exist then create the file
                if(!dst.PathExist(dst.IOPath))
                {
                    CreateEndPoint(dst, newArgs, true);
                }
                if(dst.Put(s, dst.IOPath, newArgs, null, _filesToDelete) < 0)
                {
                    result = ResultBad;
                }
            }
            return result;
        }

        static void AppendToTemp(Stream originalFileStream, string temp)
        {
            const int BufferSize = 1024 * 1024;
            var buffer = new char[BufferSize];

            using(var writer = new StreamWriter(temp, true))
            {
                using(var reader = new StreamReader(originalFileStream))
                {
                    int bytesRead;
                    while((bytesRead = reader.ReadBlock(buffer, 0, BufferSize)) != 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public string Delete(IActivityIOOperationsEndPoint src)
        {
            var result = ResultOk;
            try
            {
                if(!src.Delete(src.IOPath))
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
            if(readTypes == ReadTypes.FilesAndFolders)
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
                ValidateEndPoint(dst, args);
                result = CreateEndPoint(dst, args, createToFile);
            }
            finally
            {
                _filesToDelete.ForEach(RemoveTmpFile);
            }
            return result;
        }

        public string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                             Dev2CRUDOperationTO args)
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

        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                           Dev2CRUDOperationTO args)
        {
            string status;
            try
            {
                status = ValidateCopySourceDestinationFileOperation(src, dst, args, () =>
                    {
                        if(src.RequiresLocalTmpStorage())
                        {
                            if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                            {
                                dst.IOPath.Path = dst.Combine(GetFileNameFromEndPoint(src));
                            }

                            using(var s = src.Get(src.IOPath, _filesToDelete))
                            {

                                // for flips sake quite putting short-hand notation in-line it causes bugs!!! ;)
                                dst.Put(s, dst.IOPath, args, Path.IsPathRooted(src.IOPath.Path) ? Path.GetDirectoryName(src.IOPath.Path) : null, _filesToDelete);
                                s.Close();
                                s.Dispose();
                            }
                        }
                        else
                        {
                            var sourceFile = new FileInfo(src.IOPath.Path);
                            if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                            {
                                dst.IOPath.Path = dst.Combine(sourceFile.Name);
                            }

                            using(var s = src.Get(src.IOPath, _filesToDelete))
                            {
                                if(sourceFile.Directory != null)
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

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                           Dev2CRUDOperationTO args)
        {
            string result;

            try
            {
                result = Copy(src, dst, args);

                if(result.Equals("Success"))
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

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                            Dev2UnZipOperationTO args)
        {
            string status;

            try
            {
                status = ValidateUnzipSourceDestinationFileOperation(src, dst, args, () =>
                    {
                        ZipFile zip;
                        var tempFile = "";

                        if(src.RequiresLocalTmpStorage())
                        {
                            var tmpZip = CreateTmpFile();
                            using(var s = src.Get(src.IOPath, _filesToDelete))
                            {
                                File.WriteAllBytes(tmpZip, s.ToByteArray());
                            }

                            tempFile = tmpZip;
                            zip = ZipFile.Read(tempFile);
                        }
                        else
                        {
                            zip = ZipFile.Read(src.Get(src.IOPath, _filesToDelete));
                        }

                        if(dst.RequiresLocalTmpStorage())
                        {
                            // unzip locally then Put the contents of the archive to the dst end-point
                            var tempPath = CreateTmpDirectory();
                            ExtractFile(args, zip, tempPath);
                            var endPointPath = ActivityIOFactory.CreatePathFromString(tempPath, "", "");
                            var endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(endPointPath);
                            Move(endPoint, dst, new Dev2CRUDOperationTO(args.Overwrite));
                        }
                        else
                        {
                            ExtractFile(args, zip, dst.IOPath.Path);
                        }

                        if(src.RequiresLocalTmpStorage())
                        {
                            File.Delete(tempFile);
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

        static void ExtractFile(Dev2UnZipOperationTO args, ZipFile zip, string extractFromPath)
        {
            if(zip != null)
            {
                using(zip)
                {
                    if(!string.IsNullOrEmpty(args.ArchivePassword))
                    {
                        zip.Password = args.ArchivePassword;
                    }

                    foreach(var ze in zip)
                    {
                        try
                        {
                            ze.Extract(extractFromPath,
                                       args.Overwrite
                                           ? ExtractExistingFileAction.OverwriteSilently
                                           : ExtractExistingFileAction.DoNotOverwrite);
                        }
                        catch(BadPasswordException bpe)
                        {
                            throw new Exception("Invalid archive password", bpe);
                        }
                    }
                }
            }
        }

        #region Private Methods

        string CreateEndPoint(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
        {
            var result = ResultOk;
            // check the the dir strucutre exist
            var activityIOPath = dst.IOPath;
            var dirParts = MakeDirectoryParts(activityIOPath, dst.PathSeperator());

            // check from lowest path part up
            var deepestIndex = -1;
            var startDepth = (dirParts.Count - 1);

            var pos = startDepth;

            while(pos >= 0 && deepestIndex == -1)
            {
                var tmpPath = ActivityIOFactory.CreatePathFromString(dirParts[pos], activityIOPath.Username,
                                                                                 activityIOPath.Password, true);
                try
                {
                    if(dst.ListDirectory(tmpPath) != null)
                    {
                        deepestIndex = pos;
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch(Exception)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    //Note that we doing a recursive create should the directory not exists
                }
                finally
                {
                    pos--;
                }
            }

            // now create all the directories we need ;)
            pos = (deepestIndex + 1);
            var ok = true;

            var origPath = dst.IOPath;

            while(pos <= startDepth && ok)
            {
                var toCreate = ActivityIOFactory.CreatePathFromString(dirParts[pos], dst.IOPath.Username,
                                                                                  dst.IOPath.Password, true);
                dst.IOPath = toCreate;
                ok = CreateDirectory(dst, args);
                pos++;
            }

            dst.IOPath = origPath;

            // dir create failed
            if(!ok)
            {
                result = ResultBad;
            }
            else if(dst.PathIs(dst.IOPath) == enPathType.File && createToFile)
            {
                if(!CreateFile(dst, args))
                {
                    result = ResultBad;
                }
            }

            return result;
        }

        bool IsBase64(string payload)
        {
            return payload.StartsWith("Content-Type:BASE64");
        }

        IList<string> MakeDirectoryParts(IActivityIOPath path, string splitter)
        {
            string[] tmp;

            IList<string> result = new List<string>();

            var splitOn = splitter.ToCharArray();

            if(IsNotFtpTypePath(path))
            {
                tmp = path.Path.Split(splitOn);

            }
            else
            {
                var splitValues = path.Path.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                splitValues.RemoveAt(0);
                var newPath = string.Join("/", splitValues);
                tmp = newPath.Split(splitOn);
            }

            // remove trailing file entry if exist
            var candiate = tmp[tmp.Length - 1];
            var len = tmp.Length;
            if(candiate.Contains("*.") || candiate.Contains("."))
            {
                len = (tmp.Length - 1);
            }

            var builderPath = "";
            // build up URI parts from root down
            for(var i = 0; i < len; i++)
            {
                if(!string.IsNullOrWhiteSpace(tmp[i]))
                {
                    builderPath += tmp[i] + splitter;
                    if(!IsNotFtpTypePath(path) && !builderPath.Contains("://"))
                    {
                        var splitValues = path.Path.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        builderPath = splitValues[0] + "://" + builderPath;
                    }
                    result.Add(IsUncFileTypePath(path) ? @"\\" + builderPath : builderPath);
                }
            }
            return result;
        }

        bool CreateDirectory(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            var result = dst.CreateDirectory(dst.IOPath, args);
            return result;
        }

        bool CreateFile(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {

            var result = true;

            var tmp = CreateTmpFile();
            using(Stream s = new MemoryStream(File.ReadAllBytes(tmp)))
            {

                if(dst.Put(s, dst.IOPath, args, null, _filesToDelete) < 0)
                {
                    result = false;
                }

                s.Close();
            }

            return result;
        }

        Ionic.Zlib.CompressionLevel ExtractZipCompressionLevel(string lvl)
        {
            var lvls = Enum.GetValues(typeof(Ionic.Zlib.CompressionLevel));
            var pos = 0;
            //19.09.2012: massimo.guerrera - Changed to default instead of none
            Ionic.Zlib.CompressionLevel clvl = Ionic.Zlib.CompressionLevel.Default;

            while(pos < lvls.Length && lvls.GetValue(pos).ToString() != lvl)
            {
                pos++;
            }

            if(pos < lvls.Length)
            {
                clvl = (Ionic.Zlib.CompressionLevel)lvls.GetValue(pos);
            }

            return clvl;
        }

        /// <summary>
        /// Transfer the contents of the directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        bool TransferDirectoryContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                                               Dev2CRUDOperationTO args)
        {
            ValidateSourceAndDestinationContents(src, dst, args);

            if(args.DoRecursiveCopy)
            {
                RecursiveCopy(src, dst, args);
            }

            var srcContents = src.ListFilesInDirectory(src.IOPath);
            var result = true;
            var origDstPath = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(dst.IOPath.Path);

            if(!dst.PathExist(dst.IOPath))
            {
                CreateDirectory(dst, args);
            }

            // get each file, then put it to the correct location
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var p in srcContents)
            {
                result = PerformTransfer(src, dst, args, origDstPath, p, result);
            }
            Dev2Logger.Log.Debug(string.Format("Transfered: {0}", src.IOPath.Path));
            return result;
        }

        static bool PerformTransfer(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, string origDstPath, IActivityIOPath p, bool result)
        {
            try
            {
                if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                {
                    var cpPath =
                        ActivityIOFactory.CreatePathFromString(
                            string.Format("{0}{1}{2}", origDstPath, dst.PathSeperator(),
                                (Dev2ActivityIOPathUtils.ExtractFileName(p.Path))),
                            dst.IOPath.Username,
                            dst.IOPath.Password, true);
                    var path = cpPath.Path;
                    DoFileTransfer(src, dst, args, cpPath, p, path, ref result);
                }
                else if(args.Overwrite || !dst.PathExist(dst.IOPath))
                {
                    var tmp = origDstPath + "\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);
                    var path = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password);
                    DoFileTransfer(src, dst, args, path, p, path.Path, ref result);
                }
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(ex);
            }
            return result;
        }

        static void DoFileTransfer(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, IActivityIOPath dstPath, IActivityIOPath p, string path, ref bool result)
        {
            if(args.Overwrite || !dst.PathExist(dstPath))
            {
                result = TransferFile(src, dst, args, path, p, result);
            }
        }

        static bool TransferFile(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, string path, IActivityIOPath p, bool result)
        {
            var tmpPath = ActivityIOFactory.CreatePathFromString(path, dst.IOPath.Username, dst.IOPath.Password, true);
            var tmpEp = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
            var whereToPut = GetWhereToPut(src, dst);
            using(var s = src.Get(p, _filesToDelete))
            {
                if(tmpEp.Put(s, tmpEp.IOPath, args, whereToPut, _filesToDelete) < 0)
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
                // List directory contents
                var srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                Task.WaitAll(srcContentsFolders.Select(sourcePath => Task.Run(() =>
                    {
                        var sourceEndPoint =
                            ActivityIOFactory.CreateOperationEndPointFromIOPath(sourcePath);
                        IList<string> dirParts =
                            sourceEndPoint.IOPath.Path.Split(sourceEndPoint.PathSeperator().ToCharArray(),
                                StringSplitOptions.RemoveEmptyEntries);
                        var destinationPath =
                            ActivityIOFactory.CreatePathFromString(dst.Combine(dirParts.Last()), dst.IOPath.Username,
                                dst.IOPath.Password, true);
                        var destinationEndPoint =
                            ActivityIOFactory.CreateOperationEndPointFromIOPath(destinationPath);
                        dst.CreateDirectory(destinationPath, args);
                        TransferDirectoryContents(sourceEndPoint, destinationEndPoint, args);
                    })).ToArray());
            }
            catch(AggregateException e)
            {
                var message = e.InnerExceptions.Where(exception => exception != null && !string.IsNullOrEmpty(exception.Message)).Aggregate("", (current, exception) => current + (exception.Message + "\r\n"));
                throw new Exception(message, e);
            }
        }

        /// <summary>
        /// Transfer the contents of the directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        void ValidateSourceAndDestinationContents(IActivityIOOperationsEndPoint src,
                                                          IActivityIOOperationsEndPoint dst,
                                                          Dev2CRUDOperationTO args)
        {
            if(!args.Overwrite)
            {
                var srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                foreach(var sourcePath in srcContentsFolders)
                {
                    var sourceEndPoint =
                        ActivityIOFactory.CreateOperationEndPointFromIOPath(sourcePath);

                    IList<string> dirParts =
                        sourceEndPoint.IOPath.Path.Split(sourceEndPoint.PathSeperator().ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries);
                    var directory = dirParts.Last();
                    var destinationPath =
                        ActivityIOFactory.CreatePathFromString(dst.Combine(directory),
                                                               dst.IOPath.Username,
                                                               dst.IOPath.Password, true);

                    var destinationEndPoint =
                        ActivityIOFactory.CreateOperationEndPointFromIOPath(destinationPath);

                    if(destinationEndPoint.PathExist(destinationEndPoint.IOPath))
                    {
                        ValidateSourceAndDestinationContents(sourceEndPoint, destinationEndPoint, args);
                    }
                }


                var srcContents = src.ListFilesInDirectory(src.IOPath);
                var dstContents = dst.ListFilesInDirectory(dst.IOPath);

                var sourceFileNames = srcContents.Select(srcFile => GetFileNameFromEndPoint(src, srcFile)).ToList();
                var destinationFileNames = dstContents.Select(dstFile => GetFileNameFromEndPoint(dst, dstFile)).ToList();

                if(destinationFileNames.Count > 0)
                {
                    var commonFiles = sourceFileNames.Where(destinationFileNames.Contains).ToList();

                    if(commonFiles.Count > 0)
                    {
                        var fileNames = commonFiles.Aggregate("",
                                                                 (current, commonFile) =>
                                                                 current + ("\r\n" + commonFile));
                        throw new Exception(
                            "The following file(s) exist in the destination folder and overwrite is set to false:- " +
                            fileNames);
                    }
                }
            }
        }

        static string GetWhereToPut(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
        {
            if(src.IOPath.PathType == enActivityIOPathType.FileSystem)
            {
                // some silly chicken is not getting the directory correctly ?!
                return Path.GetDirectoryName(src.IOPath.Path);
            }
            if(dst.IOPath.PathType == enActivityIOPathType.FileSystem)
            {
                return Path.GetDirectoryName(src.IOPath.Path);
            }
            return null; // this means that neither the src or destination where local files
        }

        /// <summary>
        /// Creates a tmp file
        /// </summary>
        /// <returns></returns>
        string CreateTmpFile()
        {
            try
            {
                var tmpFile = Path.GetTempFileName();
                _filesToDelete.Add(tmpFile);
                return tmpFile;
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }

        }

        /// <summary>
        /// Remove a tmp file
        /// </summary>
        /// <param name="path"></param>
        void RemoveTmpFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }

        }

        string CreateTmpDirectory()
        {
            try
            {
                var tmpDir = Path.GetTempPath();
                var di = Directory.CreateDirectory(tmpDir + "\\" + Guid.NewGuid());

                return (di.FullName);
            }
            catch(Exception err)
            {
                Dev2Logger.Log.Error(err);
                throw;
            }

        }

        static void EnsureFilesDontExists(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
        {
            if(dst.PathExist(dst.IOPath))
            {
                // destination is a file
                if(dst.PathIs(dst.IOPath) == enPathType.File)
                {
                    throw new Exception(
                        "A file with the same name exists on the destination and overwrite is set to false");
                }

                //destination is a folder
                var dstContents = dst.ListDirectory(dst.IOPath);
                var destinationFileNames = dstContents.Select(dstFile => GetFileNameFromEndPoint(dst, dstFile));
                var sourceFile = GetFileNameFromEndPoint(src);

                if(destinationFileNames.Contains(sourceFile))
                {
                    throw new Exception(
                        "The following file(s) exist in the destination folder and overwrite is set to false :- " +
                        sourceFile);
                }
            }
        }

        static string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint)
        {
            string pathSeperator = endPoint.PathSeperator();
            return endPoint.IOPath.Path.Split(pathSeperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }

        static string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint, IActivityIOPath path)
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

                    if(src.PathIs(src.IOPath) == enPathType.Directory || Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
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
            // normal not wild char file && not directory
            var packFile = src.IOPath.Path;
            var tempFileName = CreateTmpFile();

            if(src.RequiresLocalTmpStorage())
            {
                string tempDir = CreateTmpDirectory();
                var tmpFile = Path.Combine(tempDir,
                                           src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                                 StringSplitOptions.RemoveEmptyEntries)
                                              .Last());
                packFile = tmpFile;
                using(var s = src.Get(src.IOPath, _filesToDelete))
                {
                    File.WriteAllBytes(tmpFile, s.ToByteArray());
                }
            }

            using(var zip = new ZipFile())
            {
                // set password if exist
                if(args.ArchivePassword != string.Empty)
                {
                    zip.Password = args.ArchivePassword;
                }
                // compression ratio
                zip.CompressionLevel = ExtractZipCompressionLevel(args.CompressionRatio);
                // add all files to archive
                zip.AddFile(packFile, ".");
                zip.Save(tempFileName);
            }

            return tempFileName;
        }

        string ZipDirectoryToALocalTempFile(IActivityIOOperationsEndPoint src, Dev2ZipOperationTO args)
        {
            // tmp dir for files required
            var tmpDir = CreateTmpDirectory();
            var tempFilename = CreateTmpFile();
            var tmpPath = ActivityIOFactory.CreatePathFromString(tmpDir, "", "");
            var tmpEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);

            // stage contents to local folder
            TransferDirectoryContents(src, tmpEndPoint, new Dev2CRUDOperationTO(true));
            using(var zip = new ZipFile())
            {
                zip.SaveProgress += (sender, eventArgs) =>
                {
                    if(eventArgs.CurrentEntry != null)
                    {
                        Dev2Logger.Log.Debug(string.Format("Event Type: {0} Total Entries: {1} Entries Saved: {2} Current Entry: {3}", eventArgs.EventType, eventArgs.EntriesTotal, eventArgs.EntriesSaved,  eventArgs.CurrentEntry.FileName));
                    }
                };
                // set password if exist
                if(args.ArchivePassword != string.Empty)
                {
                    zip.Password = args.ArchivePassword;
                }

                // compression ratio                    
                zip.CompressionLevel = ExtractZipCompressionLevel(args.CompressionRatio);

                var toAdd = ListDirectory(tmpEndPoint, ReadTypes.FilesAndFolders);
                // add all files to archive
                foreach(var p in toAdd)
                {
                    if(tmpEndPoint.PathIs(p) == enPathType.Directory)
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

            // remove locally staged files
            DirectoryHelper.CleanUp(tmpDir);

            return tempFilename;
        }

        string TransferTempZipFileToDestination(IActivityIOOperationsEndPoint src,
                                                        IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args,
                                                        string tmpZip)
        {
            // now transfer the zip file to the correct location
            string result;
            using(Stream s2 = new MemoryStream(File.ReadAllBytes(tmpZip)))
            {
                // add archive name to path
                dst =
                    ActivityIOFactory.CreateOperationEndPointFromIOPath(
                        ActivityIOFactory.CreatePathFromString(dst.IOPath.Path, dst.IOPath.Username,
                                                               dst.IOPath.Password, true));

                var zipTransferArgs = new Dev2CRUDOperationTO(args.Overwrite);

                result = ResultOk;

                if(src.RequiresLocalTmpStorage())
                {
                    if(dst.Put(s2, dst.IOPath, zipTransferArgs, null, _filesToDelete) < 0)
                    {
                        result = ResultBad;
                    }
                }
                else
                {
                    var fileInfo = new FileInfo(src.IOPath.Path);
                    if(fileInfo.Directory != null && Path.IsPathRooted(fileInfo.Directory.ToString()))
                    {
                        if(dst.Put(s2, dst.IOPath, zipTransferArgs, fileInfo.Directory.ToString(), _filesToDelete) < 0)
                        {
                            result = ResultBad;
                        }
                    }
                    else
                    {
                        if(dst.Put(s2, dst.IOPath, zipTransferArgs, null, _filesToDelete) < 0)
                        {
                            result = ResultBad;
                        }
                    }
                }
            }
            return result;
        }

        static bool IsNotFtpTypePath(IActivityIOPath src)
        {
            return !src.Path.StartsWith("ftp://") && !src.Path.StartsWith("ftps://") && !src.Path.StartsWith("sftp://");
        }

        static bool IsUncFileTypePath(IActivityIOPath src)
        {
            return src.Path.StartsWith(@"\\");
        }

        #endregion

        #region Validations

        string ValidateRenameSourceAndDesinationTypes(IActivityIOOperationsEndPoint src,
                                                              IActivityIOOperationsEndPoint dst,
                                                              Dev2CRUDOperationTO args)
        {
            //ensures that the source and destination locations are of the same type
            if(src.PathIs(src.IOPath) != dst.PathIs(dst.IOPath))
            {
                throw new Exception("Source and destination need to be both files or directories");
            }

            //Rename Tool if the file/folder exists then delete it and put the source there
            if(dst.PathExist(dst.IOPath))
            {
                if(!args.Overwrite)
                {
                    throw new Exception("Destination directory already exists and overwrite is set to false");
                }

                //Clear the existing folder
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
            ValidateSourceAndDestinationPaths(src, dst);

            //ensures destination folder structure exists
            var opStatus = CreateEndPoint(dst, args, dst.PathIs(dst.IOPath) == enPathType.Directory);
            if(!opStatus.Equals("Success"))
            {
                throw new Exception("Recursive Directory Create Failed For [ " + dst.IOPath.Path + " ]");
            }

            //transfer contents to destination when the source is a directory
            if(src.PathIs(src.IOPath) == enPathType.Directory)
            {
                if(!TransferDirectoryContents(src, dst, args))
                {
                    result = ResultBad;
                }
            }
            else
            {
                if(!args.Overwrite)
                {
                    EnsureFilesDontExists(src, dst);
                }

                if(!Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                {
                    return performAfterValidation();
                }

                // we have star wild cards to deal with
                if(!TransferDirectoryContents(src, dst, args))
                {
                    result = ResultBad;
                }
            }

            return result;
        }

        string ValidateUnzipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                   IActivityIOOperationsEndPoint dst,
                                                                   Dev2UnZipOperationTO args,
                                                                   Func<string> performAfterValidation)
        {
            ValidateSourceAndDestinationPaths(src, dst);

            if(dst.PathIs(dst.IOPath) != enPathType.Directory)
            {
                throw new Exception("Destination must be a directory");
            }

            if(src.PathIs(src.IOPath) != enPathType.File)
            {
                throw new Exception("Source must be a file");
            }

            if(!args.Overwrite && dst.PathExist(dst.IOPath))
            {
                throw new Exception("Destination directory already exists and overwrite is set to false");
            }

            return performAfterValidation();
        }

        string ValidateZipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                 IActivityIOOperationsEndPoint dst,
                                                                 Dev2ZipOperationTO args,
                                                                 Func<string> performAfterValidation)
        {
            AddMissingFileDirectoryParts(src, dst);


            if(dst.PathIs(dst.IOPath) == enPathType.Directory)
            {
                var sourcePart =
                    src.IOPath.Path.Split(src.PathSeperator().ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                       .Last();
                if(src.PathIs(src.IOPath) == enPathType.File)
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

            if(!args.Overwrite && dst.PathExist(dst.IOPath))
            {
                throw new Exception("Destination file already exists and overwrite is set to false");
            }

            //ensures destination folder structure exists
            var opStatus = CreateEndPoint(dst, new Dev2CRUDOperationTO(args.Overwrite),
                                             dst.PathIs(dst.IOPath) == enPathType.Directory);
            if(!opStatus.Equals("Success"))
            {
                throw new Exception("Recursive Directory Create Failed For [ " + dst.IOPath.Path + " ]");
            }

            return performAfterValidation();
        }

        void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,
                                                  IActivityIOOperationsEndPoint dst)
        {
            if(src.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception("Source can not be an empty string");
            }
            var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                    StringSplitOptions.RemoveEmptyEntries).ToList();

            if(dst.IOPath.Path.Trim().Length == 0)
            {
                dst.IOPath.Path = src.IOPath.Path;
            }
            else
            {
                if(!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
                {
                    var lastPart = sourceParts.Last();
                    dst.IOPath.Path =
                        Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                         ? src.IOPath.Path
                                         : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                }
            }
            var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries).ToList();

            while(destinationParts.Count > sourceParts.Count)
            {
                destinationParts.Remove(destinationParts.Last());
            }

            if(destinationParts.OrderBy(i => i).SequenceEqual(sourceParts.OrderBy(i => i)))
            {
                if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                {
                    var strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                             StringSplitOptions.RemoveEmptyEntries);
                    var lastPart = strings.Last();
                    dst.IOPath.Path = src.PathIs(src.IOPath) == enPathType.Directory
                                          ? Path.Combine(dst.IOPath.Path, lastPart)
                                          : dst.IOPath.Path.Replace(lastPart, "");
                }
            }
            else
            {
                if(dst.PathIs(dst.IOPath) == enPathType.Directory && src.PathIs(src.IOPath) == enPathType.Directory)
                {
                    var strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                             StringSplitOptions.RemoveEmptyEntries);
                    var lastPart = strings.Last();
                    dst.IOPath.Path = dst.Combine(lastPart);
                }
            }
        }

        void ValidateSourceAndDestinationPaths(IActivityIOOperationsEndPoint src,
                                                       IActivityIOOperationsEndPoint dst)
        {
            if(src.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception("Source can not be an empty string");
            }

            var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            if(dst.IOPath.Path.Trim().Length == 0)
            {
                dst.IOPath.Path = src.IOPath.Path;
            }
            else
            {
                if(!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
                {

                    var lastPart = sourceParts.Last();

                    dst.IOPath.Path =
                        Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                         ? src.IOPath.Path
                                         : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                }
            }

            var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            while(destinationParts.Count > sourceParts.Count)
            {
                destinationParts.Remove(destinationParts.Last());
            }

            if(destinationParts.OrderBy(i => i).SequenceEqual(
                 sourceParts.OrderBy(i => i)))
            {
                throw new Exception("Destination directory can not be a child of the source directory");
            }
        }

        void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, Dev2CRUDOperationTO args)
        {
            if(endPoint.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception("Source can not be an empty string");
            }

            if(endPoint.PathExist(endPoint.IOPath) && !args.Overwrite)
            {
                var type = endPoint.PathIs(endPoint.IOPath) == enPathType.Directory ? "Directory" : "File";
                throw new Exception(string.Format("Destination {0} already exists and overwrite is set to false",
                                                  type));
            }
        }

        #endregion
    }
}

