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

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using System.IO;
using Dev2.PathOperations;
using System.Threading;
using Dev2.Common.Common;
using Dev2.Common.Wrappers;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoPutAction : PerformIntegerIOOperation
    {
        static readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly IWindowsImpersonationContext _impersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _destination;
        protected readonly IFile _fileWrapper;
        protected readonly IFilePath _pathWrapper;
        protected readonly IDev2CRUDOperationTO _arguments;
        protected readonly Stream _currentStream;
        protected readonly string _whereToPut;

        public DoPutAction(Stream currentStream, IActivityIOPath destination, IDev2CRUDOperationTO crudArgument, string whereToPut)
            :this(currentStream, destination, crudArgument, whereToPut, new LogonProvider(), new FileWrapper(), new FilePathWrapper(), ValidateAuthorization.RequiresAuth)
        { }

        public DoPutAction(Stream currentStream, IActivityIOPath destination, IDev2CRUDOperationTO crudArgument, string whereToPut, IDev2LogonProvider logOnProvider, IFile fileWrapper, IFilePath pathWrapper, ImpersonationDelegate impersonationDelegate)
            :base(impersonationDelegate)
        {
            _logOnProvider = logOnProvider;
            _pathWrapper = pathWrapper;
            _fileWrapper = fileWrapper;
            _currentStream = currentStream;
            _destination = destination;
            _arguments = crudArgument;
            _impersonatedUser = _impersonationDelegate(_destination, _logOnProvider);
            _whereToPut = whereToPut;
        }
        public override int ExecuteOperation()
        {
            var destination = _destination;
            if (!_pathWrapper.IsPathRooted(_destination.Path) && _whereToPut != null)
            {
                destination = ActivityIOFactory.CreatePathFromString(_whereToPut + "\\" + _destination.Path, _destination.Username, _destination.Password, _destination.PrivateKeyFile);
            }
            _fileLock.EnterWriteLock();
            try
            {
                using (_currentStream)
                {
                    if (_impersonatedUser != null)
                    {
                        return ExecuteOperationWithAuth(_currentStream, destination);
                    }
                    return WriteData(_currentStream, destination);
                }
            }
            finally
            {
                _fileLock.ExitWriteLock();
                _impersonatedUser?.Undo();
            }
        }

        public override int ExecuteOperationWithAuth(Stream src, IActivityIOPath dst)
        {
            using (_impersonatedUser)
            {
                try
                {
                    return WriteData(src, dst);
                }
                finally
                {
                    _impersonatedUser.Undo();
                }
            }
        }

        int WriteData(Stream src, IActivityIOPath dst)
        {
            if (_arguments.Overwrite)
            {
                File.WriteAllBytes(dst.Path, src.ToByteArray());
                return (int)src.Length;
            }
            if (FileExist(dst, _fileWrapper) || !_arguments.Overwrite)
            {
                using (var stream = new FileStream(dst.Path, FileMode.Append))
                {
                    src.CopyTo(stream);
                }
                return (int)src.Length;
            }
            return -1;
        }
    }
}
