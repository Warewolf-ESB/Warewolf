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
using System.Text;
using System.Threading;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Extension;
using Warewolf.Resource.Errors;

namespace Dev2.PathOperations
{
    class Dev2ActivityIOBroker : IActivityOperationsBroker
    {
        readonly IFile _fileWrapper;
        readonly ICommon _common;
        static readonly ReaderWriterLockSlim FileLock = new ReaderWriterLockSlim();
        readonly List<string> _filesToDelete;
        void RemoveAllTmpFiles()
        {
            _implementation.RemoveAllTmpFiles();
            _filesToDelete.ForEach(_implementation.RemoveTmpFile);
        }

        readonly IActivityIOBrokerMainDriver _implementation;
        readonly IActivityIOBrokerValidatorDriver _validator;
        readonly IIonicZipFileWrapperFactory _zipFileFactory;

        public Dev2ActivityIOBroker()
            : this(new FileWrapper(), new Data.Util.CommonDataUtils())
        {
        }

        public Dev2ActivityIOBroker(IFile fileWrapper, ICommon common)
            :this(fileWrapper, common, new ActivityIOBrokerMainDriver(), new ActivityIOBrokerValidatorDriver(), new IonicZipFileWrapperFactory())
        {
        }

        public Dev2ActivityIOBroker(IFile fileWrapper, ICommon common, IActivityIOBrokerMainDriver implementation, IActivityIOBrokerValidatorDriver validator, IIonicZipFileWrapperFactory zipFileFactory)
        {
            _implementation = implementation;
            _validator = validator;
            _zipFileFactory = zipFileFactory;

            _fileWrapper = fileWrapper;
            _common = common;
            _filesToDelete = new List<string>();
        }

        public string Get(IActivityIOOperationsEndPoint path) => Get(path, false);
        public byte[] GetBytes(IActivityIOOperationsEndPoint path) => GetBytes(path, false);

        public string Get(IActivityIOOperationsEndPoint path, bool deferredRead)
        {
            try
            {
                return Encoding.UTF8.GetString(GetBytes(path, deferredRead));
            }
            finally
            {
                RemoveAllTmpFiles();
            }
        }
        public byte[] GetBytes(IActivityIOOperationsEndPoint path, bool deferredRead)
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

