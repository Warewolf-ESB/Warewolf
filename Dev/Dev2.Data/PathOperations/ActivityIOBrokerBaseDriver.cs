#pragma warning disable
 /*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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

            var ok = CreateDirectoriesForPath(dst, args, dirParts);
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

        private bool CreateDirectoriesForPath(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, IList<string> dirParts)
        {
            var maxDepth = dirParts.Count - 1;
            var pos = 0;
            var origPath = dst.IOPath;
            try
            {
                while (pos <= maxDepth)
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
            var builderPath = new System.Text.StringBuilder();

            var splitOn = splitter.ToCharArray();

            if (CommonDataUtils.IsUncFileTypePath(path.Path))
            {
                var uncPath = path.Path.Substring(2);
                var splitValues = uncPath.Split(splitOn).ToList();
                builderPath.Append(@"\\");
                builderPath.Append(splitValues[0]);
                builderPath.Append(@"\");
                splitValues.RemoveAt(0);
                tmp = splitValues.ToArray();
            }
            else if (CommonDataUtils.IsNotFtpTypePath(path))
            {
                tmp = path.Path.Split(splitOn);
            }
            else
            {
                var splitValues = path.Path.Split(new[] { @"://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                builderPath.Append(splitValues[0]);
                builderPath.Append(@"://");
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

            for (var i = 0; i < len; i++)
            {
                if (!string.IsNullOrWhiteSpace(tmp[i]))
                {
                    builderPath.Append(tmp[i]);
                    builderPath.Append(splitter);

                    result.Add(builderPath.ToString());
                }
            }
            return result;
        }

        bool CreateFile(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            // TODO: why create a tmp file here, surely we can just use an empty array as input to the memory stream?
            var tmp = CreateTmpFile();
            using (Stream s = new MemoryStream(_fileWrapper.ReadAllBytes(tmp)))
            {
                try
                {
                    if (dst.Put(s, dst.IOPath, args, null, _filesToDelete) >= 0)
                    {
                        return true;
                    }
                }
                finally {
                    s.Close();
                }
            }

            return false;
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
