#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Reflection;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Warewolf.Resource.Errors;


namespace Dev2.PathOperations

{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a factory for creating ActivityIO objects
    /// </summary>
    public static class ActivityIOFactory
    {

        static IList<Type> _endPoints;
        static readonly object EndPointsLock = new object();
        static IList<IActivityIOOperationsEndPoint> _referenceCheckers;

        public static IActivityIOPath CreatePathFromString(string path, string user, string pass) => CreatePathFromString(path, user, pass, "");


        public static IActivityIOPath CreatePathFromString(string path, string user, string pass, string privateKeyFile) => CreatePathFromString(path, user, pass, false, privateKeyFile);
        public static IActivityIOPath CreatePathFromString(string path, string user, string pass, bool isNotCertVerifiable) => CreatePathFromString(path, user, pass, isNotCertVerifiable, "");
        
        public static IActivityIOPath CreatePathFromString(string path, string user, string pass, bool isNotCertVerifiable, string privateKeyFile)
        {
            VerifyArgument.IsNotNull("path", path);
            var type = Dev2ActivityIOPathUtils.ExtractPathType(path);
            if (type == enActivityIOPathType.Invalid)
            {
                // Default to file system
                type = enActivityIOPathType.FileSystem;
                if (!Path.IsPathRooted(path))
                {
                    throw new IOException(ErrorResource.InvalidPath);
                }
            }

            return new Dev2ActivityIOPath(type, path, user, pass, isNotCertVerifiable, privateKeyFile);
        }

        public static IActivityIOPath CreatePathFromString(string path, bool isNotCertVerifiable) => CreatePathFromString(path, isNotCertVerifiable, "");

        public static IActivityIOPath CreatePathFromString(string path, bool isNotCertVerifiable, string privateKeyFile) => CreatePathFromString(path, string.Empty, string.Empty, isNotCertVerifiable, privateKeyFile);

        public static IActivityIOOperationsEndPoint CreateOperationEndPointFromIOPath(IActivityIOPath target)
        {
            lock(EndPointsLock)
            {
                if(_endPoints == null)
                {
                    _endPoints = new List<Type>();
                    _referenceCheckers = new List<IActivityIOOperationsEndPoint>();

                    var type = typeof(IActivityIOOperationsEndPoint);

                    var types = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => type.IsAssignableFrom(t))
                        .ToList();

                    foreach (Type t in types)
                    {
                        if(t != typeof(IActivityIOOperationsEndPoint))
                        {
                            _endPoints.Add(t);
                            _referenceCheckers.Add((IActivityIOOperationsEndPoint)Activator.CreateInstance(t));
                        }
                    }
                }
            }
            
            var pos = 0;
            while (pos < _referenceCheckers.Count && !_referenceCheckers[pos].HandlesType(target.PathType))
            {
                pos++;
            }
            IActivityIOOperationsEndPoint result;
            if (_endPoints.Count > 0)
            {
                 result =
                    (IActivityIOOperationsEndPoint)Activator.CreateInstance(_endPoints[pos]);
                result.IOPath = target;


            }
            else
            {
                result = new Dev2FileSystemProvider { IOPath = target };
            }
            return result;
        }

        public static IActivityOperationsBroker CreateOperationsBroker() => new Dev2ActivityIOBroker();

        public static IActivityOperationsBroker CreateOperationsBroker(IFile file, ICommon common)
        {
            return new Dev2ActivityIOBroker(file, common);
        }

        public static Dev2PutRawOperationTO CreatePutRawOperationTO(WriteType writeType, string contents) => new Dev2PutRawOperationTO(writeType, contents);

        public static Dev2UnZipOperationTO CreateUnzipTO(string passwd, bool overwrite)
        {
            return new Dev2UnZipOperationTO(passwd, overwrite);
        }

        public static Dev2ZipOperationTO CreateZipTO(string ratio, string passwd, string name) => CreateZipTO(ratio, passwd, name, false);

        public static Dev2ZipOperationTO CreateZipTO(string ratio, string passwd, string name, bool overwrite) => new Dev2ZipOperationTO(ratio, passwd, name, overwrite);
    }
}
