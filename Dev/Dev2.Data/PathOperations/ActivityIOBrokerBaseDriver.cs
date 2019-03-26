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
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;

namespace Dev2.PathOperations
{
    public interface IActivityIOBrokerDriver
    {
        void RemoveAllTmpFiles();

        bool CreateDirectory(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args);
        IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src, ReadTypes readTypes);
        string CreateEndPoint(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, bool createToFile);
        string CreateTmpFile();
        string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint);
        string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint, IActivityIOPath path);
        void RemoveTmpFile(string path);
    }
    internal class ActivityIOBrokerBaseDriver : IActivityIOBrokerDriver
    {
        public const string ResultOk = @"Success";
        public const string ResultBad = @"Failure";

        protected readonly IFile _fileWrapper;
        protected readonly ICommon _common;
        protected readonly List<string> _filesToDelete = new List<string>();
        public void RemoveAllTmpFiles()
        {
            _filesToDelete.ForEach(RemoveTmpFile);
        }

        internal ActivityIOBrokerBaseDriver()
            : this(new FileWrapper(), new CommonDataUtils())
        {
        }

        internal ActivityIOBrokerBaseDriver(IFile file, ICommon common)
        {
            _fileWrapper = file;
            _common = common;
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src, ReadTypes readTypes)
        {
            if (readTypes == ReadTypes.FilesAndFolders)
            {
                return src.ListDirectory(src.IOPath);
            }
            return readTypes == ReadTypes.Files ? src.ListFilesInDirectory(src.IOPath) : src.ListFoldersInDirectory(src.IOPath);
        }

        public string CreateEndPoint(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, bool createToFile)
        {
            var activityIOPath = dst.IOPath;
            var dirParts = MakeDirectoryParts(activityIOPath, dst.PathSeperator());

            var deepestIndex = -1;
            var startDepth = dirParts.Count - 1;

            var pos = startDepth;

            while (pos >= 0 && deepestIndex == -1)
            {
                var tmpPath = ActivityIOFactory.CreatePathFromString(dirParts[pos], activityIOPath.Username,
                                                                                 activityIOPath.Password, true, activityIOPath.PrivateKeyFile);
                if (!dst.PathExist(tmpPath))
                {
                    break;
                }
                try
                {
                    if (dst.ListDirectory(tmpPath) != null)
                    {
                        deepestIndex = pos;
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Warn(e.Message, "Warewolf Warn");
                    throw;
                }
                finally
                {
                    pos--;
                }
            }

            var ok = CreateDirectoriesForPath(dst, args, dirParts, deepestIndex, startDepth);
            if (!ok)
            {
                return ResultBad;
            }



            var shouldCreateFile = dst.PathIs(dst.IOPath) == enPathType.File && createToFile;
            if (shouldCreateFile)
            {
                if (CreateFile(dst, args))
                {
                    return ResultOk;
                }
            }
            else
            {
                return ResultOk;
            }


            return ResultBad;
        }

        private bool CreateDirectoriesForPath(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, IList<string> dirParts, int deepestIndex, int startDepth)
        {
            var pos = deepestIndex + 1;
            var origPath = dst.IOPath;
            try
            {
                while (pos <= startDepth)
                {
                    var toCreate = ActivityIOFactory.CreatePathFromString(dirParts[pos], dst.IOPath.Username,
                                                                                      dst.IOPath.Password, true, dst.IOPath.PrivateKeyFile);

                    if (dst.PathExist(toCreate))
                    {
                        pos++;
                        continue;
                    }
                    dst.IOPath = toCreate;
                    if (!CreateDirectory(dst, args))
                    {
                        return false;
                    }
                    pos++;
                }
            }
            finally
            {
                dst.IOPath = origPath;
            }
            return true;
        }

        static IList<string> MakeDirectoryParts(IActivityIOPath path, string splitter)
        {
            string[] tmp;

            IList<string> result = new List<string>();

            var splitOn = splitter.ToCharArray();

            if (CommonDataUtils.IsNotFtpTypePath(path))
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

            var candiate = tmp[tmp.Length - 1];
            var len = tmp.Length;
            if (candiate.Contains(@"*.") || candiate.Contains(@"."))
            {
                len = tmp.Length - 1;
            }

            var builderPath = string.Empty;
            for (var i = 0; i < len; i++)
            {
                if (!string.IsNullOrWhiteSpace(tmp[i]))
                {
                    builderPath += tmp[i] + splitter;
                    if (!CommonDataUtils.IsNotFtpTypePath(path) && !builderPath.Contains(@"://"))
                    {
                        var splitValues = path.Path.Split(new[] { @"://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        builderPath = splitValues[0] + @"://" + builderPath;
                    }
                    result.Add(CommonDataUtils.IsUncFileTypePath(path.Path) ? @"\\" + builderPath : builderPath);
                }
            }
            return result;
        }

        bool CreateFile(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {

            var result = false;

            var tmp = CreateTmpFile();
            using (Stream s = new MemoryStream(_fileWrapper.ReadAllBytes(tmp)))
            {

                if (dst.Put(s, dst.IOPath, args, null, _filesToDelete) >= 0)
                {
                    result = true;
                }

                s.Close();
            }

            return result;
        }

        public bool CreateDirectory(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            var result = dst.CreateDirectory(dst.IOPath, args);
            return result;
        }

        public string CreateTmpFile()
        {
            try
            {
                var tmpFile = Path.GetTempFileName();
                _filesToDelete.Add(tmpFile);
                return tmpFile;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint)
        {
            var pathSeperator = endPoint.PathSeperator();
            return endPoint.IOPath.Path.Split(pathSeperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }

        public string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint, IActivityIOPath path)
        {
            var pathSeperator = endPoint.PathSeperator();
            return path.Path.Split(pathSeperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }
        public void RemoveTmpFile(string path)
        {
            try
            {
                _fileWrapper.Delete(path);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }
    }
}
