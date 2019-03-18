#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Extension;
using Warewolf.Resource.Errors;

namespace Dev2.PathOperations
{
    public interface IActivityIOBrokerValidatorDriver
    {
        string ValidateCopySourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                  IActivityIOOperationsEndPoint dst,
                                                                  IDev2CRUDOperationTO args,
                                                                  Func<string> performAfterValidation);
        string ValidateUnzipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                   IActivityIOOperationsEndPoint dst,
                                                                   IDev2UnZipOperationTO args,
                                                                   Func<string> performAfterValidation);
        string ValidateZipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                 IActivityIOOperationsEndPoint dst,
                                                                 IDev2ZipOperationTO args,
                                                                 Func<string> performAfterValidation);

    }
    internal class ActivityIOBrokerValidatorDriver : ActivityIOBrokerMainDriver, IActivityIOBrokerValidatorDriver
    {
        public string ValidateCopySourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                  IActivityIOOperationsEndPoint dst,
                                                                  IDev2CRUDOperationTO args,
                                                                  Func<string> performAfterValidation)
        {
            var result = ActivityIOBrokerBaseDriver.ResultOk;
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
                    result = ActivityIOBrokerBaseDriver.ResultBad;
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
                    return performAfterValidation?.Invoke();
                }
                if (!TransferDirectoryContents(src, dst, args))
                {
                    result = ActivityIOBrokerBaseDriver.ResultBad;
                }
            }

            return result;
        }

        public string ValidateUnzipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                   IActivityIOOperationsEndPoint dst,
                                                                   IDev2UnZipOperationTO args,
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

            return performAfterValidation?.Invoke();
        }

        public string ValidateZipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                 IActivityIOOperationsEndPoint dst,
                                                                 IDev2ZipOperationTO args,
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
            if (!opStatus.Equals(ActivityIOBrokerBaseDriver.ResultOk))
            {
                throw new Exception(string.Format(ErrorResource.RecursiveDirectoryCreateFailed, dst.IOPath.Path));
            }
            return performAfterValidation?.Invoke();
        }

        void EnsureFilesDontExists(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
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
    }
}
