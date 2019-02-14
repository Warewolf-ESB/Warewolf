/*
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
using Dev2.Common.Wrappers;
using Dev2.PathOperations;
using System.Threading;
using Dev2.Common.Common;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoPutActionConfiguration
    {
        public Stream CurrentStream { get; set; }
        public IActivityIOPath Destination { get; set; }
        public IDev2CRUDOperationTO CrudArgument { get; set; }
        public string WhereToPut { get; set; }
        public IDev2LogonProvider Dev2LogonProvider { get; set; }
        public IFilePath FilePath { get; set; }
        public IFile FileWrapper { get; set; }

        public static DoPutActionConfiguration GetDoPutActionConfiguration(Stream currentStream, IActivityIOPath destination, IDev2CRUDOperationTO crudArgument, string whereToPut)
        {
            return new DoPutActionConfiguration
            {
                CurrentStream = currentStream,
                Destination = destination,
                CrudArgument = crudArgument,
                WhereToPut = whereToPut,
                Dev2LogonProvider = new LogonProvider(),
                FilePath = new FilePathWrapper(),
                FileWrapper = new FileWrapper()
            };
        }
    }

    public class DoPutAction : PerformIntegerIOOperation
    {
        static readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly IWindowsImpersonationContext ImpersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath Destination;
        protected readonly IFile _fileWrapper;
        protected readonly IFilePath _pathWrapper;
        protected readonly IDev2CRUDOperationTO _arguments;
        protected readonly Stream _currentStream;
        protected readonly string _whereToPut;

        public DoPutAction(Stream currentStream, IActivityIOPath destination, IDev2CRUDOperationTO crudArgument, string whereToPut)
            :this(new DoPutActionConfiguration { CurrentStream = currentStream, Destination = destination, CrudArgument = crudArgument, WhereToPut = whereToPut }, ValidateAuthorization.RequiresAuth)
        { }

        public DoPutAction(DoPutActionConfiguration doPutActionConfiguration, ImpersonationDelegate impersonationDelegate)
            :base(impersonationDelegate)
        {
            _logOnProvider = doPutActionConfiguration.Dev2LogonProvider;
            _pathWrapper = doPutActionConfiguration.FilePath;
            _fileWrapper = doPutActionConfiguration.FileWrapper;
            _currentStream = doPutActionConfiguration.CurrentStream;
            Destination = doPutActionConfiguration.Destination;
            _arguments = doPutActionConfiguration.CrudArgument;
            ImpersonatedUser = _impersonationDelegate(Destination, _logOnProvider);
            _whereToPut = doPutActionConfiguration.WhereToPut;
        }
        public override int ExecuteOperation()
        {
            var destination = Destination;
            if (!_pathWrapper.IsPathRooted(Destination.Path) && _whereToPut != null)
            {
                destination = ActivityIOFactory.CreatePathFromString(_whereToPut + "\\" + Destination.Path, Destination.Username, Destination.Password, Destination.PrivateKeyFile);
            }
            _fileLock.EnterWriteLock();
            try
            {
                using (_currentStream)
                {
                    if (ImpersonatedUser != null)
                    {
                        return ExecuteOperationWithAuth(_currentStream, destination);
                    }
                    return WriteData(_currentStream, destination);
                }
            }
            finally
            {
                _fileLock.ExitWriteLock();
                ImpersonatedUser?.Undo();
            }
        }

        public override int ExecuteOperationWithAuth(Stream src, IActivityIOPath dst)
        {
            using (ImpersonatedUser)
            {
                try
                {
                    return WriteData(src, dst);
                }
                finally
                {
                    ImpersonatedUser.Undo();
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