                return bytes;
            }
            finally
            {
                RemoveAllTmpFiles();
            }
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args)
        {
            string tmpFileName = null;
            try
            {
                FileLock.EnterWriteLock();
                if (dst.RequiresLocalTmpStorage())
                {
                    var tmp = _implementation.CreateTmpFile();
                    _implementation.WriteToLocalTempStorage(dst, args, tmp);
                    return _implementation.MoveTmpFileToDestination(dst, tmp);
                }
                else
                {
                    if (dst.PathExist(dst.IOPath))
                    {
                        tmpFileName = _implementation.CreateTmpFile();
                        return _implementation.WriteToRemoteTempStorage(dst, args, tmpFileName);
                    }
                    else
                    {
                        return CreateEndPointAndWriteData(dst, args);
                    }
                }
            }
            finally
            {
                if (tmpFileName != null)
                {
                    _implementation.RemoveTmpFile(tmpFileName);
                }
                FileLock.ExitWriteLock();
                RemoveAllTmpFiles();
            }
        }

        private string CreateEndPointAndWriteData(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args)
        {
            var newArgs = new Dev2CRUDOperationTO(true);

            var endPointCreated = _implementation.CreateEndPoint(dst, newArgs, true) == ActivityIOBrokerBaseDriver.ResultOk;
            if (endPointCreated)
            {
                return _implementation.WriteDataToFile(args, dst)
                    ? ActivityIOBrokerBaseDriver.ResultOk
                    : ActivityIOBrokerBaseDriver.ResultBad;
            }

            return ActivityIOBrokerBaseDriver.ResultBad;
        }

        public string Delete(IActivityIOOperationsEndPoint src)
        {
            try
            {
                if (!src.Delete(src.IOPath))
                {
                    return ActivityIOBrokerBaseDriver.ResultBad;
                }
            }
            catch
            {
                return ActivityIOBrokerBaseDriver.ResultBad;
            }
            finally
            {
                RemoveAllTmpFiles();
            }
            return ActivityIOBrokerBaseDriver.ResultOk;
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src, ReadTypes readTypes)
        {
            return _implementation.ListDirectory(src, readTypes);
        }

        public string Create(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, bool createToFile)
        {
            try
            {
                _common.ValidateEndPoint(dst, args);
                return _implementation.CreateEndPoint(dst, args, createToFile);
            }
            finally
            {
                RemoveAllTmpFiles();
            }
        }

        public string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            string performRename()
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

            try
            {
                return performRename();
            }
            finally
            {
                RemoveAllTmpFiles();
            }
        }

        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            string status;
            try
            {
                status = _validator.ValidateCopySourceDestinationFileOperation(src, dst, args, () =>
                {
                    if (src.RequiresLocalTmpStorage())
                    {
                        return CopyRequiresLocalTmpStorage(src, dst, args);
                    }
                    else
                    {
                        var sourceFile = _fileWrapper.Info(src.IOPath.Path);
                        if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                        {
                            dst.IOPath.Path = dst.Combine(sourceFile.Name);
                        }

                        using (var s = src.Get(src.IOPath, _filesToDelete))
                        {
                            if (sourceFile.Directory != null)
                            {
                                var result = dst.Put(s, dst.IOPath, args, sourceFile.Directory.ToString(), _filesToDelete);

                                return result == -1 ? ActivityIOBrokerBaseDriver.ResultBad : ActivityIOBrokerBaseDriver.ResultOk;
                            }
                        }
                    }
                    return ActivityIOBrokerBaseDriver.ResultBad;
                });
            }
            finally
            {
                RemoveAllTmpFiles();
            }
            return status;
        }

        private string CopyRequiresLocalTmpStorage(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            if (dst.PathIs(dst.IOPath) == enPathType.Directory)
            {
                dst.IOPath.Path = dst.Combine(_implementation.GetFileNameFromEndPoint(src));
            }

            using (var s = src.Get(src.IOPath, _filesToDelete))
            {
                var result = dst.Put(s, dst.IOPath, args, Path.IsPathRooted(src.IOPath.Path) ? Path.GetDirectoryName(src.IOPath.Path) : null, _filesToDelete);
                s.Close();
                return result == -1 ? ActivityIOBrokerBaseDriver.ResultBad : ActivityIOBrokerBaseDriver.ResultOk;
            }
        }

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            string result;
            try
            {
                result = Copy(src, dst, args);
                if (result.Equals(ActivityIOBrokerBaseDriver.ResultOk))
                {
                    src.Delete(src.IOPath);
                }
            }
            finally
            {
                RemoveAllTmpFiles();
            }

            return result;
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2UnZipOperationTO args)
        {
            string status;

            try
            {
                status = _validator.ValidateUnzipSourceDestinationFileOperation(src, dst, args, () =>
                {
                    IIonicZipFileWrapper zip;
                    var tempFile = string.Empty;

                    if (src.RequiresLocalTmpStorage())
                    {
                        var tmpZip = _implementation.CreateTmpFile();
                        using (var s = src.Get(src.IOPath, _filesToDelete))
                        {
                            _fileWrapper.WriteAllBytes(tmpZip, s.ToByteArray());
                        }

                        tempFile = tmpZip;
                        zip = _zipFileFactory.Read(tempFile);
                    }
                    else
                    {
                        zip = _zipFileFactory.Read(src.Get(src.IOPath, _filesToDelete));
                    }

                    if (dst.RequiresLocalTmpStorage())
                    {
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

                    return ActivityIOBrokerBaseDriver.ResultOk;
                });
            }
            finally
            {
                RemoveAllTmpFiles();
            }

            return status;
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2ZipOperationTO args)
        {
            string status;

            try
            {
                status = _validator.ValidateZipSourceDestinationFileOperation(src, dst, args, () =>
                {
                    string tempFileName;

                    tempFileName = src.PathIs(src.IOPath) == enPathType.Directory || Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path) ? _implementation.ZipDirectoryToALocalTempFile(src, args) : _implementation.ZipFileToALocalTempFile(src, args);

                    return _implementation.TransferTempZipFileToDestination(src, dst, args, tempFileName);
                });
            }
            finally
            {
                RemoveAllTmpFiles();
            }
            return status;
        }
    }
}