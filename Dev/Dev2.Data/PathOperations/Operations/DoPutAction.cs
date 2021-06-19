#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Factories;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoPutAction : PerformIntegerIOOperation
    {
        static readonly ReaderWriterLockSlim
            _fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        readonly IWindowsImpersonationContext _impersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _destination;
        protected readonly IFile _fileWrapper;
        protected readonly IFileStreamFactory _fileStreamFactory;
        protected readonly IFilePath _pathWrapper;
        protected readonly IDev2CRUDOperationTO _arguments;
        protected readonly Stream _currentStream;
        protected readonly string _whereToPut;
        protected readonly IMemoryStreamFactory _memoryStreamFactory;

        public DoPutAction(Stream currentStream, IActivityIOPath destination, IDev2CRUDOperationTO crudArgument,
            string whereToPut)
            : this(currentStream, destination, crudArgument, whereToPut, new LogonProvider(), new FileWrapper(),
                new FileStreamFactory(), new FilePathWrapper(), new MemoryStreamFactory(),
                ValidateAuthorization.RequiresAuth)
        {
        }

        public DoPutAction(Stream currentStream, IActivityIOPath destination, IDev2CRUDOperationTO crudArgument,
            string whereToPut, IDev2LogonProvider logOnProvider, IFile fileWrapper,
            IFileStreamFactory fileStreamFactory, IFilePath pathWrapper, IMemoryStreamFactory memoryStreamFactory,
            ImpersonationDelegate impersonationDelegate)
            : base(impersonationDelegate)
        {
            _logOnProvider = logOnProvider;
            _pathWrapper = pathWrapper;
            _fileWrapper = fileWrapper;
            _fileStreamFactory = fileStreamFactory;
            _memoryStreamFactory = memoryStreamFactory;
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
                destination = ActivityIOFactory.CreatePathFromString(_whereToPut + "\\" + _destination.Path,
                    _destination.Username, _destination.Password, _destination.PrivateKeyFile);
            }

            _fileLock.EnterWriteLock();
            try
            {
                if (_impersonatedUser != null)
                {
                    return ExecuteOperationWithAuth(_currentStream, destination);
                }

                return WriteData(_currentStream, destination);
            }
            finally
            {
                _fileLock.ExitWriteLock();
            }
        }

        public override int ExecuteOperationWithAuth(Stream src, IActivityIOPath dst)
        {
            using (_impersonatedUser)
            {
                return WriteData(src, dst);
            }
        }

        int WriteData(Stream src, IActivityIOPath dst)
        {
            var streamLength = (int) src.Length;
            if (_arguments.Overwrite)
            {
                _fileWrapper.WriteAllBytes(dst.Path, src.ToByteArray());
                return streamLength;
            }

            if (FileExist(dst, _fileWrapper) || !_arguments.Overwrite)
            {
                using (var stream = _fileStreamFactory.New(dst.Path, FileMode.Append))
                {
                    _memoryStreamFactory.New(src.ToByteArray()).CopyTo(stream);
                }

                return streamLength;
            }

            return -1;
        }
    }
}